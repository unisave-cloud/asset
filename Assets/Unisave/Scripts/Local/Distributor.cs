using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using Unisave.Serialization;

namespace Unisave
{
	/// <summary>
	/// Distributes data from some data repository into target instances
	/// It can collect the data back, but it does so via remembered field references
	/// </summary>
	public class Distributor
	{
		/// <summary>
		/// Repository containin the data to be distributed
		/// And the repo to collect the data back into
		/// </summary>
		private IDataRepository repository;

		/// <summary>
		/// Holds information about data distribution into a target instance
		/// </summary>
		private class DistributionRecord
		{
			/// <summary>
			/// Target instance
			/// </summary>
			public WeakReference target;

			/// <summary>
			/// Fields on the target that were distributed
			/// </summary>
			public List<DistribField> fields = new List<DistribField>();

			/// <summary>
			/// Properties on the target that were distributed
			/// </summary>
			public List<DistribProperty> properties = new List<DistribProperty>();
		}

		private class DistribField
		{
			public FieldInfo field;
			public string key;
		}

		private class DistribProperty
		{
			public PropertyInfo property;
			public string key;
		}

		/// <summary>
		/// Holds the distribution information for each target instance
		/// </summary>
		/// <typeparam name="object">The instance to which we distributed</typeparam>
		/// <typeparam name="DistributionRecord">List of distributed fields and properties</typeparam>
		private List<DistributionRecord> records = new List<DistributionRecord>();

		public Distributor(IDataRepository repository)
		{
			this.repository = repository;
		}

		/// <summary>
		/// Returns or creates distribution record for a given target
		/// (DistributionRecord is used to enumerate all distributed/collected fields and properties)
		/// </summary>
		private DistributionRecord GetDistributionRecord(object target)
		{
			if (target == null)
				throw new ArgumentNullException();

			// From cache //

			DistributionRecord record;
		
			for (int i = 0; i < records.Count; i++)
			{
				object t = records[i].target.Target;

				if (t == null)
				{
					records.RemoveAt(i);
					i--;
					continue;
				}

				if (t == target)
					return records[i];
			}

			// Create new //

			record = new DistributionRecord() {
				target = new WeakReference(target)
			};
			
			// Fields //

			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (FieldInfo fieldInfo in target.GetType().GetFields(flags))
			{
				SavedAsAttribute savedAs;
				if (!IsFieldDistributable(fieldInfo, out savedAs))
					continue;

				record.fields.Add(new DistribField {
					field = fieldInfo,
					key = savedAs.Key
				});
			}

			// Properties //

			flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (PropertyInfo propertyInfo in target.GetType().GetProperties(flags))
			{
				SavedAsAttribute savedAs;
				if (!IsPropertyDistributable(propertyInfo, out savedAs))
					continue;

				record.properties.Add(new DistribProperty {
					property = propertyInfo,
					key = savedAs.Key
				});
			}

			// Store and return //

			records.Add(record);
			return record;
		}

		/// <summary>
		/// Distributes data into the target instance and remembers references for collection
		/// </summary>
		public void Distribute(object target)
		{
			DistributionRecord record = GetDistributionRecord(target);

			DistributeFields(target, record.fields);
			DistributeProperties(target, record.properties);
		}

		private void DistributeFields(object target, List<DistribField> fields)
		{
			foreach (DistribField field in fields)
			{
				// missing key leaves default value
				if (!repository.Has(field.key))
					continue;

				JsonValue distributedValue = repository.Get(field.key);

				// loading null into NonNull leaves default
				if (distributedValue.IsNull && IsNonNull(field.field))
					continue;

				field.field.SetValue(
					target,
					Loader.Load(distributedValue, field.field.FieldType) // json -> c#
				);
			}
		}

		private void DistributeProperties(object target, List<DistribProperty> properties)
		{
			foreach (DistribProperty property in properties)
			{
				// missing key leaves default value
				if (!repository.Has(property.key))
					continue;

				JsonValue distributedValue = repository.Get(property.key);

				// loading null into NonNull leaves default
				if (distributedValue.IsNull && IsNonNull(property.property))
					continue;

				property.property.GetSetMethod().Invoke(
					target,
					new object[] {
						Loader.Load(distributedValue, property.property.PropertyType) // json -> c#
					}
				);
			}
		}

		private bool IsFieldDistributable(FieldInfo fieldInfo, out SavedAsAttribute savedAs)
		{
			savedAs = null;

			// field has to be marked
			object[] marks = fieldInfo.GetCustomAttributes(typeof(SavedAsAttribute), false);
			
			if (marks.Length == 0)
				return false;

			// and non-static
			if (fieldInfo.IsStatic)
			{
				Debug.LogWarning(
					"Unisave: Static fields cannot be marked as [SavedAs] at " + fieldInfo
				);
				return false;
			}

			savedAs = (SavedAsAttribute)marks[0];
			return true;
		}

		private bool IsPropertyDistributable(PropertyInfo propertyInfo, out SavedAsAttribute savedAs)
		{
			savedAs = null;

			// both accessors needed
			if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
				return false;

			// has to be marked
			object[] marks = propertyInfo.GetCustomAttributes(typeof(SavedAsAttribute), false);

			if (marks.Length == 0)
				return false;

			MethodInfo setter = propertyInfo.GetSetMethod();
			MethodInfo getter = propertyInfo.GetGetMethod();

			// both accessors non static
			if (setter.IsStatic || getter.IsStatic)
			{
				Debug.LogWarning(
					"Unisave: Static properties cannot be marked as [SavedAs] at " + propertyInfo
				);
				return false;
			}

			savedAs = (SavedAsAttribute)marks[0];
			return true;
		}

		/// <summary>
		/// Is the given field marked as NonNull?
		/// </summary>
		private static bool IsNonNull(FieldInfo fieldInfo)
		{
			return fieldInfo.GetCustomAttributes(typeof(NonNullAttribute), false).Length > 0;
		}

		/// <summary>
		/// Is the given property marked as NonNull?
		/// </summary>
		private static bool IsNonNull(PropertyInfo propertyInfo)
		{
			return propertyInfo.GetCustomAttributes(typeof(NonNullAttribute), false).Length > 0;
		}

		/// <summary>
		/// Collects data based on references remembered for this instance
		/// </summary>
		public void Collect(object target)
		{
			DistributionRecord record = GetDistributionRecord(target);

			CollectFields(target, record.fields);
			CollectProperties(target, record.properties);
		}

		private void CollectFields(object target, List<DistribField> fields)
		{
			foreach (DistribField field in fields)
			{
				JsonValue collectedValue = Saver.Save(field.field.GetValue(target));
				repository.Set(field.key, collectedValue);
			}
		}

		private void CollectProperties(object target, List<DistribProperty> properties)
		{
			foreach (DistribProperty property in properties)
			{
				JsonValue collectedValue = Saver.Save(
					property.property.GetGetMethod().Invoke(target, new object[] {})
				);
				repository.Set(property.key, collectedValue);
			}
		}

		/// <summary>
		/// Collects data from all remembered instances
		/// </summary>
		public void Collect()
		{
			for (int i = 0; i < records.Count; i++)
			{
				object t = records[i].target.Target;

				if (t == null)
				{
					records.RemoveAt(i);
					i--;
					continue;
				}

				Collect(t);
			}
		}
	}
}

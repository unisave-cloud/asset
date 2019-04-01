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
			/// Fields on the target that were distributed
			/// </summary>
			public List<FieldInfo> fields = new List<FieldInfo>();

			/// <summary>
			/// Properties on the target that were distributed
			/// </summary>
			public List<PropertyInfo> properties = new List<PropertyInfo>();
		}

		/// <summary>
		/// Holds the distribution information for each target instance
		/// </summary>
		/// <typeparam name="object">The instance to which we distributed</typeparam>
		/// <typeparam name="DistributionRecord">List of distributed fields and properties</typeparam>
		private Dictionary<object, DistributionRecord> records = new Dictionary<object, DistributionRecord>();

		public Distributor(IDataRepository repository)
		{
			this.repository = repository;
		}

		/// <summary>
		/// Distributes data into the target instance and remembers references for collection
		/// </summary>
		public void Distribute(object target)
		{
			DistributionRecord record = new DistributionRecord();

			record.fields.AddRange(DistributeFields(target));
			record.properties.AddRange(DistributeProperties(target));

			records[target] = record;
		}

		private IEnumerable<FieldInfo> DistributeFields(object target)
		{
			foreach (FieldInfo fieldInfo in target.GetType().GetFields())
			{
				SavedAsAttribute savedAs;
				if (!IsFieldDistributable(fieldInfo, out savedAs))
					continue;

				yield return fieldInfo;

				// missing key leaves default value
				if (!repository.Has(savedAs.Key))
					continue;

				JsonValue distributedValue = repository.Get(savedAs.Key);

				// loading null into NonNull leaves default
				if (distributedValue.IsNull && IsNonNull(fieldInfo))
					continue;

				fieldInfo.SetValue(
					target,
					Loader.Load(distributedValue, fieldInfo.FieldType) // json -> c#
				);
			}
		}

		private IEnumerable<PropertyInfo> DistributeProperties(object target)
		{
			foreach (PropertyInfo propertyInfo in target.GetType().GetProperties())
			{
				SavedAsAttribute savedAs;
				if (!IsPropertyDistributable(propertyInfo, out savedAs))
					continue;

				yield return propertyInfo;

				// missing key leaves default value
				if (!repository.Has(savedAs.Key))
					continue;

				JsonValue distributedValue = repository.Get(savedAs.Key);

				// loading null into NonNull leaves default
				if (distributedValue.IsNull && IsNonNull(propertyInfo))
					continue;

				propertyInfo.GetSetMethod().Invoke(
					target,
					new object[] {
						Loader.Load(distributedValue, propertyInfo.PropertyType) // json -> c#
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
			MethodInfo getter = propertyInfo.GetSetMethod();

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
			DistributionRecord record;
			
			if (!records.TryGetValue(target, out record))
				throw new ArgumentException(
					"Cannot collect data from instance " + target
					+ " because there was no distribution in the first place."
				);

			// collect
		}

		/// <summary>
		/// Collects data from all remembered instances
		/// </summary>
		public void Collect()
		{
			foreach (KeyValuePair<object, DistributionRecord> pair in records)
				Collect(pair.Key);
		}
	}
}

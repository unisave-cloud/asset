using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Provides API for managing the local part of Unisave
	/// Most usual usage is through the static UnisaveLocal
	/// </summary>
	public class LocalManager
	{
		/// <summary>
		/// Prefix for keys in PlayerPrefs
		/// </summary>
		public const string PlayerPrefsKeyPrefix = "unisave:";

		/// <summary>
		/// The data is loaded from and saved to this repository
		/// </summary>
		private IDataRepository repository;

		/// <summary>
		/// The distributor used by the manager
		/// </summary>
		private Distributor distributor;

		/// <summary>
		/// Creates the instance that is used via the UnisaveLocal facade
		/// </summary>
		public static LocalManager CreateDefaultInstance()
		{
			var repo = new PlayerPrefsDataRepository(PlayerPrefsKeyPrefix);
			repo.SaveImmediately = false;
			return new LocalManager(repo);
		}

		/// <summary>
		/// Creates a local manager that stores data into the provided repository
		/// </summary>
		public LocalManager(IDataRepository repository)
		{
			this.repository = repository;
			distributor = new Distributor(repository);
		}

		/// <summary>
		/// Loads marked fields from local storage
		/// </summary>
		/// <param name="target">Your script, containing marked fields</param>
		public void Load(object target)
		{
			distributor.Distribute(target);
		}

		/// <summary>
		/// Saves marked fields to local storage
		/// </summary>
		/// <param name="target">Your script, containing marked fields</param>
		public void Save(object target)
		{
			distributor.Collect(target);
			repository.Save();
		}

		/// <summary>
		/// Saves all data in all scripts that have been loaded at some point
		/// </summary>
		public void Save()
		{
			distributor.Collect();
			repository.Save();
		}
	}
}

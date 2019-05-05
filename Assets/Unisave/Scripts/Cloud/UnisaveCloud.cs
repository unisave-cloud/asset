using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave.Framework;

namespace Unisave
{
	/// <summary>
	/// Facade for controlling cloud part of Unisave
	/// </summary>
	public static class UnisaveCloud
	{
		private static IBackend backendInstance;

		/// <summary>
		/// Underlying backend instance
		/// </summary>
		public static IBackend Backend
		{
			get
			{
				if (backendInstance == null)
					CreateDefaultBackend();

				return backendInstance;
			}
		}

		private static void CreateDefaultBackend()
		{
			StaticBase.Base = new PunishingFrameworkBase();

			CreateBackendFromPreferences(
				UnisavePreferences.LoadPreferences()
			);

			Debug.Log("Default UnisaveCloud built");
		}

		public static void CreateBackendFromPreferences(UnisavePreferences preferences)
		{
			if (preferences.runAgainstLocalDatabase)
			{
				var database = new LocalDatabase(preferences.localDatabaseName);
				backendInstance = new LocalBackend(
					database,
					new LocalFrameworkBase(database)
				);

				if (preferences.loginOnStart)
				{
					backendInstance.Login(null, preferences.loginOnStartEmail, "");
				}
			}
			else
			{
				throw new NotImplementedException();
				// backend = new CloudBackend(...);
			}
		}

		/// <summary>
		/// If true, we have an authorized player session and we can make requests
		/// </summary>
		public static bool LoggedIn
		{
			get
			{
				return Backend.LoggedIn;
			}
		}

		/// <summary>
		/// The logged-in player
		/// </summary>
		public static Player Player
		{
			get
			{
				return Backend.Player;
			}
		}

		/// <summary>
		/// Starts the login coroutine
		/// </summary>
		/// <returns>False if the login request was ignored for some reason</returns>
		public static bool Login(ILoginCallback callback, string email, string password)
		{
			return Backend.Login(callback, email, password);
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// <returns>False if the logout request was ignored for some reason</returns>
		/// </summary>
		public static bool Logout()
		{
			return Backend.Logout();
		}

		/// <summary>
		/// Starts the player registration coroutine
		/// </summary>
		/// <returns>False if the registration request was ignored for some reason</returns>
		public static bool Register(IRegistrationCallback callback, string email, string password)
		{
			return Backend.Register(callback, email, password);
		}

		/// <summary>
        /// Get controller instance to execute an action on it
        /// </summary>
        public static T Controller<T>() where T : Controller, new()
        {
            return Unisave.Framework.Controller.CreateInstance<T>(Player);
        }



		///////////
		// Waste //
		///////////

		[Obsolete]
		public static void Load(object target)
		{
			throw new NotImplementedException();
		}

		[Obsolete]
		public static bool Save()
		{
			throw new NotImplementedException();
		}
	}
}

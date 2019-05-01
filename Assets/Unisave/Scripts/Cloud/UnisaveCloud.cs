using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Facade for controlling cloud part of Unisave
	/// </summary>
	public static class UnisaveCloud
	{
		/// <summary>
		/// Underlying backend instance
		/// </summary>
		private static IBackend backend;

		static UnisaveCloud()
		{
			var preferences = UnisavePreferences.LoadPreferences();

			if (true) // if run against local backend
			{
				backend = new LocalBackend(new LocalDatabase());

				// if login on start
				if (true)
				{
					backend.Login(null, "local", "password");
				}
			}
			else
			{
				throw new NotImplementedException();
				// backend = new CloudBackend(...);
			}
		}

		/// <summary>
		/// Returns the underlying backend instance
		/// </summary>
		public static IBackend GetBackend()
		{
			return backend;
		}

		/// <summary>
		/// If true, we have an authorized player session and we can make requests
		/// </summary>
		public static bool LoggedIn
		{
			get
			{
				return backend.LoggedIn;
			}
		}

		/// <summary>
		/// Starts the login coroutine
		/// </summary>
		/// <returns>False if the login request was ignored for some reason</returns>
		public static bool Login(ILoginCallback callback, string email, string password)
		{
			return backend.Login(callback, email, password);
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// <returns>False if the logout request was ignored for some reason</returns>
		/// </summary>
		public static bool Logout()
		{
			return backend.Logout();
		}

		/// <summary>
		/// Starts the player registration coroutine
		/// </summary>
		/// <returns>False if the registration request was ignored for some reason</returns>
		public static bool Register(IRegistrationCallback callback, string email, string password)
		{
			return backend.Register(callback, email, password);
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

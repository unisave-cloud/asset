using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Static API for controlling cloud part of Unisave
	/// For more advanced usage see CloudManager class
	/// </summary>
	public static class UnisaveCloud
	{
		/// <summary>
		/// Underlying cloud manager instance
		/// </summary>
		private static CloudManager manager = CloudManager.CreateDefaultInstance();

		/// <summary>
		/// If true, we have an authorized player session and we can make requests
		/// </summary>
		public static bool LoggedIn
		{
			get
			{
				return manager.LoggedIn;
			}
		}

		/// <summary>
		/// Starts the login coroutine
		/// </summary>
		/// <param name="callback">Calls methods here after coroutine finishes</param>
		/// <param name="email">Player email address</param>
		/// <param name="password">Player password</param>
		/// <returns>False if the login request was ignored for some reason</returns>
		public static bool Login(ILoginCallback callback, string email, string password)
		{
			return manager.Login(callback, email, password);
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// <returns>False if the logout request was ignored for some reason</returns>
		/// </summary>
		public static bool Logout()
		{
			return manager.Logout();
		}

		/// <summary>
		/// Registers the script to be loaded after login succeeds
		/// Or loads it now, if user already logged in
		/// </summary>
		public static void LoadAfterLogin(object target)
		{
			manager.LoadAfterLogin(target);
		}

		/// <summary>
		/// Distributes cloud data from cache to a given script instance
		/// Player needs to be logged in. Local debug player is logged in if in editor
		/// </summary>
		public static void Load(object target)
		{
			manager.Load(target);
		}

		/// <summary>
		/// Saves all changes to the server by starting a saving coroutine
		/// <returns>False if the save request was ignored for some reason</returns>
		/// </summary>
		public static bool Save()
		{
			return manager.Save();
		}
	}
}

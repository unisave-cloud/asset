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
		private static CloudManager manager;

		static UnisaveCloud()
		{
			manager = new CloudManager();
		}

		/// <summary>
		/// Starts the login coroutine
		/// </summary>
		/// <param name="callback">Calls methods here after coroutine finishes</param>
		/// <param name="email">Player email address</param>
		/// <param name="password">Player password</param>
		public static void Login(ILoginCallback callback, string email, string password)
		{
			manager.Login(callback, email, password);
		}

		/// <summary>
		/// Starts the logout coroutine or does nothing if already logged out
		/// </summary>
		public static void Logout()
		{
			manager.Logout();
		}

		/// <summary>
		/// Registers the behaviour to be loaded after login succeeds
		/// Or loads it now, if user already logged in
		/// </summary>
		public static void LoadAfterLogin(MonoBehaviour behaviour)
		{
			manager.LoadAfterLogin(behaviour);
		}

		/// <summary>
		/// Distributes cloud data from cache to a given behavior instance
		/// </summary>
		public static void Load(MonoBehaviour behaviour)
		{
			manager.Load(behaviour);
		}
	}
}

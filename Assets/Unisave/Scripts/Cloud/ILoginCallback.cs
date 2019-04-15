using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	public interface ILoginCallback
	{
		/// <summary>
		/// Called, when the login succeeds
		/// </summary>
		void LoginSucceeded();

		/// <summary>
		/// Called, when the login fails
		/// </summary>
		/// <param name="failure">The reason, why login failed</param>
		void LoginFailed(LoginFailure failure);
	}
}

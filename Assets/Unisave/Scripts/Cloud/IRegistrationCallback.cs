using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	public interface IRegistrationCallback
	{
		/// <summary>
		/// Called, when the player registration succeeds
		/// </summary>
		void RegistrationSucceeded();

		/// <summary>
		/// Called, when the player registration fails
		/// </summary>
		/// <param name="failure">The reason, why registration failed</param>
		void RegistrationFailed(RegistrationFailure failure);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Holds all preferences of Unisave
	/// </summary>
	public class UnisavePreferences : ScriptableObject
	{
		public string serverApiUrl = "https://unisave.cloud/api/game/v1.0/";

		public string gameToken;

		public string localDebugPlayerEmail = "local";
	}
}

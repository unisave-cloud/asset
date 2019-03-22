using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	public class UnisavePreferences : ScriptableObject
	{
		public string serverApiUrl = "https://unisave.cloud/api/1.0/";

		public string gameToken;

		public string localDebugPlayerEmail = "local.debug@unirest.cloud";
	}
}

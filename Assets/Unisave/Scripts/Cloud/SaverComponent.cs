using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Handles continual saving
	/// </summary>
	public class SaverComponent : MonoBehaviour
	{
		private const float PeriodInSeconds = 5 * 60.0f; // 5 minutes

		private static SaverComponent globalInstance = null;

		/// <summary>
		/// Finds or creates a new saver instance and returns it
		/// </summary>
		public static SaverComponent GetInstance()
		{
			if (globalInstance == null)
			{
				GameObject go = new GameObject("UnisaveSaver");
				globalInstance = go.AddComponent<SaverComponent>();
			}
			
			return globalInstance;
		}

		void Awake()
		{
			DontDestroyOnLoad(this.gameObject);

			InvokeRepeating("SavingTick", PeriodInSeconds, PeriodInSeconds);
		}

		public void SavingTick()
		{
			if (UnisaveCloud.LoggedIn)
				UnisaveCloud.Save();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Used for it's StartCoroutine method by the rest of Unisave
	/// </summary>
	public class CoroutineRunnerComponent : MonoBehaviour
	{
		private static CoroutineRunnerComponent globalInstance = null;

		/// <summary>
		/// Finds or creates a new runner instance and returns it
		/// </summary>
		public static CoroutineRunnerComponent GetInstance()
		{
			if (globalInstance == null)
			{
				GameObject go = new GameObject("UnisaveCoroutineRunner");
				globalInstance = go.AddComponent<CoroutineRunnerComponent>();
			}
			
			return globalInstance;
		}

		void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
		}
	}
}

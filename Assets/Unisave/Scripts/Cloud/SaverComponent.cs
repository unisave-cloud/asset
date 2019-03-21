using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Runs the continual saving coroutines
	/// (or rather will run once implemented)
	/// </summary>
	public class SaverComponent : MonoBehaviour
	{
		void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
		}
	}
}

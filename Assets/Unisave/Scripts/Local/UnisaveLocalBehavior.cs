using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Performs automatic loading and saving to local storage
	/// during Awake and OnDestroy. Saves only marked public fields.
	/// </summary>
	public class UnisaveLocalBehavior : MonoBehaviour
	{
		protected virtual void Awake()
		{
			Debug.Log("Load stuff!");
		}
		
		protected virtual void OnDestroy()
		{
			Debug.Log("Save stuff!");
		}
	}
}

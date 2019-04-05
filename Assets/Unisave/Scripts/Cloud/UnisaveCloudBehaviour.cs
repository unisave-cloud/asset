using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave;

namespace Unisave
{
	/// <summary>
	/// Performs automatic loading from cloud storage during Awake
	/// Player needs to already be logged in
	/// Saving occurs on logout and also periodically by the unisave system
	/// </summary>
	public class UnisaveCloudBehaviour : MonoBehaviour
	{
		protected virtual void Awake()
		{
			UnisaveCloud.Load(this);
		}
	}
}

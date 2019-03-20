using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	public class SaverComponent : MonoBehaviour
	{
		void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
		}
	}
}

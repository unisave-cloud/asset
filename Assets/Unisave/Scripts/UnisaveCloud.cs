using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave;

public static class UnisaveCloud
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Foo()
	{
		var objectName = "UnisavePreferencesInstance"; // without ".asset" extension

		var preferences = Resources.Load<UnisavePreferences>(objectName);

		Debug.Log(preferences.foo);
	}
}

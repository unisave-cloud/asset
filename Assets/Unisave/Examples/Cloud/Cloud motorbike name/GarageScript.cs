using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;
using Unisave.Framework;

public class PDE : PlayerSingleton
{
	public string MotorbikeName { get; set; }
}

public class GC // : Controller
{

}

namespace Unisave.Examples.Cloud.Motorbike
{
	public class GarageScript : MonoBehaviour
	{
		public InputField motorbikeNameField;

		public string motorbikeName
		{
			get
			{
				return motorbikeNameField.text;
			}

			set
			{
				motorbikeNameField.text = value;
			}
		}

		void Awake()
		{
			motorbikeNameField.text = "Loading...";

			Entity.OfPlayer(UnisaveCloud.Player).Request<PDE>(p => {
				motorbikeNameField.text = p.MotorbikeName;
			});

			// send all singletons with login
			
			// TESTING
			Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
			foreach (Type type in types)
			{
				if (typeof(PlayerSingleton).IsAssignableFrom(type))
					Debug.Log(type.ToString());
			}
		}

		public void OnLogoutButtonClick()
		{
			// call action -> save

			UnisaveCloud.Logout();
			SceneManager.LoadSceneAsync("Unisave/Examples/Cloud/Cloud motorbike name/LoginScene", LoadSceneMode.Single);
		}
	}
}

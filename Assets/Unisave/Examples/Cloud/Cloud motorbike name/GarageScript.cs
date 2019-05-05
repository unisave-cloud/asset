using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;
using Unisave.Framework;

// TODO: move to a separate file
public class PDE : PlayerSingleton
{
	public string MotorbikeName { get; set; }
}

namespace Unisave.Examples.Cloud.Motorbike
{
	// TODO: move to a separate file
	public class GarageController : Controller
	{
		[Action]
		public void ChangeMotorbikeName(string newName)
		{
			var pde = Entity.OfPlayer(UnisaveCloud.Player).Get<PDE>();
			pde.MotorbikeName = newName;
			pde.Save();
		}
	}

	public class GarageScript : MonoBehaviour
	{
		public InputField motorbikeNameField;

		void Awake()
		{
			// Load motorbike name
			Entity.OfPlayer(UnisaveCloud.Player).Request<PDE>(p => {
				motorbikeNameField.text = p.MotorbikeName;
			});

			// TODO: send all singletons with login
			
			// TESTING
			/*Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
			foreach (Type type in types)
			{
				if (typeof(PlayerSingleton).IsAssignableFrom(type))
					Debug.Log(type.ToString());
			}*/
		}

		public void OnLogoutButtonClick()
		{
			// Save motorbike name
			Controller.OfType<GarageController>().ChangeMotorbikeName(motorbikeNameField.text);

			UnisaveCloud.Logout();
			SceneManager.LoadSceneAsync(
				"Unisave/Examples/Cloud/Cloud motorbike name/LoginScene",
				LoadSceneMode.Single
			);
		}
	}
}

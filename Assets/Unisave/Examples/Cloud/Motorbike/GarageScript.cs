using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;

namespace Unisave.Examples.Cloud.Motorbike
{
	public class GarageScript : MonoBehaviour
	{
		public InputField motorbikeNameField;

		void Awake()
		{
			// Load motorbike name
			RequestEntity.OfPlayer(UnisaveCloud.Player).Request<PlayerData>(p => {
				motorbikeNameField.text = p.MotorbikeName;
			});
		}

		public void OnLogoutButtonClick()
		{
			// Save motorbike name
			ControllerAction.On<GarageController>().ChangeMotorbikeName(motorbikeNameField.text);

			UnisaveCloud.Logout();
			SceneManager.LoadSceneAsync(
				"Unisave/Examples/Cloud/Cloud motorbike name/LoginScene",
				LoadSceneMode.Single
			);
		}
	}
}

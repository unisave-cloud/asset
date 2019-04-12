using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;

public class GarageController : UnisaveCloudBehaviour
{
	public InputField motorbikeNameField;

	[SavedAs("motorbike-name")]
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

	public void OnLogoutButtonClick()
	{
		UnisaveCloud.Logout();

		SceneManager.LoadSceneAsync("Unisave/Examples/Cloud/Cloud motorbike name/LoginScene", LoadSceneMode.Single);
	}
}

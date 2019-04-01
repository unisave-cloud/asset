using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;

public class GarageController : MonoBehaviour
{
	public InputField motorbikeNameField;

	[SavedAs("motorbike.name")]
	public string motorbikeName;

	void Awake()
	{
		UnisaveCloud.Load(this);

		motorbikeNameField.text = motorbikeName;
	}

	void Update()
	{
		motorbikeName = motorbikeNameField.text;
	}

	public void OnLogoutButtonClick()
	{
		UnisaveCloud.Logout();

		SceneManager.LoadSceneAsync("Unisave/Examples/Cloud/Cloud motorbike name/LoginScene", LoadSceneMode.Single);
	}
}

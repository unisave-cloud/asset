using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;

public class GarageController : MonoBehaviour
{
	public InputField motorbikeName;

	void Awake()
	{
		
	}
	
	void OnDestroy()
	{
		
	}

	public void OnLogoutButtonClick()
	{
		UnisaveCloud.Logout();

		SceneManager.LoadSceneAsync("Unisave/Examples/Cloud motorbike name/LoginScene", LoadSceneMode.Single);
	}
}

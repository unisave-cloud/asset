using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;

public class LoginController : MonoBehaviour, ILoginController
{
	public InputField email;
	public InputField password;

	public void OnLoginButtonClick()
	{
		UnisaveCloud.Login(this, email.text, password.text);
	}

	public void LoginSucceeded()
	{
		SceneManager.LoadSceneAsync("Unisave/Examples/Cloud motorbike name/GarageScene", LoadSceneMode.Single);
	}

	public void LoginFailed(string message)
	{
		Debug.LogError("Login returned a failure.");
	}
}

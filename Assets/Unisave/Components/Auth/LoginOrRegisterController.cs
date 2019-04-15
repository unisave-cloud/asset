using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unisave
{
	/// <summary>
	/// Controls the default login / register form prefab
	/// </summary>
	public class LoginOrRegisterController : MonoBehaviour, ILoginCallback
	{
		public GameObject loginForm, registerForm;

		public InputField loginEmailField, loginPasswordField;
		public Button loginButton;
		public InputField registerEmailField, registerPasswordField, registerConfirmPasswordField;
		public Button registerButton;
		
		public Button gotoRegistration, gotoLogin;
		
		public Text messageText;

		public string sceneNameToLoad;

		void Awake()
		{
			loginButton.onClick.AddListener(this.OnLoginClicked);
			registerButton.onClick.AddListener(this.OnRegisterClicked);

			gotoRegistration.onClick.AddListener(this.OnGotoRegistration);
			gotoLogin.onClick.AddListener(this.OnGotoLogin);

			// hide text
			messageText.gameObject.SetActive(false);
		}

		void OnLoginClicked()
		{
			messageText.text = "...";
			messageText.gameObject.SetActive(true);

			UnisaveCloud.Login(this, loginEmailField.text, loginPasswordField.text);
		}

		public void LoginSucceeded()
		{
			SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Single);
		}

		public void LoginFailed(LoginFailure failure)
		{
			messageText.text = failure.message;
			messageText.gameObject.SetActive(true);
		}

		void OnRegisterClicked()
		{
			messageText.text = "...";
			messageText.gameObject.SetActive(true);

			// TODO registration api calls
			messageText.text = "TODO: registration...";
			messageText.gameObject.SetActive(true);
		}

		void OnGotoRegistration()
		{
			registerEmailField.text = loginEmailField.text;

			loginForm.SetActive(false);
			registerForm.SetActive(true);
			messageText.gameObject.SetActive(false);
		}

		void OnGotoLogin()
		{
			loginForm.SetActive(true);
			registerForm.SetActive(false);
			messageText.gameObject.SetActive(false);
		}
	}
}

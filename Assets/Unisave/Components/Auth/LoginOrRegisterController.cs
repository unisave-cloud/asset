using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave.Authentication;
using Unisave.Exceptions;
using Unisave.Exceptions.ServerConnection;
using Unisave.Exceptions.PlayerRegistration;

namespace Unisave
{
	/// <summary>
	/// Controls the default login / register form prefab
	/// </summary>
	public class LoginOrRegisterController : MonoBehaviour
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

			Auth.Login(loginEmailField.text, loginPasswordField.text)
				.Then(() => {
					SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Single);
				})
				.Catch(failure => {
					messageText.text = ((LoginFailure)failure).message;
					messageText.gameObject.SetActive(true);
				});
		}

		void OnRegisterClicked()
		{
			messageText.text = "...";
			messageText.gameObject.SetActive(true);

			if (registerPasswordField.text != registerConfirmPasswordField.text)
			{
				messageText.text = "Password confirmation does not match the password.";
				return;
			}

			// TODO: add optional "Name" field
			Dictionary<string, object> hookArguments = new Dictionary<string, object>();
			hookArguments.Add("name", "Peter Peterson!");

			Auth.Register(registerEmailField.text, registerPasswordField.text, hookArguments)
				.Then(() => {
					loginEmailField.text = registerEmailField.text;
					loginPasswordField.text = registerPasswordField.text;

					OnGotoLogin();
					OnLoginClicked();
				})
				.Catch(exception => {
					if (exception is ExceptionWithPlayerMessage)
						messageText.text = ((ExceptionWithPlayerMessage)exception).MessageForPlayer;
					else
						messageText.text = "Unknown error.";

					Debug.LogException(exception);
					messageText.gameObject.SetActive(true);
				});
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

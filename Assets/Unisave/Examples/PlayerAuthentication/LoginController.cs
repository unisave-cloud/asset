using System;
using Unisave.Examples.PlayerAuthentication.Backend;
using Unisave.Facades;
using UnityEngine;
using UnityEngine.UI;

namespace Unisave.Examples.PlayerAuthentication
{
    public class LoginController : MonoBehaviour
    {
        public InputField emailInputField;
        public InputField passwordInputField;
        public Button loginButton;

        private void Start()
        {
            if (emailInputField == null)
                throw new ArgumentNullException(
                    nameof(emailInputField),
                    nameof(LoginController) + " field has not been linked."
                );
            
            if (passwordInputField == null)
                throw new ArgumentNullException(
                    nameof(passwordInputField),
                    nameof(LoginController) + " field has not been linked."
                );
            
            if (loginButton == null)
                throw new ArgumentNullException(
                    nameof(loginButton),
                    nameof(LoginController) + " field has not been linked."
                );
            
            loginButton.onClick.AddListener(LoginButtonClicked);
        }

        private async void LoginButtonClicked()
        {
            bool success = await OnFacet<AuthFacet>.CallAsync<bool>(
                nameof(AuthFacet.Login),
                emailInputField.text,
                passwordInputField.text
            );
            
            if (success)
                LoginSucceeded();
            else
                LoginFailed();
        }

        private void LoginSucceeded()
        {
            // implement your own logic here
            
            Debug.Log("Hooray! You are now logged in.");
        }

        private void LoginFailed()
        {
            // implement your own logic here
            
            Debug.LogError("Login failed! Provided credentials were invalid.");
        }
    }
}
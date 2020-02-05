using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;

namespace Unisave.Examples.Chess
{
	public class HomeController : MonoBehaviour
	{
		private PlayerEntity playerEntity;

		void Start()
		{
			// TODO: show loader

			OnFacet<HomeFacet>.Call<PlayerEntity>("DownloadPlayerEntity")
				.Then(playerEntity => {
					this.playerEntity = playerEntity;

					// TODO: hide loader
				})
				.Done();
		}

		public void OnEnterMatchButtonClick()
		{
			// TODO: join the lobby
		}

		public void OnLogoutButtonClick()
		{
			// TODO: outdated auth
			//Auth.Logout();
			SceneManager.LoadSceneAsync(
				"Unisave/Examples/Chess/Scenes/Login",
				LoadSceneMode.Single
			);
		}
	}
}

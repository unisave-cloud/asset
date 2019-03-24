using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unisave.Examples.Local.Leaderboard
{
	// Handles the game logic
	// After the game finishes, we tell the leaderboard to update itself
	public class Game : MonoBehaviour
	{
		public const float GAME_TIME = 5f; // seconds

		// score of the currently played game (click count)
		private int score;

		// how many seconds remain until the game ends
		private float gameTimeRemaining;

		// is a game currently running?
		private bool gameRunning = false;

		
		#region "Component references"

			public GameObject startGameButton;
			
			public GameObject clickMeButton;

			public InputField playerName;

			public Text statusText;

			public Leaderboard leaderboard;

		#endregion

		void Start()
		{
			statusText.text = "";
			clickMeButton.SetActive(false);
		}
		
		void Update()
		{
			if (gameRunning)
			{
				gameTimeRemaining -= Time.deltaTime;

				if (gameTimeRemaining <= 0f)
					GameIsOver();

				// update UI
				statusText.text = "Score: " + score
					+ "\nTime: " + (int)Mathf.Ceil(gameTimeRemaining) + "s";
			}
		}

		// Start button was clicked
		public void StartTheGame()
		{
			if (gameRunning)
				return;

			gameRunning = true;
			score = 0;
			gameTimeRemaining = GAME_TIME;

			startGameButton.SetActive(false);
			clickMeButton.SetActive(true);

			Debug.Log("Game has started!");
		}

		// ClickMe button was clicked
		public void ClickMeClicked()
		{
			if (!gameRunning)
				return;
			
			score++;
		}

		// called when the time runs out
		public void GameIsOver()
		{
			gameRunning = false;

			statusText.text = "";
			startGameButton.SetActive(true);
			clickMeButton.SetActive(false);

			Debug.Log("Player '" + playerName.text
				+ "' has finished the game with score: " + score + " clicks");

			leaderboard.GameHasFinished(playerName.text, score);
		}

		public void OpenExampleDocumentation()
		{
			Application.OpenURL("https://github.com/Jirka-Mayer/UnisaveDocs/blob/master/leaderboard.md");
		}
	}
}

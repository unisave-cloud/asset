using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unisave;

/*
 * The only pieces of code that actually describe data persistence are:
 * - marking the 'records' field as a saved field
 * - inheriting from 'UnisaveLocalBehavior' which handles loading and saving
 */

// Stores the leaderboard data
// Updates the data after a game has finished
// Updates the leaderboard UI
public class Leaderboard : UnisaveLocalBehavior
{
	// max number of records
	public const int BOARD_SIZE = 5;

	// represents a single leaderboard record
	[Serializable]
	public struct Record
	{
		public int score;
		public string playerName;
	}

	// the leaderboard data
	// stored under the key "leaderboard"
	// default (initial) value is an empty list
	[SavedAs("leaderboard")]
	[NonNull]
	public List<Record> records = new List<Record>();

	public Text leaderboardText;

	void Start()
	{
		UpdateUI();
	}

	// called when a game finishes
	public void GameHasFinished(string playerName, int score)
	{
		records.Add(new Record() {
			playerName = playerName,
			score = score
		});

		records.Sort((a, b) => b.score - a.score);

		if (records.Count > BOARD_SIZE)
			records.RemoveRange(BOARD_SIZE, records.Count - BOARD_SIZE);

		UpdateUI();
	}

	// updates the displayed table to reflect the data
	public void UpdateUI()
	{
		leaderboardText.text = "<b>Leaderboard</b>\n\n<i>Name: Score</i>\n"
			+ "-------------------------\n";

		int index = 1;
		foreach (Record r in records)
		{
			leaderboardText.text += index + ") " + r.playerName + ": " + r.score + "\n";
			index++;
		}

		if (records.Count == 0)
			leaderboardText.text += "Empty :'(";
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unisave;

namespace Unisave.Examples.Local.Leaderboard
{
	/*
		How it works:
	
		1) Awake() is called
			- inside UnisaveLocalBehaviour (parent class)
			- it searches for all [SavedAs("foo")] fields and loads them
		2) You modify these fields
			- however you like (just like any other fields)
			- whenever you like
		3) OnDestroy() is called
			- again inside parent class
			- it searches for all marked fields and saves them
	 */

	public class Leaderboard : UnisaveLocalBehavior
	{
		// max number of records
		public const int BOARD_SIZE = 5;

		// represents a single leaderboard record
		[Serializable]
		public class Record
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

		// the UI component displaying the leaderboard text
		public Text leaderboardText;

		void Start()
		{
			// display and empty or loaded leaderboard
			// (the data is already loaded, Awake has been already called)
			UpdateUI();
		}

		// called when a game finishes
		// by the Game.cs script
		public void GameHasFinished(string playerName, int score)
		{
			// add a new record to the end of the table
			// (but now the order of records is most likely wrong)
			records.Add(new Record() {
				playerName = playerName,
				score = score
			});

			// so sort those records again from best to worst
			records.Sort((a, b) => b.score - a.score);

			// and if we have too many records, keep only the first few best records
			if (records.Count > BOARD_SIZE)
				records.RemoveRange(BOARD_SIZE, records.Count - BOARD_SIZE);

			// and update the displayed text to match the "records" list
			UpdateUI();
		}

		// updates the displayed table to reflect the data (the "records" list)
		public void UpdateUI()
		{
			// title
			leaderboardText.text = "<b>Leaderboard</b>\n\n<i>Name: Score</i>\n"
				+ "-------------------------\n";

			// rows
			int index = 1;
			foreach (Record r in records)
			{
				leaderboardText.text += index + ") " + r.playerName + ": " + r.score + "\n";
				index++;
			}

			// empty table message
			if (records.Count == 0)
				leaderboardText.text += "Empty :'(";
		}
	}
}

/*
	Note:
	
	The code is not very efficient. I should insert new record at a proper place and not
	sort the whole list. Also I should use a StringBuilder in the UpdateUI method.

	But I tried to make the code clear as to what it does so I chose to get rid
	of the added complexity.
*/

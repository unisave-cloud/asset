using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;

namespace Unisave
{
	public class PlayerInformation
	{
		/// <summary>
		/// Globally unique identifier of the player
		/// When saving player-related data anywhere, use this as an identifier
		/// This value does not change during player's lifetime
		/// </summary>
		public string Id { get; private set; }

		public PlayerInformation(string id)
		{
			this.Id = id;
		}

		public static PlayerInformation FromJsonObject(JsonObject json)
		{
			return new PlayerInformation(
				json["id"]
			);
		}
	}
}

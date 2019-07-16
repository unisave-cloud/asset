using System;

namespace Unisave.Auth
{
	/// <summary>
	/// Describes the reason a login attempt has failed
	/// </summary>
	public struct LoginFailure
	{
		public enum FailureType
		{
			/// <summary>Player account not existing or incorect password provided</summary>
			BadCredentials,

			/// <summary>The player has been banned</summary>
			PlayerBanned,

			/// <summary>Server is down for maintenance reasons</summary>
			ServerUnderMaintenance,

			/// <summary>This game version is too old for the server</summary>
			GameClientOutdated,

			/// <summary>
			/// Server wasn't reached. Either missing internet access,
			/// or server down, or network problem.
			/// </summary>
			ServerNotReachable
		}

		/// <summary>
		/// What kind of problem happened
		/// </summary>
		public FailureType type;

		/// <summary>
		/// A message for the player (currently english only)
		/// </summary>
		public string message;

		public static FailureType TypeFromApiResultType(ServerAuthApi.LoginResultType type)
		{
			switch (type)
			{
				case ServerAuthApi.LoginResultType.InvalidCredentials:
					return FailureType.BadCredentials;

				case ServerAuthApi.LoginResultType.PlayerBanned:
					return FailureType.PlayerBanned;

				case ServerAuthApi.LoginResultType.ServerUnderMaintenance:
					return FailureType.ServerUnderMaintenance;

				case ServerAuthApi.LoginResultType.GameClientOutdated:
					return FailureType.GameClientOutdated;

				default:
					return FailureType.ServerNotReachable;
			}
		}
	}
}

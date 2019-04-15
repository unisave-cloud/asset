using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Describes the reason a registration has failed
	/// </summary>
	public struct RegistrationFailure
	{
		public enum FailureType
		{
			/// <summary>Email address is already registered</summary>
			EmailAlreadyRegistered,

			/// <summary>Server is down for maintenance reasons</summary>
			ServerUnderMaintenance,

			/// <summary>Provided email address is invalid</summary>
			InvalidEmail,

			/// <summary>Provided password is invalid</summary>
			InvalidPassword,
			
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

		public static FailureType TypeFromApiResultType(ServerApi.RegistrationResultType type)
		{
			switch (type)
			{
				case ServerApi.RegistrationResultType.InvalidEmail:
					return FailureType.InvalidEmail;

				case ServerApi.RegistrationResultType.InvalidPassword:
					return FailureType.InvalidPassword;

				case ServerApi.RegistrationResultType.EmailAlreadyRegistered:
					return FailureType.EmailAlreadyRegistered;

				case ServerApi.RegistrationResultType.ServerUnderMaintenance:
					return FailureType.ServerUnderMaintenance;

				default:
					return FailureType.ServerNotReachable;
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	public interface ILoginController
	{
		Coroutine StartCoroutine(IEnumerator routine);

		void LoginSucceeded();

		void LoginFailed(string message);
	}
}

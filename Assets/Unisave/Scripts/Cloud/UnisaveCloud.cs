using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Unisave;

public static class UnisaveCloud
{
	private static SaverComponent saver;

	private static string foo = "";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Foo()
	{
		var objectName = "UnisavePreferencesInstance"; // without ".asset" extension

		var preferences = Resources.Load<UnisavePreferences>(objectName);

		Debug.Log(preferences.foo);

		foo = preferences.foo;

		// TESTING: instantiation of GO with script

		GameObject go = new GameObject("UnisaveSaver");
		saver = go.AddComponent<SaverComponent>();
	}

	public static void Login(ILoginController controller, string email, string password)
	{
		controller.StartCoroutine(LoginCoroutine(controller, email, password));
	}

	private static IEnumerator LoginCoroutine(ILoginController controller, string email, string password)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>() {
			{"email", email},
			{"password", password},
			{"foo", foo}
		};

		UnityWebRequest request = UnityWebRequest.Post("https://ptsv2.com/t/o9ukl-1553114563/post", fields);
		
		yield return request.SendWebRequest();

		if (request.isNetworkError || request.isHttpError)
		{
            Debug.Log(request.error);
			controller.LoginFailed(request.error);
		}
        else
		{
			Debug.Log("Request done!");

			Debug.Log(request.downloadHandler.text);
            
			controller.LoginSucceeded();
		}
	}

	public static void Logout(MonoBehaviour controller)
	{
		//controller.StartCoroutine(LogoutCoroutine(controller));

		saver.StartCoroutine(LogoutCoroutine());
	}

	private static IEnumerator LogoutCoroutine()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>() {
			{"lorem", "ipsum"},
			{"data", "is saved here"},
		};

		UnityWebRequest request = UnityWebRequest.Post("https://ptsv2.com/t/o9ukl-1553114563/post", fields);
		
		yield return request.SendWebRequest();

		if (request.isNetworkError || request.isHttpError)
		{
            Debug.Log(request.error);
		}
        else
		{
			Debug.Log("Request done!");
			Debug.Log(request.downloadHandler.text);
		}
	}
}

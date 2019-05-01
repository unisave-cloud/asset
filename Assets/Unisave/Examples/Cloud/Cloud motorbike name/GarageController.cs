using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unisave;
using Unisave.Framework;

public class PDE : PlayerSingleton
{

}

public class GarageController : MonoBehaviour
{
	public InputField motorbikeNameField;

	public string motorbikeName
	{
		get
		{
			return motorbikeNameField.text;
		}

		set
		{
			motorbikeNameField.text = value;
		}
	}

	void Start()
	{
		UnisaveCloud.GetBackend().RequestEntity<PDE>(
			EntityQuery.WithPlayers(new string[] { "LOCAL_ID" }),
			pdes => {
				foreach (PDE p in pdes)
				{
					Debug.Log(p);
					Debug.Log(p.ID);
				}
			}
		);

		//Entity.OfPlayer(Player.Me).Get<PDE>();
	}

	public void OnLogoutButtonClick()
	{
		UnisaveCloud.Logout();

		SceneManager.LoadSceneAsync("Unisave/Examples/Cloud/Cloud motorbike name/LoginScene", LoadSceneMode.Single);
	}
}

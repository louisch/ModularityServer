using UnityEngine;
using System.Collections;

public class ServerManager : MonoBehaviour {
	public string gameName = "Cooking_Foxes";


	// Use this for initialization
	void Start () {
		Debug.Log ("Starting server");
		startServer ();
	}

	void OnServerInitialized ()
	{
		Debug.Log ("Server Started");
		MasterServer.RegisterHost (gameName, "Testing The Unity Netwrok Stuffs", "This is a tutorial thing.");
	}

	void OnMasterServerEvent (MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.RegistrationSucceeded)
		{
			Debug.Log ("Server registration successful");
		}
	}

	void startServer ()
	{
		Network.incomingPassword = "foxesinboxes";
		bool useNat = !Network.HavePublicAddress ();
		Network.InitializeServer (10, 25000, useNat);
	}
}

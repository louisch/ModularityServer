using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class ServerManager : MonoBehaviour {
	public string gameName = "Cooking_Foxes";

	private NetworkView nv;


	// Use this for initialization
	void Start () {
		nv = GetComponent<NetworkView> ();
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

	[RPC]
	void InstantiateWorld () {
		Debug.Log ("Sent instantiation command to player");
	}

	void OnPlayerConnected (NetworkPlayer player)
	{
		Debug.Log ("Connected player " + player.ToString());
		nv.RPC ("InstantiateWorld", RPCMode.Others);
	}
}

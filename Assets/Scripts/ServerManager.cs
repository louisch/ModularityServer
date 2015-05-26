using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
public class ServerManager : MonoBehaviour {
	// server settings
	public string passwd = "foxesinboxes";
	public int serverPopulationLimit = 10;
	public int port = 25000;

	// master server registration info
	public string uniqueGameID = "Cooking_Foxes";
	public string serverName = "Testing The Unity Netwrok Stuffs";
	public string serverComment = "This is a tutorial thing.";

	// network view tied to this server
	private NetworkView nv;

	// default player object
	public Transform player;

	// Server initialisation code
	void Start () {
		nv = GetComponent<NetworkView> ();
		Debug.Log ("Starting server");
		startServer ();
	}

	void startServer ()
	{
		Network.incomingPassword = passwd;
		bool useNat = !Network.HavePublicAddress ();
		Network.InitializeServer (serverPopulationLimit, port, useNat);
	}

	// Register server with MS once it is setup
	void OnServerInitialized ()
	{
		Debug.Log ("Server Started");
		MasterServer.RegisterHost (uniqueGameID, serverName, serverComment);
	}

	// Confirm registration
	void OnMasterServerEvent (MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.RegistrationSucceeded)
		{
			Debug.Log ("Server registration successful");
		}
	}

	// Response to new player connection
	void OnPlayerConnected (NetworkPlayer player)
	{
		Debug.Log ("Connected player " + player.ToString());
		NetworkViewID viewID = Network.AllocateViewID();
		GetComponent<NetworkView> ().RPC ("SpawnPlayer",
											RPCMode.AllBuffered,
											player, viewID);
	}

	void OnPlayerDisconnected (NetworkPlayer player)
	{
		Debug.Log ("Player " + player.ToString () + " disonnected.");
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	[RPC]
	void SpawnPlayer (NetworkPlayer player, NetworkViewID viewID)
	{
		Transform playerShip = Instantiate (this.player, new Vector3(0,0,0), Quaternion.identity) as Transform;
		playerShip.GetComponent<NetworkView>().viewID = viewID;
	}
}

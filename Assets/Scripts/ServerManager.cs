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
	public GameObject playerModel;
	private List<PlayerManager> playerTracker = new List<PlayerManager> ();
	private List<NetworkPlayer> scheduledSpawns = new List<NetworkPlayer> ();

	enum NetworkGroup {DEFAULT = 0, PLAYER = 1, SERVER = 2};

	private bool processSpawnRequests = false;

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
		Debug.Log ("Connected player " + player.guid);
		scheduledSpawns.Add (player);
		processSpawnRequests = true;
	}

	[RPC]
	// players must request spawn
	void RequestSpawn (NetworkPlayer requestee)
	{
		if (!processSpawnRequests)
		{
			Debug.Log ("Invalid spawn request: not currently spawning.");
			return;
		}
		foreach (NetworkPlayer spawn in scheduledSpawns)
		{
			Debug.Log ("Checking spawn " + spawn.guid);
			if (spawn == requestee)
			{
				Debug.Log ("Found requestee in pending spawns list");
				GameObject handle = Instantiate (playerModel) as GameObject;
				PlayerManager man = handle.GetComponent<PlayerManager> ();
				if (!man)
				{
					Debug.LogError ("This player has no PlayerManager!");
					return;
				}
				man.SetOwner (requestee);
				nv.RPC ("SpawnPlayer",
									RPCMode.Others,
									requestee,
									man.GetViewID ());
				playerTracker.Add (man);
				break;
			}
		}

		scheduledSpawns.Remove (requestee);
		if (scheduledSpawns.Count == 0)
		{
			Debug.Log ("Done spawning!");
			processSpawnRequests = false;
		}
	}

	[RPC]
	void SpawnPlayer (NetworkPlayer p, NetworkViewID v)
	{
		Debug.Log ("Spawning player " + p.guid + " in clients.");
	}

	[RPC]
	void DespawnPlayer (NetworkPlayer p)
	{
		Debug.Log ("Despawning player " + p.guid + " in clients.");
	}

	// response to player disconnected
	void OnPlayerDisconnected (NetworkPlayer player)
	{
		Debug.Log ("Player " + player.ToString () + " disonnected.");
		foreach (PlayerManager p in playerTracker)
		{
			if (p.GetOwner () == player)
			{
				//Network.RemoveRPCs (player);
				//Network.RemoveRPCs (p.GetViewID ());

				nv.RPC ("DespawnPlayer", RPCMode.Others, player);
				Destroy (p.gameObject);
			}
		}
	}
}

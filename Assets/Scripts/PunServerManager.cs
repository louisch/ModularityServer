using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
* This script provides functions for setting up the server and managing player connection,
* as well as setting up the game.
*/
[RequireComponent(typeof(PhotonView))]
public class PunServerManager : MonoBehaviour {
	/* Game/server/room information */
	public string gameVersion = "v0.1dev";
	public string roomName = "The Room";
	bool connected = false;
	bool roomOn = false;

	/* Server's photon view controller */
	PhotonView View {get; set;}

	/* Manual button setup (provisional) */
	public float mod = 0.1f;
	float btnX, btnY, btnW, btnH;

	/* Player management */
	// Keeps track of connected, but unspawned players
	ICollection<PhotonPlayer> spawnRequests = new List<PhotonPlayer> ();
	// Keeps track of spawned players
	ICollection<PunPlayerManager> playersInGame = new List<PunPlayerManager> ();
	// default player object prefab
	public GameObject playerModel;
	// true if server is currently accepting spawn requests
	bool playersSpawning = false;

	/**
	* Self-documenting setup method.
	*/
	void Start ()
	{
		btnX = Screen.width * mod;
		btnY = Screen.height * mod;
		btnH = Screen.width * mod;
		btnW = Screen.width * mod;

		View = GetComponent<PhotonView> ();
	}

	/**
	* Called when the server establishes a connection with the photon cloud.
	* Supposedly. It actually doesn't work. Sike!
	*/
	void OnConnectedToMaster ()
	{
		Debug.Log ("Creating room");
		PhotonNetwork.CreateRoom (roomName);
	}

	/**
	* Adds newly connected players to the unspawned player list.
	* Enables spawning (unsafe-ish)
	*/
	void OnPhotonPlayerConnected (PhotonPlayer player)
	{
		Debug.LogFormat ("Adding player {0} to spawn list", player.ToString ());
		spawnRequests.Add (player);
		playersSpawning = true;
	}

	/**
	* Despawns disconnected players locally.
	* PUN calls this method automatically on other clients.
	*/
	void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		Debug.Log ("Player " + disconnected.ToString () + " disconnected.");
		PunPlayerManager found = null;
		foreach (PunPlayerManager player in playersInGame)
		{
			if (player.Owner == disconnected)
			{
				found = player;
				PhotonNetwork.RemoveRPCs (disconnected);
				PhotonNetwork.RemoveRPCs (player.View);

				Destroy (player.gameObject);
				PhotonNetwork.UnAllocateViewID (player.ViewID);
			}
		}

		if (found)
		{
			playersInGame.Remove (found);
		}
	}

	/**
	* Client-side call to request the spawning of their player.
	* Checks if the player is waiting to be spawned. If found, spawns local player object
	* and causes the clients to also spawn a new player object with the same ViewID.
	*/
	[RPC]
	void RequestSpawn (PhotonMessageInfo info)
	{
		if (!playersSpawning)
		{
			Debug.Log ("Invalid spawn request: not currently spawning.");
			return;
		}
		foreach (PhotonPlayer spawn in spawnRequests)
		{
			Debug.Log ("Checking spawn " + spawn.ToString ());
			if (spawn == info.sender)
			{
				Debug.Log ("Found requestee in pending spawns list");
				GameObject handle = Instantiate (playerModel) as GameObject;
				PunPlayerManager man = handle.GetComponent<PunPlayerManager> ();
				if (!man)
				{
					Debug.LogError ("Spawned player lacks a player manager");
					return;
				}
				int viewID = PhotonNetwork.AllocateViewID ();
				man.Owner = info.sender;
				man.ViewID = viewID;
				Debug.LogFormat ("Sending rpc spawn with id {0}", viewID);
				View.RPC ("SpawnPlayer", PhotonTargets.Others, info.sender, viewID);
				playersInGame.Add (man);
				break;
			}
		}

		spawnRequests.Remove (info.sender);
		if (spawnRequests.Count == 0)
		{
			Debug.Log ("Done spawning!");
			playersSpawning = false;
		}
	}

	/**
	* Button and connection setup. Should be replaced with proper UI elements when applicable.
	*/
	void OnGUI ()
	{
        if(!connected && GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Connect to cloud server"))
		{
			Debug.Log ("Connecting to cloud server");
			connected = PhotonNetwork.ConnectUsingSettings (gameVersion);
			if (connected)
			{
				Debug.Log ("Connection successful");
			}
		}
        if(connected && !roomOn && GUI.Button(new Rect(btnX, btnY + 1 + btnH, btnW, btnH), "Create room"))
		{
			Debug.Log ("Creating Room");
			roomOn = PhotonNetwork.CreateRoom (roomName);
			if (roomOn)
			{
				Debug.Log ("Room maded");
			}
		}
	}
}

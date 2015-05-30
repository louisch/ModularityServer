using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class PunServerManager : MonoBehaviour {
	public string gameVersion = "v0.1dev";
	public string roomName = "The Room";
	bool connected = false;
	bool roomOn = false;

	// client net controller netview
	PhotonView View {get; set;}

	// buttons
	public float mod = 0.1f;
	float btnX, btnY, btnW, btnH;

	// player spawn control
	ICollection<PhotonPlayer> spawnRequests = new List<PhotonPlayer> ();
	ICollection<PunPlayerManager> playersInGame = new List<PunPlayerManager> ();
	public GameObject playerModel;
	bool playersSpawning = false;

	void Start ()
	{
		btnX = Screen.width * mod;
		btnY = Screen.height * mod;
		btnH = Screen.width * mod;
		btnW = Screen.width * mod;

		View = GetComponent<PhotonView> ();
	}

	void OnConnectedToMaster ()
	{
		Debug.Log ("Creating room");
		PhotonNetwork.CreateRoom (roomName);
	}

	void OnPhotonPlayerConnected (PhotonPlayer player)
	{
		Debug.Log ("Adding player to spawn list");
		spawnRequests.Add (player);
		playersSpawning = true;
	}

	void OnPhotonPlayerDisconnected (PhotonPlayer disonnected)
	{
		Debug.Log ("Player " + disonnected.ToString () + " disonnected.");
		PunPlayerManager found = null;
		foreach (PunPlayerManager player in playersInGame)
		{
			if (player.Owner == disonnected)
			{
				found = player;
				PhotonNetwork.RemoveRPCs (disonnected);
				PhotonNetwork.RemoveRPCs (player.View);

				View.RPC ("DespawnPlayer", PhotonTargets.Others, disonnected);
				Destroy (player.gameObject);
				PhotonNetwork.UnAllocateViewID (player.ViewID);
			}
		}

		if (found)
		{
			playersInGame.Remove (found);
		}
	}

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

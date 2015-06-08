using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
* Script for controlling spawning-despawning behaviour in clients, as well as general purpose object synchronisation.
*/
[RequireComponent(typeof(PhotonView))]
public class ServerObjectManager : MonoBehaviour {
	PhotonView view;

	/* Player management */
	// Keeps track of connected, but unspawned players
	ICollection<PhotonPlayer> playersInLobby = new List<PhotonPlayer> ();
	// true if server is currently accepting spawn requests
	bool playersSpawning = false;

	public GameObject playerModel;
	public GameObject turretModel;

	// Keeps track of spawned players
	ICollection<GameObject> playersInGame = new List<GameObject> ();
	ICollection<GameObject> modulesInGame = new List<GameObject> ();

	public Vector2 mapSize;
	public int samples;
	List<Vector2> spawnPoints = new List<Vector2> ();


	Vector2 UniformSampleMap ()
	{
		return new Vector2 (Random.Range (-mapSize.x, mapSize.x), Random.Range (-mapSize.y, mapSize.y));
	}

	void SampleMapForSpawnPoints ()
	{
		for (int i = 0; i < samples; i++)
		{
			spawnPoints.Add (UniformSampleMap ());
		}
	}

	void Awake ()
	{
		view = GetComponent<PhotonView> ();
		SampleMapForSpawnPoints ();
	}


	/* Player connection/spawning. */
	/**
	* Adds newly connected players to the unspawned player list.
	* Enables spawning (unsafe-ish)
	*/
	void OnPhotonPlayerConnected (PhotonPlayer player)
	{
		Debug.LogFormat ("Adding player {0} to spawn list", player.ToString ());
		playersInLobby.Add (player);
		playersSpawning = true;
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
		foreach (PhotonPlayer spawn in playersInLobby)
		{
			Debug.Log ("Checking spawn " + spawn.ToString ());
			if (spawn == info.sender)
			{
				Debug.Log ("Found requestee in pending spawns list");
				SpawnPlayer (info.sender);
				break;
			}
		}

		playersInLobby.Remove (info.sender);
		if (playersInLobby.Count == 0)
		{
			Debug.Log ("Done spawning!");
			playersSpawning = false;
		}
	}

	/* Spawns player in every client and in the server. */
	bool SpawnPlayer (PhotonPlayer player)
	{
		int i = Random.Range (0, spawnPoints.Count);
		Debug.Log (i);
		Vector2 pos = spawnPoints[i];
		spawnPoints.RemoveAt (i);
		float rot = 0;

		GameObject playerModule = ObjectConstructor.ConstructPlayer (playerModel, player, pos, rot);
		int controllerID = playerModule.GetComponent<PlayerController>().view.viewID;

		Debug.LogFormat ("Spawning player {0} with id {1}", player.ToString (), controllerID);

		foreach (GameObject inGame in playersInGame)
		{
			PhotonPlayer playerInGame = inGame.GetComponent<PlayerController> ().owner;
			view.RPC ("SpawnPlayer", playerInGame, player, controllerID, pos, rot);
		}
		view.RPC ("SpawnPlayer", player, player, controllerID, pos, rot);

		SpawnAllInPlayer (player);
		playersInGame.Add (playerModule);
		return true;
	}

	/* Spawns all currently in-game objects in a player. */
	bool SpawnAllInPlayer (PhotonPlayer player)
	{
		// Spawn all players already in game in the connecting player.
		foreach (GameObject inGame in playersInGame)
		{
			PlayerController playerInGame = inGame.GetComponent<PlayerController> ();

			Vector2 pos = inGame.transform.position;
			float rot = inGame.transform.rotation.eulerAngles.z;
			view.RPC ("SpawnPlayer", player, playerInGame.owner, playerInGame.view.viewID, pos, rot);
		}

		return true;
	}


	/* De-spawning/disconnection. */
	/**
	* Removes disconnected players locally.
	* PUN calls this method automatically on other clients.
	*/
	void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		Debug.Log ("Player " + disconnected.ToString () + " disconnected.");

		RemovePlayerFromLobby (disconnected);
		RemovePlayerFromGame (disconnected);
	}

	/* Removes player from the lobby list and clears their RPC list. */
	bool RemovePlayerFromLobby (PhotonPlayer player)
	{
		bool found = false;
		foreach (PhotonPlayer lobbyPlayer in playersInLobby)
		{
			if (player == lobbyPlayer)
			{
				found = true;
				PhotonNetwork.RemoveRPCs (player);
				break;
			}
		}
		if (found)
		{
			playersInLobby.Remove (player);
		}
		return found;
	}

	/* Removes player from game and clears all associated RPCs. */
	bool RemovePlayerFromGame (PhotonPlayer player)
	{
		PlayerController found = null;
		foreach (GameObject inGame in playersInGame)
		{
			if (inGame.GetComponent<PlayerController>().owner == player)
			{
				found = inGame.GetComponent<PlayerController>();
			}
		}

		if (found != null)
		{
			found.Disconnect ();
			playersInGame.Remove (found.gameObject);
		}
		return found != null;
	}
}

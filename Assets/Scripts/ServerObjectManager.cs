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

	public PrefabTracker prefabTracker;
	public GameObject defaultPrefab;

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
				SpawnAllInPlayer (info.sender);
				
				GameObject playerPrefab;
				string prefabPath;
				GetPlayerModel (out playerPrefab, out prefabPath);
				GameObject player = SpawnModule (spawn, playerPrefab, prefabPath);

				playersInGame.Add (player);
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

	void GetPlayerModel (out GameObject playerPrefab, out string prefabPath)
	{
		string[] players = prefabTracker.players.ToArray();
		prefabPath = players[Random.Range(0,players.Length)];
		playerPrefab = GetModulePrefabFromPath(prefabPath);
		Debug.Log (prefabPath);
	}

	GameObject GetModulePrefabFromPath (string path)
	{
		GameObject modulePrefab = Resources.Load<GameObject> (path);
		if (modulePrefab == null)
		{
			Debug.LogWarningFormat ("Could not load asset at path '{0}'. Loading default asset instead.", path);
			modulePrefab = defaultPrefab;
		}
		return modulePrefab;
	}

	GameObject SpawnModule (PhotonPlayer owner, GameObject prefab, string prefabPath)
	{
		Vector2 pos = spawnPoints[0];
		spawnPoints.RemoveAt (0);
		float rot = 0;

		Debug.Log ("Spawning " + prefab.name + " for " + owner.ToString());
		
		GameObject module = ObjectConstructor.ConstructModule (prefab, owner, pos, rot);
		int controllerID = module.GetComponent<ModuleController>().view.viewID;

		foreach (GameObject inGame in playersInGame)
		{
			PhotonPlayer playerInGame = inGame.GetComponent<PlayerController> ().owner;
			view.RPC ("SpawnModule", playerInGame, prefabPath, owner, controllerID, pos, rot);
		}
		if (!owner.isLocal)
		{
			view.RPC ("SpawnModule", owner, prefabPath, owner, controllerID, pos, rot);
		}

		return module;
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
			view.RPC ("SpawnModule", player, playerInGame.owner, playerInGame.view.viewID, pos, rot);
		}
		foreach (GameObject inGame in modulesInGame)
		{
			TurretController turret = inGame.GetComponent<TurretController> ();

			Vector2 pos = inGame.transform.position;
			float rot = inGame.transform.rotation.eulerAngles.z;
			view.RPC ("SpawnModule", player, "Shared/Prefabs/Turrets/TurretBase4x1", turret.owner, turret.view.viewID, pos, rot);

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

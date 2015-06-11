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

	// Keeps track of spawned players
	ICollection<PhotonPlayer> playersInGame = new List<PhotonPlayer> ();

	// Keeps track of spawned objects
	Dictionary<GameObject, string> inGameModules = new Dictionary<GameObject, string>();

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
				playersInGame.Add (spawn);
				
				GameObject playerPrefab;
				string prefabPath;
				prefabTracker.GetRandomPlayerModel (out playerPrefab, out prefabPath);

				SpawnModule (spawn, playerPrefab, prefabPath);
				SpawnAllInPlayer (spawn);
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

	GameObject SpawnModule (PhotonPlayer owner, GameObject prefab, string prefabPath)
	{
		Vector2 pos = spawnPoints[0];
		spawnPoints.RemoveAt (0);
		float rot = 0;

		Debug.Log ("Spawning " + prefab.name + " for " + owner.ToString());
		
		GameObject module = ObjectConstructor.ConstructModule (prefab, owner, pos, rot);
		inGameModules.Add (module, prefabPath);
		int controllerID = module.GetComponent<ModuleController>().view.viewID;

		foreach (PhotonPlayer player in playersInGame)
		{
			view.RPC ("SpawnModule", player, prefabPath, owner, controllerID, pos, rot);
		}

		return module;
	}

	/* Spawns all currently in-game objects in a player. */
	void SpawnAllInPlayer (PhotonPlayer player)
	{
		// Spawn all players already in game in the connecting player.
		foreach (KeyValuePair<GameObject,string> inGame in inGameModules)
		{
			ModuleController controller = inGame.Key.GetComponent<ModuleController> ();
			Vector2 pos = inGame.Key.transform.position;
			float rot = inGame.Key.transform.rotation.eulerAngles.z;
			view.RPC ("SpawnModule", player, inGame.Value, controller.owner, controller.view.viewID, pos, rot);
		}
	}


	/* De-spawning/disconnection. */
	/**
	* Removes disconnected players locally.
	* PUN calls this method automatically on other clients.
	*/
	void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		Debug.Log ("Player " + disconnected.ToString () + " disconnected.");

		RemovePlayerFromList (disconnected, playersInLobby);
		RemovePlayerFromList (disconnected, playersInGame);
	}

	/* Removes player from given list and clears their RPC list. */
	bool RemovePlayerFromList (PhotonPlayer player, IEnumerable<PhotonPlayer> playerList)
	{
		bool found = false;
		foreach (PhotonPlayer lobbyPlayer in playerList)
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
}

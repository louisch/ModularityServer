﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
* Script for controlling spawning-despawning behaviour in clients, as well as general purpose object synchronisation.
*/
[RequireComponent(typeof(PhotonView))]
public class ServerObjectManager : MonoBehaviour {
	PhotonView view;

	ObjectConstructor constructor;

	/* Player management */
	// Keeps track of connected, but unspawned players
	ICollection<PhotonPlayer> playersInLobby = new List<PhotonPlayer> ();
	// true if server is currently accepting spawn requests
	bool playersSpawning = false;

	// Keeps track of spawned players
	ICollection<IController> playersInGame = new List<IController> ();
	ICollection<IController> modulesInGame = new List<IController> ();

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
		constructor = GetComponent<ObjectConstructor> ();
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
			SpawnRandomModules ();

		}
	}

	void SpawnRandomModules ()
	{
		foreach (Vector2 pos in spawnPoints)
		{
			IController module = constructor.ConstructRandomModule (pos, 0);
			view.RPC ("SpawnObject", PhotonTargets.Others, module.StatusTracker.TrackerID, module.ControllerID, module.Rb.position, module.Rb.rotation);
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

		IController controller = constructor.ConstructPlayer (player, pos, rot);
		int controllerID = controller.ControllerID;
		int trackerID = controller.StatusTracker.TrackerID;

		Debug.LogFormat ("Spawning player {0} with id {1}", player.ToString (), controllerID);

		foreach (IController inGame in playersInGame)
		{
			view.RPC ("SpawnPlayer", inGame.Owner, player, trackerID, controllerID, pos, rot);
		}
		view.RPC ("SpawnPlayer", player, player, trackerID, controllerID, pos, rot);

		SpawnAllInPlayer (player);
		playersInGame.Add (controller);
		return true;
	}

	/* Spawns all currently in-game objects in a player. */
	bool SpawnAllInPlayer (PhotonPlayer player)
	{
		// Spawn all players already in game in the connecting player.
		foreach (IController inGame in playersInGame)
		{
			Vector2 pos = inGame.Rb.position;
			float rot = inGame.Rb.rotation;
			Debug.LogFormat ("Spawning at {0}, {1}", pos, rot);
			view.RPC ("SpawnPlayer", player, inGame.Owner, inGame.StatusTracker.TrackerID, inGame.ControllerID, pos, rot);
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
		IController found = null;
		foreach (IController inGame in playersInGame)
		{
			if (inGame.Owner == player)
			{
				found = inGame;
			}
		}

		if (found != null)
		{
			found.Disconnect ();
			playersInGame.Remove (found);
		}
		return found != null;
	}
}

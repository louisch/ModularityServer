using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
* Script for controlling spawning-despawning behaviour in clients, as well as general purpose object synchronisation.
*/
[RequireComponent(typeof(PhotonView))]
public class ServerObjectManager : MonoBehaviour {
	PhotonView View {get; set;}

	/* Player management */
	// Keeps track of connected, but unspawned players
	ICollection<PhotonPlayer> playersInLobby = new List<PhotonPlayer> ();
	// true if server is currently accepting spawn requests
	bool playersSpawning = false;

	// Keeps track of spawned players
	ICollection<PlayerController> playersInGame = new List<PlayerController> ();
	// default player object prefab
	public GameObject playerModel;

	void Awake ()
	{
		View = GetComponent<PhotonView> ();
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
		GameObject handle = Instantiate (playerModel) as GameObject;
		PlayerController controller = handle.GetComponent<PlayerController> ();
		if (!controller)
		{
			Debug.LogError ("Spawn failed: no player controller found on player object.");
			return false;
		}
		int viewID = PhotonNetwork.AllocateViewID ();
		controller.Owner = player;
		controller.ViewID = viewID;

		Debug.LogFormat ("Spawning player {0} with id {1}", player.ToString (), viewID);
		foreach (PlayerController inGame in playersInGame)
		{
			View.RPC ("SpawnPlayer", inGame.Owner, player, viewID);
		}
		View.RPC ("SpawnPlayer", player, player, viewID);

		SpawnAllInPlayer (player);
		playersInGame.Add (controller);
		return true;
	}

	/* Spawns all currently in-game objects in a player. */
	bool SpawnAllInPlayer (PhotonPlayer player)
	{
		// Spawn all players already in game in the connecting player.
		foreach (PlayerController inGame in playersInGame)
		{
			View.RPC ("SpawnPlayer", player, inGame.Owner, inGame.ViewID);
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
		foreach (PlayerController inGame in playersInGame)
		{
			if (inGame.Owner == player)
			{
				found = inGame;
				PhotonNetwork.RemoveRPCs (player);
				PhotonNetwork.RemoveRPCs (inGame.View);

				Destroy (inGame.gameObject);
				PhotonNetwork.UnAllocateViewID (inGame.ViewID);
			}
		}

		if (found)
		{
			playersInGame.Remove (found);
		}
		return found != null;
	}
}

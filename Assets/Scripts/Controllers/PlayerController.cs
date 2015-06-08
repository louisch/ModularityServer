using UnityEngine;
using System.Collections;

/**
* This script is for keeping track of players on the server.
* It receives and applies input updates from client players
* and broadcasts position updates to all clients.
*/
public class PlayerController : MonoBehaviour {
	// Object connection info
	public PhotonPlayer owner;
	public PhotonView view;
	
	// Reference to object's rigid body
	Rigidbody2D rb;

	// Movement modifiers
	public PlayerMovement move;
	// Movement input from latest client update
	float strafe;
	float thrust;
	float torque;
	// Fields used to determine whether the state of an object changed since the previous call to FixedUpdate
	Vector2 previousPosition;
	float previousRotation;

	/**
	* Runs setup on newly created player object.
	* (self-documenting)
	*/
	void Awake ()
	{
		Debug.Log ("Player created");
		move = new PlayerMovement ();
	}

	public void Setup (PhotonPlayer owner, PhotonView view)
	{

		this.owner = owner;
		this.view = view;

		rb = GetComponent<Rigidbody2D> ();
		previousRotation = rb.rotation;
		previousPosition = rb.position;	
	}

	/**
	* Every update, computes movement based on the client's input and applies it.
	* Additionally, checks if the object moved since previous call.
	*/
	void FixedUpdate ()
	{
		// apply input to client model
		move.Move (ref rb, strafe, thrust, torque);
	}

	/**
	* RPC called by clients to update the object's input info.
	* Only the client that owns the object is allowed to update its input info.
	*/
	[RPC]
	public void UpdateInput (float strafe, float thrust, float torque, PhotonMessageInfo info)
	{
		if (info.sender != owner)
		{
			Debug.LogWarningFormat ("Illegal UpdateInput attempt: player {0} attempted to move player {1}",
									info.sender.ToString (),
									owner.ToString ());
			return;
		}
		this.strafe = strafe;
		this.thrust = thrust;
		this.torque = torque;
	}

	/**
	* Serialises state changes for client.
	* Currently, the whole state is sent, even if only part of the state changed.
	*/
	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (DetectChange () && stream.isWriting)
		{
			Vector2 pos = rb.position;
			float rotation = rb.rotation;

			stream.Serialize(ref pos);
			stream.Serialize(ref rotation);
		}
		else if (!stream.isWriting)
		{
			Debug.LogError ("Server object is receiving positional info from client");
		}
	}

	/**
	* Checks if object's state has changed since last call.
	* Updates the state tracking fields if it has.
	*/
	bool DetectChange ()
	{
		// sets update only if position or rotation has changed
		bool update = previousPosition != rb.position || previousRotation != rb.rotation;
		if (update)
		{
			// update state fields
			previousPosition = rb.position;
			previousRotation = rb.rotation;
		}
		return update;
	}

	void OnDestroy ()
	{
		Debug.Log ("Player " + owner.ToString() + " despawned");
		PhotonNetwork.RemoveRPCs (view);
		PhotonNetwork.RemoveRPCs (owner);
		PhotonNetwork.UnAllocateViewID (view.viewID);
	}

	void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		if (disconnected == owner)
		{
			Destroy (gameObject);
		}
	}

	public void Disconnect ()
	{
		OnPhotonPlayerDisconnected (owner);
	}
}

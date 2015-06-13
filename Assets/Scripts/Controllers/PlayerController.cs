using UnityEngine;
using System.Collections;

/**
* This script is for keeping track of players on the server.
* It receives and applies input updates from client players
* and broadcasts position updates to all clients.
*/
public class PlayerController : ModuleController, IController {
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
	protected override void Awake ()
	{
		base.Awake();
		Debug.Log ("Player created");
		move = new PlayerMovement ();
		rb = GetComponent<Rigidbody2D> ();
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
		if (info.sender != this.info.owner)
		{
			Debug.LogWarningFormat ("Illegal UpdateInput attempt: player {0} attempted to move player {1}",
									info.sender.ToString (),
									this.info.owner.ToString ());
			return;
		}
		this.strafe = strafe;
		this.thrust = thrust;
		this.torque = torque;
	}

	protected override void OnDestroy ()
	{
		Debug.Log ("Player " + info.owner.ToString() + " despawned");
		base.OnDestroy ();
	}
}

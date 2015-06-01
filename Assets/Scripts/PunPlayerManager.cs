using UnityEngine;
using System.Collections;

/**
* This script is for keeping track of players on the server.
* It receives and applies input updates from client players
* and broadcasts position updates to all clients.
*/
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
public class PunPlayerManager : MonoBehaviour {
	// Object connection info
	public PhotonPlayer Owner {get; set;}
	public PhotonView View {get; private set;}
	public int ViewID
	{
		get
		{
			return View.viewID;
		}
		set
		{
			View.viewID = value;
		}
	}

	// Movement modifiers
	public float strafeModifier = 100;
	public float thrustModifier = 100;
	public float torqueModifier = 5;
	// Movement input from latest client update
	float strafe;
	float thrust;
	float torque;
	// Fields used to determine whether the state of an object changed since the previous call to FixedUpdate
	Vector2 previousPosition;
	float previousRotation;
	bool update = false;
	
	// Reference to object's rigid body
	Rigidbody2D rb;

	// Runs setup on newly created player object
	void Awake ()
	{
		Debug.Log ("Player created");
		rb = GetComponent<Rigidbody2D> ();
		View = GetComponent<PhotonView> ();
		previousRotation = rb.rotation;
		previousPosition = rb.position;
	}

	void FixedUpdate ()
	{
		// check if rb changed since last FixedUpdate
		CheckChanges ();

		// apply input to movement
		Vector2 moveForce = new Vector2 (strafe,thrust).normalized;
		moveForce = new Vector2(moveForce.x * strafeModifier * Time.fixedDeltaTime,
								moveForce.y * thrustModifier * Time.fixedDeltaTime);
		rb.AddForce (moveForce);
		rb.AddTorque (torque * torqueModifier * Time.fixedDeltaTime);
	}

	// checks if rb changed since last call and updates rb info cache
	void CheckChanges ()
	{
		// sets update only if position or rotation has changed
		update = previousPosition != rb.position || previousRotation != rb.rotation;

		// update movement cache
		previousPosition = rb.position;
		previousRotation = rb.rotation;

	}

	// client call to update input data
	[RPC]
	public void UpdateInput (float strafe, float thrust, float torque, PhotonMessageInfo info)
	{
		if (info.sender != Owner)
		{
			Debug.LogWarningFormat ("Illegal move: player {0} attempted to move player {1}",
									info.sender.ToString (),
									Owner.ToString ());
			return;
		}
		this.strafe = strafe;
		this.thrust = thrust;
		this.torque = torque;
	}

	// determines which information is transferred on object update
	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (update && stream.isWriting)
		{
			Debug.Log ("Serialising position information");
			Vector2 pos = rb.position;
			Vector2 velocity = rb.velocity;
			float rotation = rb.rotation;

			stream.Serialize(ref pos);
			stream.Serialize(ref velocity);
			stream.Serialize(ref rotation);
		}
		else if (!stream.isWriting)
		{
			// server should not be recieving information
			Debug.LogError ("Server object is recieving position data from client");
		}
	}
}


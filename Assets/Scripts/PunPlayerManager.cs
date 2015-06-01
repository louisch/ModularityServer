using UnityEngine;
using System.Collections;

// controls players on the server
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
public class PunPlayerManager : MonoBehaviour {
	// network info
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

	// move variables
	public float strafeModifier = 100;
	public float thrustModifier = 100;
	public float torqueModifier = 5;

	float strafe;
	float thrust;
	float torque;

	Vector2 previousPosition;
	float previousRotation;
	bool update = false;
	
	// object position info
	Rigidbody2D rb;

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
		moveForce = new Vector2(moveForce.x * strafeModifier * Time.fixedDeltaTime, moveForce.y * thrustModifier * Time.fixedDeltaTime);
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


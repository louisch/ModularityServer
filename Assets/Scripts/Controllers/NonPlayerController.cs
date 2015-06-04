using UnityEngine;
using System.Collections;

public class NonPlayerController : MonoBehaviour, IController {
// Object connection info
	public PhotonPlayer Owner {get;set;}
	PhotonView view;
	public PhotonView View
	{
		get
		{
			return view;
		}
		set
		{
			view = value;
		}
	}
	public int ControllerID
	{
		get
		{
			return view.viewID;
		}
		set
		{
			view.viewID = value;
		}
	}
	public ObjectStatusTracker StatusTracker {get;set;}	

	// Reference to object's rigid body
	Rigidbody2D rb;
	public Rigidbody2D Rb
	{
		get
		{
			return rb;
		}
		set
		{
			rb = value;
			previousRotation = rb.rotation;
			previousPosition = rb.position;
			
		}
	}

	// Fields used to determine whether the state of an object changed since the previous call to FixedUpdate
	Vector2 previousPosition;
	float previousRotation;

	/**
	* Runs setup on newly created player object.
	* (self-documenting)
	*/
	void Awake ()
	{
		Debug.Log ("Object created");
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

	/**
	* Serialises state changes for client.
	* Currently, the whole state is sent, even if only part of the state changed.
	*/
	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (DetectChange () && stream.isWriting)
		{
			Vector2 pos = rb.position;
			Vector2 velocity = rb.velocity;
			float rotation = rb.rotation;

			stream.Serialize(ref pos);
			stream.Serialize(ref velocity);
			stream.Serialize(ref rotation);
		}
		else if (!stream.isWriting)
		{
			Debug.LogError ("Server object is receiving positional info from client");
		}
	}

	void OnDestroy ()
	{
		PhotonNetwork.RemoveRPCs (view);
		PhotonNetwork.UnAllocateViewID (ControllerID);
	}

	public void Disconnect ()
	{
		// do nothing	
	}
}

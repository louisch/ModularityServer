using UnityEngine;
using System.Collections;

public class ModuleController : MonoBehaviour, IController {
	// Object connection info
	public PhotonPlayer owner;
	public PhotonView view;

	// Fields used to determine whether the state of an object changed since the previous call to FixedUpdate
	Vector2 previousPosition;
	float previousRotation;


	public virtual void Setup (PhotonPlayer owner, PhotonView view)
	{

		this.owner = owner;
		this.view = view;

		previousRotation = transform.rotation.eulerAngles.z;
		previousPosition = transform.position;	
	}

	/**
	* Serialises state changes for client.
	* Currently, the whole state is sent, even if only part of the state changed.
	*/
	protected virtual void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (DetectChange () && stream.isWriting)
		{
			Vector2 pos = transform.position;
			float rotation = transform.rotation.eulerAngles.z;
			stream.Serialize(ref pos);
			stream.Serialize(ref rotation);
		}
		else if (!stream.isWriting)
		{
			Debug.LogWarning ("Server object is receiving positional info from client");
		}
	}

	/**
	* Checks if object's state has changed since last call.
	* Updates the state tracking fields if it has.
	*/
	protected virtual bool DetectChange ()
	{
		// sets update only if position or rotation has changed
		bool update = previousPosition != (Vector2)transform.position || previousRotation != transform.rotation.eulerAngles.z;
		if (update)
		{
			// update state fields
			previousPosition = transform.position;
			previousRotation = transform.rotation.eulerAngles.z;
		}
		return update;
	}

	protected virtual void OnDestroy ()
	{
		PhotonNetwork.RemoveRPCs (view);
		PhotonNetwork.RemoveRPCs (owner);
		PhotonNetwork.UnAllocateViewID (view.viewID);
	}

	protected virtual void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		if (disconnected == owner)
		{
			ServerObjectManager.RemoveObjectFromGame(gameObject);
			Destroy (gameObject);
		}
	}

	public void Disconnect ()
	{
		OnPhotonPlayerDisconnected (owner);
	}
}

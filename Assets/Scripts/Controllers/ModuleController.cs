using UnityEngine;
using System.Collections;

public class ModuleController : MonoBehaviour, IController {
	public ModuleInfo info;

	// Fields used to determine whether the state of an object changed since the previous call to FixedUpdate
	Vector2 previousPosition;
	float previousRotation;

	protected virtual void Awake ()
	{
		Debug.Log ("Fetching module info");
		info = GetComponent<ModuleInfo> ();
		if (info == null)
		{
			Debug.LogWarning ("Could not find module info component for module " + gameObject.name);
		}
	}

	protected virtual void OnEnable ()
	{
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
		PhotonNetwork.RemoveRPCs (info.view);
		PhotonNetwork.RemoveRPCs (info.owner);
		PhotonNetwork.UnAllocateViewID (info.view.viewID);
	}

	protected virtual void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		if (disconnected == info.owner)
		{
			ServerObjectManager.RemoveObjectFromGame(gameObject);
			Destroy (gameObject);
		}
	}

	[RPC]
	protected void DetachModule (PhotonMessageInfo info)
	{
		if (info.sender == this.info.owner)
		{
			Debug.Log ("Got detach request");
			this.info.owner = PhotonNetwork.player;
			UpdateOwnership ();
		}
	}

	[RPC]
	protected void AttachModule (PhotonMessageInfo info)
	{
		if (this.info.owner.isLocal)
		{
			Debug.Log ("Got attach request");
			this.info.owner = info.sender;
			UpdateOwnership ();
		}
	}

	protected void UpdateOwnership ()
	{
		info.view.RPC ("ChangeOwner", PhotonTargets.Others, this.info.owner);
	}

	public void Disconnect ()
	{
		OnPhotonPlayerDisconnected (info.owner);
	}
}

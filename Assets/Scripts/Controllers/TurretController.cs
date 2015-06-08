using UnityEngine;
using System.Collections;

public class TurretController : ModuleController {
	public Turret[] turrets;

	Vector2 mouse;

	public override void Setup (PhotonPlayer owner, PhotonView view)
	{
		base.Setup (owner, view);
		turrets = GetComponentsInChildren<Turret> (true);
	}

	void FixedUpdate ()
	{
		if (!owner.isLocal)
		{
			foreach (Turret turret in turrets)
			{
				Vector2 mouseOffset = mouse - (Vector2)turret.transform.position;
				float angle = Mathf.Atan2(mouseOffset.y, mouseOffset.x) * Mathf.Rad2Deg; 
				turret.rotateTo = Quaternion.Euler (0,0,angle - 90);
				turret.Rotate ();
			}
		}
		else
		{
			foreach (Turret turret in turrets)
			{
				turret.IdleRotate ();
			}	
		}
	}

	[RPC]
	void MouseUpdate (Vector2 mouse, PhotonMessageInfo info)
	{
		if (info.sender != owner)
		{
			Debug.LogFormat ("Player {0} is attempting to control object belonging to player {1}", info.sender.ToString(), owner.ToString());
			return;
		}
		this.mouse = mouse;
	}

	[RPC]
	void FireAllTurrets (PhotonMessageInfo info)
	{
		if (info.sender != owner)
		{
			Debug.LogWarningFormat ("Player {0} is attempting to control object belonging to player {1}", info.sender.ToString(), owner.ToString());
			return;
		}
		foreach (Turret turret in turrets)
		{
			turret.FIRE ();
		}
		view.RPC ("OpenFire", PhotonTargets.Others);
	}

	[RPC]
	void Select (PhotonMessageInfo info)
	{
		Debug.Log ("Selection request received for " + gameObject.name);
		if (owner.isLocal)
		{
			owner = info.sender;
			view.RPC ("ChangeOwnership", PhotonTargets.Others, owner);
		}
		else if (owner != info.sender)
		{
			Debug.LogWarningFormat ("Player {0} is attempting to take control of player {1}'s module", info.sender.ToString(), owner.ToString());
			return;
		}
	}

	[RPC]
	void Deselect (PhotonMessageInfo info)
	{
		Debug.Log ("deselection request received for " + gameObject.name);
		if (owner == info.sender)
		{
			OnPhotonPlayerDisconnected (info.sender);
			return;
		}
	}


	protected override void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			foreach (Turret turret in turrets)
			{
				Quaternion rotation = turret.transform.rotation;
				stream.SendNext (rotation);
			}
		}
		base.OnPhotonSerializeView(stream, info);
	}

	protected override void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		if (disconnected == owner)
		{
			Debug.Log ("Resetting turret ownership");
			owner = PhotonNetwork.player;
			view.RPC ("ChangeOwnership", PhotonTargets.Others, owner);
		}
	}
}

using UnityEngine;
using System.Collections;

public class TurretController : ModuleController {
	public Turret[] turrets;

	Vector2 mouse;

	protected override void OnEnable ()
	{
		turrets = GetComponentsInChildren<Turret> (true);
	}

	void FixedUpdate ()
	{
		if (!this.info.owner.isLocal)
		{
			foreach (Turret turret in turrets)
			{
				Vector2 mouseOffset = mouse - (Vector2)turret.transform.position;
				float angle = Mathf.Atan2(mouseOffset.y, mouseOffset.x) * Mathf.Rad2Deg; 
				turret.rotateTo = Quaternion.Euler (0,0,angle - 90);
				turret.Rotate ();
			}
		}
	}

	[RPC]
	void MouseUpdate (Vector2 mouse, PhotonMessageInfo info)
	{
		if (info.sender != this.info.owner)
		{
			Debug.LogFormat ("Player {0} is attempting to control object belonging to player {1}", info.sender.ToString(), this.info.owner.ToString());
			return;
		}
		this.mouse = mouse;
	}

	[RPC]
	void FireAllTurrets (PhotonMessageInfo info)
	{
		if (info.sender != this.info.owner)
		{
			Debug.LogWarningFormat ("Player {0} is attempting to control object belonging to player {1}", info.sender.ToString(), this.info.owner.ToString());
			return;
		}
		foreach (Turret turret in turrets)
		{
			turret.FIRE ();
		}
		this.info.view.RPC ("OpenFire", PhotonTargets.Others);
	}

	protected override void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			bool controlled = !this.info.owner.isMasterClient;
			stream.SendNext (controlled);
			if (controlled)
			{
				Debug.Log ("Sending");
				foreach (Turret turret in turrets)
				{
					float rotation = turret.transform.rotation.eulerAngles.z;
					stream.SendNext (rotation);
				}
			}
		}
		base.OnPhotonSerializeView(stream, info);
	}

	protected override void OnPhotonPlayerDisconnected (PhotonPlayer disconnected)
	{
		if (disconnected == this.info.owner)
		{
			Debug.Log ("Resetting turret ownership");
			this.info.owner = PhotonNetwork.player;
			this.info.view.RPC ("ChangeOwnership", PhotonTargets.Others, this.info.owner);
		}
	}
}

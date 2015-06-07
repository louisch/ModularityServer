﻿using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour {
	public PhotonPlayer owner;
	public PhotonView view;

	public Turret[] turrets;

	Vector2 mouse;
	
	void FixedUpdate ()
	{
		if (owner == null)
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
		this.mouse = mouse;
	}

	[RPC]
	void FireAllTurrets (PhotonMessageInfo info)
	{
		foreach (Turret turret in turrets)
		{
			turret.FIRE ();
		}
		view.RPC ("OpenFire", PhotonTargets.Others);
	}


	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			foreach (Turret turret in turrets)
			{
				Quaternion rotation = turret.transform.rotation;
				stream.SendNext (rotation);
			}
		}
	}
}

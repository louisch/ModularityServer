using UnityEngine;
using System.Collections;

/* Script used to control the shooting behaviour of a turret or a guns. */
public class Barrel : MonoBehaviour {
	// barrel info
	public GameObject shot;
	public float reloadDelay = 0.5f;
	public float reloadUntil = 0;

	public void FIRE (PhotonPlayer owner, float rangeMod, float speedMod)
	{
		if (reloadUntil <= Time.time)
		{
			BulletPhysics bullet = ((GameObject)Instantiate (shot, transform.position, transform.rotation)).GetComponent<BulletPhysics>();
			bullet.Setup (owner, rangeMod, speedMod);
			reloadUntil = Time.time + reloadDelay;
		}
	}

	public void ReduceCooldown (float reduceReloadBy)
	{
		reloadUntil -= reduceReloadBy;
	}
}

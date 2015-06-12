using UnityEngine;
using System.Collections;

/* Script used to control the shooting behaviour of a turret or a guns. */
public class Barrel : MonoBehaviour {

	public Turret turret;
	public ModuleController controller;
	public GameObject shot;
	public float reloadDelay = 0.5f;
	public float reloadUntil = 0;

	public void FIRE ()
	{
		if (reloadUntil <= Time.time)
		{
			BulletPhysics bullet = ((GameObject)Instantiate (shot, transform.position, transform.rotation)).GetComponent<BulletPhysics>();
			bullet.turretRange += turret.range;
			bullet.owner = controller.owner;
			reloadUntil = Time.time + reloadDelay;
		}
	}

	public void ReduceCooldown (float reduceReloadBy)
	{
		reloadUntil -= reduceReloadBy;
	}
}

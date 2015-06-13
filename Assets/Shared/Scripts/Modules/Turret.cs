using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour {
	public ModuleInfo info;

	// turret setup ref
	public Barrel[] barrels;
	public float range;
	public float speedMod;
	public float turningSpeed;
	public float idleTurningMod = 4;

	public Quaternion rotateTo;

	void FixedUpdate ()
	{
		if (info.owner.isMasterClient)
		{
			transform.rotation *= Quaternion.Euler (0,0,turningSpeed/idleTurningMod);
		}
	}

	public void Rotate ()
	{
		transform.rotation = Quaternion.RotateTowards (transform.rotation, rotateTo, turningSpeed);
	}

	public void FIRE ()
	{
		foreach (Barrel barrel in barrels)
		{
			barrel.FIRE(info.owner, range, speedMod);
		}
	}
}

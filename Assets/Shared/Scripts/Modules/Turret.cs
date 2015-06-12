using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour {
	public Barrel[] barrels;
	public float turningSpeed;
	public bool owned = false;
	public float range = 6;

	public Quaternion rotateTo;

	void FixedUpdate ()
	{
		if (!owned)
		{
			transform.rotation *= Quaternion.Euler (0,0,turningSpeed/4);
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
			barrel.FIRE();
		}
	}
}

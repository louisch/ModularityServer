using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour {
	public Barrel[] barrels;
	public float turningSpeed;

	public Quaternion rotateTo;

	public void Rotate ()
	{
		transform.rotation = Quaternion.RotateTowards (transform.rotation, rotateTo, turningSpeed);
	}

	public void IdleRotate ()
	{
		transform.rotation *= Quaternion.Euler (0,0,turningSpeed);
	}

	public void FIRE ()
	{
		foreach (Barrel barrel in barrels)
		{
			barrel.FIRE();
		}
	}
}

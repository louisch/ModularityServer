using UnityEngine;
using System.Collections;

/* Script used to control the shooting behaviour of a turret or a guns. */
public class Barrel : MonoBehaviour {

	public GameObject shot;
	public float reloadDelay = 0.5f;
	public float reloadUntil = 0;

	public void FIRE ()
	{
		if (reloadUntil <= Time.time)
		{
			Instantiate (shot, transform.position, transform.rotation);
			reloadUntil = Time.time + reloadDelay;
		}
	}

	public void ReduceReload (float reduceReloadBy)
	{
		reloadUntil -= reduceReloadBy;
	}
}

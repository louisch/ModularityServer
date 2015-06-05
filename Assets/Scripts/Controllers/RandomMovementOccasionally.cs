using UnityEngine;
using System.Collections;

public class RandomMovementOccasionally : MonoBehaviour {

	Rigidbody2D Rb
	{
		get
		{
			return GetComponent<Rigidbody2D> ();
		}
	}

	float nextTick;
	public float minTick = 0.5f;
	public float maxTick = 10;
	public float speed = 5;

	void FixedUpdate ()
	{
		if (Rb != null && nextTick < Time.time)
		{
			nextTick = Random.Range (minTick, maxTick) + Time.time;
			Vector2 dir = new Vector2(Random.Range(-1,1), Random.Range(-1,1)).normalized;
			Rb.AddForce (dir * speed);
		}
	}
}

using UnityEngine;
using System.Collections;

/* Script for controlling how an individual bullet moves through space and stuff. */
public class BulletPhysics : MonoBehaviour {

	public float speed = 25;
	public Rigidbody2D rb;

	void FixedUpdate ()
	{
		rb.AddForce (speed*transform.up);
	}
}

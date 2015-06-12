using UnityEngine;
using System.Collections;

/* Script for controlling how an individual bullet moves through space and stuff. */
public class BulletPhysics : MonoBehaviour {

	public float speed = 25;
	public float defaultRange = 10;
	public float turretRange = 0;
	public Rigidbody2D rb;
	public PhotonPlayer owner;

	Vector3 shotFrom;

	void Awake ()
	{
		shotFrom = transform.position;
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		Debug.Log ("Triggered by " + other.gameObject.name);
		ModuleController controller = other.gameObject.GetComponent<ModuleController>();
		if (controller != null && controller.owner != owner)
		{
			Destroy (gameObject);
		}
	}

	void FixedUpdate ()
	{
		if ((shotFrom - transform.position).magnitude > defaultRange + turretRange)
		{
			Destroy(gameObject);
		}
		rb.AddForce (speed*transform.up);
	}
}

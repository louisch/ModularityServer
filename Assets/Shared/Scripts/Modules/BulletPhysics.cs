using UnityEngine;
using System.Collections;

/* Script for controlling how an individual bullet moves through space and stuff. */
public class BulletPhysics : MonoBehaviour {
	public PhotonPlayer owner;
	public float speed = 25;
	public float range = 10;
	public float damage = 5;

	public Rigidbody2D rb;

	Vector3 shotFrom;

	void Awake ()
	{
		shotFrom = transform.position;
	}

	public void Setup (PhotonPlayer owner, float rangeMod, float speedMod)
	{
		this.owner = owner;
		range += rangeMod;
		speed += speedMod;
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		Debug.Log ("Triggered by " + other.gameObject.name);
		ModuleInfo info = other.gameObject.GetComponent<ModuleInfo>();
		if (info != null && info.owner != owner)
		{
			Destroy (gameObject);
			info.TakeDamage (damage);
		}
	}

	void FixedUpdate ()
	{
		if ((shotFrom - transform.position).magnitude > range)
		{
			Extinguish();
		}
		rb.velocity = transform.up * speed;
	}

	void Extinguish ()
	{
			Destroy(gameObject);
	}
}

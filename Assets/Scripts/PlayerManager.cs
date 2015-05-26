using UnityEngine;
using System.Collections;

// controls players on the server
[RequireComponent(typeof(NetworkView))]
public class PlayerManager : MonoBehaviour {
	public Rigidbody controller;

	public float horitontalSpeed = 50;
	public float verticalSpeed = 100;

	private float h;
	private float v;

	void FixedUpdate ()
	{
		if (Network.isClient)
		{
			Debug.Log ("Why am *I* the client?!?");
			return;
		}

		controller.velocity = new Vector3 (h * horitontalSpeed * Time.fixeDeltaTime,
											0,
											v * verticalSpeed * Time.fixedDeltaTime);
	}

	// client call to update motion data
	[RPC]
	public void UpdateClientMotion (float h, float v)
	{
		this.h = h;
		this.v = v;
	}
}

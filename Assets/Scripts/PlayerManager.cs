using UnityEngine;
using System.Collections;

// controls players on the server
[RequireComponent(typeof(NetworkView))]
public class PlayerManager : MonoBehaviour {
	public Rigidbody controller;

	public float horitontalSpeed = 10;
	public float verticalSpeed = 10;

	private NetworkPlayer owner;
	private NetworkView nv;
	private float h;
	private float v;

	// Once the object is created server-side, cause spawn in clients
	void Awake ()
	{
		Debug.Log ("Player created");
		nv = GetComponent<NetworkView> ();
	}

	public void SetOwner (NetworkPlayer owner)
	{
		this.owner = owner;
	}

	public NetworkPlayer GetOwner ()
	{
		return owner;
	}

	public NetworkView GetView ()
	{
		return nv;
	}

	public NetworkViewID GetViewID ()
	{
		return nv.viewID;
	}

	void FixedUpdate ()
	{
		if (Network.isClient)
		{
			Debug.LogError ("Why am *I* the client?!?");
			return;
		}

		controller.velocity = new Vector3 (h * horitontalSpeed,
											0,
											v * verticalSpeed);
	}

	// client call to update motion data
	[RPC]
	public void UpdateClientMotion (float h, float v)
	{
		this.h = h;
		this.v = v;
	}
}

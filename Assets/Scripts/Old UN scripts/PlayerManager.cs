using UnityEngine;
using System.Collections;

// controls players on the server
[RequireComponent(typeof(NetworkView))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerManager : MonoBehaviour {
	private NetworkPlayer owner;

	public float hSpeed = 10;
	public float vSpeed = 10;
	public float maxDelta = 0.001F; // precision for serialized floating point values

	private float hInput;
	private float vInput;
	
	// object components
	private Rigidbody rb;
	private NetworkView nv;
	private Transform objectTrans;

	void Awake ()
	{
		Debug.Log ("Player created");
		rb = GetComponent<Rigidbody> ();
		nv = GetComponent<NetworkView> ();
		objectTrans = GetComponent<Transform> ();
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
			Debug.LogError ("Server is running in client mode!");
			return;
		}

		// normalise input vecor
		Vector3 moveBy = new Vector3 (hInput,0,vInput).normalized;
		// compute movement delta vector (frame independent)
		moveBy = new Vector3(moveBy.x * hSpeed * Time.fixedDeltaTime, 0, moveBy.z * vSpeed * Time.fixedDeltaTime);

		rb.MovePosition(objectTrans.position + moveBy);
	}

	// client call to update input data
	[RPC]
	public void UpdateInput (float h, float v)
	{
		this.hInput = h;
		this.vInput = v;
	}

	// determines which information is transferred on object update
	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		Debug.Log ("Serialising is happening");

		if (stream.isWriting)
		{
			Debug.Log ("Serialising position information");
			Vector3 pos = objectTrans.position;
			Quaternion rot = objectTrans.rotation;
			stream.Serialize(ref pos);//, maxDelta);
			stream.Serialize(ref rot);//, maxDelta);
		}
		else
		{
			// server should not be recieving information
			Debug.LogError ("Server object is recieving position data from client!");
		}
	}
}

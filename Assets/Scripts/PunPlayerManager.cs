using UnityEngine;
using System.Collections;

// controls players on the server
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
public class PunPlayerManager : MonoBehaviour {
	// network info
	public PhotonPlayer Owner {get; set;}
	public PhotonView View {get; private set;}
	public int ViewID
	{
		get
		{
			return View.viewID;
		}
	}

	// move variables
	public float hSpeed = 10;
	public float vSpeed = 10;
	public float maxDelta = 0.001F; // precision for serialized floating point values

	float hInput;
	float vInput;
	
	// object position info
	Rigidbody rb;
	Transform trans;

	void Awake ()
	{
		Debug.Log ("Player created");
		rb = GetComponent<Rigidbody> ();
		View = GetComponent<PhotonView> ();
		trans = GetComponent<Transform> ();
	}

	void FixedUpdate ()
	{
		// normalise input vecor
		Vector3 moveBy = new Vector3 (hInput,0,vInput).normalized;
		// compute movement delta vector (frame independent)
		moveBy = new Vector3(moveBy.x * hSpeed * Time.fixedDeltaTime, 0, moveBy.z * vSpeed * Time.fixedDeltaTime);
		rb.MovePosition(trans.position + moveBy);
	}

	// client call to update input data
	[RPC]
	public void UpdateInput (float h, float v)
	{
		this.hInput = h;
		this.vInput = v;
	}

	// determines which information is transferred on object update
	void OnPhotonSerializeNetworkView (BitStream stream, NetworkMessageInfo info)
	{
		Debug.Log ("Serialising is happening");

		if (stream.isWriting)
		{
			Debug.Log ("Serialising position information");
			Vector3 pos = trans.position;
			Quaternion rot = trans.rotation;
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


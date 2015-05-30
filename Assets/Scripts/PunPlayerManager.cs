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
		set
		{
			View.viewID = value;
		}
	}

	// move variables
	public float hSpeed = 10;
	public float vSpeed = 10;
	public float maxDelta = 0.001F; // precision for serialized floating point values

	float hInput;
	float vInput;
	bool update = false;
	
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
		Vector3 moveBy = new Vector3 (hInput,0,vInput).normalized;
		moveBy = new Vector3(moveBy.x * hSpeed * Time.fixedDeltaTime, 0, moveBy.z * vSpeed * Time.fixedDeltaTime);
		rb.MovePosition(trans.position + moveBy);
		update = moveBy != Vector3.zero;
	}

	// client call to update input data
	[RPC]
	public void UpdateInput (float h, float v)
	{
		hInput = h;
		vInput = v;
	}

	// determines which information is transferred on object update
	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (update && stream.isWriting)
		{
			Debug.Log ("Serialising position information");
			Vector3 pos = trans.position;
			Quaternion rot = trans.rotation;
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
		}
		else if (!stream.isWriting)
		{
			// server should not be recieving information
			Debug.LogError ("Server object is recieving position data from client");
		}
	}
}


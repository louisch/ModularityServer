using UnityEngine;
using System.Collections;

// controls players on the server
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
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

	float hInput;
	float vInput;
	bool update = false;
	
	// object position info
	Rigidbody2D rb;

	void Awake ()
	{
		Debug.Log ("Player created");
		rb = GetComponent<Rigidbody2D> ();
		View = GetComponent<PhotonView> ();
	}

	void FixedUpdate ()
	{
		Vector2 moveBy = new Vector2 (hInput,vInput).normalized;
		moveBy = new Vector2(moveBy.x * hSpeed * Time.fixedDeltaTime, moveBy.y * vSpeed * Time.fixedDeltaTime);
		rb.MovePosition(rb.position + moveBy);
		update = moveBy != Vector2.zero;
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
			Vector2 pos = rb.position;
			Vector2 velocity = rb.velocity;
			float rotation = rb.rotation;

			stream.Serialize(ref pos);
			stream.Serialize(ref velocity);
		}
		else if (!stream.isWriting)
		{
			// server should not be recieving information
			Debug.LogError ("Server object is recieving position data from client");
		}
	}
}


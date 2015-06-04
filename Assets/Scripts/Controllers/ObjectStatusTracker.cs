using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
public class ObjectStatusTracker : MonoBehaviour {
	public PhotonView view;
	public int TrackerID
	{
		get
		{
			return view.viewID;
		}
		set
		{
			view.viewID = value;
		}
	}

	Rigidbody2D rb;
	public Rigidbody2D RB
	{
		set
		{
			rb = value;
		}

	}

	float prev_rb_mass;
	float prev_rb_drag;
	float prev_rb_angularDrag;

	bool DetectChange ()
	{
		bool update = rb.mass != prev_rb_mass || rb.drag != prev_rb_drag || rb.angularDrag != prev_rb_angularDrag;
		if (update)
		{
			prev_rb_drag = rb.drag;
			prev_rb_mass = rb.mass;
			prev_rb_angularDrag = rb.angularDrag;
			Debug.Log ("Gasp!");
		}
		return update;
	}

	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (DetectChange () && stream.isWriting)
		{
			Debug.Log ("Sending this.");
			string toSend = "Got this!";
			stream.Serialize(ref toSend);
		}
		else if (!stream.isWriting)
		{
			Debug.LogError ("Server object is receiving positional info from client");
		}
	}

	void OnDestroy ()
	{
		Debug.Log ("Tracker destroyed");
		PhotonNetwork.RemoveRPCs (view);
		PhotonNetwork.UnAllocateViewID (TrackerID);
	}
}

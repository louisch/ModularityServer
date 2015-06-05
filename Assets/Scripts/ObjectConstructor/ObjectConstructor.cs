using UnityEngine;
using System.Collections;

public class ObjectConstructor : MonoBehaviour {

	// player variables
	public GameObject playerModule;
	public GameObject randomModule;

	public float moduleSpawnZ = 3;

	/* Constructs a player object in game. */
	public IController ConstructPlayer (PhotonPlayer owner, Vector2 position, float rotation)
	{
		GameObject player = InstantiateModule(playerModule, owner, "player");

		Rigidbody2D rb = AddRigidbody (player, new PilotModuleRigidbodyInfo (), position, rotation);
		ObjectStatusTracker tracker = AddStatusTracker (player, rb);
		return AddController (player, owner, rb, tracker);
	}

	/* Constructs a random module module in game. */
	public IController ConstructRandomModule (Vector2 position, float rotation)
	{
		GameObject module = InstantiateModule(randomModule, PhotonNetwork.player, "randomModule");
		Rigidbody2D rb = AddRigidbody (module, new RandomModuleRigidbodyInfo (), position, rotation);
		ObjectStatusTracker tracker = AddStatusTracker (module, rb);
		return AddController (module, PhotonNetwork.player, rb, tracker);
	}

	GameObject InstantiateModule (GameObject prefab, PhotonPlayer owner, string nameString)
	{
		GameObject module = Instantiate<GameObject> (prefab);
		module.name = "(construct)" + nameString + " " + owner.ToString ();
		return module;
	}

	IController AddModuleComponents (GameObject module, PhotonPlayer owner, RigidbodyInfo info, Vector2 position, float rotation)
	{
		Rigidbody2D rb = AddRigidbody (module, info, position, rotation);
		ObjectStatusTracker tracker = AddStatusTracker (module, rb);
		return AddController (module, owner, rb, tracker);
	}

	Rigidbody2D AddRigidbody (GameObject module, RigidbodyInfo info, Vector2 position, float rotation)
	{
		Vector3 pos = (Vector3)position + new Vector3 (0,0,moduleSpawnZ);
		Rigidbody2D rb = module.AddComponent<Rigidbody2D> ();
		// disable ridigbody
		rb.Sleep ();
		// set up rigidbody info
		rb.mass = info.mass;
		rb.drag = info.drag;
		rb.angularDrag = info.angularDrag;
		rb.gravityScale = info.gravityScale;
		// set object's positional info
		module.transform.position = pos;
		rb.rotation = rotation;
		// re-enable rigidbody
		rb.WakeUp ();

		return rb;
	}

	ObjectStatusTracker AddStatusTracker (GameObject module, Rigidbody2D rb)
	{
		// add photon view and tracker
		PhotonView view = module.AddComponent<PhotonView> ();
		ObjectStatusTracker tracker = module.AddComponent<ObjectStatusTracker> ();
		tracker.view = view;
		tracker.RB = rb;
		tracker.TrackerID = PhotonNetwork.AllocateViewID ();

		// setup view to use the tracker
		view.observed = tracker;
		view.synchronization = ViewSynchronization.UnreliableOnChange;

		return tracker;
	}

	IController AddController (GameObject module, PhotonPlayer owner, Rigidbody2D rb, ObjectStatusTracker tracker)
	{
		PhotonView view = module.AddComponent<PhotonView> ();
		IController controller;
		if (!owner.isLocal)
		{
			controller = module.AddComponent<PlayerController> ();
		}
		else
		{
			controller = module.AddComponent<NonPlayerController> ();
		}

		controller.Owner = owner;
		controller.View = view;
		controller.ControllerID = PhotonNetwork.AllocateViewID ();
		controller.StatusTracker = tracker;
		controller.Rb = rb;

		// setup view to observe the controller
		view.observed = controller as Component;
		view.synchronization = ViewSynchronization.UnreliableOnChange;

		return controller;
	}
}

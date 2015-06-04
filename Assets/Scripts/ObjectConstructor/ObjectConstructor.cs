﻿using UnityEngine;
using System.Collections;

public class ObjectConstructor : MonoBehaviour {
	// general variables
	float gravityScale = 0;

	// player variables
	public GameObject playerModule;


	public float defaultPlayerMass = 1;
	public float defaultPlayerDrag = 4;
	public float defaultPlayerAngularDrag = 10;

	public float playerZ = 3;

	/* Constructs a player object in game. */
	public PlayerController ConstructPlayer (PhotonPlayer owner, int trackerID, int controllerID)
	{
		GameObject player = Instantiate (playerModule) as GameObject;
		player.name = "(construct)Player " + owner.ToString ();
		player.transform.position = new Vector3 (0,0,playerZ);

		// add rigid body
		player.AddComponent<Rigidbody2D> ();
		Rigidbody2D rb = player.GetComponent<Rigidbody2D> ();
		rb.mass = defaultPlayerMass;
		rb.drag = defaultPlayerDrag;
		rb.angularDrag = defaultPlayerAngularDrag;
		rb.gravityScale = gravityScale;

		// add photon views
		player.AddComponent<PhotonView> ();
		player.AddComponent<PhotonView> ();
		PhotonView[] views = player.GetComponents<PhotonView> ();

		// setup status tracker
		player.AddComponent<ObjectStatusTracker> ();
		ObjectStatusTracker tracker = player.GetComponent<ObjectStatusTracker> ();
		tracker.view = views[0];
		tracker.RB = rb;
		tracker.TrackerID = trackerID;
		// setup view to use the tracker
		views[0].observed = tracker;
		views[0].synchronization = ViewSynchronization.UnreliableOnChange;

		// setup controller
		player.AddComponent<PlayerController> ();
		PlayerController controller = player.GetComponent<PlayerController> ();
		controller.owner = owner;
		controller.view = views[1];
		controller.ControllerID = controllerID;
		controller.statusTracker = tracker;
		controller.RB = rb;
		// setup view to observe the controller
		views[1].observed = controller;
		views[1].synchronization = ViewSynchronization.UnreliableOnChange;

		return controller;
	}
}
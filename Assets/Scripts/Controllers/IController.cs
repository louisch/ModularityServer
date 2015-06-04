using UnityEngine;
using System.Collections;

public interface IController {
	PhotonPlayer Owner {get;set;}
	PhotonView View {get;set;}
	int ControllerID {get;set;}
	ObjectStatusTracker StatusTracker {get;set;}
	Rigidbody2D Rb {get;set;}

	void Disconnect ();
}

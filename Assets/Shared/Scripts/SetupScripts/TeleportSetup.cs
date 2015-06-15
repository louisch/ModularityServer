using UnityEngine;
using System.Collections;

public class TeleportSetup : MonoBehaviour, ISetup {

	public ModuleController AddController (GameObject module)
	{
		return module.AddComponent<TeleportController> ();
	}
}

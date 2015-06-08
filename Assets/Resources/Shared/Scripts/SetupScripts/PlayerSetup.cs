using UnityEngine;
using System.Collections;

public class PlayerSetup : MonoBehaviour, ISetup {
	public ModuleController AddController (GameObject module)
	{
		return module.AddComponent<PlayerController> ();
	}
}

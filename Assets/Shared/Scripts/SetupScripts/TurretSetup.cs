using UnityEngine;
using System.Collections;

public class TurretSetup : MonoBehaviour, ISetup {
	public ModuleController AddController (GameObject module)
	{
		return module.AddComponent<TurretController> ();
	}
}

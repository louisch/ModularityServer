using UnityEngine;
using System.Collections;

public class ObjectConstructor : MonoBehaviour {
	public static float moduleSpawnZ = 3;

	/* Constructs a player object in game. */
	public static GameObject ConstructModule (GameObject prefab, PhotonPlayer owner, Vector2 position, float rotation)
	{
		GameObject module = InstantiateModuleWithName (prefab, position, rotation, prefab.name);
		module.SetActive (false);
		AddRigidbody (module, new RandomModuleRigidbodyInfo ());

		ISetup setupScript = prefab.GetComponent<ISetup> ();
		if (setupScript == null)
		{
			Debug.LogErrorFormat ("Prefab module '{0}' does not have a setup script of type ISetup", prefab.name);
			return null;
		}

		ModuleController controller = setupScript.AddController (module);

		PhotonView view = AddViewForComponent (controller, PhotonNetwork.AllocateViewID ());
		controller.info = module.GetComponent<ModuleInfo> ();
		controller.info.Setup (owner, view);
		
		module.SetActive (true);
		return module;
	}

	static GameObject InstantiateModuleWithName (GameObject prefab, Vector2 position, float rotation, string nameString)
	{
		GameObject module = Instantiate (prefab, position, Quaternion.Euler (0,0,rotation)) as GameObject;
		module.name = "(construct)" + nameString;

		return module;
	}

	static PhotonView AddViewForComponent (Component component, int id)
	{
		PhotonView view = component.gameObject.AddComponent<PhotonView> ();

		view.observed = component;
		view.viewID = id;
		view.synchronization = ViewSynchronization.UnreliableOnChange;

		return view;
	}

	static Rigidbody2D AddRigidbody (GameObject module, RigidbodyInfo info)
	{
		Rigidbody2D rb = module.AddComponent<Rigidbody2D> ();
		// disable ridigbody
		rb.Sleep ();
		// set up rigidbody info
		rb.mass = info.mass;
		rb.drag = info.drag;
		rb.angularDrag = info.angularDrag;
		rb.gravityScale = info.gravityScale;
		// re-enable rigidbody
		rb.WakeUp ();

		return rb;
	}
}

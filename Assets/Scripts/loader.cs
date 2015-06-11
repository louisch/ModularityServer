using UnityEngine;
using System.Collections;

public class loader : MonoBehaviour {
	public GameObject thing;
	public string path;

	// Use this for initialization
	void Update () {
		if (thing == null)
		{
			thing = Resources.Load<GameObject> (path);
			Debug.Log (path);
		}
	}
}

using UnityEngine;
using System.Collections.Generic;

public class ModuleInfo : MonoBehaviour {
	// ownership information
	public PhotonPlayer owner;

	// status information
	public List<AttributeModifier> modifiers = new List<AttributeModifier> ();
	public float mass;

	// descriptions
	public string name;
	public string description;
	public string actionDescription;




	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

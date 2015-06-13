using UnityEngine;
using System.Collections.Generic;

public class ModuleInfo : MonoBehaviour, ISelectable {
	// ownership information
	public PhotonPlayer owner;
	public PhotonView view;

	// status information
	public List<AttributeModifier> modifiers = new List<AttributeModifier> ();
	public float mass;

	// descriptions
	public string moduleName;
	public string description;
	public string actionDescription;

	// selection information
	public bool selected;

	// called externally to finish initialising module
	public void Setup (PhotonPlayer owner, PhotonView view)
	{
		this.owner = owner;
		this.view = view;
	}

	// called when module is selected
	public void Select ()
	{
		selected = true;
		GetComponent<SpriteRenderer> ().color = Color.blue;
	}

	// called when module is deselected
	public void Deselect ()
	{
		selected = false;
		GetComponent<SpriteRenderer> ().color = Color.cyan;
	}

	public bool CanAttach ()
	{
		return owner.isMasterClient && selected;
	}

	public bool CanDetach ()
	{
		return owner.isLocal && selected;
	}
}

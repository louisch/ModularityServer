using UnityEngine;
using System.Collections.Generic;

public class ModuleInfo : MonoBehaviour, ISelectable {
	// ownership information
	public PhotonPlayer owner;
	public PhotonView view;

	// status information
	public List<AttributeModifier> modifiers = new List<AttributeModifier> ();
	public float mass = 6;
	public float hp = 10;
	public bool lethalDamage = false;
	public float damageFlashPeriod = .25f;
	public float armor = 0;

	// descriptions
	public string moduleName;
	public string description;
	public string actionDescription;

	// selection information
	public bool selected;

	// Sprite color info
	public SpriteRenderer sprite;

	void Awake ()
	{
		sprite = GetComponent<SpriteRenderer> ();
	}

	void Update ()
	{
		if (lethalDamage)
		{
			Flash ();
		}
	}

	void Flash ()
	{
		float lerp = Mathf.Sin (Time.time/damageFlashPeriod);
		sprite.color = Color.Lerp (Color.blue, Color.cyan, lerp);
	}

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
		sprite.color = Color.blue;
	}

	// called when module is deselected
	public void Deselect ()
	{
		selected = false;
		sprite.color = Color.cyan;
	}

	public void RestoreHealth (float health)
	{
		hp += health;
	}

	public void TakeDamage (float damage)
	{
		hp -= (damage - armor);
		if (hp < 0)
		{
			if (lethalDamage)
			{
				BlowUp ();	
			}
			else
			{
				lethalDamage = true;
			}
		}
	}

	void BlowUp ()
	{
		Destroy (gameObject);
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

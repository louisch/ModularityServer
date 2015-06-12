using UnityEngine;

[System.Serializable]
public class AttributeModifier {
	public string attribute;
	public float modifier;

	public AttributeModifier (string attribute, float modifier)
	{
		this.attribute = attribute;
		this.modifier = modifier;
	}
}

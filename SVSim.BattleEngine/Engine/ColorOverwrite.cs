using UnityEngine;

public class ColorOverwrite : MonoBehaviour
{
	public enum Change
	{
		No,
		UseDeckColorSet,
		UseBingoButtonSet
	}

	[SerializeField]
	private Change _change;

	[SerializeField]
	private bool _dontChangeEffectDistance;

	[SerializeField]
	private bool _dontChangeEffectStyle;

	public Change ColorChange => _change;

	public bool DontChangeEffectDistance => _dontChangeEffectDistance;

	public bool DontChangeEffectStyle => _dontChangeEffectStyle;
}

using UnityEngine;

namespace Wizard;

[RequireComponent(typeof(UILabel))]
public class UILabelEffectOverwriter : ParameterOverwriterBase
{
	private class EffectParameters
	{
		public UILabel.Effect EffectStyle { get; }

		public Color EffectColor { get; }

		public Vector2 EffectDistance { get; }

		public EffectParameters(UILabel label)
		{
			EffectStyle = label.effectStyle;
			EffectColor = label.effectColor;
			EffectDistance = label.effectDistance;
		}

		public void Apply(UILabel label)
		{
			label.effectStyle = EffectStyle;
			label.effectColor = EffectColor;
			label.effectDistance = EffectDistance;
		}
	}

	[SerializeField]
	private UILabel _targetLabel;

	[SerializeField]
	private UILabel.Effect _effectStyle;

	[SerializeField]
	private string _effectColorId;

	[SerializeField]
	private Vector2 _effectDistance = Vector2.one;

	private EffectParameters _originalParameters;

	public eColorCodeId EffectColorId
	{
		set
		{
			_effectColorId = value.ToString();
			RequestOverwrite();
		}
	}

	protected override GameObject TargetObject => _targetLabel.gameObject;

	protected override void SaveOriginalParameters()
	{
		_originalParameters = new EffectParameters(_targetLabel);
	}

	protected override void UndoParameters()
	{
		_originalParameters.Apply(_targetLabel);
	}

	protected override void OverwriteParameters()
	{
		OverwriteEffectStyle();
		OverwriteEffectColor();
		OverwriteEffectDistance();
	}

	private void OverwriteEffectStyle()
	{
		_targetLabel.effectStyle = _effectStyle;
	}

	private void OverwriteEffectColor()
	{
		if (ColorTryParse(_effectColorId, out var color))
		{
			_targetLabel.effectColor = color;
		}
	}

	private void OverwriteEffectDistance()
	{
		_targetLabel.effectDistance = _effectDistance;
	}
}

using UnityEngine;

namespace Wizard;

[RequireComponent(typeof(UILabel))]
public class UILabelGradientOverwriter : ParameterOverwriterBase
{
	private class GradientParameters
	{
		public bool ApplyGradient { get; }

		public Color GradientTop { get; }

		public Color GradientBottom { get; }

		public GradientParameters(UILabel label)
		{
			ApplyGradient = label.applyGradient;
			GradientTop = label.gradientTop;
			GradientBottom = label.gradientBottom;
		}

		public void Apply(UILabel label)
		{
			label.applyGradient = ApplyGradient;
			label.gradientTop = GradientTop;
			label.gradientBottom = GradientBottom;
		}
	}

	[SerializeField]
	private UILabel _targetLabel;

	[SerializeField]
	private bool _applyGradient;

	[SerializeField]
	private string _gradientTopColorId;

	[SerializeField]
	private string _gradientBottomColorId;

	private GradientParameters _originalParameters;

	public eColorCodeId GradientTopColorId
	{
		set
		{
			_gradientTopColorId = value.ToString();
			RequestOverwrite();
		}
	}

	public eColorCodeId GradientBottomColorId
	{
		set
		{
			_gradientBottomColorId = value.ToString();
			RequestOverwrite();
		}
	}

	protected override GameObject TargetObject => _targetLabel.gameObject;

	protected override void SaveOriginalParameters()
	{
		_originalParameters = new GradientParameters(_targetLabel);
	}

	protected override void UndoParameters()
	{
		_originalParameters.Apply(_targetLabel);
	}

	protected override void OverwriteParameters()
	{
		OverwriteApplyGradient();
		OverwriteGradientTop();
		OverwriteGradientBottom();
	}

	private void OverwriteApplyGradient()
	{
		_targetLabel.applyGradient = _applyGradient;
	}

	private void OverwriteGradientTop()
	{
		if (ColorTryParse(_gradientTopColorId, out var color))
		{
			_targetLabel.gradientTop = color;
		}
	}

	private void OverwriteGradientBottom()
	{
		if (ColorTryParse(_gradientBottomColorId, out var color))
		{
			_targetLabel.gradientBottom = color;
		}
	}
}

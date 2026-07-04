using UnityEngine;

namespace Wizard;

public class PuzzleHintDialog : MonoBehaviour
{
	[SerializeField]
	private UILabel _winConditionLabel;

	[SerializeField]
	private UILabel _hintLabel;

	public void Setup(string winConditionText, string hintText)
	{
		_winConditionLabel.text = winConditionText;
		_hintLabel.text = hintText;
	}
}

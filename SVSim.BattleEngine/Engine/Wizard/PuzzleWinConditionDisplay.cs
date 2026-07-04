using System;
using UnityEngine;

namespace Wizard;

public class PuzzleWinConditionDisplay : MonoBehaviour
{
	[SerializeField]
	private UILabel _winConditionLabel;

	[SerializeField]
	private UIButton _button;

	[SerializeField]
	private UIPanel _panel;

	private bool _isAppeared;

	private Action _onComplete;

	public void Setup(string winConditionText, Action onComplete)
	{
		_winConditionLabel.text = winConditionText;
		_panel.alpha = 0f;
		_button.onClick.Add(new EventDelegate(OnClick));
		_onComplete = onComplete;
		TweenAlpha.Begin(base.gameObject, 0.5f, 1f).onFinished.Add(new EventDelegate(delegate
		{
			_isAppeared = true;
		}));
	}

	private void OnClick()
	{
		if (_isAppeared)
		{

			TweenAlpha.Begin(base.gameObject, 0.1f, 0f).onFinished.Add(new EventDelegate(delegate
			{
				_onComplete();
			}));
		}
	}
}

using System;
using UnityEngine;

namespace Wizard;

public abstract class QuestSelectionButtonBase : MonoBehaviour
{
	public enum ButtonType
	{
		None = 99
	}

	[SerializeField]
	protected TweenAlpha _selectSpriteTween;

	protected QuestSelectionPage _questSelectionPage;

	public abstract void Initialize(QuestSelectionButtonData data, bool isOpenExtra, bool isLastDay, Action onClick = null);

	public void SetActiveSelectSprite(bool isActive)
	{
		if (isActive != _selectSpriteTween.gameObject.activeSelf)
		{
			_selectSpriteTween.gameObject.SetActive(isActive);
			if (isActive)
			{
				_selectSpriteTween.PlayPingPong(isIncreaseAlpha: false);
			}
		}
	}

	protected abstract void SetTexture(string textureId);

	protected abstract void SetButtonRootToGrey(bool isGrey);

	public abstract void SelectChara();

	public abstract void OnDecideButtonClick();

	public void SetQuestSelectionPage(QuestSelectionPage questSelectionPage)
	{
		_questSelectionPage = questSelectionPage;
	}
}

using System;
using Cute;
using UnityEngine;

namespace Wizard;

public class PuzzleQuestSelectItem : MonoBehaviour
{
	[SerializeField]
	private UITexture _charaTexture;

	[SerializeField]
	private UILabel _clearLabel;

	[SerializeField]
	private UILabel _deckNameLabel;

	[SerializeField]
	private UIButton _button;

	[SerializeField]
	private UILabel _newLabel;

	[SerializeField]
	private UILabel _unlockConditionLabel;

	private PuzzleQuestSelectDialog.DisplayData _displayData;

	private static readonly Vector3 PushAnimationScale = new Vector3(0.95f, 0.95f, 1f);

	public void Setup(PuzzleQuestSelectDialog.DisplayData displayData, Action<PuzzleQuestSelectDialog.DisplayData> onClick)
	{
		_displayData = displayData;
		_button.onClick.Add(new EventDelegate(delegate
		{
			onClick(_displayData);
		}));
		UIEventListener uIEventListener = UIEventListener.Get(_button.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject obj, bool state)
		{
			PlayPushAnimation(state);
		});
		_deckNameLabel.text = Data.SystemText.Get(displayData.Data.QuestNameTextId);
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(displayData.Data.PlayerSkin.ToString(), ResourcesManager.AssetLoadPathType.DeckListTexture, isfetch: true);
		_charaTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath);
		_clearLabel.gameObject.SetActive(displayData.IsCleared);
		_newLabel.gameObject.SetActive(displayData.IsDisplayNew);
		SetUiAccordingLockState(displayData);
	}

	private void PlayPushAnimation(bool isPress)
	{
		Vector3 scale = (isPress ? PushAnimationScale : Vector3.one);
		TweenScale.Begin(base.gameObject, 0.03f, scale);
	}

	private void SetUiAccordingLockState(PuzzleQuestSelectDialog.DisplayData displayData)
	{
		if (displayData.IsUnlocked)
		{
			SetUnlockStateUI();
		}
		else
		{
			SetLockStateUI(displayData.UnlockConditionText);
		}
	}

	private void SetUnlockStateUI()
	{
		_unlockConditionLabel.gameObject.SetActive(value: false);
	}

	private void SetLockStateUI(string unlockConditionText)
	{
		UIManager.SetObjectToGrey(_button.gameObject, b: true);
		_unlockConditionLabel.gameObject.SetActive(value: true);
		_unlockConditionLabel.text = unlockConditionText;
	}
}

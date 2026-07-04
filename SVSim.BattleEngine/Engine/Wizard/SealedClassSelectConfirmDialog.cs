using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class SealedClassSelectConfirmDialog : MonoBehaviour
{
	[SerializeField]
	private ClassInfoParts _classInfoParts;

	[SerializeField]
	private UILabel _cardListLabel;

	[SerializeField]
	private UIToggle _cardAutoOpenToggleButton;

	private bool isFirstToggleChange = true;

	public void Init(int classId, List<int> cardList)
	{
		_classInfoParts.InitByCharaPrm(null); // Pre-Phase-5b: no chara master headless
		_cardListLabel.text = string.Empty;
		for (int i = 0; i < cardList.Count; i++)
		{
			if (i != 0)
			{
				_cardListLabel.text += "\n";
			}
			_cardListLabel.text += CardMaster.GetInstance(FormatBehaviorManager.GetDefaultBehaviour(Format.Sealed).CardMasterId).GetCardParameterFromId(cardList[i]).CardName;
		}
		_cardAutoOpenToggleButton.value = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.CARDPACK_CARD_AUTO_OPEN);
		EventDelegate.Add(_cardAutoOpenToggleButton.onChange, OnChangeCardAutoOpenToggleButton);
	}

	private void OnChangeCardAutoOpenToggleButton()
	{
		if (isFirstToggleChange)
		{
			isFirstToggleChange = false;
			return;
		}
		bool value = _cardAutoOpenToggleButton.value;

		PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.CARDPACK_CARD_AUTO_OPEN, value);
	}
}

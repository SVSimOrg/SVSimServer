using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class DeckBuildShortageCardView : MonoBehaviour
{
	[SerializeField]
	private CardSelectListConfirmPagerView _cardListView;

	[SerializeField]
	private GameObject _parentCraftUI;

	[SerializeField]
	private GameObject _parentNotCraftUI;

	[SerializeField]
	private UILabel _labelExplain_1;

	[SerializeField]
	private UILabel _labelExplain_2;

	[SerializeField]
	private GameObject _iconExplainRedether;

	[SerializeField]
	private UILabel _labelHaveRedetherNum;

	[SerializeField]
	private UILabel _labelBeforeRedetherNum;

	[SerializeField]
	private UILabel _labelAfterRedetherNum;

	private Dictionary<int, int> _dictCreateCardIdNum = new Dictionary<int, int>();

	private Action _onCraftCardAllCallback;

	protected DialogBase _dialog;

	private CardDetailUI _cardDetailUI;

	private CardMaster.CardMasterId _cardMasterId;

	public void Init(List<int> cardIdList, CardDetailUI cardDetailUI, DialogBase dialog, Action callback, Action onCraftCardALL, CardMaster.CardMasterId cardMasterId, Action onClose)
	{
		_cardMasterId = cardMasterId;
		_cardDetailUI = cardDetailUI;
		_dialog = dialog;
		_onCraftCardAllCallback = onCraftCardALL;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < cardIdList.Count; i++)
		{
			int key = cardIdList[i];
			if (dictionary.ContainsKey(key))
			{
				dictionary[key]++;
			}
			else
			{
				dictionary.Add(key, 1);
			}
		}
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			bool flag = false; // Pre-Phase-5b: no maintenance list headless
			if (!CardMaster.GetInstance(_cardMasterId).GetCardParameterFromId(item.Key).IsNotCraftDestruct && !flag)
			{
				_dictCreateCardIdNum.Add(item.Key, item.Value);
			}
		}
		_cardListView.Init(dictionary, cardDetailUI, delegate
		{
			UpdateExplainText();
			callback.Call();
		}, _cardMasterId);
		_dialog.OnClose = delegate
		{
			_cardListView.OnCloseView();
			onClose.Call();
		};
	}

	public void RemoveCardNum(int cardId, int num)
	{
		if (_dictCreateCardIdNum.ContainsKey(cardId))
		{
			int num2 = _dictCreateCardIdNum[cardId];
			if (num2 >= num)
			{
				_dictCreateCardIdNum[cardId] = num2 - num;
			}
		}
		_cardListView.DecrementCardNum(cardId, num);
		if (IsExistCreateCardId())
		{
			return;
		}
		if (_cardDetailUI.GetIsDetailOn())
		{
			_cardDetailUI.OnClose = delegate
			{
				_dialog.Close();
				_cardDetailUI.OnClose = null;
			};
		}
		else
		{
			_dialog.Close();
		}
	}

	public void AddCardNum(int cardId, int num)
	{
		if (_dictCreateCardIdNum.ContainsKey(cardId))
		{
			int num2 = _dictCreateCardIdNum[cardId];
			_dictCreateCardIdNum[cardId] = num2 + num;
		}
		_cardListView.IncrementCardNum(cardId, num);
		if (_cardDetailUI.OnClose != null)
		{
			_cardDetailUI.OnClose = null;
		}
	}

	public void UpdateExplainText()
	{
		int num = CalculateTotalUseRedetherNum();
		int userRedEtherCount = PlayerStaticData.UserRedEtherCount;
		if (!IsExistCreateCardId())
		{
			_parentCraftUI.SetActive(value: false);
			_parentNotCraftUI.SetActive(value: true);
			_labelExplain_1.text = Data.SystemText.Get("Card_0159");
			_labelExplain_2.text = Data.SystemText.Get("Card_0158");
			_iconExplainRedether.SetActive(value: false);
			_labelHaveRedetherNum.text = userRedEtherCount.ToString();
			_dialog.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			_dialog.onPushButton1 = null;
		}
		else if (num <= userRedEtherCount)
		{
			_parentCraftUI.SetActive(value: true);
			_parentNotCraftUI.SetActive(value: false);
			_labelExplain_1.text = Data.SystemText.Get("Card_0154", num.ToString());
			_labelExplain_2.text = Data.SystemText.Get("Card_0155");
			_iconExplainRedether.SetActive(value: true);
			_labelBeforeRedetherNum.text = userRedEtherCount.ToString();
			_labelAfterRedetherNum.text = Data.SystemText.Get("Shop_0045", (userRedEtherCount - num).ToString());
			_dialog.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
			_dialog.SetButtonText(Data.SystemText.Get("Dia_DeckEdit_014_Button"));
			_dialog.SetButtonDelegate(StartCreateShortageCard);
		}
		else
		{
			_parentCraftUI.SetActive(value: false);
			_parentNotCraftUI.SetActive(value: true);
			int num2 = num - userRedEtherCount;
			_labelExplain_1.text = Data.SystemText.Get("Card_0157", num2.ToString());
			_labelExplain_2.text = Data.SystemText.Get("Card_0158");
			_iconExplainRedether.SetActive(value: true);
			_labelHaveRedetherNum.text = userRedEtherCount.ToString();
			_dialog.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			_dialog.onPushButton1 = null;
		}
	}

	private bool IsExistCreateCardId()
	{
		foreach (KeyValuePair<int, int> item in _dictCreateCardIdNum)
		{
			if (item.Value > 0)
			{
				return true;
			}
		}
		return false;
	}

	private int CalculateTotalUseRedetherNum()
	{
		int num = 0;
		foreach (KeyValuePair<int, int> item in _dictCreateCardIdNum)
		{
			int key = item.Key;
			int value = item.Value;
			int useRedEther = CardMaster.GetInstance(_cardMasterId).GetCardParameterFromId(key).UseRedEther;
			num += useRedEther * value;
		}
		return num;
	}

	private void StartCreateShortageCard()
	{
		if (!IsExistCreateCardId())
		{
			_onCraftCardAllCallback.Call();
			return;
		}
		UIManager.GetInstance().createInSceneCenterLoading();
		CardMake component = base.gameObject.GetComponent<CardMake>();
		component.OnCardBuy = delegate
		{
			_onCraftCardAllCallback.Call();
			UIManager.GetInstance().closeInSceneCenterLoading();
		};
		component.StartCraftAll(_dictCreateCardIdNum);
	}
}

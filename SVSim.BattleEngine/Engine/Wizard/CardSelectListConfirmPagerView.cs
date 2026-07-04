using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class CardSelectListConfirmPagerView : MonoBehaviour
{

	[SerializeField]
	private UIButton _buttonNext;

	[SerializeField]
	private UIButton _buttonPrev;

	[SerializeField]
	private UIGrid _gridCardList;

	[SerializeField]
	protected GameObject _cardObjPool;

	[SerializeField]
	private UIToggle _indicatorBase;

	[SerializeField]
	private GameObject _cardInfoTextOriginal;

	private List<string> _loadedAssetPathList = new List<string>();

	protected List<UIBase_CardManager.CardObjData> _cardObjList = new List<UIBase_CardManager.CardObjData>();

	private List<UIBase_CardManager.CardObjData> _removedCardObjList = new List<UIBase_CardManager.CardObjData>();

	private Dictionary<int, List<UIBase_CardManager.CardObjData>> _dictPageCardObj = new Dictionary<int, List<UIBase_CardManager.CardObjData>>();

	private Dictionary<int, int> _dictCardIdNum = new Dictionary<int, int>();

	protected int _currentPage;

	private List<UIToggle> _indicatorList = new List<UIToggle>();

	protected CardDetailUI _cardDetailUI;

	private CardMaster.CardMasterId _cardMasterId;

	protected bool _isInit;

	private bool _isDrag;

	public Dictionary<int, int> DictCardIdNum
	{
		get
		{
			return _dictCardIdNum;
		}
		private set
		{
			_dictCardIdNum = value;
		}
	}

	private void OnDrag(GameObject obj, Vector2 dir)
	{
		if (!_isDrag && Mathf.Abs(dir.x) >= 70f)
		{
			_isDrag = true;
			if (dir.x < 0f)
			{
				OnNextButton();
			}
			else
			{
				OnPrevButton();
			}
		}
	}

	private void OnDragEnd(GameObject obj)
	{
		_isDrag = false;
	}

	public void Init(Dictionary<int, int> cardIdDict, CardDetailUI cardDetailUI, Action callback, CardMaster.CardMasterId cardMasterId)
	{
		_cardMasterId = cardMasterId;
		DictCardIdNum = cardIdDict;
		List<int> cardIdList = new List<int>(DictCardIdNum.Keys);
		_cardDetailUI = cardDetailUI;
		_cardInfoTextOriginal.SetActive(value: false);
		LoadCardList(cardIdList, delegate
		{
			_currentPage = 1;
			UpdateCardListView();
			_isInit = true;
			callback.Call();
		});
	}

	public void UpdateCardListView()
	{
		DividCardObjListEachPages(_cardObjList);
		CreateIndicator();
		ViewCardList(_currentPage);
	}

	public void DecrementCardNum(int cardId, int num)
	{
		if (!DictCardIdNum.ContainsKey(cardId))
		{
			return;
		}
		int num2 = DictCardIdNum[cardId];
		if (num2 >= num)
		{
			UIBase_CardManager.CardObjData cardObjData = _cardObjList.Find((UIBase_CardManager.CardObjData data) => data.ids == cardId);
			DictCardIdNum[cardId] = num2 - num;
			if (DictCardIdNum[cardId] <= 0)
			{
				DictCardIdNum.Remove(cardId);
				_cardObjList.Remove(cardObjData);
				_removedCardObjList.Add(cardObjData);
				cardObjData.CardObj.transform.parent = _cardObjPool.transform;
				cardObjData.CardObj.transform.localPosition = Vector3.zero;
			}
			else
			{
				cardObjData.CardObj.GetComponent<CardListTemplate>().SetNum(DictCardIdNum[cardId]);
			}
		}
		UpdateCardListView();
	}

	public void IncrementCardNum(int cardId, int num)
	{
		if (DictCardIdNum.ContainsKey(cardId))
		{
			int num2 = DictCardIdNum[cardId];
			DictCardIdNum[cardId] = num2 + num;
			_cardObjList.Find((UIBase_CardManager.CardObjData data) => data.ids == cardId).CardObj.GetComponent<CardListTemplate>().SetNum(DictCardIdNum[cardId]);
		}
		else
		{
			UIBase_CardManager.CardObjData cardObjData = _removedCardObjList.Find((UIBase_CardManager.CardObjData data) => data.ids == cardId);
			DictCardIdNum.Add(cardId, num);
			_cardObjList.Add(cardObjData);
			_removedCardObjList.Remove(cardObjData);
			cardObjData.CardObj.GetComponent<CardListTemplate>().SetNum(DictCardIdNum[cardId]);
			cardObjData.CardObj.transform.parent = _cardObjPool.transform;
			cardObjData.CardObj.transform.localPosition = Vector3.zero;
		}
		UpdateCardListView();
	}

	public void OnCloseView()
	{
		if (_loadedAssetPathList != null && _loadedAssetPathList.Count > 0)
		{
			Toolbox.ResourcesManager.RemoveAssetGroup(_loadedAssetPathList);
			_loadedAssetPathList.Clear();
		}
	}

	private void ViewCardList(int page)
	{
		if (page > _dictPageCardObj.Count)
		{
			page = 1;
		}
		_currentPage = page;
		if (page > 1)
		{
			_buttonPrev.gameObject.SetActive(value: true);
		}
		else
		{
			_buttonPrev.gameObject.SetActive(value: false);
		}
		if (page < _dictPageCardObj.Count)
		{
			_buttonNext.gameObject.SetActive(value: true);
		}
		else
		{
			_buttonNext.gameObject.SetActive(value: false);
		}
		if (_indicatorList.Count > 1)
		{
			_indicatorList[page - 1].value = true;
		}
		RemoveCardObjFromGrid(_gridCardList);
		AddCardObjToGrid(_dictPageCardObj[page]);
	}

	private void CreateIndicator()
	{
		GameObject gameObject = _indicatorBase.transform.parent.gameObject;
		if (_dictPageCardObj.Count <= 1)
		{
			gameObject.SetActive(value: false);
			return;
		}
		gameObject.SetActive(value: true);
		if (_indicatorList.Count > 0)
		{
			for (int i = 0; i < _indicatorList.Count; i++)
			{
				if (i >= _dictPageCardObj.Count)
				{
					_indicatorList[i].gameObject.SetActive(value: false);
					continue;
				}
				_indicatorList[i].gameObject.SetActive(value: true);
				_indicatorList[i].Set(state: false);
			}
		}
		else
		{
			_indicatorList.Add(_indicatorBase);
			for (int j = 1; j < _dictPageCardObj.Count; j++)
			{
				_indicatorList.Add(NGUITools.AddChild(gameObject, _indicatorBase.gameObject).GetComponent<UIToggle>());
			}
		}
		gameObject.GetComponent<UIGrid>().Reposition();
	}

	private void LoadCardList(List<int> cardIdList, Action callback = null)
	{
		Toolbox.ResourcesManager.CardListAssetPathList.Clear();
		UIManager.GetInstance().CardLoadSelect(base.gameObject, cardIdList, 24, is2D: true, delegate
		{
			_cardObjList = UIManager.GetInstance().getCardList2DObjs();
			_loadedAssetPathList.AddRange(Toolbox.ResourcesManager.CardListAssetPathList);
			string text = Data.SystemText.Get("System_0022");
			for (int i = 0; i < _cardObjList.Count; i++)
			{
				GameObject cardObj = _cardObjList[i].CardObj;
				int ids = _cardObjList[i].ids;
				CardListTemplate component = cardObj.GetComponent<CardListTemplate>();
				component.SetNum(DictCardIdNum[ids]);
				cardObj.AddComponent<BoxCollider>().size = component._cardTexture.localSize;
				UIEventListener uIEventListener = UIEventListener.Get(cardObj);
				uIEventListener.onClick = _cardDetailUI.OnPushCardDetailOn;
				uIEventListener.onDrag = OnDrag;
				uIEventListener.onDragEnd = OnDragEnd;
				if (false /* Pre-Phase-5b: no maintenance list */)
				{
					SetBlackOutCardInfoText(cardObj, text);
				}
				else if (IsNotCraftDestructCard(ids))
				{
					string text2 = string.Empty;
					CardParameter cardParameterFromId = CardMaster.GetInstance(_cardMasterId).GetCardParameterFromId(ids);
					if (cardParameterFromId.IsBasicCard)
					{
						text2 = Data.SystemText.Get("Card_0160");
					}
					if (cardParameterFromId.IsPreReleaseCard)
					{
						text2 = Data.SystemText.Get("Card_0245");
					}
					SetBlackOutCardInfoText(cardObj, text2);
				}
				cardObj.transform.parent = _cardObjPool.transform;
				cardObj.transform.localPosition = Vector3.zero;
				cardObj.SetActive(value: false);
				cardObj.SetActive(value: true);
			}
			callback.Call();
		}, isDefaultSleeve: true);
	}

	private void SetBlackOutCardInfoText(GameObject cardObj, string text)
	{
		GameObject obj = UnityEngine.Object.Instantiate(_cardInfoTextOriginal);
		obj.gameObject.SetActive(value: true);
		obj.transform.parent = cardObj.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		obj.GetComponent<UILabel>().text = text;
	}

	public void OnNextButton()
	{
		if (_isInit && !_cardDetailUI.GetIsDetailOn() && _currentPage < _dictPageCardObj.Count)
		{
			ViewCardList(++_currentPage);

		}
	}

	public void OnPrevButton()
	{
		if (_isInit && !_cardDetailUI.GetIsDetailOn() && _currentPage > 1)
		{
			ViewCardList(--_currentPage);

		}
	}

	private void DividCardObjListEachPages(List<UIBase_CardManager.CardObjData> cardObjList)
	{
		_dictPageCardObj.Clear();
		List<UIBase_CardManager.CardObjData> list = new List<UIBase_CardManager.CardObjData>();
		int num = 0;
		int num2 = 1;
		List<int> list2 = cardObjList.ConvertAll((UIBase_CardManager.CardObjData conv) => conv.ids);
		SortCardIdList(list2);
		cardObjList = list2.ConvertAll((int conv) => cardObjList.Find((UIBase_CardManager.CardObjData find) => find.ids == conv));
		for (int num3 = 0; num3 < cardObjList.Count; num3++)
		{
			if (num >= 8)
			{
				num = 0;
				_dictPageCardObj.Add(num2, list);
				list = new List<UIBase_CardManager.CardObjData>();
				num2++;
			}
			list.Add(cardObjList[num3]);
			num++;
		}
		_dictPageCardObj.Add(num2, list);
	}

	private List<int> SortCardIdList(List<int> idList)
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		for (int i = 0; i < idList.Count; i++)
		{
			int num = idList[i];
			if (dataMgr.IsMaintenanceCard(num))
			{
				list.Add(num);
			}
			else if (IsNotCraftDestructCard(num))
			{
				list2.Add(num);
			}
			else
			{
				list3.Add(num);
			}
		}
		list = UIManager.GetInstance().getUIBase_CardManager().SortIDList(list, _cardMasterId);
		list2 = UIManager.GetInstance().getUIBase_CardManager().SortIDList(list2, _cardMasterId);
		list3 = UIManager.GetInstance().getUIBase_CardManager().SortIDList(list3, _cardMasterId);
		idList.Clear();
		idList.AddRange(list);
		idList.AddRange(list2);
		idList.AddRange(list3);
		return idList;
	}

	private bool IsNotCraftDestructCard(int cardId)
	{
		return CardMaster.GetInstance(_cardMasterId).GetCardParameterFromId(cardId).IsNotCraftDestruct;
	}

	private void RemoveCardObjFromGrid(UIGrid gridObj)
	{
		List<Transform> childList = gridObj.GetChildList();
		for (int i = 0; i < childList.Count; i++)
		{
			childList[i].transform.parent = _cardObjPool.transform;
			childList[i].transform.localPosition = Vector3.zero;
		}
	}

	private void AddCardObjToGrid(List<UIBase_CardManager.CardObjData> cardObjList)
	{
		for (int i = 0; i < cardObjList.Count; i++)
		{
			GameObject cardObj = cardObjList[i].CardObj;
			_gridCardList.AddChild(cardObj.transform);
			cardObj.transform.localScale = Vector3.one * 0.55f;
			cardObj.SetActive(value: true);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public class CardBundleControllerBase
{
	protected UIBase_CardManager.FilterParameter _filter;

	private bool _isCraftMode;

	private readonly Transform _parentSelectionObj;

	private readonly Transform _parentPagingObj;

	private readonly UITexture _sleeveOriginal;

	protected GameObject _cardInfoOriginal;

	private List<int> _filteredAllCardIdListCache;

	protected UIBase_CardManager.FilterParameter _lastExecutedFilterParam;

	private readonly CardCreator _cardCreator;

	private readonly List<int> _listNewCardDisplayedIds = new List<int>();

	private readonly bool _isSelectableSpotCard;

	public readonly bool CanUseNonPossessionCard;

	private int CARD_PER_PAGE => 8;

	private float CARD_WIDTH => 120f;

	private float SELECTION_AREA_CARD_SCALE => 0.5f;

	private float PAGE_CARD_SCALE => 0.6f;

	public bool IsReady { get; protected set; }

	public int CurrentPage { get; set; }

	public int MaxPage { get; set; }

	public FilteringCardBundle SelectionAreaList { get; private set; }

	public CachingCardBundle PagingList { get; private set; }

	public UIBase_CardManager.FilterParameter FilterParameter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
		}
	}

	public bool IsCraftMode
	{
		get
		{
			return _isCraftMode;
		}
		protected set
		{
			_isCraftMode = value;
			_filter.Own = ((!value) ? 1 : 0);
		}
	}

	protected IFormatBehavior FormatBehavior { get; private set; }

	public MyRotationInfo MyRotationInfo { get; private set; }

	public FilterController.MyRotationFilterType MyRotationFilterType { get; private set; }

	public event Action OnCreatePagingSleeve;

	public event Action OnCreatePagingCard;

	public event Action<GameObject> OnClickSelectionAreaCard;

	public event Action<GameObject> OnClickPagingCard;

	public event Action<GameObject, Vector2> OnDragPagingCard;

	public event Action<GameObject> OnDragOverPagingCard;

	public event Action<GameObject, float> OnScrollPagingCard;

	public CardBundleControllerBase(Transform parentSelection, Transform parentPage, UITexture sleeveOriginal, GameObject cardInfoOriginal, IFormatBehavior formatBehavior, bool isIncludingSpotCard, bool isSelectableSpotCard, bool isHideZeroSpotCardNum, bool canUseNonPossessionCard)
	{
		IsReady = false;
		_parentSelectionObj = parentSelection;
		_parentPagingObj = parentPage;
		_sleeveOriginal = sleeveOriginal;
		_cardInfoOriginal = cardInfoOriginal;
		_cardCreator = new CardCreator();
		FormatBehavior = formatBehavior;
		SelectionAreaList = new FilteringCardBundle(_cardCreator, _parentSelectionObj, _sleeveOriginal, SELECTION_AREA_CARD_SCALE, formatBehavior, isIncludingSpotCard, isHideZeroSpotCardNum, canUseNonPossessionCard);
		SelectionAreaList.OnCreateCard += OnCreateSelectionEachCard;
		PagingList = new CachingCardBundle(_cardCreator, _parentPagingObj, _sleeveOriginal, PAGE_CARD_SCALE, formatBehavior, isIncludingSpotCard);
		PagingList.OnCreateCard += OnCreatePagingEachCard;
		PagingList.OnCreateSleeve += OnCreatePagingEachSleeve;
		_isSelectableSpotCard = isSelectableSpotCard;
		CanUseNonPossessionCard = canUseNonPossessionCard;
	}

	public void Tick()
	{
		_cardCreator.Tick();
	}

	public void Destroy()
	{
		_cardCreator.Clear();
		SelectionAreaList.DestroyAll();
		PagingList.DestroyAll();
	}

	public void DisableNewFlagDisplayedCards()
	{
		for (int i = 0; i < _listNewCardDisplayedIds.Count(); i++)
		{
			/* Pre-Phase-5b: SetIsNewCard write dropped */
		}
	}

	public bool LoadPagingCard(int page, bool isDestroyImmediate)
	{
		List<int> filteringIDList = GetFilteringIDList(FormatBehavior);
		MaxPage = (filteringIDList.Count - 1) / CARD_PER_PAGE;
		if (page < 0 || page > MaxPage)
		{
			this.OnCreatePagingSleeve.Call();
			this.OnCreatePagingCard.Call();
			return false;
		}
		CurrentPage = page;
		List<int> idList = filteringIDList.Skip(page * CARD_PER_PAGE).Take(CARD_PER_PAGE).ToList();
		return PagingList.CreateCards(idList, isDestroyImmediate, IsReady, this.OnCreatePagingSleeve, delegate
		{
			this.OnCreatePagingCard.Call();
		});
	}

	public bool FetchPagingCard()
	{
		return LoadPagingCard(FindCurrentPage(), isDestroyImmediate: false);
	}

	public int FindCurrentPage()
	{
		if (PagingList == null || PagingList.CountKind == 0)
		{
			return 0;
		}
		List<int> filteringIDList = GetFilteringIDList(FormatBehavior);
		int count = filteringIDList.Count;
		MaxPage = (filteringIDList.Count - 1) / CARD_PER_PAGE;
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		List<int> list = new List<int>();
		num = 0;
		while (true)
		{
			list.Clear();
			for (int i = 0; i < CARD_PER_PAGE; i++)
			{
				int num2 = num * CARD_PER_PAGE + i;
				if (num2 >= count)
				{
					flag2 = true;
					break;
				}
				list.Add(filteringIDList[num2]);
				for (int j = 0; j < PagingList.CountKind; j++)
				{
					flag = flag || PagingList.FindWithIndex(j).CardId == filteringIDList[num2];
				}
			}
			if (flag || flag2)
			{
				break;
			}
			num++;
		}
		if (flag)
		{
			return num;
		}
		return 0;
	}

	protected virtual void OnCreateSelectionEachCard(CardObject card)
	{
		UIEventListener.Get(card.CardObj).onClick = this.OnClickSelectionAreaCard.Invoke;
		card.CardObj.AddComponent<UIDragScrollView>();
		card.ResetMaterial();
		if (false /* Pre-Phase-5b: no user card state headless */)
		{
			_listNewCardDisplayedIds.Add(card.CardId);
		}
		if (card.IsNonPossessionCard)
		{
			card.AttachGrayShader();
		}
	}

	private void OnCreatePagingEachCard(CardObject card)
	{
		UIEventListener.Get(card.CardObj).onClick = this.OnClickPagingCard.Invoke;
		UIEventListener.Get(card.CardObj).onDrag = this.OnDragPagingCard.Invoke;
		UIEventListener.Get(card.CardObj).onDragOver = this.OnDragOverPagingCard.Invoke;
		UIEventListener.Get(card.CardObj).onScroll = this.OnScrollPagingCard.Invoke;
		card.ResetMaterial();
		UpdateCardInfo(card);
		if (false /* Pre-Phase-5b: no user card state headless */)
		{
			_listNewCardDisplayedIds.Add(card.CardId);
		}
	}

	private void OnCreatePagingEachSleeve(CardObject sleeve)
	{
		sleeve.CardObj.transform.localPosition = Vector3.right * ((float)PagingList.IndexOf(sleeve) * CARD_WIDTH);
	}

	public virtual int InsertToSelectionArea(CardObject card)
	{
		bool isVisibleCursorEffect = card.IsVisibleCursorEffect;
		card.ActiveCursorEffect(isActive: false);
		CardObject cardObject = SelectionAreaList.Insert(card, dontCreate: false);
		cardObject.AttachParent(_parentSelectionObj);
		PagingList.Remove(card, isDestroyObject: false);
		UpdatePagingCardInfoAll();
		card.ActiveCursorEffect(isVisibleCursorEffect);
		return SelectionAreaList.IndexOf(cardObject);
	}

	public virtual int RemoveFromSelectionArea(CardObject card)
	{
		if (card == null)
		{
			return -1;
		}
		bool isVisibleCursorEffect = card.IsVisibleCursorEffect;
		card.ActiveCursorEffect(isActive: false);
		SelectionAreaList.Remove(card, isDestroyObject: true);
		CardObject card2 = PagingList.Insert(card, dontCreate: true);
		UpdatePagingCardInfoAll();
		card.ActiveCursorEffect(isVisibleCursorEffect);
		return PagingList.IndexOf(card2);
	}

	public void InvalidateFilteredIdListCache()
	{
		_filteredAllCardIdListCache = null;
	}

	protected virtual List<int> GetFilteringIDList(IFormatBehavior formatBehavior)
	{
		if (_filteredAllCardIdListCache != null && _filter == _lastExecutedFilterParam)
		{
			return _filteredAllCardIdListCache;
		}
		if (MyRotationInfo != null)
		{
			FilterController.SetMyRotationFilterParam(_filter, MyRotationFilterType, MyRotationInfo);
		}
		_filteredAllCardIdListCache = UIManager.GetInstance().getUIBase_CardManager().SelectCardIDInConditionMask(formatBehavior.SortedDeckUsableCardList, _filter, formatBehavior, MyRotationInfo, alreadySorted: true, IsCraftMode)
			.ToList();
		_lastExecutedFilterParam = new UIBase_CardManager.FilterParameter(_filter);
		return _filteredAllCardIdListCache;
	}

	public int GetHaveNum(int cardId)
	{
		return FormatBehavior.GetPossessionCardNum(cardId, isIncludingSpotCard: true);
	}

	public int CountCardNumInSelectionArea(int cardId, bool isStrictSameCard)
	{
		if (SelectionAreaList == null)
		{
			return 0;
		}
		if (isStrictSameCard)
		{
			return SelectionAreaList.CardList.Where((CardObject c) => c.CardId == cardId).Sum((CardObject c) => c.TotalCardNum);
		}
		CardMaster cardMaster = CardMaster.GetInstance(FormatBehavior.CardMasterId);
		int baseCardId = cardMaster.GetCardParameterFromId(cardId).BaseCardId;
		return SelectionAreaList.CardList.Where((CardObject c) => cardMaster.GetCardParameterFromId(c.CardId).BaseCardId == baseCardId).Sum((CardObject c) => c.TotalCardNum);
	}

	protected virtual void UpdateCardInfo(CardObject card)
	{
	}

	protected void UpdatePagingCardInfoAll()
	{
		for (int i = 0; i < PagingList.CountKind; i++)
		{
			UpdateCardInfo(PagingList.FindWithIndex(i));
		}
	}

	public void OnCreateCard(int cardId)
	{
		if (PagingList.FindWithCardId(cardId) == null)
		{
			InvalidateFilteredIdListCache();
			FetchPagingCard();
		}
	}

	public void AccordCardInfo()
	{
		AccordCardInfoInSelectionArea();
		AccordCardInfoInPagingList();
	}

	protected virtual void AccordCardInfoInSelectionArea()
	{
		if (CanUseNonPossessionCard)
		{
			AccordSelectionAreaCardInfoWithNonPossessionCard();
			return;
		}
		for (int num = SelectionAreaList.CountKindNoFilter - 1; num >= 0; num--)
		{
			CardObject cardObject = SelectionAreaList.FindWithIndexNoFilter(num);
			int selectableCardNum = GetSelectableCardNum(cardObject.CardId);
			int num2 = CountCardNumInSelectionArea(cardObject.CardId, isStrictSameCard: true);
			if (selectableCardNum < num2)
			{
				int i = 0;
				for (int num3 = num2 - selectableCardNum; i < num3; i++)
				{
					RemoveFromSelectionArea(cardObject);
				}
			}
			if (selectableCardNum != 0)
			{
				UpdateMainAndSubNumInSelectionArea(cardObject);
			}
		}
	}

	private void AccordSelectionAreaCardInfoWithNonPossessionCard()
	{
		for (int num = SelectionAreaList.CountKindNoFilter - 1; num >= 0; num--)
		{
			CardObject selectionAreaCard = SelectionAreaList.FindWithIndexNoFilter(num);
			int num2 = CountCardNumInSelectionArea(selectionAreaCard.CardId, isStrictSameCard: true);
			if (num2 <= 0)
			{
				RemoveFromSelectionAreaWithCount(selectionAreaCard, selectionAreaCard.TotalCardNum);
			}
			else
			{
				int possessionCardNum = FormatBehavior.GetPossessionCardNum(selectionAreaCard.CardId, _isSelectableSpotCard);
				bool flag = false;
				bool flag2 = false;
				bool flag3 = DeckCardEditUI.IsSelectableNonPossessionCard(CardMaster.GetInstance(FormatBehavior.CardMasterId).GetCardParameterFromId(selectionAreaCard.CardId));
				if (selectionAreaCard.IsNonPossessionCard)
				{
					if (flag3)
					{
						CardObject cardObject = SelectionAreaList.CardList.Find((CardObject c) => c.CardId == selectionAreaCard.CardId && !c.IsNonPossessionCard);
						if (num2 <= possessionCardNum)
						{
							if (cardObject != null)
							{
								flag = true;
								cardObject.MainCardNum += selectionAreaCard.TotalCardNum;
							}
							else
							{
								selectionAreaCard.IsNonPossessionCard = false;
								selectionAreaCard.AttachColorShader();
							}
						}
						else if (possessionCardNum > 0 && cardObject == null)
						{
							selectionAreaCard.IsNonPossessionCard = false;
							selectionAreaCard.AttachColorShader();
							flag2 = true;
						}
					}
					else
					{
						flag = true;
					}
				}
				else if (flag3)
				{
					bool flag4 = SelectionAreaList.CardList.Any((CardObject c) => c.CardId == selectionAreaCard.CardId && c.IsNonPossessionCard);
					if (possessionCardNum == 0)
					{
						if (flag4)
						{
							flag = true;
						}
						else
						{
							selectionAreaCard.IsNonPossessionCard = true;
							selectionAreaCard.AttachGrayShader();
						}
					}
					else if (num2 > possessionCardNum && !flag4)
					{
						flag2 = true;
					}
				}
				else if (num2 > possessionCardNum)
				{
					RemoveFromSelectionAreaWithCount(selectionAreaCard, num2 - possessionCardNum);
				}
				if (flag)
				{
					RemoveFromSelectionAreaWithCount(selectionAreaCard, selectionAreaCard.TotalCardNum);
				}
				else
				{
					UpdateMainAndSubNumInSelectionArea(selectionAreaCard);
					if (flag2)
					{
						CardObject cardObject2 = SelectionAreaList.Insert(selectionAreaCard, dontCreate: false);
						cardObject2.AttachParent(_parentSelectionObj);
						SelectionAreaList.CountMainAndSubNum(selectionAreaCard.CardId, !selectionAreaCard.IsNonPossessionCard, out var mainNum, out var subNum);
						cardObject2.MainCardNum = mainNum;
						cardObject2.SubCardNum = subNum;
					}
				}
			}
		}
	}

	private void RemoveFromSelectionAreaWithCount(CardObject card, int count)
	{
		for (int i = 0; i < count; i++)
		{
			RemoveFromSelectionArea(card);
		}
	}

	protected virtual void AccordCardInfoInPagingList()
	{
		bool flag = false;
		for (int i = 0; i < PagingList.CountKind; i++)
		{
			CardObject cardObject = PagingList.FindWithIndex(i);
			int selectableCardNum = GetSelectableCardNum(cardObject.CardId);
			flag = flag || selectableCardNum == 0;
			if (FormatBehavior.GetPossessionCardNum(cardObject.CardId, isIncludingSpotCard: false) == 0)
			{
				flag = true;
			}
		}
		UpdatePagingCardInfoAll();
		if (flag)
		{
			InvalidateFilteredIdListCache();
			FetchPagingCard();
		}
	}

	protected virtual int GetSelectableCardNum(int cardId)
	{
		return FormatBehavior.GetPossessionCardNum(cardId, _isSelectableSpotCard);
	}

	private void UpdateMainAndSubNumInSelectionArea(CardObject card)
	{
		SelectionAreaList.CountMainAndSubNum(card.CardId, card.IsNonPossessionCard, out var mainNum, out var subNum);
		card.MainCardNum = mainNum;
		card.SubCardNum = subNum;
	}

	public void UpdateMyRotationInfo(MyRotationInfo info, FilterController.MyRotationFilterType filter)
	{
		MyRotationInfo = info;
		MyRotationFilterType = filter;
	}
}

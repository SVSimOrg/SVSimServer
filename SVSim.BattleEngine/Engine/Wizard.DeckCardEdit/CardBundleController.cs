using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public class CardBundleController : CardBundleControllerBase
{
	private DeckData _copySourceDeck;

	private int _rawSkinId;

	private bool _isRandomLeaderSkin;

	private List<int> _leaderSkinIdList;

	private long _sleeveId;

	private ConventionDeckList _conventionDeckList;

	private DeckGroupListData _deckListGroupData;

	private Format _format;

	private int DECK_CARD_NUM_MAX => base.FormatBehavior.DeckCardNumMax;

	private int DECK_CARD_NUM_EDIT_MAX => base.FormatBehavior.DeckSavableCardNumMax;

	public string DeckName { get; set; }

	public int DeckId { get; set; }

	public ClassSet ClassSet { get; set; }

	public bool IsNewDeck { get; set; }

	public event Action OnCreateDeckSleeve;

	public event Action OnCreateDeckCard;

	public event Action OnCreateAutoDeck;

	public event Action<int> OnInsertDeckCard;

	public event Action<int> OnRemoveDeckCard;

	public CardBundleController(Transform parentDeck, Transform parentPage, UITexture sleeveOriginal, GameObject cardInfoOriginal, IFormatBehavior formatBehavior, bool isIncludingSpotCard, bool isSelectableSpotCard, bool isHideZeroSpotCardNum, bool canUseNonPossessionCard)
		: base(parentDeck, parentPage, sleeveOriginal, cardInfoOriginal, formatBehavior, isIncludingSpotCard, isSelectableSpotCard, isHideZeroSpotCardNum, canUseNonPossessionCard)
	{
	}

	public void Setup(string name, DeckData deck, ClassSet classSet, int rawSkinId, bool isRandomLeaderSkin, List<int> leaderSkinIdList, long sleeveId, DeckData copySrc, Format format, ConventionDeckList conventionDeckList, DeckGroupListData deckGroupListData)
	{
		base.IsReady = false;
		DeckName = name;
		DeckId = deck.GetDeckID();
		_format = format;
		ClassSet = classSet;
		_rawSkinId = rawSkinId;
		_isRandomLeaderSkin = isRandomLeaderSkin;
		_leaderSkinIdList = leaderSkinIdList;
		_sleeveId = sleeveId;
		_conventionDeckList = conventionDeckList;
		_deckListGroupData = deckGroupListData;
		_copySourceDeck = copySrc;
		base.CurrentPage = 0;
		base.MaxPage = 0;
		base.IsCraftMode = false;
		_filter.Craftable = 1;
		if (_format == Format.Unlimited)
		{
			_filter.IsEnableResurgentCard = false;
		}
		IsNewDeck = deck.IsNoCard();
		List<int> order = GetFilteringIDList(base.FormatBehavior).Take(32).ToList();
		base.PagingList.Load(order, isPreferentially: false, delegate(List<UIBase_CardManager.CardObjData> created)
		{
			created.ForEach(delegate(UIBase_CardManager.CardObjData entry)
			{
				entry.CardObj.SetActive(value: false);
			});
			List<int> idList = null;
			if (_copySourceDeck != null)
			{
				idList = _copySourceDeck.GetCardIdList();
			}
			else if (!IsNewDeck)
			{
				idList = deck.GetCardIdList();
			}
			Action onSetupDeckCard = delegate
			{
				LoadPagingCard(0, isDestroyImmediate: true);
			};
			Action onSetupPagingCard = null;
			onSetupPagingCard = delegate
			{
				OnCreateDeckCard -= onSetupDeckCard;
				base.OnCreatePagingCard -= onSetupPagingCard;
				base.SelectionAreaList.ApplyFilter(new UIBase_CardManager.FilterParameter());
				base.IsReady = true;
			};
			OnCreateDeckCard += onSetupDeckCard;
			base.OnCreatePagingCard += onSetupPagingCard;
			LoadDeckCard(idList);
		});
	}

	public bool LoadDeckCard(List<int> idList, Action onFirstAnimationFinish = null, float cardRotateDelayTimeMax = float.MaxValue, bool isSkipSameDeckCheck = false)
	{
		if (idList == null)
		{
			this.OnCreateDeckSleeve.Call();
			this.OnCreateDeckCard.Call();
			OnCreateDeckCard -= this.OnCreateAutoDeck;
			return false;
		}
		idList = UIManager.GetInstance().getUIBase_CardManager().SortIDList(idList, base.FormatBehavior.CardMasterId);
		return base.SelectionAreaList.CreateCards(idList, isDestroyImmediate: true, base.IsReady, this.OnCreateDeckSleeve, delegate
		{
			UpdateDeckCardNumFromList(idList);
			DisplayAttentionMessageOnRestrictedCard(idList);
			UpdatePagingCardInfoAll();
			this.OnCreateDeckCard.Call();
			OnCreateDeckCard -= this.OnCreateAutoDeck;
		}, onFirstAnimationFinish, cardRotateDelayTimeMax, isSkipSameDeckCheck);
	}

	public override int InsertToSelectionArea(CardObject card)
	{
		if (card == null)
		{
			return -1;
		}
		if (base.SelectionAreaList.CountSum + 1 > DECK_CARD_NUM_EDIT_MAX)
		{
			SystemText systemText = Data.SystemText;
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_008_Title"));
			dialogBase.SetText(systemText.Get("Card_0048", DECK_CARD_NUM_EDIT_MAX.ToString()));
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			return -1;
		}
		CardObject cardObject = base.SelectionAreaList.FindWithCardId(card.CardId);
		CardParameter cardParameterFromId = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(card.CardId);
		int num = cardParameterFromId.GetSameKindNumMaxInFormat(classType: ClassUtil.GetClassType(cardParameterFromId, _format, ClassSet), inFormat: _format, behavior: base.FormatBehavior, myRotationInfo: base.MyRotationInfo);
		if (base.MyRotationFilterType == FilterController.MyRotationFilterType.CARD_POOL_ALL_PACK)
		{
			num = 3;
		}
		if (cardObject != null && cardObject.TotalCardNum + 1 > num)
		{
			SystemText systemText2 = Data.SystemText;
			DialogBase dialogBase2 = UIManager.GetInstance().CreateDialogClose();
			dialogBase2.SetTitleLabel(systemText2.Get("Dia_DeckEdit_009_Title"));
			dialogBase2.SetText(systemText2.Get("Card_0049", num.ToString()));
			dialogBase2.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			return -1;
		}
		int result = base.InsertToSelectionArea(card);
		this.OnInsertDeckCard.Call(card.CardId);
		return result;
	}

	public override int RemoveFromSelectionArea(CardObject card)
	{
		int result = base.RemoveFromSelectionArea(card);
		this.OnRemoveDeckCard(card.CardId);
		DisplayAttentionMessageOnRestrictedCard(base.SelectionAreaList.IdList);
		return result;
	}

	public void ChangeCraftMode(bool isCraft)
	{
		base.IsCraftMode = isCraft;
		FetchPagingCard();
	}

	public void CreateAutoDeck(bool forceClear, int tournamentId, MyRotationInfo myRotationInfo)
	{
		List<int> list = FindAllAvailableCardInFormat(base.SelectionAreaList.IdList, _format, myRotationInfo);
		if (list.Count >= DECK_CARD_NUM_MAX || forceClear)
		{
			list.Clear();
		}
		GetAutoCreateDeckCards(list, tournamentId, myRotationInfo, delegate(List<int> autoCreatedDeckCards)
		{
			List<int> idList = UIManager.GetInstance().getUIBase_CardManager().SortIDList(autoCreatedDeckCards, base.FormatBehavior.CardMasterId);
			UIManager.GetInstance().OpenNotTouch();
			OnCreateDeckCard += this.OnCreateAutoDeck;
			if (!LoadDeckCard(idList, OnCreateAutoAnimationFinish))
			{
				OnCreateDeckCard -= this.OnCreateAutoDeck;
			}
		});
	}

	private void GetAutoCreateDeckCards(List<int> deck, int tournamentId, MyRotationInfo myRotationInfo, Action<List<int>> onFinish)
	{
		DeckAutoCreateTask task = new DeckAutoCreateTask();
		if (FormatBehaviorManager.GetDefaultBehaviour(_format).UseSubClass)
		{
			task.SetParameter(_format, (int)ClassSet.MainClass, (int)ClassSet.SubClass, tournamentId, deck.ToArray());
		}
		else
		{
			task.SetParameter(_format, (int)ClassSet.MainClass, tournamentId, deck.ToArray(), myRotationInfo);
		}
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			deck = task._autoDeckCreateCardList.ToList();
			onFinish.Call(deck);
		}));
	}

	private void OnCreateAutoAnimationFinish()
	{
		UIManager.GetInstance().offNotTouch();
	}

	public void CreateDeckAddCard(List<int> addIdList)
	{
		List<int> idList = base.SelectionAreaList.IdList;
		if (idList.Count + addIdList.Count <= DECK_CARD_NUM_EDIT_MAX)
		{
			idList.AddRange(addIdList);
			List<int> idList2 = UIManager.GetInstance().getUIBase_CardManager().SortIDList(idList, base.FormatBehavior.CardMasterId);
			LoadDeckCard(idList2, null, float.MaxValue, isSkipSameDeckCheck: true);
		}
	}

	public void CreateDeckRemoveCard(int removeId)
	{
		List<int> idList = base.SelectionAreaList.IdList;
		if (idList.Count >= 0)
		{
			if (idList.Contains(removeId))
			{
				idList.Remove(removeId);
			}
			List<int> idList2 = UIManager.GetInstance().getUIBase_CardManager().SortIDList(idList, base.FormatBehavior.CardMasterId);
			LoadDeckCard(idList2, null, float.MaxValue, isSkipSameDeckCheck: true);
		}
	}

	public void ReloadDeckCard(List<int> cardIdList)
	{
		List<int> idList = UIManager.GetInstance().getUIBase_CardManager().SortIDList(cardIdList, base.FormatBehavior.CardMasterId);
		LoadDeckCard(idList, null, float.MaxValue, isSkipSameDeckCheck: true);
	}

	protected override void AccordCardInfoInSelectionArea()
	{
		LoadDeckCard(base.SelectionAreaList.IdList, null, float.MaxValue, isSkipSameDeckCheck: true);
	}

	public void SaveDeck(Action onFinish, Action<bool> saveCompleteDialogAction, bool needsClearSkin)
	{
		DeckSave.Option option = new DeckSave.Option
		{
			CardIds = base.SelectionAreaList.IdList.ToArray(),
			DeckId = DeckId,
			DeckName = DeckName,
			ClassType = (int)ClassSet.MainClass,
			RawSkinId = _rawSkinId,
			IsRandomLeaderSkin = _isRandomLeaderSkin,
			LeaderSkinIdList = _leaderSkinIdList,
			SleeveId = _sleeveId,
			IsNew = IsNewDeck,
			Format = _format,
			ConventionDeckList = _conventionDeckList,
			MyRotationId = ((base.MyRotationInfo != null) ? base.MyRotationInfo.Id : null),
			OnFinish = onFinish,
			OnSaveCompleteDialog = saveCompleteDialogAction
		};
		if (base.FormatBehavior.UseSubClass)
		{
			option.SubClassType = (int)ClassSet.SubClass;
		}
		if (needsClearSkin)
		{
			option.RawSkinId = 0;
			option.IsRandomLeaderSkin = false;
			option.LeaderSkinIdList = new List<int> { 0 };
		}
		new DeckSave().Start(option);
	}

	public List<int> FindAllAvailableCardInFormat(List<int> inSouceCardList, Format inFormat, MyRotationInfo myRotationInfo)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < inSouceCardList.Count; i++)
		{
			int num = inSouceCardList[i];
			CardParameter param = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(num);
			ClassType classType = ClassUtil.GetClassType(param, inFormat, ClassSet);
			if (param.IsAvailableFormat(inFormat, classType, myRotationInfo) && list.Count((int data) => CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(data).BaseCardId == param.BaseCardId) < param.GetSameKindNumMaxInFormat(inFormat, base.FormatBehavior, classType, myRotationInfo))
			{
				list.Add(num);
			}
		}
		return list;
	}

	protected override void UpdateCardInfo(CardObject card)
	{
		int haveNum = GetHaveNum(card.CardId);
		int usedNum = CountCardNumInSelectionArea(card.CardId, isStrictSameCard: true);
		int usedNumWithFoil = CountCardNumInSelectionArea(card.CardId, isStrictSameCard: false);
		int haveNumWithFoil = GetHaveNumTotalSameKindWithLimit(card.CardId);
		bool isMaintenance = false; // Pre-Phase-5b: no maintenance list headless
		if (base.FormatBehavior.IsConventionMode)
		{
			haveNum = 3;
			haveNumWithFoil = 3;
		}
		CardParameter cardParameterFromId = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(card.CardId);
		ClassType classType = ClassUtil.GetClassType(cardParameterFromId, _format, ClassSet);
		int sameKindNumMax = cardParameterFromId.GetSameKindNumMaxInFormat(_format, base.FormatBehavior, classType, base.MyRotationInfo);
		if (base.MyRotationInfo != null && base.MyRotationFilterType == FilterController.MyRotationFilterType.CARD_POOL_ALL_PACK)
		{
			sameKindNumMax = base.MyRotationInfo.GetSameCardCount(cardParameterFromId.BaseCardId);
		}
		card.UpdateCardInfo(_cardInfoOriginal, haveNum, haveNumWithFoil, usedNum, usedNumWithFoil, sameKindNumMax, isMaintenance);
	}

	private int GetHaveNumTotalSameKindWithLimit(int ids)
	{
		int baseCardId = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(ids).BaseCardId;
		int deckSameKindCardNumMax = base.FormatBehavior.DeckSameKindCardNumMax;
		return Mathf.Min(base.FormatBehavior.GetPossessionBaseCardNum(baseCardId, isIncludingSpotCard: true), deckSameKindCardNumMax);
	}

	private void UpdateDeckCardNumFromList(List<int> idList)
	{
		for (int i = 0; i < base.SelectionAreaList.CountKindNoFilter; i++)
		{
			CardObject cardObject = base.SelectionAreaList.FindWithIndexNoFilter(i);
			base.SelectionAreaList.CountMainAndSubNum(idList, cardObject.CardId, cardObject.IsNonPossessionCard, out var mainNum, out var subNum);
			cardObject.MainCardNum = mainNum;
			cardObject.SubCardNum = subNum;
		}
	}

	private void DisplayAttentionMessageOnRestrictedCard(List<int> idList)
	{
		CardMaster cardMaster = CardMaster.GetInstance(base.FormatBehavior.CardMasterId);
		List<int> source = idList.Select((int id) => cardMaster.GetCardParameterFromId(id).BaseCardId).ToList();
		for (int num = 0; num < base.SelectionAreaList.CountKindNoFilter; num++)
		{
			CardObject cardObject = base.SelectionAreaList.FindWithIndexNoFilter(num);
			if (!cardObject.IsAttachedCardObjData)
			{
				continue;
			}
			CardParameter cardParam = cardMaster.GetCardParameterFromId(cardObject.CardId);
			ClassType classType = ClassUtil.GetClassType(cardParam, _format, ClassSet);
			if (!cardParam.IsAvailableFormat(_format, classType, base.MyRotationInfo))
			{
				cardObject.SetCardToBanCard(_cardInfoOriginal);
			}
			else if (false /* Pre-Phase-5b: no maintenance list headless */)
			{
				cardObject.SetCardToMaintenance(_cardInfoOriginal);
				cardObject.AttachColorShader();
			}
			else if (base.FormatBehavior.ExistsRestrictedCard)
			{
				int sameKindNumMaxInFormat = cardParam.GetSameKindNumMaxInFormat(_format, base.FormatBehavior, classType, base.MyRotationInfo);
				if (sameKindNumMaxInFormat > 0)
				{
					int num2 = source.Count((int baseCardId) => baseCardId == cardParam.BaseCardId);
					cardObject.UpdateSameKindNumMaxCard(num2 > sameKindNumMaxInFormat, _cardInfoOriginal);
				}
			}
			else
			{
				cardObject.DestroyUseInfo();
			}
		}
	}

	protected override int GetSelectableCardNum(int cardId)
	{
		if (CanUseNonPossessionCard)
		{
			CardParameter cardParameterFromId = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(cardId);
			return cardParameterFromId.GetSameKindNumMaxInFormat(classType: ClassUtil.GetClassType(cardParameterFromId, _format, ClassSet), inFormat: _format, behavior: base.FormatBehavior, myRotationInfo: base.MyRotationInfo);
		}
		return base.GetSelectableCardNum(cardId);
	}

	public void UpdateCardDisplay()
	{
		DisplayAttentionMessageOnRestrictedCard(base.SelectionAreaList.IdList);
		UpdatePagingCardInfoAll();
	}

	protected override void OnCreateSelectionEachCard(CardObject card)
	{
		base.OnCreateSelectionEachCard(card);
		if (base.FormatBehavior.ExistsRestrictedCard)
		{
			CardMaster cardMaster = CardMaster.GetInstance(base.FormatBehavior.CardMasterId);
			CardParameter cardParameterFromId = cardMaster.GetCardParameterFromId(card.CardId);
			int baseCardId = cardParameterFromId.BaseCardId;
			ClassType classType = ClassUtil.GetClassType(cardParameterFromId, _format, ClassSet);
			int sameKindNumMaxInFormat = cardParameterFromId.GetSameKindNumMaxInFormat(_format, base.FormatBehavior, classType, base.MyRotationInfo);
			if (base.SelectionAreaList.CardList.Where((CardObject cardObj) => cardMaster.GetCardParameterFromId(cardObj.CardId).BaseCardId == baseCardId).Sum((CardObject cardObj) => cardObj.TotalCardNum) > sameKindNumMaxInFormat)
			{
				card.AttachRedShader();
			}
			if (!cardParameterFromId.IsAvailableFormat(_format, classType, base.MyRotationInfo))
			{
				card.SetCardToBanCard(_cardInfoOriginal);
			}
		}
	}
}

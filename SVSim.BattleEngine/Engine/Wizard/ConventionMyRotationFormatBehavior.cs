using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.DeckCardEdit;

namespace Wizard;

public class ConventionMyRotationFormatBehavior : IFormatBehavior
{
	private ConventionDeckList _conventionDeckList;

	public string Name => Data.SystemText.Get("Common_0178");

	public string SmallIconSpriteName => "icon_myrotation_s";

	public CardMaster.CardMasterId CardMasterId => CardMaster.CardMasterId.Default;

	public GenerateDeckCodeTask.SubmitDeckType DeckCodeType => GenerateDeckCodeTask.SubmitDeckType.MY_ROTATION;

	public bool ExistsRestrictedCard => true;

	public List<int> SortedDeckUsableCardList => CardMaster.GetInstance(CardMasterId).GetAllCardIds();

	public int DeckCardNumMin => DeckCardNumMax;

	public int DeckCardNumMax => 40;

	public int DeckSameKindCardNumMax => 3;

	public int DeckSavableCardNumMax => 50;

	public bool IsShowDeckName => true;

	public bool IsEmphasizeDeckCardShortage => false;

	public bool IsEmphasizeDeckCardOverage => true;

	public bool IsSavableLastSelectDeck => true;

	public bool CanShowQRCode => true;

	public bool IsShowFirstTipsAtDeckEdit => true;

	public bool IsShowAutoDeckCreateButtonAtDeckEdit => true;

	public bool IsCraftableCardAtDeckEdit => false;

	public UIManager.ViewScene DeckEditBackScene => UIManager.ViewScene.DeckList;

	public Action<CardBundleController> DeckSaveFunc => null;

	public bool UseSubClass => false;

	public List<CardSetName> AvailableCardSetNameList => (from id in Data.Load.data.RotationCardSetList.Distinct()
		select Data.Master.CardSetNameMgr.Get(id.ToString())).ToList();

	public bool IsShowPrizeCardSetFilter => true;

	public bool IsShowPhantomCardSetFilter => false;

	public bool IsShowFormatFilter => false;

	public bool IsShowFavoriteFilter => true;

	public bool IsShowSpotCardFilter => false;

	public bool IsConventionMode => true;

	public ConventionMyRotationFormatBehavior(ConventionDeckList conventionDeckList)
	{
		_conventionDeckList = conventionDeckList;
	}

	public bool IsEnableDeckShareButton(int cardNum, int cardNumMax)
	{
		return cardNum == DeckCardNumMax;
	}

	public IDictionary<int, int> GetCardPool(bool isIncludingSpotCard)
	{
		return _conventionDeckList.CardPool;
	}

	public int GetPossessionCardNum(int cardId, bool isIncludingSpotCard)
	{
		if (_conventionDeckList.CardPool.ContainsKey(cardId))
		{
			return _conventionDeckList.CardPool[cardId];
		}
		return 0;
	}

	public Dictionary<int, int> ClonePossessionCardDictionary(bool isIncludingSpotCard)
	{
		return new Dictionary<int, int>(_conventionDeckList.CardPool);
	}

	public int GetPossessionBaseCardNum(int baseCardId, bool isIncludingSpotCard)
	{
		return DataMgr.GetPossessionBaseCardNum(baseCardId, _conventionDeckList.CardPool, CardMasterId);
	}
}

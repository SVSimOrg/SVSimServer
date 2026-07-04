using System;
using System.Collections.Generic;
using Wizard.DeckCardEdit;

namespace Wizard;

public class HofFormatBehavior : IFormatBehavior
{
	public string Name => string.Empty;

	public string SmallIconSpriteName => string.Empty;

	public CardMaster.CardMasterId CardMasterId => CardMaster.CardMasterId.Default;

	public GenerateDeckCodeTask.SubmitDeckType DeckCodeType => GenerateDeckCodeTask.SubmitDeckType.NORMAL;

	public bool ExistsRestrictedCard => false;

	public List<int> SortedDeckUsableCardList => CardMaster.GetInstance(CardMasterId).GetAllCardIds();

	public int DeckCardNumMin => DeckCardNumMax;

	public int DeckCardNumMax => 40;

	public int DeckSameKindCardNumMax => 3;

	public int DeckSavableCardNumMax => 50;

	public bool IsShowDeckName => true;

	public bool IsEmphasizeDeckCardShortage => false;

	public bool IsEmphasizeDeckCardOverage => true;

	public bool IsSavableLastSelectDeck => true;

	public bool CanShowQRCode => false;

	public bool IsShowFirstTipsAtDeckEdit => true;

	public bool IsShowAutoDeckCreateButtonAtDeckEdit => true;

	public bool IsCraftableCardAtDeckEdit => true;

	public UIManager.ViewScene DeckEditBackScene => UIManager.ViewScene.DeckList;

	public Action<CardBundleController> DeckSaveFunc => null;

	public bool UseSubClass => false;

	public List<CardSetName> AvailableCardSetNameList => Data.Master.CardSetNameMgr.GetListBasicAndPack();

	public bool IsShowPrizeCardSetFilter => true;

	public bool IsShowPhantomCardSetFilter => false;

	public bool IsShowFormatFilter => true;

	public bool IsShowFavoriteFilter => true;

	public bool IsShowSpotCardFilter => false; // headless: no user inventory / spot cards

	public bool IsConventionMode => false;

	public bool IsEnableDeckShareButton(int cardNum, int cardNumMax)
	{
		return cardNum == cardNumMax;
	}

	public IDictionary<int, int> GetCardPool(bool isIncludingSpotCard)
	{
		return new System.Collections.Generic.Dictionary<int, int>(); // headless: empty inventory
	}

	public Dictionary<int, int> ClonePossessionCardDictionary(bool isIncludingSpotCard)
	{
		return new System.Collections.Generic.Dictionary<int, int>(); // headless: empty inventory
	}

	public int GetPossessionCardNum(int cardId, bool isIncludingSpotCard)
	{
		return 0; // headless: no user card counts
	}

	public int GetPossessionBaseCardNum(int baseCardId, bool isIncludingSpotCard)
	{
		return 0; // headless: no user card counts
	}
}

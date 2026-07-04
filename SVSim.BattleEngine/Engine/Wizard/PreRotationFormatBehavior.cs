using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.DeckCardEdit;

namespace Wizard;

public class PreRotationFormatBehavior : IFormatBehavior
{
	public string Name => UIUtil.GetFormatName(Format.PreRotation);

	public string SmallIconSpriteName => "icon_prerotation_s";

	public CardMaster.CardMasterId CardMasterId => Prerelease.Instance.CardMasterId;

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

	public bool CanShowQRCode => true;

	public bool IsShowFirstTipsAtDeckEdit => true;

	public bool IsShowAutoDeckCreateButtonAtDeckEdit => true;

	public bool IsCraftableCardAtDeckEdit => true;

	public UIManager.ViewScene DeckEditBackScene => UIManager.ViewScene.DeckList;

	public Action<CardBundleController> DeckSaveFunc => null;

	public bool UseSubClass => false;

	public bool IsConventionMode => false;

	public List<CardSetName> AvailableCardSetNameList => (from id in Prerelease.Instance.RotationCardSetList.Distinct()
		select Data.Master.CardSetNameMgr.Get(id.ToString())).ToList();

	public bool IsShowPrizeCardSetFilter => true;

	public bool IsShowPhantomCardSetFilter => false;

	public bool IsShowFormatFilter => false;

	public bool IsShowFavoriteFilter => true;

	public bool IsShowSpotCardFilter => false; // headless: no user inventory / spot cards

	public bool IsEnableDeckShareButton(int cardNum, int cardNumMax)
	{
		return cardNum == DeckCardNumMax;
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

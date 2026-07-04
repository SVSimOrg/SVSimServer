using System;
using System.Collections.Generic;
using Wizard.DeckCardEdit;

namespace Wizard;

public interface IFormatBehavior
{
	string Name { get; }

	string SmallIconSpriteName { get; }

	CardMaster.CardMasterId CardMasterId { get; }

	GenerateDeckCodeTask.SubmitDeckType DeckCodeType { get; }

	bool ExistsRestrictedCard { get; }

	List<int> SortedDeckUsableCardList { get; }

	int DeckCardNumMin { get; }

	int DeckCardNumMax { get; }

	int DeckSameKindCardNumMax { get; }

	int DeckSavableCardNumMax { get; }

	bool IsShowDeckName { get; }

	bool IsEmphasizeDeckCardShortage { get; }

	bool IsEmphasizeDeckCardOverage { get; }

	bool IsSavableLastSelectDeck { get; }

	bool IsShowFirstTipsAtDeckEdit { get; }

	bool IsShowAutoDeckCreateButtonAtDeckEdit { get; }

	bool IsCraftableCardAtDeckEdit { get; }

	UIManager.ViewScene DeckEditBackScene { get; }

	Action<CardBundleController> DeckSaveFunc { get; }

	bool UseSubClass { get; }

	List<CardSetName> AvailableCardSetNameList { get; }

	bool IsShowPrizeCardSetFilter { get; }

	bool IsShowPhantomCardSetFilter { get; }

	bool IsShowFormatFilter { get; }

	bool IsShowFavoriteFilter { get; }

	bool IsShowSpotCardFilter { get; }

	bool IsConventionMode { get; }

	bool CanShowQRCode { get; }

	bool IsEnableDeckShareButton(int cardNum, int cardNumMax);

	IDictionary<int, int> GetCardPool(bool isIncludingSpotCard);

	Dictionary<int, int> ClonePossessionCardDictionary(bool isIncludingSpotCard);

	int GetPossessionCardNum(int cardId, bool isIncludingSpotCard);

	int GetPossessionBaseCardNum(int baseCardId, bool isIncludingSpotCard);
}

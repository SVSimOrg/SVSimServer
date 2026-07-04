using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using Wizard.DeckCardEdit;

namespace Wizard;

public class SealedFormatBehavior : IFormatBehavior
{
	public string Name => string.Empty;

	public string SmallIconSpriteName => "icon_sealed_s";

	public CardMaster.CardMasterId CardMasterId => CardMaster.CardMasterId.Default;

	public GenerateDeckCodeTask.SubmitDeckType DeckCodeType => GenerateDeckCodeTask.SubmitDeckType.SEALED;

	public bool ExistsRestrictedCard => false;

	public List<int> SortedDeckUsableCardList => SealedData.SortedOwnSealedCardList.Distinct().ToList();

	public int DeckCardNumMin => Data.ArenaData.SealedMyPageResponseData.DeckCardNumMin;

	public int DeckCardNumMax => SealedData.DeckCardNumMax.Value;

	public int DeckSameKindCardNumMax => DeckCardNumMax;

	public int DeckSavableCardNumMax => DeckCardNumMax;

	public bool IsShowDeckName => false;

	public bool IsEmphasizeDeckCardShortage => true;

	public bool IsEmphasizeDeckCardOverage => true;

	public bool IsSavableLastSelectDeck => false;

	public bool CanShowQRCode => false;

	public bool IsShowFirstTipsAtDeckEdit => false;

	public bool IsShowAutoDeckCreateButtonAtDeckEdit => false;

	public bool IsCraftableCardAtDeckEdit => false;

	public UIManager.ViewScene DeckEditBackScene => UIManager.ViewScene.Sealed;

	public Action<CardBundleController> DeckSaveFunc => SaveDeck;

	public bool UseSubClass => false;

	public List<CardSetName> AvailableCardSetNameList => (from x in Data.ArenaData.SealedMyPageResponseData.CardPackIdList.Distinct().Select(Data.Master.CardSetNameMgr.Get)
		orderby x.ID
		select x).ToList();

	public bool IsShowPrizeCardSetFilter => false;

	public bool IsShowPhantomCardSetFilter => true;

	public bool IsShowFormatFilter => false;

	public bool IsShowFavoriteFilter => false;

	public bool IsShowSpotCardFilter => false;

	public bool IsConventionMode => false;

	private SealedData SealedData => Data.ArenaData.SealedData;

	public bool IsEnableDeckShareButton(int cardNum, int cardNumMax)
	{
		if (DeckCardNumMin <= cardNum)
		{
			return cardNum <= DeckCardNumMax;
		}
		return false;
	}

	private void SaveDeck(CardBundleController deckCardBundle)
	{
		List<int> cardList = deckCardBundle.SelectionAreaList.IdList;
		int count = cardList.Count;
		UIManager uiMgr = UIManager.GetInstance();
		SystemText text = Data.SystemText;
		Action action = delegate
		{
			uiMgr.StartCoroutine(Toolbox.NetworkManager.Connect(new SealedUpdateDeckTask(cardList), delegate
			{
				UIManager.GetInstance().CreateConfirmationDialog(text.Get("Card_0019")).OnCloseStart = delegate
				{
					uiMgr.ChangeViewScene(UIManager.ViewScene.Sealed);
				};
			}));
		};
		if (count < DeckCardNumMin)
		{
			DialogBase dialogBase = uiMgr.CreateDialogClose();
			dialogBase.SetTitleLabel(text.Get("Dia_DeckEdit_005_Title"));
			dialogBase.SetText(text.Get("Card_0050", DeckCardNumMin.ToString()));
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
			dialogBase.SetButtonText(text.Get("Dia_DeckEdit_005_Button"), text.Get("Dia_DeckEdit_005_Button_2"));
			dialogBase.onPushButton1 = action;
		}
		else
		{
			action();
		}
	}

	public IDictionary<int, int> GetCardPool(bool isIncludingSpotCard)
	{
		return null;
	}

	public Dictionary<int, int> ClonePossessionCardDictionary(bool isIncludingSpotCard)
	{
		return null;
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

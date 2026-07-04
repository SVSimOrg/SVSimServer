using System;
using UnityEngine;
using Wizard;

// PASS-8/Phase-2 STUB: 2,429-line client-side card-inspection popup reduced to the
// compile-time surface external callers touch. Held alive by deck editor, card
// collection, gacha reveal, and shop paths referencing CardDetailUI instances.
// Twenty-one members preserved (all missed by static-ref grep because callers reach them
// via typed local variables). See Task 2b of PASS8-PLAN.md.
public class CardDetailUI : UIBase
{
    // Event delegates
    public Action<int> OnCardSellId;
    public Action OnCardBuy;
    public Action OnClose;
    public Action<Vector2> OnDragCard;
    public Action OnDetailCardUpdate { get; set; }

    // Data
    public CardParameter CardData { get; private set; }

    // Settable flags
    public bool IsShowFlavorTextButton { get; set; }
    public bool IsShowVoiceButton { get; set; }
    public bool IsShowEvolutionButton { get; set; }
    public bool IsShowCraftButtons { get; set; }
    public bool IsOwnCardNum { get; set; }
    public bool IsShortageUI { get; set; }
    public bool LeftButtonVisible { get; set; }
    public bool RightButtonVisible { get; set; }
    public bool IsEnableShowDetail { get; set; }

    // Methods
    public void Initialize(int detailLayer, CardMaster.CardMasterId cardMasterId, IFormatBehavior formatBehaviorForCardPoolChange = null) { }
    public void OnPushCardDetailOn(GameObject g) { }
    public bool GetIsDetailOn() => false;
    public void ChangeCardMaster(CardMaster.CardMasterId cardMasterId) { }
    public bool ShowCardDetail(GameObject g) => false;
    public void CloseDefault(bool playSe) { }
}

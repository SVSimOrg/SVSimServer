using System.Collections.Generic;
using UnityEngine;
using Wizard;

// PASS-8/Phase-1 STUB: 1,066-line client-side mypage hub reduced to its compile-time
// surface. Nothing constructs a MyPageMenu in the headless node; the ~50 external
// callers reach `MyPageMenu.Instance.Foo()` from UI event handlers that never fire on
// the node's IsRecovery=true receive path. `Instance` returns null so those chains
// NRE at read-time if they ever ran — they don't. The three genuinely-static members
// (IsMyPageRequestEnd flag, SetEnableReloadCard, CreateDialogForTutorial) return
// defaults that match the "no UI ran" state.
public class MyPageMenu : UIBase
{
    public static MyPageMenu Instance => null;

    public MyPageItemHome HomeMenu => null;
    public bool IsEnableFooterCurrentMenu => false;
    public bool IsVisible => false;

    public static void SetEnableReloadCard() { }
    public static DialogBase CreateDialogForTutorial() => null;

    public void ChangeMenu(int index, bool isCutCardMotion = false) { }
    public void OnReadGift() { }
    public void SetGuideEffect(Transform parent, Vector3 localPosition, float rotation) { }
    public void SetGuideToOkOnlyDialog(DialogBase dialog) { }
    public void UpdateMissionCount() { }
    public void UpdateCrystalCount() { }
    public void UpdateRupyCount() { }

    public void GoToCardDeck() { }
    public void GoToChallengeMenu() { }
    public void GoToColosseum(bool isColosseumTask = true) { }
    public void GoToConventionActionMenu(ConventionInfo conventionInfo) { }
    public void GoToConventionListMenu() { }
    public void GoToFreeMatch() { }
    public void GoToPracticeTypeSelect() { }
    public void GoToShopCard() { }
    public void GoToShopSupply() { }
}

// AUTO-GENERATED (m1_iface_impl) — explicit no-op interface impls layered onto hierarchy bases.
// CONTAINS HAND-EDITS. Before any regen, grep this file for "HEADLESS-FIX" and re-apply those blocks;
// a plain regen will clobber them.

namespace Wizard.Battle.View {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Wizard.Battle.View.Vfx;
    public partial class BattleCardView : global::Wizard.Battle.View.IBattleCardView {
        Vector3 global::Wizard.Battle.View.IBattleCardView.ForecastIconPosition { get => default!; }
        Vector3 global::Wizard.Battle.View.IBattleCardView.ForecastIconScale { get => default!; }
        float global::Wizard.Battle.View.IBattleCardView.OriginalRootYPosition { get => default!; }
        IReadOnlyBattleCardInfo global::Wizard.Battle.View.IBattleCardView.CardInfo { get => HeadlessCardInfo; } // HEADLESS-FIX (M-HC-4a): the backing card (from BuildInfo) — AttackSelectControl.IsCardTranslatable reads CardInfo.IsClass
        BattlePlayerReadOnlyInfoPair global::Wizard.Battle.View.IBattleCardView.PlayerInfoPair { get => default!; }
        IReadOnlyVoiceInfo global::Wizard.Battle.View.IBattleCardView.VoiceInfo { get => global::Wizard.Battle.View.HeadlessVoiceInfo.Instance; } // HEADLESS-FIX (M7): non-null voice info for the death-voice tail
        GameObject global::Wizard.Battle.View.IBattleCardView.GameObject { get => default!; }
        GameObject global::Wizard.Battle.View.IBattleCardView.CardWrapObject { get => default!; }
        Transform global::Wizard.Battle.View.IBattleCardView.Transform { get => default!; }
        CardTemplate global::Wizard.Battle.View.IBattleCardView.CardTemplate { get => default!; }
        BoxCollider global::Wizard.Battle.View.IBattleCardView.Collider { get => default!; }
        private BattleCardIconAnimations _headlessIconAnims; // HEADLESS-FIX (N1)
        BattleCardIconAnimations global::Wizard.Battle.View.IBattleCardView.BattleCardIconAnimations { get => _headlessIconAnims ??= global::Wizard.Battle.View.HeadlessIconAnimations.Create(); } // HEADLESS-FIX (N1/M-HC-0b): non-null no-op so ReplaceReceivedCard.CreateActualCard's follower icon-init AND the receive play path's BattlePlayerBase.UpdateInPlayBattleCardIconLabel (HasInductionNumberSkill iterates the private `collection`) don't NRE. HeadlessIconAnimations.Create seeds an empty SkillCollectionBase so the induction-label check is a clean false.
        Func<bool> global::Wizard.Battle.View.IBattleCardView.GetIsOnMove { get => default!; }
        bool global::Wizard.Battle.View.IBattleCardView.InPlayModelActive { get => default!; set { } }
        BattleCamera global::Wizard.Battle.View.IBattleCardView.m_BattleCamera { get => default!; }
        BackGroundBase global::Wizard.Battle.View.IBattleCardView.m_BackGround { get => default!; }
        HandParameter global::Wizard.Battle.View.IBattleCardView.HandParam { get => default!; }
        BattleCardView.AttackTargetSelectInfo global::Wizard.Battle.View.IBattleCardView._attackTargetSelectInfo { get => default!; set { } }
        InPlayCardFrameEffectControl global::Wizard.Battle.View.IBattleCardView._inPlayFrameEffect { get => _headlessInPlayFrameEffect; set { _headlessInPlayFrameEffect = value; } } // HEADLESS-FIX (M-HC-4a): non-null no-op frame-effect control (HideFrameEffect/UpdateCanAttackEffect are no-ops) for the receive ATTACK path
        bool global::Wizard.Battle.View.IBattleCardView.areArrowsForcedOff { get => default!; set { } }
        bool global::Wizard.Battle.View.IBattleCardView._isCardQueuedToBePlayed { get => default!; set { } }
        bool global::Wizard.Battle.View.IBattleCardView.isHiddenFromHandView { get => default!; set { } }
        bool global::Wizard.Battle.View.IBattleCardView.isHiddenFromInPlayView { get => default!; set { } }
        bool global::Wizard.Battle.View.IBattleCardView.isHideFrameEffect { get => default!; set { } }
        bool global::Wizard.Battle.View.IBattleCardView._hasCardEnteredPlayQueue { get => default!; set { } }
        bool global::Wizard.Battle.View.IBattleCardView.playVoiceOnDeath { get => default!; set { } }
        Coroutine global::Wizard.Battle.View.IBattleCardView._inPlayRearrangeCoroutine { get => default!; set { } }
        Coroutine global::Wizard.Battle.View.IBattleCardView._waitUntilCardIsInQueueCoroutine { get => default!; set { } }
        bool global::Wizard.Battle.View.IBattleCardView.IsNullView { get => default!; }
        bool global::Wizard.Battle.View.IBattleCardView.IsLoadResorces { get => default!; }
        void global::Wizard.Battle.View.IBattleCardView.InitializeVoiceInfo(int cardID) { }
        void global::Wizard.Battle.View.IBattleCardView.SetupIconAnimations(BattleCardBase card, SkillCollectionBase skills) { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.LoadResource() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.GetResourcePathes(List<BattleManagerBase.ResourceInfo> resourceInfos) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.LoadChoiceTransformCardsResources(BattleCardBase card) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.GetChoiceTransformCardsResourcePathes(BattleCardBase card, List<BattleManagerBase.ResourceInfo> resourceInfos, bool isRecoveryFinish = false) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattleCardView.ResetTemplate() { }
        bool global::Wizard.Battle.View.IBattleCardView.HasChild(string objectName) => default!;
        void global::Wizard.Battle.View.IBattleCardView.AttachChild(string objectName, GameObject gameObject, bool isDestoryEarlierAttached = false) { }
        void global::Wizard.Battle.View.IBattleCardView.ReserveAttachChild(string objectName) { }
        bool global::Wizard.Battle.View.IBattleCardView.HasReservedAttachChild(string objectName) => default!;
        GameObject global::Wizard.Battle.View.IBattleCardView.DetachChild(string objectName) => default!;
        void global::Wizard.Battle.View.IBattleCardView.DestroyChild(string objectName) { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.UnloadResource() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattleCardView.UpdateMovability() { }
        void global::Wizard.Battle.View.IBattleCardView.HideCanPlayEffect() { }
        GameObject global::Wizard.Battle.View.IBattleCardView.GetCardMeshGameObject() => default!;
        void global::Wizard.Battle.View.IBattleCardView.UpdateParameterView(int offence, int life, int cost, string name, bool isOnField, bool isRecovery = false, bool useNormalCost = false) { }
        void global::Wizard.Battle.View.IBattleCardView.UpdateOffence(int offence) { }
        void global::Wizard.Battle.View.IBattleCardView.UpdateLife(int life) { }
        void global::Wizard.Battle.View.IBattleCardView.UpdateCost(List<int> costList, bool isGenerateInHand = true, bool playEffect = true, bool isForceUpdate = false, bool isOnlyFixedUseCost = false) { }
        List<int> global::Wizard.Battle.View.IBattleCardView.GetUseCostList(int cost, bool useNomalCost = false) => default!;
        void global::Wizard.Battle.View.IBattleCardView.UpdateCostWithoutFixedUse(int cost) { }
        void global::Wizard.Battle.View.IBattleCardView.SetTillingAndOffset(Vector2 tilling, Vector2 offset) { }
        void global::Wizard.Battle.View.IBattleCardView.SetVoiceFileCueName(string cueName) { }
        void global::Wizard.Battle.View.IBattleCardView.PlayVoice(string voiceName) { }
        void global::Wizard.Battle.View.IBattleCardView.StopVoice() { }
        void global::Wizard.Battle.View.IBattleCardView.ShowInHandFrameEffect(bool enable) { }
        void global::Wizard.Battle.View.IBattleCardView.ShowInHandFrameEffect(bool enable, HandCardFrameEffectType type) { }
        void global::Wizard.Battle.View.IBattleCardView.ShowFusionMetamorphoseFrameEffect(bool enable) { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.ResetCardView(CardParameter baseParameter) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.RecoveryInPlay() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.RecoveryInHand() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.ShowHandCardInfo(bool isRecovery = false, bool modifyParameterLabel = true) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattleCardView.HideHandCardInfo() { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.ShowAttackFinished() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.ShowAttackFinished(SkillBase skill) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattleCardView.HideAttackFinished() { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.InitializeBattleCardIcon(BattleCardBase card, SkillCollectionBase collection, bool isStackWhiteRitual = false) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.InitializeBattleCardStackIcon(BattleCardBase card, SkillCollectionBase collection) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.ShowBattleCardIcon() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattleCardView.SetCostLabelEnable(bool isEnable) { }
        void global::Wizard.Battle.View.IBattleCardView.SetNormalLabelEnable(bool isEnable) { }
        GameObject global::Wizard.Battle.View.IBattleCardView.GetChild(string objectName) => default!;
        void global::Wizard.Battle.View.IBattleCardView.InitHandParameter() { }
        void global::Wizard.Battle.View.IBattleCardView.UpdateCostViewStrategy(bool isForceUpdate = false) { }
        void global::Wizard.Battle.View.IBattleCardView.InitHandParameterIconPos(HandParameter.IconLayout layout) { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.UpdateBattleCardIconLabelNumber(BattleCardBase card, SkillCollectionBase collection) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.UpdateStackWhiteRitualIconNumber() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattleCardView.SetCutInLayerNormalObject() { }
        void global::Wizard.Battle.View.IBattleCardView.ResetPlayQueueFlags() { }
        void global::Wizard.Battle.View.IBattleCardView.SetParameterIconEnable(bool isEnable) { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.AddBattleCardIcon(string iconType, string iconFileName) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.DeleteBattleCardIcon(string iconType) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattleCardView.SetNotCancelColliderEnable(bool isEnable) { }
        void global::Wizard.Battle.View.IBattleCardView.InitCostViewAnim() { }
        VfxBase global::Wizard.Battle.View.IBattleCardView.LoadEvolveFrameEffect() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattleCardView.HideBattleCardIcon() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
    }
}
namespace Wizard.Battle.View {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Wizard.Battle.UI;
    using Wizard.Battle.View.Vfx;
    public partial class BattlePlayerView : global::Wizard.Battle.View.IPlayerView {
        bool global::Wizard.Battle.View.IPlayerView._isEvolutionSkillSelect { get => default!; set { } }
        bool global::Wizard.Battle.View.IPlayerView.IsEvolutionStart { get => default!; set { } }
        bool global::Wizard.Battle.View.IPlayerView.IsEvolutionVfx { get => default!; set { } }
        bool global::Wizard.Battle.View.IPlayerView.IsMenuOpen { get => default!; set { } }
        BattleCardBase global::Wizard.Battle.View.IPlayerView.DetailOpenCard { get => default!; }
        bool global::Wizard.Battle.View.IPlayerView.CanPlayerEndTurnImmediately { get => default!; }
        bool global::Wizard.Battle.View.IPlayerView.IsShowTurnEndDialogOfNotAttackingOrPlaying { get => default!; }
        bool global::Wizard.Battle.View.IPlayerView.IsShowTurnEndDialogOfNotUsingHeroSkill { get => default!; }
        bool global::Wizard.Battle.View.IPlayerView.IsMenuCloseEscape { get => default!; set { } }
        GameObject global::Wizard.Battle.View.IPlayerView.CardMoveEffect { get => default!; set { } }
        void global::Wizard.Battle.View.IPlayerView.HideDetailPanel() { }
        void global::Wizard.Battle.View.IPlayerView.ShowTurnEndDialog(GameObject return_obj = null) { }
        void global::Wizard.Battle.View.IPlayerView.UpdateTurnEndPulseEffect() { }
        void global::Wizard.Battle.View.IPlayerView.CallOnOpenEvolveDialoguePanel() { }
        void global::Wizard.Battle.View.IPlayerView.DragArrowStart(BattleManagerBase battleMgr, BattleCardBase attackCard, GameObject arrowHead) { }
        void global::Wizard.Battle.View.IPlayerView.DragArrowStart(BattleManagerBase battleMgr, GameObject startObject, GameObject arrowHead, bool isTargettingEnemy = true) { }
        void global::Wizard.Battle.View.IPlayerView.DragArrow(BattleManagerBase battleMgr, GameObject arrowHead, Vector3 pos) { }
        void global::Wizard.Battle.View.IPlayerView.ShowTurnEndButton(bool showEffect = true) { }
        void global::Wizard.Battle.View.IPlayerView.MoveCardCancel(BattleCardBase hitCard, Vector3 position, Quaternion rotation, bool IsPress) { }
        bool global::Wizard.Battle.View.IPlayerView.IsDetailOn() => default!;
        void global::Wizard.Battle.View.IPlayerView.MoveCardStart(BattleCardBase moveCard, bool isEffectAndSoundOn) { }
        void global::Wizard.Battle.View.IPlayerView.CancelCardDrag(BattleCardBase cardBeingDragged) { }
        void global::Wizard.Battle.View.IPlayerView.ShowDetailPanel(BattleManagerBase battleMgrBase, OperateMgr operateMgr, BattleCardBase card, DetailPanelControl.ShowRequest showRequest, BattleLogItem.CardTextureOption textureOption = BattleLogItem.CardTextureOption.Null, BuffInfo buff = null, string divergenceId = "", int logTextureId = 0) { }
        BattleCardBase global::Wizard.Battle.View.IPlayerView.GetDetailCard() => default!;
        void global::Wizard.Battle.View.IPlayerView.ResetTouchable() { }
        VfxBase global::Wizard.Battle.View.IPlayerView.HideTurnEndPulseEffect() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        bool global::Wizard.Battle.View.IPlayerView.IsMoving() => default!;
        void global::Wizard.Battle.View.IPlayerView.OffNotHideAndNotCreate() { }
        void global::Wizard.Battle.View.IPlayerView.ForceShowTurnEndButton() { }
        void global::Wizard.Battle.View.IPlayerView.ClearDifferentiatePopUp(List<BattlePlayerViewBase.BattleDialogItem> deselectionItem) { }
        void global::Wizard.Battle.View.IPlayerView.ShowPlayerTurnEnd(bool isAuto = false) { }
        void global::Wizard.Battle.View.IPlayerView.HideSubDetailPanel() { }
        void global::Wizard.Battle.View.IPlayerView.ShowKeyPanel(int page) { }
        void global::Wizard.Battle.View.IPlayerView.HideKeyPanel() { }
        DialogBase global::Wizard.Battle.View.IPlayerView.CreateKeyPanel(BattleCardBase card, UILabel label, CardMaster.CardMasterId cardMasterId, CardParameter baseParameter) => default!;
        DialogBase global::Wizard.Battle.View.IPlayerView.ShowRetireConfirmPanel() => default!;
        DialogBase global::Wizard.Battle.View.IPlayerView.CreateBattleSetting() => default!;
        void global::Wizard.Battle.View.IPlayerView.MoveCard(BattleCardBase hitCard, Vector3 pos) { }
        void global::Wizard.Battle.View.IPlayerView.CardMoveEffectSwitch(bool on) { }
        void global::Wizard.Battle.View.IPlayerView.SetDetailScreenPosition(bool right) { }
        Effect global::Wizard.Battle.View.IPlayerView.DetailPanelSelectEffectOn(BattleCardBase selectedCard, DetailPanelControl.ShowRequest request) => default!;
        void global::Wizard.Battle.View.IPlayerView.DetailPanelSelectEffectOff() { }
        void global::Wizard.Battle.View.IPlayerView.GetCardSelectedWithButton(Camera camera, ref UIButton button, ref BattleCardBase card, ref GameObject check) { }
        void global::Wizard.Battle.View.IPlayerView.ShowDetailPanelList(BattleManagerBase battleMgrBase, OperateMgr operateMgr, List<BattleCardBase> cards, DetailPanelControl.ShowRequest showRequest) { }
        void global::Wizard.Battle.View.IPlayerView.LockOnAttackTarget(BattleCardBase Attacker, BattleCardBase Target) { }
        bool global::Wizard.Battle.View.IPlayerView.IsFieldDetailOn() => default!;
        DialogBase global::Wizard.Battle.View.IPlayerView.ShowFusionCardPlayDialog(EventDelegate onClickOk, Action onClose) => default!;
        void global::Wizard.Battle.View.IPlayerView.HideModeEffect(bool on) { }
        void global::Wizard.Battle.View.IPlayerView.DetailReverseOver() { }
        void global::Wizard.Battle.View.IPlayerView.AddPopUpPanel(NonDialogPopup popup, BattlePlayerViewBase.BattleDialogItem item) { }
        event Action global::Wizard.Battle.View.IPlayerView.OnRetire { add { } remove { } }
        event Func<bool> global::Wizard.Battle.View.IPlayerView.OnCheckImmediateTurnEnd { add { } remove { } }
        event Action global::Wizard.Battle.View.IPlayerView.OnStartMoveCard { add { } remove { } }
        event Action global::Wizard.Battle.View.IPlayerView.OnCancelMoveCard { add { } remove { } }
        event Action global::Wizard.Battle.View.IPlayerView.OnOpenEvolveDialoguePanel { add { } remove { } }
        event Action global::Wizard.Battle.View.IPlayerView.OnLockOn { add { } remove { } }
        event Action global::Wizard.Battle.View.IPlayerView.OnReleaseLockOn { add { } remove { } }
        event Action global::Wizard.Battle.View.IPlayerView.OnOpenDetailPanel { add { } remove { } }
        ITurnEndButtonUI global::Wizard.Battle.View.IBattlePlayerView.TurnEndButtonUI { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.EpIcon { get => default!; }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsSelecting { get => default!; }
        // HEADLESS-FIX (M13): generator emitted `default!` (null); the OperateMgr emit path calls
        // BattleView.HandView.RemoveCardFromView (BattlePlayerBase.cs:1422). Redirect to a shared no-op
        // HandViewBase so the presentation call is a safe no-op (the played card is never in its list).
        HandViewBase global::Wizard.Battle.View.IBattlePlayerView.HandView { get => global::Wizard.Battle.View.HeadlessHandViewStub.Instance; }
        HandControl global::Wizard.Battle.View.IBattlePlayerView.HandControl { get => default!; }
        BattleCardBase global::Wizard.Battle.View.IBattlePlayerView.SelectSkillActCard { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.TurnEndBtn { get => default!; }
        BattleCardBase global::Wizard.Battle.View.IBattlePlayerView.m_CurrentTarget { get => default!; }
        // HEADLESS-FIX (M-HC-0b): generator emitted `default!` (null); the RECEIVE-conductor play path
        // calls BattleView.PlayQueueView.AddCardToViewVfx (OperateMgr.cs:201/203/219/221). Redirect to a
        // shared no-op PlayQueueViewBase so the presentation call is a safe NullVfx (the authoritative
        // play mutation runs in PlayHandCardReflection.Play, not in this view).
        PlayQueueViewBase global::Wizard.Battle.View.IBattlePlayerView.PlayQueueView { get => global::Wizard.Battle.View.HeadlessPlayQueueViewStub.Instance; }
        AttackSelectControl global::Wizard.Battle.View.IBattlePlayerView.AttackSelectControl { get => global::Wizard.Battle.View.HeadlessAttackSelectControl.Instance; } // HEADLESS-FIX (M-HC-4a): non-null no-op attack-select-control for the receive ATTACK path (RegisterPairToAttackSelectControl + ActionProcessor.Attack reset arm)
        InPlayViewBase global::Wizard.Battle.View.IBattlePlayerView.InPlayView { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.StatusParentPanel { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.AnchorL { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.CommonPanel { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.EpPanel { get => default!; }
        UIGrid global::Wizard.Battle.View.IBattlePlayerView.HandDeck { get => default!; }
        UIGrid global::Wizard.Battle.View.IBattlePlayerView.SetDeck { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.CemeteryParent { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.BanishParent { get => default!; }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsNowTurnEnd { get => default!; }
        Action<bool> global::Wizard.Battle.View.IBattlePlayerView.OnCancelSkillTargetSelect { get => default!; set { } }
        Action<bool> global::Wizard.Battle.View.IBattlePlayerView.OnCancelPlayCard { get => default!; set { } }
        Action global::Wizard.Battle.View.IBattlePlayerView.OnSelect { get => default!; set { } }
        Transform global::Wizard.Battle.View.IBattlePlayerView.ChoiceBraveButtonTransform { get => default!; }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsShowCantChoiceBraveText { get => default!; }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.Recovery(bool doseFirst, bool isFocusHand = true) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryTurnStart() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        IList<BattleCardBase> global::Wizard.Battle.View.IBattlePlayerView.GetSelectCardList() => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.ForceStopShowSelect() { }
        void global::Wizard.Battle.View.IBattlePlayerView.AllClear(bool popUpClose = false, bool isRemoveSideLog = true, bool isStopDrag = true, bool isResetDetail = true) { }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsTouchable() => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.LockOnEffectOff() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowCommonPanel() { }
        void global::Wizard.Battle.View.IBattlePlayerView.DragArrowStop(BattleManagerBase battleMgr) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.HandUnfocus() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.HandFocus() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        bool global::Wizard.Battle.View.IBattlePlayerView.ShowAlertMessageTouchCard(ref BattleCardBase hitCard, ref BattleManagerBase battleMgr) => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.DisableSettingFlag() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideAlertDialogue() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideAlertDialogue(PanelMgr.BattleAlertType alertType) { }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsShowingAlert() => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.ClearPlayQueue() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowAlert(PanelMgr.BattleAlertType AlertType, bool isClass, string text = null) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RearrangeHand() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.StopShowSelect(BattleCardBase actCard, bool isAct, bool isTransformskill = false, bool isNewReplayMoveTurn = false) { }
        void global::Wizard.Battle.View.IBattlePlayerView.RegisterPlayCard(BattleCardBase actCard) { }
        UIButton global::Wizard.Battle.View.IBattlePlayerView.GetChoiceButtonFromIndex(int index) => default!;
        GameObject global::Wizard.Battle.View.IBattlePlayerView.GetCheckFromIndex(int index) => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.SetTouchable(bool enable) { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideTurnEndButton() { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetCancelSkillChoiceTransformCards(BattleCardBase actCard, BattleCardBase transformCard) { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetCancelPlayChoiceTransformCards(BattleCardBase actCard, BattleCardBase transformCard) { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetCancelPlayCardWithChoice(BattleCardBase actCard, List<BattleCardBase> choiceCards) { }
        void global::Wizard.Battle.View.IBattlePlayerView.ReleaseLockOnTarget() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowChoiceAlert(BattleCardBase card, bool isEvolve, int count, int max) { }
        void global::Wizard.Battle.View.IBattlePlayerView.StopChoiceSelectUI() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideCommonPanel() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ClearSelectCardList() { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetSelectCardList(List<BattleCardBase> list) { }
        Vector3 global::Wizard.Battle.View.IBattlePlayerView.GetPPLabelPosition() => default!;
        Vector3 global::Wizard.Battle.View.IBattlePlayerView.GetBPLabelPosition() => default!;
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.CreateBeforeFusionVfx(BattleCardBase fusionCard, List<BattleCardBase> ingredientCards) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.ReturnActCardAfterFusion(IBattleCardView fusionCardView, bool isFusionMetamorphose = false) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        SideLogControl global::Wizard.Battle.View.IBattlePlayerView.GetSideLogControl(bool isSkillTargetSelect) => new SideLogControl(); // HEADLESS-FIX (M13): generator emitted default! (null); the emit path calls GetSideLogControl and enumerates the result.
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.SetIsNowTurnEnd(bool flg) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryInPlayCards() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryClassAndInPlayCardAttachSkillEffect() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryInHandCards() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryBattleUI() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.CreateStopAttackFloatVfx(IBattleCardView battleCardView) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.CreateStopShowSelectVfx(BattleCardBase actCard, bool isAct, bool stopChoiceSelectUiImmediately = true, bool isTransformskill = false, bool isNewReplayMoveTurn = false) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.ClearSelectSkillActCard() { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.StartShowSelect(BattleCardBase actCard, SkillBase skill, IEnumerable<BattleCardBase> selectableCards, bool isEvol) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.CancelPlayCard(BattleCardBase actCard, bool isPlay = false, bool isNewReplayMoveTurn = false) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.StartShowChoice(BattleCardBase actCard, SkillBase choiceSkill, List<BattleCardBase> choiceCards, bool isEvol, BattleCardBase accelerateCard, bool isChoiceBrave) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.StartShowFusionUI(BattleCardBase actCard, IEnumerable<BattleCardBase> selectableCards, int maxSelectCount, EventDelegate onClickDecision) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RemoveFusionSelectedCardFromHand(List<BattleCardBase> selectedCards) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.StopFusionUI() { }
        void global::Wizard.Battle.View.IBattlePlayerView.Setup(GameObject statusPanel, GameObject uiContainer, GameObject btlContainer, GameObject battle3DContainer) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryMulligan() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.PrepareCardsForAttackSequenceVfx(IBattleCardView attackInitiator, IBattleCardView attackTarget) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.SelectedFusionIngredientCard(int index, bool isActive, int maxSelectCount = 8) { }
        void global::Wizard.Battle.View.IBattlePlayerView.UpdateFusionUi(bool isTouchableDecisionButton) { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetNotCancelCollider(List<BattleCardBase> cards, bool isEnable) { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowChoiceSelectUI(BattleCardBase actCard, IList<BattleCardBase> choiceCards, SkillBase skill, bool isEvolve, bool isChoiceBrave) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.HideCardAttackEffects(IList<BattleCardBase> _targetCards) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.ShowChoiceBraveButton(bool isNewReplay) { }
        void global::Wizard.Battle.View.IBattlePlayerView.UpdateChoiceBraveActivatingEffect(bool isActivating) { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideChoiceBraveButton() { }
        void global::Wizard.Battle.View.IBattlePlayerView.UpdateChoiceBraveButtonPulsateEffectAndSprite() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideChoiceBraveButtonPulsateEffect() { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.SetBp(int num) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
    }
}
namespace Wizard.Battle.View {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Wizard.Battle.View.Vfx;
    public partial class BattleEnemyView : global::Wizard.Battle.View.IBattlePlayerView {
        ITurnEndButtonUI global::Wizard.Battle.View.IBattlePlayerView.TurnEndButtonUI { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.EpIcon { get => default!; }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsSelecting { get => default!; }
        // HEADLESS-FIX (M13): generator emitted `default!` (null); the OperateMgr emit path calls
        // BattleView.HandView.RemoveCardFromView (BattlePlayerBase.cs:1422). Redirect to a shared no-op
        // HandViewBase so the presentation call is a safe no-op (the played card is never in its list).
        HandViewBase global::Wizard.Battle.View.IBattlePlayerView.HandView { get => global::Wizard.Battle.View.HeadlessHandViewStub.Instance; }
        HandControl global::Wizard.Battle.View.IBattlePlayerView.HandControl { get => default!; }
        BattleCardBase global::Wizard.Battle.View.IBattlePlayerView.SelectSkillActCard { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.TurnEndBtn { get => default!; }
        BattleCardBase global::Wizard.Battle.View.IBattlePlayerView.m_CurrentTarget { get => default!; }
        // HEADLESS-FIX (M-HC-0b): generator emitted `default!` (null); the RECEIVE-conductor play path
        // calls BattleView.PlayQueueView.AddCardToViewVfx (OperateMgr.cs:201/203/219/221). Redirect to a
        // shared no-op PlayQueueViewBase so the presentation call is a safe NullVfx (the authoritative
        // play mutation runs in PlayHandCardReflection.Play, not in this view).
        PlayQueueViewBase global::Wizard.Battle.View.IBattlePlayerView.PlayQueueView { get => global::Wizard.Battle.View.HeadlessPlayQueueViewStub.Instance; }
        AttackSelectControl global::Wizard.Battle.View.IBattlePlayerView.AttackSelectControl { get => global::Wizard.Battle.View.HeadlessAttackSelectControl.Instance; } // HEADLESS-FIX (M-HC-4a): non-null no-op attack-select-control for the receive ATTACK path (RegisterPairToAttackSelectControl + ActionProcessor.Attack reset arm)
        InPlayViewBase global::Wizard.Battle.View.IBattlePlayerView.InPlayView { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.StatusParentPanel { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.AnchorL { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.CommonPanel { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.EpPanel { get => default!; }
        UIGrid global::Wizard.Battle.View.IBattlePlayerView.HandDeck { get => default!; }
        UIGrid global::Wizard.Battle.View.IBattlePlayerView.SetDeck { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.CemeteryParent { get => default!; }
        GameObject global::Wizard.Battle.View.IBattlePlayerView.BanishParent { get => default!; }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsNowTurnEnd { get => default!; }
        Action<bool> global::Wizard.Battle.View.IBattlePlayerView.OnCancelSkillTargetSelect { get => default!; set { } }
        Action<bool> global::Wizard.Battle.View.IBattlePlayerView.OnCancelPlayCard { get => default!; set { } }
        Action global::Wizard.Battle.View.IBattlePlayerView.OnSelect { get => default!; set { } }
        Transform global::Wizard.Battle.View.IBattlePlayerView.ChoiceBraveButtonTransform { get => default!; }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsShowCantChoiceBraveText { get => default!; }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.Recovery(bool doseFirst, bool isFocusHand = true) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryTurnStart() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        IList<BattleCardBase> global::Wizard.Battle.View.IBattlePlayerView.GetSelectCardList() => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.ForceStopShowSelect() { }
        void global::Wizard.Battle.View.IBattlePlayerView.AllClear(bool popUpClose = false, bool isRemoveSideLog = true, bool isStopDrag = true, bool isResetDetail = true) { }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsTouchable() => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.LockOnEffectOff() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowCommonPanel() { }
        void global::Wizard.Battle.View.IBattlePlayerView.DragArrowStop(BattleManagerBase battleMgr) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.HandUnfocus() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.HandFocus() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        bool global::Wizard.Battle.View.IBattlePlayerView.ShowAlertMessageTouchCard(ref BattleCardBase hitCard, ref BattleManagerBase battleMgr) => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.DisableSettingFlag() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideAlertDialogue() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideAlertDialogue(PanelMgr.BattleAlertType alertType) { }
        bool global::Wizard.Battle.View.IBattlePlayerView.IsShowingAlert() => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.ClearPlayQueue() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowAlert(PanelMgr.BattleAlertType AlertType, bool isClass, string text = null) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RearrangeHand() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.StopShowSelect(BattleCardBase actCard, bool isAct, bool isTransformskill = false, bool isNewReplayMoveTurn = false) { }
        void global::Wizard.Battle.View.IBattlePlayerView.RegisterPlayCard(BattleCardBase actCard) { }
        UIButton global::Wizard.Battle.View.IBattlePlayerView.GetChoiceButtonFromIndex(int index) => default!;
        GameObject global::Wizard.Battle.View.IBattlePlayerView.GetCheckFromIndex(int index) => default!;
        void global::Wizard.Battle.View.IBattlePlayerView.SetTouchable(bool enable) { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideTurnEndButton() { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetCancelSkillChoiceTransformCards(BattleCardBase actCard, BattleCardBase transformCard) { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetCancelPlayChoiceTransformCards(BattleCardBase actCard, BattleCardBase transformCard) { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetCancelPlayCardWithChoice(BattleCardBase actCard, List<BattleCardBase> choiceCards) { }
        void global::Wizard.Battle.View.IBattlePlayerView.ReleaseLockOnTarget() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowChoiceAlert(BattleCardBase card, bool isEvolve, int count, int max) { }
        void global::Wizard.Battle.View.IBattlePlayerView.StopChoiceSelectUI() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideCommonPanel() { }
        void global::Wizard.Battle.View.IBattlePlayerView.ClearSelectCardList() { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetSelectCardList(List<BattleCardBase> list) { }
        Vector3 global::Wizard.Battle.View.IBattlePlayerView.GetPPLabelPosition() => default!;
        Vector3 global::Wizard.Battle.View.IBattlePlayerView.GetBPLabelPosition() => default!;
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.CreateBeforeFusionVfx(BattleCardBase fusionCard, List<BattleCardBase> ingredientCards) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.ReturnActCardAfterFusion(IBattleCardView fusionCardView, bool isFusionMetamorphose = false) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        SideLogControl global::Wizard.Battle.View.IBattlePlayerView.GetSideLogControl(bool isSkillTargetSelect) => new SideLogControl(); // HEADLESS-FIX (M13): generator emitted default! (null); the emit path calls GetSideLogControl and enumerates the result.
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.SetIsNowTurnEnd(bool flg) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryInPlayCards() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryClassAndInPlayCardAttachSkillEffect() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryInHandCards() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryBattleUI() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.CreateStopAttackFloatVfx(IBattleCardView battleCardView) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.CreateStopShowSelectVfx(BattleCardBase actCard, bool isAct, bool stopChoiceSelectUiImmediately = true, bool isTransformskill = false, bool isNewReplayMoveTurn = false) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.ClearSelectSkillActCard() { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.StartShowSelect(BattleCardBase actCard, SkillBase skill, IEnumerable<BattleCardBase> selectableCards, bool isEvol) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.CancelPlayCard(BattleCardBase actCard, bool isPlay = false, bool isNewReplayMoveTurn = false) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.StartShowChoice(BattleCardBase actCard, SkillBase choiceSkill, List<BattleCardBase> choiceCards, bool isEvol, BattleCardBase accelerateCard, bool isChoiceBrave) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.StartShowFusionUI(BattleCardBase actCard, IEnumerable<BattleCardBase> selectableCards, int maxSelectCount, EventDelegate onClickDecision) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RemoveFusionSelectedCardFromHand(List<BattleCardBase> selectedCards) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.StopFusionUI() { }
        void global::Wizard.Battle.View.IBattlePlayerView.Setup(GameObject statusPanel, GameObject uiContainer, GameObject btlContainer, GameObject battle3DContainer) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.RecoveryMulligan() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.PrepareCardsForAttackSequenceVfx(IBattleCardView attackInitiator, IBattleCardView attackTarget) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.SelectedFusionIngredientCard(int index, bool isActive, int maxSelectCount = 8) { }
        void global::Wizard.Battle.View.IBattlePlayerView.UpdateFusionUi(bool isTouchableDecisionButton) { }
        void global::Wizard.Battle.View.IBattlePlayerView.SetNotCancelCollider(List<BattleCardBase> cards, bool isEnable) { }
        void global::Wizard.Battle.View.IBattlePlayerView.ShowChoiceSelectUI(BattleCardBase actCard, IList<BattleCardBase> choiceCards, SkillBase skill, bool isEvolve, bool isChoiceBrave) { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.HideCardAttackEffects(IList<BattleCardBase> _targetCards) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        void global::Wizard.Battle.View.IBattlePlayerView.ShowChoiceBraveButton(bool isNewReplay) { }
        void global::Wizard.Battle.View.IBattlePlayerView.UpdateChoiceBraveActivatingEffect(bool isActivating) { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideChoiceBraveButton() { }
        void global::Wizard.Battle.View.IBattlePlayerView.UpdateChoiceBraveButtonPulsateEffectAndSprite() { }
        void global::Wizard.Battle.View.IBattlePlayerView.HideChoiceBraveButtonPulsateEffect() { }
        VfxBase global::Wizard.Battle.View.IBattlePlayerView.SetBp(int num) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
    }
}
namespace Wizard.Battle.UI {
    using UnityEngine;
    using Wizard.Battle.View.Vfx;
}

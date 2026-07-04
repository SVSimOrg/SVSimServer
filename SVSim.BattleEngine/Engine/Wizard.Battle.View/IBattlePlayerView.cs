using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.View;

public interface IBattlePlayerView
{
	ITurnEndButtonUI TurnEndButtonUI { get; }

	GameObject EpIcon { get; }

	bool IsSelecting { get; }

	HandViewBase HandView { get; }

	HandControl HandControl { get; }

	BattleCardBase SelectSkillActCard { get; }

	GameObject TurnEndBtn { get; }

	BattleCardBase m_CurrentTarget { get; }

	PlayQueueViewBase PlayQueueView { get; }

	AttackSelectControl AttackSelectControl { get; }

	InPlayViewBase InPlayView { get; }

	GameObject StatusParentPanel { get; }

	GameObject AnchorL { get; }

	GameObject CommonPanel { get; }

	GameObject EpPanel { get; }

	UIGrid HandDeck { get; }

	UIGrid SetDeck { get; }

	GameObject CemeteryParent { get; }

	GameObject BanishParent { get; }

	bool IsNowTurnEnd { get; }

	Action<bool> OnCancelSkillTargetSelect { get; set; }

	Action<bool> OnCancelPlayCard { get; set; }

	Action OnSelect { get; set; }

	Transform ChoiceBraveButtonTransform { get; }

	bool IsShowCantChoiceBraveText { get; }

	VfxBase Recovery(bool doseFirst, bool isFocusHand = true);

	VfxBase RecoveryTurnStart();

	IList<BattleCardBase> GetSelectCardList();

	void ForceStopShowSelect();

	void AllClear(bool popUpClose = false, bool isRemoveSideLog = true, bool isStopDrag = true, bool isResetDetail = true);

	bool IsTouchable();

	void LockOnEffectOff();

	void ShowCommonPanel();

	void DragArrowStop(BattleManagerBase battleMgr);

	VfxBase HandUnfocus();

	VfxBase HandFocus();

	bool ShowAlertMessageTouchCard(ref BattleCardBase hitCard, ref BattleManagerBase battleMgr);

	void DisableSettingFlag();

	void HideAlertDialogue();

	void HideAlertDialogue(PanelMgr.BattleAlertType alertType);

	bool IsShowingAlert();

	void ClearPlayQueue();

	void ShowAlert(PanelMgr.BattleAlertType AlertType, bool isClass, string text = null);

	VfxBase RearrangeHand();

	void StopShowSelect(BattleCardBase actCard, bool isAct, bool isTransformskill = false, bool isNewReplayMoveTurn = false);

	void RegisterPlayCard(BattleCardBase actCard);

	UIButton GetChoiceButtonFromIndex(int index);

	GameObject GetCheckFromIndex(int index);

	void SetTouchable(bool enable);

	void HideTurnEndButton();

	void SetCancelSkillChoiceTransformCards(BattleCardBase actCard, BattleCardBase transformCard);

	void SetCancelPlayChoiceTransformCards(BattleCardBase actCard, BattleCardBase transformCard);

	void SetCancelPlayCardWithChoice(BattleCardBase actCard, List<BattleCardBase> choiceCards);

	void ReleaseLockOnTarget();

	void ShowChoiceAlert(BattleCardBase card, bool isEvolve, int count, int max);

	void StopChoiceSelectUI();

	void HideCommonPanel();

	void ClearSelectCardList();

	void SetSelectCardList(List<BattleCardBase> list);

	Vector3 GetPPLabelPosition();

	Vector3 GetBPLabelPosition();

	VfxBase CreateBeforeFusionVfx(BattleCardBase fusionCard, List<BattleCardBase> ingredientCards);

	VfxBase ReturnActCardAfterFusion(IBattleCardView fusionCardView, bool isFusionMetamorphose = false);

	SideLogControl GetSideLogControl(bool isSkillTargetSelect);

	VfxBase SetIsNowTurnEnd(bool flg);

	VfxBase RecoveryInPlayCards();

	VfxBase RecoveryClassAndInPlayCardAttachSkillEffect();

	VfxBase RecoveryInHandCards();

	VfxBase RecoveryBattleUI();

	VfxBase CreateStopAttackFloatVfx(IBattleCardView battleCardView);

	VfxBase CreateStopShowSelectVfx(BattleCardBase actCard, bool isAct, bool stopChoiceSelectUiImmediately = true, bool isTransformskill = false, bool isNewReplayMoveTurn = false);

	void ClearSelectSkillActCard();

	VfxBase StartShowSelect(BattleCardBase actCard, SkillBase skill, IEnumerable<BattleCardBase> selectableCards, bool isEvol);

	void CancelPlayCard(BattleCardBase actCard, bool isPlay = false, bool isNewReplayMoveTurn = false);

	VfxBase StartShowChoice(BattleCardBase actCard, SkillBase choiceSkill, List<BattleCardBase> choiceCards, bool isEvol, BattleCardBase accelerateCard, bool isChoiceBrave);

	void StartShowFusionUI(BattleCardBase actCard, IEnumerable<BattleCardBase> selectableCards, int maxSelectCount, EventDelegate onClickDecision);

	VfxBase RemoveFusionSelectedCardFromHand(List<BattleCardBase> selectedCards);

	void StopFusionUI();

	void Setup(GameObject statusPanel, GameObject uiContainer, GameObject btlContainer, GameObject battle3DContainer);

	VfxBase RecoveryMulligan();

	VfxBase PrepareCardsForAttackSequenceVfx(IBattleCardView attackInitiator, IBattleCardView attackTarget);

	void SelectedFusionIngredientCard(int index, bool isActive, int maxSelectCount = 8);

	void UpdateFusionUi(bool isTouchableDecisionButton);

	void SetNotCancelCollider(List<BattleCardBase> cards, bool isEnable);

	void ShowChoiceSelectUI(BattleCardBase actCard, IList<BattleCardBase> choiceCards, SkillBase skill, bool isEvolve, bool isChoiceBrave);

	VfxBase HideCardAttackEffects(IList<BattleCardBase> _targetCards);

	void ShowChoiceBraveButton(bool isNewReplay);

	void UpdateChoiceBraveActivatingEffect(bool isActivating);

	void HideChoiceBraveButton();

	void UpdateChoiceBraveButtonPulsateEffectAndSprite();

	void HideChoiceBraveButtonPulsateEffect();

	VfxBase SetBp(int num);
}

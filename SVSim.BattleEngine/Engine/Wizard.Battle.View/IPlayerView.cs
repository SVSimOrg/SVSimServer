using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.View;

public interface IPlayerView : IBattlePlayerView
{
	bool _isEvolutionSkillSelect { get; set; }

	bool IsEvolutionStart { get; set; }

	bool IsEvolutionVfx { get; set; }

	bool IsMenuOpen { get; set; }

	BattleCardBase DetailOpenCard { get; }

	bool CanPlayerEndTurnImmediately { get; }

	bool IsShowTurnEndDialogOfNotAttackingOrPlaying { get; }

	bool IsShowTurnEndDialogOfNotUsingHeroSkill { get; }

	bool IsMenuCloseEscape { get; set; }

	GameObject CardMoveEffect { get; set; }

	event Action OnRetire;

	event Func<bool> OnCheckImmediateTurnEnd;

	event Action OnStartMoveCard;

	event Action OnCancelMoveCard;

	event Action OnOpenEvolveDialoguePanel;

	event Action OnLockOn;

	event Action OnReleaseLockOn;

	event Action OnOpenDetailPanel;

	void HideDetailPanel();

	void ShowTurnEndDialog(GameObject return_obj = null);

	void UpdateTurnEndPulseEffect();

	void CallOnOpenEvolveDialoguePanel();

	void DragArrowStart(BattleManagerBase battleMgr, BattleCardBase attackCard, GameObject arrowHead);

	void DragArrowStart(BattleManagerBase battleMgr, GameObject startObject, GameObject arrowHead, bool isTargettingEnemy = true);

	void DragArrow(BattleManagerBase battleMgr, GameObject arrowHead, Vector3 pos);

	void ShowTurnEndButton(bool showEffect = true);

	void MoveCardCancel(BattleCardBase hitCard, Vector3 position, Quaternion rotation, bool IsPress);

	bool IsDetailOn();

	void MoveCardStart(BattleCardBase moveCard, bool isEffectAndSoundOn);

	void CancelCardDrag(BattleCardBase cardBeingDragged);

	void ShowDetailPanel(BattleManagerBase battleMgrBase, OperateMgr operateMgr, BattleCardBase card, DetailPanelControl.ShowRequest showRequest, BattleLogItem.CardTextureOption textureOption = BattleLogItem.CardTextureOption.Null, BuffInfo buff = null, string divergenceId = "", int logTextureId = 0);

	BattleCardBase GetDetailCard();

	void ResetTouchable();

	VfxBase HideTurnEndPulseEffect();

	bool IsMoving();

	void OffNotHideAndNotCreate();

	void ForceShowTurnEndButton();

	void ClearDifferentiatePopUp(List<BattlePlayerViewBase.BattleDialogItem> deselectionItem);

	void ShowPlayerTurnEnd(bool isAuto = false);

	void HideSubDetailPanel();

	void ShowKeyPanel(int page);

	void HideKeyPanel();

	DialogBase CreateKeyPanel(BattleCardBase card, UILabel label, CardMaster.CardMasterId cardMasterId, CardParameter baseParameter);

	DialogBase ShowRetireConfirmPanel();

	DialogBase CreateBattleSetting();

	void MoveCard(BattleCardBase hitCard, Vector3 pos);

	void CardMoveEffectSwitch(bool on);

	void SetDetailScreenPosition(bool right);

	Effect DetailPanelSelectEffectOn(BattleCardBase selectedCard, DetailPanelControl.ShowRequest request);

	void DetailPanelSelectEffectOff();

	void GetCardSelectedWithButton(Camera camera, ref UIButton button, ref BattleCardBase card, ref GameObject check);

	void ShowDetailPanelList(BattleManagerBase battleMgrBase, OperateMgr operateMgr, List<BattleCardBase> cards, DetailPanelControl.ShowRequest showRequest);

	void LockOnAttackTarget(BattleCardBase Attacker, BattleCardBase Target);

	bool IsFieldDetailOn();

	DialogBase ShowFusionCardPlayDialog(EventDelegate onClickOk, Action onClose);

	void HideModeEffect(bool on);

	void DetailReverseOver();

	void AddPopUpPanel(NonDialogPopup popup, BattlePlayerViewBase.BattleDialogItem item);
}

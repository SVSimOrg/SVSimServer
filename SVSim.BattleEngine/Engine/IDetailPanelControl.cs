using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public interface IDetailPanelControl
{
	bool IsShow { get; }

	BattleCardBase _card { get; }

	bool forceEvolutionConfirm { get; set; }

	UIButton EvolveButton { get; }

	GameObject EvoTargetPanelColliderGameObject { get; }

	DetailPanelControl.ShowRequest CurrentShowRequest { get; }

	EvolutionConfirmation _evolutionConfirmation { get; }

	event Action OnHideOneTime;

	void UpdateCardDescriptionOnEvent();

	void UpdateCardDescriptionOnEvolutionEvent();

	void Show(BattleManagerBase battleMgrBase, OperateMgr operateMgr, BattleCardBase card, DetailPanelControl.ShowRequest showRequest);

	void ShowList(BattleManagerBase battleMgrBase, OperateMgr operateMgr, List<BattleCardBase> cards, DetailPanelControl.ShowRequest showRequest, BuffInfo buff, BattleLogItem.CardTextureOption textureOption = BattleLogItem.CardTextureOption.Null, string divergenceId = "", int logTextureId = 0);

	void Hide();

	void SetSize(float percent);

	void UpdateBuffInfo(BattleCardBase targetCard, List<BattlePlayerBase.MyRotationBonusCondition> otationBonusList);

	void UpdateLogItemBuffInfo(BattleCardBase targetCard);

	void SetScreenPosition(bool right);

	VfxBase ShowEvolutionButton(BattleCardBase card);

	void CreateNextPanel();

	void SetKeyBtnActive(List<bool> hasKeyword);

	void ShowKeySubPanel(int page);

	void HideKeySubPanel();

	bool IsDisplayedRight();

	List<BuffInfo> GetDistinctBuffList(List<BuffInfo> buffInfoList);

	List<NetworkBattleReceiver.ReplayBuffInfoLabel> GetBuffDetailLabel(BattleCardBase targetCard);
}

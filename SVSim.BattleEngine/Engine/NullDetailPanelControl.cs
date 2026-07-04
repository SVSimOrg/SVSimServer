using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class NullDetailPanelControl : IDetailPanelControl
{
	public bool IsShow => false;

	public BattleCardBase _card => null;

	public bool forceEvolutionConfirm { get; set; }

	public UIButton EvolveButton => null;

	public DetailPanelControl.ShowRequest CurrentShowRequest { get; }

	public GameObject EvoTargetPanelColliderGameObject => null;

	public EvolutionConfirmation _evolutionConfirmation => null;

	public event Action OnHideOneTime
	{
		add
		{
		}
		remove
		{
		}
	}

	public void UpdateCardDescriptionOnEvent()
	{
	}

	public void UpdateCardDescriptionOnEvolutionEvent()
	{
	}

	public void Show(BattleManagerBase battleMgrBase, OperateMgr operateMgr, BattleCardBase card, DetailPanelControl.ShowRequest showRequest)
	{
	}

	public void ShowList(BattleManagerBase battleMgrBase, OperateMgr operateMgr, List<BattleCardBase> cards, DetailPanelControl.ShowRequest showRequest, BuffInfo buff, BattleLogItem.CardTextureOption textureOption = BattleLogItem.CardTextureOption.Null, string divergenceId = "", int logTextureId = 0)
	{
	}

	public void Hide()
	{
	}

	public void SetSize(float percent)
	{
	}

	public void UpdateBuffInfo(BattleCardBase targetCard, List<BattlePlayerBase.MyRotationBonusCondition> myRotationBonusList)
	{
	}

	public void UpdateLogItemBuffInfo(BattleCardBase targetCard)
	{
	}

	public void SetScreenPosition(bool right)
	{
	}

	public VfxBase ShowEvolutionButton(BattleCardBase card)
	{
		return NullVfx.GetInstance();
	}

	public void CreateNextPanel()
	{
	}

	public void SetKeyBtnActive(List<bool> hasKeyword)
	{
	}

	public void ShowKeySubPanel(int page)
	{
	}

	public void HideKeySubPanel()
	{
	}

	public bool IsDisplayedRight()
	{
		return false;
	}

	public List<BuffInfo> GetDistinctBuffList(List<BuffInfo> buffInfoList)
	{
		return new List<BuffInfo>();
	}

	public List<NetworkBattleReceiver.ReplayBuffInfoLabel> GetBuffDetailLabel(BattleCardBase targetCard)
	{
		return new List<NetworkBattleReceiver.ReplayBuffInfoLabel>();
	}
}

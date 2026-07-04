using UnityEngine;
using Wizard.Battle.View.Vfx;

public class NullStatusPanelControl : IStatusPanelControl
{
	public Vector3 EpPanelOffScreenPosition { get; }

	public void SetUp(BattleManagerBase battleMgr)
	{
	}

	public void ShowStatus(bool isNewReplayMoveTurn)
	{
	}

	public void ShowPpEp(bool isNotEPMax3, bool fixDirection = false, bool isNewReplay = false, bool isBanmenkun = false)
	{
	}

	public void HideUI()
	{
	}

	public void SetDeck(int num)
	{
	}

	public void SetGrave(int num)
	{
	}

	public void SetHandCount(int num)
	{
	}

	public void ShowStatusPanelAlways()
	{
	}

	public void HideStatusPanelAlways()
	{
	}

	public void ShowStatusPanelOnBattle()
	{
	}

	public void SetPp(int num, int max, bool isNewReplayMoveTurn = false)
	{
	}

	public void PlayIncreasePpAnimation(int oldPp, int newPp)
	{
	}

	public void SetEp(int evo, int cnt)
	{
	}

	public VfxBase PlayIncreaseMaxEpAnimation(int oldMaxEp, int newEp)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase PlayIncreaseUsableEpAnimation(int oldUsableEpAmount, int amountOfUsableEpGained, int maxEp)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase PlayDecreaseUsableEpAnimation(int oldUsableEpAmount, int usedEp)
	{
		return NullVfx.GetInstance();
	}

	public GameObject GetPPPanel()
	{
		return null;
	}

	public GameObject GetEPIcon()
	{
		return null;
	}

	public Transform GetClassInfoAnchor()
	{
		return null;
	}
}

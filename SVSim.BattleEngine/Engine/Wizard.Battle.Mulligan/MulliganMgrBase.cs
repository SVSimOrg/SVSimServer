using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 13 of 18 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.MulliganMgrBase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Mulligan;

public abstract class MulliganMgrBase : IMulliganMgr
{
	protected OpponentMulliganCtrl _opponentMulliganControl;

	private Coroutine mulliganTimeoutCoroutine;

	public PlayerMulliganCtrl PlayerMlgCtrl { get; protected set; }

	public OpponentMulliganCtrl OpponentMlgCtrl => _opponentMulliganControl;

	public Action OnSubmit { get; set; }

	public VfxBase StartDeal(List<int> playerDealIdxList, List<int> oppoDealIdxList, SkillProcessor skillProcessor)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(InstantVfx.Create(StopTimeout));
		PlayerMlgCtrl.DealIdxList = playerDealIdxList;
		_opponentMulliganControl.DealIdxList = oppoDealIdxList;
		PlayerMlgCtrl.CreateMulliganDealList(playerDealIdxList);
		_opponentMulliganControl.CreateMulliganDealList(oppoDealIdxList);
		VfxBase instance = NullVfx.GetInstance();
		VfxBase instance2 = NullVfx.GetInstance();
		instance = PlayerMlgCtrl.StartMulliganVfx(skillProcessor);
		instance2 = _opponentMulliganControl.StartMulliganVfx(skillProcessor);
		parallelVfxPlayer.Register(instance);
		parallelVfxPlayer.Register(instance2);
		if (false /* Pre-Phase-5b: recovery+mulligan-end guard headless-safe as false */)
		{
			return NullVfx.GetInstance();
		}
		return parallelVfxPlayer;
	}

	protected virtual void StartTimeout()
	{
		StopTimeout();
		mulliganTimeoutCoroutine = BattleCoroutine.GetInstance().StartCoroutine(MulliganNetworkTimeout());
	}

	protected virtual void StopTimeout()
	{
		if (mulliganTimeoutCoroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(mulliganTimeoutCoroutine);
			mulliganTimeoutCoroutine = null;
		}
		/* Pre-Phase-5b: HideAlertDialogue dropped; no BattleView headless */
	}

	private IEnumerator MulliganNetworkTimeout()
	{
		long matchedTimer = TimeUtil.GetAbsoluteTime().Ticks;
		do
		{
			yield return null;
			if (false /* Pre-Phase-5b: IsBattleEnd guard on coroutine timeout; unreachable headless */)
			{
				StopTimeout();
				yield break;
			}
		}
		while (!((float)NetworkUtility.GetTimeSpanSecond(matchedTimer) >= 5f));
		/* Pre-Phase-5b: ShowAlert dropped; no BattleView headless */
	}

	public virtual VfxBase Submit(BattleManagerBase m_BtlMgrIns)
	{
		OnSubmit.Call();
		return NullVfx.GetInstance();
	}

	private void AddBattleLogMulliganResult(BattleManagerBase battleMgr)
	{
		BattleLogManager instance = BattleLogManager.GetInstance();
		instance.AddLogMulliganChanged(battleMgr.BattlePlayer, PlayerMlgCtrl.GetChangedNum());
		instance.AddLogMulliganChanged(battleMgr.BattleEnemy, OpponentMlgCtrl.GetChangedNum());
	}

	public virtual VfxBase PlayerChangeCardVfx(BattleManagerBase btlMgrIns)
	{
		VfxBase result = PlayerMlgCtrl.SubmitMulliganVfx(PlayerMlgCtrl.AbandonList);
		List<int> completeCards = btlMgrIns.BattlePlayer.HandCardList.Select((BattleCardBase c) => c.Index).ToList();
		btlMgrIns.BattlePlayer.CallRecordingMulligan(PlayerMlgCtrl.AbandonList, completeCards);
		btlMgrIns.BattlePlayer.CallRecordingMulliganEnd(btlMgrIns.BattlePlayer.HandCardList);
		return result;
	}

	public abstract VfxBase EnemyChangeCardVfx(BattleManagerBase btlMgrIns);

	public virtual VfxBase CompleteMulligan(BattleManagerBase battleMgr)
	{
		if (!battleMgr.IsVirtualBattle && !battleMgr.GameMgr.IsNewReplayBattle)
		{
			AddBattleLogMulliganResult(battleMgr);
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase InitMulligan(BattleManagerBase mgr, MulliganInfoControl mulliganInfo, IPlayerView view)
	{
		PlayerMlgCtrl = new PlayerMulliganCtrl(mgr.BattlePlayer, mulliganInfo, view);
		_opponentMulliganControl = new OpponentMulliganCtrl(mgr.BattleEnemy, mulliganInfo, isUseExchangeMark: false);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase MulliganStartDraw(bool firstAttack, SkillProcessor skillProcessor)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		VfxBase instance = NullVfx.GetInstance();
		VfxBase instance2 = NullVfx.GetInstance();
		if (firstAttack)
		{
			instance = PlayerMlgCtrl.StartMulliganVfx(skillProcessor);
			instance2 = _opponentMulliganControl.StartMulliganVfx(skillProcessor);
		}
		else
		{
			instance2 = _opponentMulliganControl.StartMulliganVfx(skillProcessor);
			instance = PlayerMlgCtrl.StartMulliganVfx(skillProcessor);
		}
		parallelVfxPlayer.Register(instance);
		parallelVfxPlayer.Register(instance2);
		return parallelVfxPlayer;
	}

	public virtual VfxBase RecoverMulligan(bool didPlayerSubmitMulligan, BattleManagerBase battleMgr)
	{
		return NullVfx.GetInstance();
	}

	public MulliganInfoControl GetMulliganInfo()
	{
		return PlayerMlgCtrl.GetMulliganInfo();
	}
}

using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 3 of 9 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.NetworkMulliganMgr
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Mulligan;

public class NetworkMulliganMgr : MulliganMgrBase
{
	private NetworkBattleManagerBase _networkBattleManager;

	private readonly NetworkBattleSender _networkBattleSender;

	public NetworkMulliganMgr(NetworkBattleSender sender)
	{
		_networkBattleSender = sender;
	}

	public override VfxBase InitMulligan(BattleManagerBase mgr, MulliganInfoControl mulliganInfo, IPlayerView view)
	{
		_networkBattleManager = mgr as NetworkBattleManagerBase;
		base.PlayerMlgCtrl = new NetworkPlayerMulliganCtrl(mgr.BattlePlayer, mulliganInfo, view);
		_opponentMulliganControl = new NetworkOpponentMulliganCtrl(mgr.BattleEnemy, mulliganInfo, isUseExchangeMark: false);
		return NullVfx.GetInstance();
	}

	public override VfxBase MulliganStartDraw(bool firstAttack, SkillProcessor skillProcessor)
	{
		if (_networkBattleManager.OperateReceive.DealVfx != null)
		{
			return _networkBattleManager.OperateReceive.DealVfx;
		}
		_networkBattleSender.SendDeal();
		return InstantVfx.Create(StartTimeout);
	}

	public override VfxBase Submit(BattleManagerBase m_BtlMgrIns)
	{
		List<int> swapIndexList = base.PlayerMlgCtrl.AbandonList.Select((BattleCardBase x) => x.Index).ToList();
		_networkBattleSender.SendSwapInfo(swapIndexList);
		StartTimeout();
		return base.Submit(m_BtlMgrIns);
	}

	public void SetPlayerHandCardIndexList(List<int> list)
	{
		base.PlayerMlgCtrl.SetMulliganAfterCardIndexList(list);
	}

	public void SetOpponentMulliganAfterCardIndexList(List<int> list)
	{
		_opponentMulliganControl.SetMulliganAfterCardIndexList(list);
	}

	public override VfxBase PlayerChangeCardVfx(BattleManagerBase btlMgrIns)
	{
		StopTimeout();
		if (!btlMgrIns.IsRecovery)
		{
			return base.PlayerChangeCardVfx(btlMgrIns);
		}
		List<BattleCardBase> retCardList = null;
		List<int> retPosList = null;
		base.PlayerMlgCtrl.GetAbandonCardList(btlMgrIns, ref retCardList, ref retPosList);
		btlMgrIns.BattlePlayer.CallRecordingMulligan(retCardList, new int[0]);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(base.PlayerMlgCtrl.SubmitMulliganVfx(retCardList));
		btlMgrIns.BattlePlayer.CallRecordingMulliganEnd(btlMgrIns.BattlePlayer.HandCardList);
		return sequentialVfxPlayer;
	}

	public override VfxBase EnemyChangeCardVfx(BattleManagerBase btlMgrIns)
	{
		List<BattleCardBase> retCardList = null;
		List<int> retPosList = null;
		_opponentMulliganControl.GetAbandonCardList(btlMgrIns, ref retCardList, ref retPosList);
		_opponentMulliganControl.DrawFirstMulliganCard();
		btlMgrIns.BattleEnemy.CallRecordingMulligan(retCardList, _opponentMulliganControl.GetMulliganAfterCardIndexList());
		btlMgrIns.BattleEnemy.CallRecordingMulliganEnd(_opponentMulliganControl.GetMulliganAfterCardIndexList());
		return _opponentMulliganControl.SubmitMulliganVfx(retCardList);
	}

	public override VfxBase CompleteMulligan(BattleManagerBase battleMgr)
	{
		LocalLog.AccumulateLastTraceLog("CompleteMulligan");
		VfxBase vfx = base.CompleteMulligan(battleMgr);
		battleMgr.TouchControl.ResetDetail();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(vfx);
		sequentialVfxPlayer.Register(battleMgr.StartBattle());
		return sequentialVfxPlayer;
	}

	public override VfxBase RecoverMulligan(bool didPlayerSubmitMulligan, BattleManagerBase battleMgr)
	{
		if (!didPlayerSubmitMulligan)
		{
			return NullVfx.GetInstance();
		}
		MulliganInfoControl mulliganInfo = GetMulliganInfo();
		List<BattleCardBase> firstDrawList = base.PlayerMlgCtrl.GetFirstDrawList();
		List<BattleCardBase> stockList = base.PlayerMlgCtrl.GetStockList();
		base.OnSubmit.Call();
		mulliganInfo.HideButtons();
		if (stockList.Count < 3)
		{
			mulliganInfo.GetAbandonZonePlayer().alpha = 0f;
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<BattleCardBase> handCardList = base.PlayerMlgCtrl.GetBattlePlayer().HandCardList;
		for (int i = 0; i < handCardList.Count; i++)
		{
			if (firstDrawList[i] != handCardList[i])
			{
				BattleCardBase battleCardBase = firstDrawList[i];
				battleCardBase.BattleCardView.GameObject.SetActive(value: false);
				battleCardBase.BattleCardView.Transform.SetParent(battleMgr.CardHolder.transform);
				battleCardBase.BattleCardView.Transform.position = battleMgr.CardHolder.transform.position;
				list.Add(handCardList[i]);
			}
			IBattleCardView battleCardView = handCardList[i].BattleCardView;
			battleCardView.Transform.SetParent(mulliganInfo.GetKeepZonePlayer().gameObject.transform);
			battleCardView.Transform.localPosition = mulliganInfo.GetMulliganZoneCardPos(i, isAbandon: false, handCardList[i].IsPlayer);
			battleCardView.Transform.localScale = mulliganInfo.GetMulliganZoneCardScale();
			battleCardView.Transform.localRotation = Quaternion.Euler(MulliganViewBase.CARD_ROTATION);
			battleCardView.GameObject.SetActive(value: true);
		}
		VfxBase dummyDeckRemoveCardVfx = NullVfx.GetInstance();
		return ParallelVfxPlayer.Create(dummyDeckRemoveCardVfx, _networkBattleManager.LoadCardResources(list));
	}
}

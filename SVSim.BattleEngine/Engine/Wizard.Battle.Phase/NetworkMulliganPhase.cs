using System;
using System.Collections.Generic;
using Cute;
using Wizard.Battle.Mulligan;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 6 of 11 methods unrun in baseline
//   Type: Wizard.Battle.Phase.NetworkMulliganPhase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Phase;

public class NetworkMulliganPhase : MulliganPhaseBase
{
	protected readonly NetworkBattleManagerBase _networkBattleMgr;

	protected readonly NetworkMulliganMgr _networkMulliganMgr;

	protected readonly SingleMulliganMgr _singleMulliganMgr;

	private Action OnNetworkAlive;

	public event Func<VfxBase> OnEndMulligan;

	public NetworkMulliganPhase(NetworkBattleManagerBase battleMgr, NetworkBattleSender sender)
		: base(battleMgr)
	{
		_networkBattleMgr = battleMgr;
		if (_networkBattleMgr.GameMgr.IsAINetwork)
		{
			_singleMulliganMgr = new SingleMulliganMgr();
		}
		else
		{
			_networkMulliganMgr = new NetworkMulliganMgr(sender);
		}
		IMulliganMgr mulliganMgr;
		if (!_networkBattleMgr.GameMgr.IsAINetwork)
		{
			IMulliganMgr networkMulliganMgr = _networkMulliganMgr;
			mulliganMgr = networkMulliganMgr;
		}
		else
		{
			IMulliganMgr networkMulliganMgr = _singleMulliganMgr;
			mulliganMgr = networkMulliganMgr;
		}
		Initialize(mulliganMgr);
	}

	public override VfxBase Setup()
	{
		VfxBase result = base.Setup();
		MulliganEventSetting();
		if (!_networkBattleMgr.IsRecovery)
		{
			SetUpSubmitEvent();
		}
		return result;
	}

	protected void SetUpSubmitEvent()
	{
		IMulliganMgr mulliganMgr = _mulliganMgr;
		mulliganMgr.OnSubmit = (Action)Delegate.Combine(mulliganMgr.OnSubmit, (Action)delegate
		{
			if (_networkBattleMgr.GameMgr.IsAINetwork)
			{
				SingleMulliganMgr singleMulligan = _mulliganMgr as SingleMulliganMgr;
				OnNetworkAlive = (Action)Delegate.Combine(OnNetworkAlive, (Action)delegate
				{
					singleMulligan.AIMulliganEndAction(_networkBattleMgr);
					OnNetworkAlive = null;
				});
			}
		});
	}

	public override VfxWith<IPhase> Update(float dt)
	{
		if (_networkBattleMgr.GameMgr.IsAINetwork && _networkBattleMgr.InstanceNetworkAgent != null && _networkBattleMgr.InstanceNetworkAgent.PlayerNetworkStatus.IsAlive)
		{
			OnNetworkAlive.Call();
		}
		return base.Update(dt);
	}

	public void MulliganEventSetting()
	{
		if (!_networkBattleMgr.GameMgr.IsAINetwork)
		{
			OperateReceive operateReceive = _networkBattleMgr.OperateReceive;
			operateReceive.OnEndMulligan = (Func<VfxBase>)Delegate.Combine(operateReceive.OnEndMulligan, new Func<VfxBase>(EndMulligan));
			OperateReceive operateReceive2 = _networkBattleMgr.OperateReceive;
			operateReceive2.OnReceiveDeal = (Action<List<int>, List<int>>)Delegate.Combine(operateReceive2.OnReceiveDeal, new Action<List<int>, List<int>>(base.StartDeal));
			OperateReceive operateReceive3 = _networkBattleMgr.OperateReceive;
			operateReceive3.OnReceivePlayerMulligan = (Func<List<int>, VfxBase>)Delegate.Combine(operateReceive3.OnReceivePlayerMulligan, new Func<List<int>, VfxBase>(ReceivePlayerMulligan));
			OperateReceive operateReceive4 = _networkBattleMgr.OperateReceive;
			operateReceive4.OnReceiveOpponentMulligan = (Func<List<int>, VfxBase>)Delegate.Combine(operateReceive4.OnReceiveOpponentMulligan, new Func<List<int>, VfxBase>(ReceiveOpponentMulligan));
		}
	}

	public override VfxBase Teardown()
	{
		VfxBase result = base.Teardown();
		if (_networkBattleMgr.GameMgr.IsAINetwork)
		{
			return result;
		}
		OperateReceive operateReceive = _networkBattleMgr.OperateReceive;
		operateReceive.OnEndMulligan = (Func<VfxBase>)Delegate.Remove(operateReceive.OnEndMulligan, new Func<VfxBase>(EndMulligan));
		OperateReceive operateReceive2 = _networkBattleMgr.OperateReceive;
		operateReceive2.OnReceiveDeal = (Action<List<int>, List<int>>)Delegate.Remove(operateReceive2.OnReceiveDeal, new Action<List<int>, List<int>>(base.StartDeal));
		OperateReceive operateReceive3 = _networkBattleMgr.OperateReceive;
		operateReceive3.OnReceivePlayerMulligan = (Func<List<int>, VfxBase>)Delegate.Remove(operateReceive3.OnReceivePlayerMulligan, new Func<List<int>, VfxBase>(ReceivePlayerMulligan));
		OperateReceive operateReceive4 = _networkBattleMgr.OperateReceive;
		operateReceive4.OnReceiveOpponentMulligan = (Func<List<int>, VfxBase>)Delegate.Remove(operateReceive4.OnReceiveOpponentMulligan, new Func<List<int>, VfxBase>(ReceiveOpponentMulligan));
		return result;
	}

	private VfxBase EndMulligan()
	{
		LocalLog.AccumulateLastTraceLog("EndMulligan");
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(this.OnEndMulligan.GetAllFuncVfxResults());
		sequentialVfxPlayer.Register(_networkBattleMgr.GameMgr.IsAINetwork ? _singleMulliganMgr.CompleteMulligan(_networkBattleMgr) : _networkMulliganMgr.CompleteMulligan(_networkBattleMgr));
		return sequentialVfxPlayer;
	}

	protected virtual VfxBase ReceivePlayerMulligan(List<int> mulliganAfterCardIndexes)
	{
		if (_networkBattleMgr.GameMgr.IsAINetwork)
		{
			return NullVfx.GetInstance();
		}
		_networkMulliganMgr.SetPlayerHandCardIndexList(mulliganAfterCardIndexes);
		return _networkMulliganMgr.PlayerChangeCardVfx(_networkBattleMgr);
	}

	protected VfxBase ReceiveOpponentMulligan(List<int> mulliganAfterCardIndexes)
	{
		LocalLog.AccumulateLastTraceLog("ReceiveOpponentMulligan");
		if (_networkBattleMgr.GameMgr.IsAINetwork)
		{
			return NullVfx.GetInstance();
		}
		_networkMulliganMgr.SetOpponentMulliganAfterCardIndexList(mulliganAfterCardIndexes);
		VfxBase vfx = _networkMulliganMgr.EnemyChangeCardVfx(_networkBattleMgr);
		_networkBattleMgr.ClearRegisterCardList();
		OnEndMulligan += () => vfx;
		return NullVfx.GetInstance();
	}
}

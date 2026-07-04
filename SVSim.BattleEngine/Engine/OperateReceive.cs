using System;
using System.Collections.Generic;
using Cute;
using Wizard;
using Wizard.Battle.View.Vfx;

public class OperateReceive
{
	protected NetworkBattleManagerBase _battleMgr;

	protected OperateMgr _operateMgr;

	protected PlayHandCardReflection _networkPlayCardAction;

	protected InPlayCardReflection _networkInPlayAction;

	protected NetworkBattleData _networkBattleData;

	private RegisterActionManager _registerActionList;

	private NetworkBattleDefine.NetworkBattleURI _lastReceivedUri;

	private long _lastReceivedTick;

	public Func<List<int>, VfxBase> OnReceivePlayerMulligan;

	public Func<List<int>, VfxBase> OnReceiveOpponentMulligan;

	public Func<VfxBase> OnEndMulligan;

	private static int _lastSpinCount = -1;

	public Action<List<int>, List<int>> OnReceiveDeal { get; set; }

	public VfxBase DealVfx { get; set; }

	public OperateReceive(NetworkBattleManagerBase networkBattleMgr, RegisterActionManager registerCardList, OperateMgr operateMgr, NetworkBattleData networkBattleData)
	{
		_battleMgr = networkBattleMgr;
		_registerActionList = registerCardList;
		_operateMgr = operateMgr;
		_networkBattleData = networkBattleData;
		_networkPlayCardAction = CreateNetworkPlayCardAction();
		_networkInPlayAction = CreateNetworkInPlayAction();
		OnEndMulligan = (Func<VfxBase>)Delegate.Combine(OnEndMulligan, (Func<VfxBase>)delegate
		{
			_battleMgr.IsMulliganEnd = true;
			return NullVfx.GetInstance();
		});
	}

	protected virtual PlayHandCardReflection CreateNetworkPlayCardAction()
	{
		return new PlayHandCardReflection(_battleMgr, _operateMgr, _networkBattleData);
	}

	protected virtual InPlayCardReflection CreateNetworkInPlayAction()
	{
		return new InPlayCardReflection(_battleMgr, _operateMgr);
	}

	public virtual void StartOperate(NetworkOperationCollectionBase networkOperationCollection, NetworkBattleReceiver.ReceiveData receivedData)
	{
		networkOperationCollection.WriteOperationToTraceLog();
		if (receivedData.spin > 0)
		{
			if (receivedData.spin == _lastSpinCount)
			{
				LocalLog.AccumulateLastTraceLog("Received same spin count. " + _lastSpinCount);
			}
			_lastSpinCount = receivedData.spin;
		}
		for (int i = 0; i < receivedData.spin; i++)
		{
			_battleMgr.StableRandomDouble();
		}
		receivedData.spin = 0;
		switch (receivedData.dataUri)
		{
		case NetworkBattleDefine.NetworkBattleURI.Retry:
			networkOperationCollection.RetryOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.Deal:
			networkOperationCollection.DealOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.Swap:
			networkOperationCollection.SwapOperation(OnReceiveOpponentMulligan, OnReceivePlayerMulligan);
			break;
		case NetworkBattleDefine.NetworkBattleURI.Ready:
			networkOperationCollection.SecondMulliganOperation(OnReceiveOpponentMulligan, OnReceivePlayerMulligan, OnEndMulligan);
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnStart:
		{
			long num = NetworkUtility.GetTimeSpanSecond(_lastReceivedTick);
			networkOperationCollection.TurnStartOperation(_lastReceivedUri, (int)num);
			break;
		}
		case NetworkBattleDefine.NetworkBattleURI.TurnEnd:
			networkOperationCollection.TurnEndOperation(_networkPlayCardAction, _networkInPlayAction);
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnEndActions:
			networkOperationCollection.TurnEndWithSkillActivationOperation(_networkPlayCardAction, _networkInPlayAction);
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnEndFinal:
			networkOperationCollection.TurnEndFinalOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.Judge:
			networkOperationCollection.JudgeOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.PlayActions:
			_registerActionList.SetBeforeOpponentHandCardList(_battleMgr);
			networkOperationCollection.InPlayActionOperation(_networkPlayCardAction, _networkInPlayAction);
			break;
		case NetworkBattleDefine.NetworkBattleURI.Retire:
			networkOperationCollection.RetireOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.ChatStamp:
			networkOperationCollection.ChatStampOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.End:
			networkOperationCollection.DataInconsistencyBattleEndOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.Touch:
			networkOperationCollection.TouchOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.SelectSkill:
			networkOperationCollection.SelectSkillOperation(_networkPlayCardAction, _networkInPlayAction);
			break;
		case NetworkBattleDefine.NetworkBattleURI.SelectObject:
			networkOperationCollection.SelectObjectOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.TurnEndReady:
			networkOperationCollection.TurnEndReady();
			break;
		case NetworkBattleDefine.NetworkBattleURI.SlideObject:
			networkOperationCollection.SlideObject();
			break;
		case NetworkBattleDefine.NetworkBattleURI.BattleFinish:
		case NetworkBattleDefine.NetworkBattleURI.ReplayFinish:
			networkOperationCollection.BattleFinishOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.Maintenance:
			networkOperationCollection.MaintenanceOperation();
			break;
		case NetworkBattleDefine.NetworkBattleURI.JudgeResult:
			networkOperationCollection.JudgeResultOperation();
			break;
		}
		networkOperationCollection.SendEcho();
		_lastReceivedUri = receivedData.dataUri;
		_lastReceivedTick = TimeUtil.GetAbsoluteTime().Ticks;
	}

	public virtual void RecordSelectSkillInRecovery(NetworkBattleReceiver.ReceiveData receiveData)
	{
	}

	public virtual void CheckLatestReplayInfoInRecovery()
	{
	}

	public ReceivePlayActionsReflectionBase GetPlayActionsReflection(bool isEvolve)
	{
		if (isEvolve)
		{
			return _networkInPlayAction;
		}
		return _networkPlayCardAction;
	}
}

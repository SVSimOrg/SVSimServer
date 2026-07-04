using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public abstract class NetworkOperationCollectionBase
{
	protected readonly NetworkBattleManagerBase _networkBattleMgr;

	protected readonly OperateMgr _operateMgr;

	protected readonly NetworkBattleReceiver.ReceiveData _receivedData;

	protected readonly NetworkBattleData _networkBattleData;

	protected readonly bool _isPlayer;

	public NetworkOperationCollectionBase(NetworkBattleManagerBase networkBattleMgr, OperateMgr operateMgr, NetworkBattleReceiver.ReceiveData receivedData, NetworkBattleData networkBattleData, bool isPlayer)
	{
		_networkBattleMgr = networkBattleMgr;
		_operateMgr = operateMgr;
		_receivedData = receivedData;
		_networkBattleData = networkBattleData;
		_isPlayer = isPlayer;
	}

	public abstract void RetryOperation();

	public virtual void DealOperation()
	{
		if (_networkBattleMgr.OperateReceive.OnReceiveDeal != null)
		{
			_networkBattleMgr.OperateReceive.OnReceiveDeal(_receivedData.selfIdxList, _receivedData.oppoIdxList);
			return;
		}
		_networkBattleMgr.OperateReceive.DealVfx = InstantVfx.Create(delegate
		{
			_networkBattleMgr.OperateReceive.OnReceiveDeal(_receivedData.selfIdxList, _receivedData.oppoIdxList);
		});
	}

	public abstract void SwapOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan);

	public abstract void SecondMulliganOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan, Func<VfxBase> OnEndMulligan);

	public abstract void TurnStartOperation(NetworkBattleDefine.NetworkBattleURI lastReceivedUri, int lastReceivedTime);

	public abstract void TurnEndOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction);

	public abstract void TurnEndFinalOperation();

	public abstract void TurnEndWithSkillActivationOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction);

	public abstract void JudgeOperation();

	public abstract void PlayHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null, bool isChoice = false);

	public virtual void FusionCardOperation(PlayHandCardReflection playHandReceiver, bool isPlayer, List<NetworkBattleReceiver.TargetData> actionDictionary)
	{
		BattlePlayerBase battlePlayer = _networkBattleMgr.GetBattlePlayer(_isPlayer);
		playHandReceiver.ReadySetting(_receivedData.playCardIndex, actionDictionary);
		_networkBattleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			playHandReceiver.FusionMove(battlePlayer);
		}));
	}

	public void ChoiceBraveOperation(PlayHandCardReflection playHandReceiver, List<int> choiceIdList)
	{
		BattlePlayerBase battlePlayer = _networkBattleMgr.GetBattlePlayer(_isPlayer);
		playHandReceiver.ReadySetting(0, _isPlayer ? _receivedData.PlayerTargetDataList : _receivedData.OpponentTargetDataList);
		_networkBattleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			playHandReceiver.ChoiceBraveMove(battlePlayer, choiceIdList);
		}));
	}

	protected void CommonPlayHandCardOperation(PlayHandCardReflection playHandReceiver, BattlePlayerBase actionPlayer, bool isPlayer, List<NetworkBattleReceiver.TargetData> actionDictionary, List<int> choiceIdList, bool isChoice)
	{
		if (actionPlayer.Turn <= 1 && !isPlayer)
		{
			LocalLog.AccumulateLastTraceLog("Play699661");
		}
		SetupNetworkPlayCardAction(playHandReceiver, actionDictionary);
		_networkBattleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			playHandReceiver.Play(actionPlayer, isPlayer, choiceIdList, isChoice);
		}));
	}

	public abstract void PlaySkillSelectHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null);

	public abstract void InPlayActionOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction);

	public abstract void RetireOperation();

	public abstract void ChatStampOperation();

	public abstract void DataInconsistencyBattleEndOperation();

	public abstract void TouchOperation();

	public abstract void SelectSkillOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction);

	public abstract void SelectObjectOperation();

	public abstract void TurnEndReady();

	public abstract void SlideObject();

	public abstract void BattleFinishOperation();

	public abstract void MaintenanceOperation();

	public abstract void JudgeResultOperation();

	public virtual void WriteOperationToTraceLog()
	{
	}

	public virtual void SendEcho()
	{
	}

	protected virtual void RegisterSequentialVfx(VfxBase operationVfx)
	{
		_networkBattleMgr.VfxMgr.RegisterSequentialVfx(operationVfx);
	}

	protected virtual int GetPlayedCardIndex()
	{
		return _networkBattleData.GetPlayCard().Index;
	}

	protected void SetupNetworkPlayCardAction(PlayHandCardReflection networkPlayCardAction, List<NetworkBattleReceiver.TargetData> targetDataList)
	{
		int playedCardIndex = GetPlayedCardIndex();
		networkPlayCardAction.ReadySetting(playedCardIndex, targetDataList);
	}

	protected VfxBase OperatePlayerMulligan(NetworkBattleReceiver.ReceiveData receiveData, Func<List<int>, VfxBase> OnReceivePlayerMulligan)
	{
		if (_networkBattleData.isPlayerMulliganEnd)
		{
			return NullVfx.GetInstance();
		}
		if (receiveData.selfIdxList != null && receiveData.selfIdxList.Count >= 1)
		{
			_networkBattleData.isPlayerMulliganEnd = true;
			return OnReceivePlayerMulligan.GetAllFuncVfxResults(receiveData.selfIdxList);
		}
		return NullVfx.GetInstance();
	}

	protected virtual VfxBase OperateOppoMulligan(NetworkBattleReceiver.ReceiveData receiveData, Func<List<int>, VfxBase> OnReceiveOpponentMulligan)
	{
		LocalLog.AccumulateLastTraceLog("OperateOppoMulligan");
		if (_networkBattleData.isOppoMulliganEnd)
		{
			return NullVfx.GetInstance();
		}
		if (receiveData.oppoIdxList != null && receiveData.oppoIdxList.Count >= 1)
		{
			if (OnReceiveOpponentMulligan == null || OnReceiveOpponentMulligan.GetInvocationList() == null || OnReceiveOpponentMulligan.GetInvocationList().Count() == 0)
			{
				LocalLog.AccumulateLastTraceLog("No OppoMulligan delegate func");
				return NullVfx.GetInstance();
			}
			_networkBattleData.isOppoMulliganEnd = true;
			return OnReceiveOpponentMulligan.GetAllFuncVfxResults(receiveData.oppoIdxList);
		}
		LocalLog.AccumulateLastTraceLog("idx is null or empty");
		return NullVfx.GetInstance();
	}

	protected void JudgeEndTypeToLose(NetworkBattleReceiver.JudgeEndType battleEndType)
	{
		switch (battleEndType)
		{
		case NetworkBattleReceiver.JudgeEndType.JUDGE:
		case NetworkBattleReceiver.JudgeEndType.FIRSTCARD:
			_networkBattleMgr.ReceiveConsistencyLose();
			break;
		case NetworkBattleReceiver.JudgeEndType.VALIDATE:
			_networkBattleMgr.ReceiveInvalidLose();
			break;
		}
	}
}

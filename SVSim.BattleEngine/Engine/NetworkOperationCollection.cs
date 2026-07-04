using System;
using System.Collections.Generic;
using Wizard;
using Wizard.Battle.View.Vfx;

public class NetworkOperationCollection : NetworkOperationCollectionBase
{
	private bool _sendEcho;

	private bool _isTurnStart;

	private int _playCardIndexToEcho = -1;

	private NetworkBattleDefine.PlayActionType _actionTypeToEcho;

	private bool _isReceivedBattleFinish;

	public NetworkOperationCollection(NetworkBattleManagerBase networkBattleMgr, OperateMgr operateMgr, NetworkBattleReceiver.ReceiveData receivedData, NetworkBattleData networkBattleData, bool isPlayer)
		: base(networkBattleMgr, operateMgr, receivedData, networkBattleData, isPlayer)
	{
	}

	public override void RetryOperation()
	{
		_networkBattleMgr.NetworkSender.EmitRetry(_receivedData.receiveUri);
	}

	public override void SwapOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan)
	{
		RegisterSequentialVfx(OperatePlayerMulligan(_receivedData, OnReceivePlayerMulligan));
	}

	public override void SecondMulliganOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan, Func<VfxBase> OnEndMulligan)
	{
		LocalLog.AccumulateLastTraceLog("SecondMulliganOperation" + _networkBattleData.isOppoMulliganEnd);
		RegisterSequentialVfx(OperatePlayerMulligan(_receivedData, OnReceivePlayerMulligan));
		RegisterSequentialVfx(OperateOppoMulligan(_receivedData, OnReceiveOpponentMulligan));
		if (_networkBattleData.isOppoMulliganEnd)
		{
			RegisterSequentialVfx(OnEndMulligan.GetAllFuncVfxResults());
		}
	}

	public override void TurnStartOperation(NetworkBattleDefine.NetworkBattleURI lastReceivedUri, int lastReceivedTime)
	{
		if (_networkBattleMgr.InstanceNetworkAgent.GetTurnState() || _networkBattleMgr.BattleEnemy.IsExtraTurn || _networkBattleData.isEnemyFirstTurn)
		{
			if (_networkBattleMgr.BattleEnemy.IsExtraTurn)
			{
				RegisterSequentialVfx(_networkBattleMgr.ControlTurnStartPlayer());
			}
			else
			{
				RegisterSequentialVfx(_networkBattleMgr.ControlTurnStartOpponent());
			}
			_sendEcho = true;
			_isTurnStart = true;
		}
		if (lastReceivedUri == NetworkBattleDefine.NetworkBattleURI.Judge && _networkBattleData.GetReceiveData().NodeResultCode == NetworkBattleDefine.ReceiveNodeResultCode.CurrentBattleError)
		{
			string[] obj = new string[6]
			{
				"#732070_",
				(_isReceivedBattleFinish ? 1 : 0).ToString(),
				"_",
				null,
				null,
				null
			};
			int result = (int)_receivedData.result;
			obj[3] = result.ToString();
			obj[4] = "_";
			obj[5] = lastReceivedTime.ToString();
			LocalLog.AccumulateTraceLog(string.Concat(obj));
		}
	}

	public override void TurnEndWithSkillActivationOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
		if (_networkBattleData.nowReceiveUri != NetworkBattleDefine.NetworkBattleURI.TurnEndActions)
		{
			_networkBattleData.isReceiveTurnEndAction = true;
			RegisterSequentialVfx(_operateMgr.TurnEndOperation(isPlayer: false));
			_sendEcho = true;
		}
	}

	public override void TurnEndOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
		if (_networkBattleData.nowReceiveUri != NetworkBattleDefine.NetworkBattleURI.TurnEnd)
		{
			if (_networkBattleMgr.JudgeCurrentFinishStatus() == NetworkBattleReceiver.RESULT_CODE.NotFinish && !_networkBattleData.isReceiveTurnEndAction)
			{
				RegisterSequentialVfx(_operateMgr.TurnEndOperation(isPlayer: false));
			}
			_networkBattleData.isReceiveTurnEndAction = false;
			_networkBattleMgr.SendJudge();
		}
		else
		{
			LocalLog.AccumulateLastTraceLog("AlreadyTurnEndSend");
		}
	}

	public override void TurnEndFinalOperation()
	{
		_networkBattleMgr.SendJudge();
	}

	public override void JudgeOperation()
	{
		if (_networkBattleMgr.JudgeCurrentFinishStatus() != NetworkBattleReceiver.RESULT_CODE.NotFinish)
		{
			_networkBattleMgr.BattleFinishReceiveAfterFinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.BattleFinishToJudge);
		}
		else
		{
			RegisterSequentialVfx(_networkBattleMgr.ControlTurnStartPlayer());
		}
	}

	public override void PlayHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null, bool isChoice = false)
	{
		CommonPlayHandCardOperation(networkPlayCardAction, _networkBattleMgr.BattleEnemy, isPlayer: false, _receivedData.OpponentTargetDataList, choiceIdList, isChoice);
	}

	public override void PlaySkillSelectHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null)
	{
		SetupNetworkPlayCardAction(networkPlayCardAction, _receivedData.OpponentTargetDataList);
		RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			networkPlayCardAction.PlayAction(isPlayer: false, choiceIdList);
		}));
	}

	public override void InPlayActionOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
		LocalLog.AccumulateLastTraceLog("PlayActionsReceive");
		switch (_receivedData.actionType)
		{
		case NetworkBattleDefine.PlayActionType.PLAY_HAND:
			if (!_receivedData.IsChoiceBrave)
			{
				PlayHandCardOperation(networkPlayCardAction, _receivedData.choiceIdList, _receivedData.IsChoice);
			}
			else
			{
				ChoiceBraveOperation(networkPlayCardAction, _receivedData.choiceIdList);
			}
			break;
		case NetworkBattleDefine.PlayActionType.PLAY_HAND_SELECT:
			if (!_receivedData.IsChoiceBrave)
			{
				PlaySkillSelectHandCardOperation(networkPlayCardAction, _receivedData.choiceIdList);
			}
			else
			{
				ChoiceBraveOperation(networkPlayCardAction, _receivedData.choiceIdList);
			}
			break;
		case NetworkBattleDefine.PlayActionType.ATTACK:
		case NetworkBattleDefine.PlayActionType.EVOLUTION:
		case NetworkBattleDefine.PlayActionType.EVOLUTION_SELECT:
			networkInPlayAction.ReadySetting(_receivedData.OpponentTargetDataList, _receivedData.actionType, _receivedData.playCardIndex);
			networkInPlayAction.Play(_isPlayer, _receivedData.choiceIdList, _receivedData.IsChoice);
			_sendEcho = true;
			_playCardIndexToEcho = _receivedData.playCardIndex;
			break;
		case NetworkBattleDefine.PlayActionType.FUSION:
			FusionCardOperation(networkPlayCardAction, isPlayer: false, _receivedData.OpponentTargetDataList);
			break;
		}
		_actionTypeToEcho = _receivedData.actionType;
	}

	public override void RetireOperation()
	{
		_networkBattleMgr.ReceiveRetire(_receivedData.isWin);
	}

	public override void ChatStampOperation()
	{
		ClassCharaPrm.EmotionType oppoChatStamp = (ClassCharaPrm.EmotionType)_receivedData.oppoChatStamp;
		if (!ClassCharaPrm.IsEvolutionEmotionType(oppoChatStamp))
		{
			VfxBase vfx = _networkBattleMgr.BattleEnemy.Emotion.PlayEmotion(oppoChatStamp, 1.5f);
		}
	}

	public override void DataInconsistencyBattleEndOperation()
	{
		JudgeEndTypeToLose(_receivedData.judgeEndType);
	}

	public override void TouchOperation()
	{
	}

	public override void SelectSkillOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
	}

	public override void SelectObjectOperation()
	{
	}

	public override void TurnEndReady()
	{
	}

	public override void SlideObject()
	{
	}

	public override void BattleFinishOperation()
	{
		_isReceivedBattleFinish = true;
		_networkBattleMgr.JudgeResultReceive(_receivedData.result);
	}

	public override void MaintenanceOperation()
	{
	}

	public override void JudgeResultOperation()
	{
		_networkBattleMgr.JudgeResultReceive(_receivedData.result);
	}

	public override void WriteOperationToTraceLog()
	{
		LocalLog.AccumulateLastTraceLog("StartOperateURI:" + _receivedData.dataUri);
	}

	public override void SendEcho()
	{
		if (_sendEcho)
		{
			_networkBattleMgr.SendEcho(_playCardIndexToEcho, _actionTypeToEcho, isNotActiveSeq: false, _isTurnStart);
		}
	}
}

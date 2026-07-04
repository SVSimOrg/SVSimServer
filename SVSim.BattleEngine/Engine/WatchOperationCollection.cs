using System;
using System.Collections.Generic;
using Wizard;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class WatchOperationCollection : NetworkOperationCollectionBase
{
	private NetworkWatchBattleMgr _watchBattleMgr;

	private int _lastIndex;

	public WatchOperationCollection(NetworkWatchBattleMgr watchBattleMgr, OperateMgr operateMgr, NetworkBattleReceiver.ReceiveData receivedData, NetworkBattleData networkBattleData, bool isPlayer)
		: base(watchBattleMgr, operateMgr, receivedData, networkBattleData, isPlayer)
	{
		_watchBattleMgr = watchBattleMgr;
	}

	public WatchOperationCollection(NetworkBattleManagerBase networkBattleMgr, OperateMgr operateMgr, NetworkBattleReceiver.ReceiveData receivedData, NetworkBattleData networkBattleData, bool isPlayer)
		: base(networkBattleMgr, operateMgr, receivedData, networkBattleData, isPlayer)
	{
	}

	public override void RetryOperation()
	{
	}

	public override void SwapOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan)
	{
		OperateMulligan(OnReceiveOpponentMulligan, OnReceivePlayerMulligan);
	}

	public override void SecondMulliganOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan, Func<VfxBase> OnEndMulligan)
	{
		OperateMulligan(OnReceiveOpponentMulligan, OnReceivePlayerMulligan);
		RegisterSequentialVfx(OnEndMulligan.GetAllFuncVfxResults());
	}

	protected void OperateMulligan(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan)
	{
		if (_isPlayer)
		{
			RegisterSequentialVfx(OperatePlayerMulligan(_receivedData, OnReceivePlayerMulligan));
		}
		else
		{
			RegisterSequentialVfx(OperateOppoMulligan(_receivedData, OnReceiveOpponentMulligan));
		}
	}

	public override void TurnStartOperation(NetworkBattleDefine.NetworkBattleURI lastReceivedUri, int lastReceivedTime)
	{
		if (_isPlayer)
		{
			RegisterSequentialVfx(_networkBattleMgr.ControlTurnStartPlayer());
		}
		else
		{
			RegisterSequentialVfx(_networkBattleMgr.ControlTurnStartOpponent());
		}
	}

	public override void TurnEndOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
		CheckStateAndCancel(networkPlayCardAction, networkInPlayAction, _isPlayer);
		if (!_networkBattleData.isReceiveTurnEndAction)
		{
			RegisterSequentialVfx(_operateMgr.TurnEndOperation(_isPlayer));
		}
		_networkBattleData.isReceiveTurnEndAction = false;
	}

	public override void TurnEndFinalOperation()
	{
	}

	public override void TurnEndWithSkillActivationOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
		CheckStateAndCancel(networkPlayCardAction, networkInPlayAction, _isPlayer);
		_networkBattleData.isReceiveTurnEndAction = true;
		RegisterSequentialVfx(ParallelVfxPlayer.Create(_operateMgr.TurnEndOperation(_isPlayer), InstantVfx.Create(delegate
		{
			TurnEndTimeController turnEndTimeController = _networkBattleMgr.turnEndTimeController;
			if (turnEndTimeController != null)
			{
				if (turnEndTimeController.IsCountdownRunning())
				{
					turnEndTimeController.EndCountDown("watchTurnEndWithSkillActivationOperation");
				}
				_watchBattleMgr.SlideObjectReceiveCtrl.CancelSlide();
			}
		})));
	}

	public override void JudgeOperation()
	{
	}

	public override void PlayHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null, bool isChoice = false)
	{
		List<NetworkBattleReceiver.TargetData> targetDataList = (_isPlayer ? _receivedData.PlayerTargetDataList : _receivedData.OpponentTargetDataList);
		SetupNetworkPlayCardAction(networkPlayCardAction, targetDataList);
		BattlePlayerBase battlePlayer = _networkBattleMgr.GetBattlePlayer(_isPlayer);
		RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			BattleCardBase playedCard = networkPlayCardAction.Play(battlePlayer, _isPlayer, choiceIdList, isChoice);
			HideDetailPanelOfPlayedCard(playedCard);
		}));
	}

	public override void PlaySkillSelectHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null)
	{
		List<NetworkBattleReceiver.TargetData> targetDataList = (_isPlayer ? _receivedData.PlayerTargetDataList : _receivedData.OpponentTargetDataList);
		SetupNetworkPlayCardAction(networkPlayCardAction, targetDataList);
		RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			BattleCardBase playedCard = networkPlayCardAction.PlayAction(_isPlayer, choiceIdList);
			IPlayerView playerBattleView = _networkBattleMgr.BattlePlayer.PlayerBattleView;
			if (playerBattleView.DetailOpenCard != null && !playerBattleView.DetailOpenCard.IsClass)
			{
				HideDetailPanelOfPlayedCard(playedCard);
			}
		}));
	}

	private void HideDetailPanelOfPlayedCard(BattleCardBase playedCard)
	{
		IPlayerView playerBattleView = _networkBattleMgr.BattlePlayer.PlayerBattleView;
		if (playedCard == playerBattleView.DetailOpenCard && playerBattleView.DetailOpenCard != null && !playerBattleView.DetailOpenCard.IsClass)
		{
			_networkBattleMgr.BattlePlayer.PlayerBattleView.HideDetailPanel();
		}
	}

	public override void InPlayActionOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
		LocalLog.AccumulateLastTraceLog("PlayActionsReceive");
		CheckStateAndCancel(networkPlayCardAction, networkInPlayAction, _isPlayer);
		switch (_receivedData.actionType)
		{
		case NetworkBattleDefine.PlayActionType.PLAY_HAND:
			if (_receivedData.IsChoiceBrave && (_watchBattleMgr.IsRecovery || (!_receivedData.isSelf && !_watchBattleMgr.GameMgr.IsAdminWatch)))
			{
				ChoiceBraveOperation(networkPlayCardAction, _receivedData.choiceIdList);
			}
			else
			{
				PlayHandCardOperation(networkPlayCardAction, _receivedData.choiceIdList, _receivedData.IsChoice || _receivedData.IsChoiceBrave);
			}
			CallCompleteEvent(networkPlayCardAction);
			break;
		case NetworkBattleDefine.PlayActionType.PLAY_HAND_SELECT:
			if (_receivedData.IsChoiceBrave && (_watchBattleMgr.IsRecovery || (!_receivedData.isSelf && !_watchBattleMgr.GameMgr.IsAdminWatch)))
			{
				ChoiceBraveOperation(networkPlayCardAction, _receivedData.choiceIdList);
			}
			else
			{
				PlaySkillSelectHandCardOperation(networkPlayCardAction, _receivedData.choiceIdList);
			}
			CallCompleteEvent(networkPlayCardAction);
			break;
		case NetworkBattleDefine.PlayActionType.ATTACK:
		case NetworkBattleDefine.PlayActionType.EVOLUTION:
		case NetworkBattleDefine.PlayActionType.EVOLUTION_SELECT:
		{
			PlayCancelSlide();
			List<NetworkBattleReceiver.TargetData> action = (_isPlayer ? _receivedData.PlayerTargetDataList : _receivedData.OpponentTargetDataList);
			networkInPlayAction.ReadySetting(action, _receivedData.actionType, _receivedData.playCardIndex);
			networkInPlayAction.Play(_isPlayer, _receivedData.choiceIdList, _receivedData.IsChoice);
			CallCompleteEvent(networkInPlayAction);
			break;
		}
		case NetworkBattleDefine.PlayActionType.FUSION:
			FusionCardOperation(networkPlayCardAction, _isPlayer, _isPlayer ? _receivedData.PlayerTargetDataList : _receivedData.OpponentTargetDataList);
			CallCompleteEvent(networkPlayCardAction);
			break;
		}
	}

	protected virtual void CallCompleteEvent(ReceivePlayActionsReflectionBase networkAction)
	{
		if (networkAction.CompleteSelectDataIns != null)
		{
			_networkBattleMgr.VfxMgr.RegisterSequentialVfx((networkAction.CompleteSelectDataIns.PlayCardVfx == null) ? NullVfx.GetInstance() : networkAction.CompleteSelectDataIns.PlayCardVfx);
			networkAction.CompleteSelectDataIns = null;
		}
		if (networkAction.CompleteChoiceDataIns != null)
		{
			_networkBattleMgr.VfxMgr.RegisterSequentialVfx((networkAction.CompleteChoiceDataIns.PlayCardVfx == null) ? NullVfx.GetInstance() : networkAction.CompleteChoiceDataIns.PlayCardVfx);
			networkAction.CompleteChoiceDataIns = null;
		}
		networkAction.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
	}

	protected virtual void PlayCancelSlide()
	{
		_watchBattleMgr.SlideObjectReceiveCtrl.CancelSlide();
	}

	public override void RetireOperation()
	{
		_networkBattleMgr.ReceiveRetire(_isPlayer);
	}

	public override void ChatStampOperation()
	{
		ClassCharaPrm.EmotionType result2;
		if (_isPlayer)
		{
			if (Enum.TryParse<ClassCharaPrm.EmotionType>(_receivedData.playChatStamp.ToString(), out var result) && !ClassCharaPrm.IsEvolutionEmotionType(result))
			{
				VfxBase vfx = _networkBattleMgr.BattlePlayer.Emotion.PlayEmotion(result, 1.5f);
				_networkBattleMgr.VfxMgr.RegisterSequentialVfx(vfx);
			}
		}
		else if (Enum.TryParse<ClassCharaPrm.EmotionType>(_receivedData.oppoChatStamp.ToString(), out result2) && !ClassCharaPrm.IsEvolutionEmotionType(result2))
		{
			VfxBase vfx2 = _networkBattleMgr.BattleEnemy.Emotion.PlayEmotion(result2, 1.5f);
			_networkBattleMgr.VfxMgr.RegisterSequentialVfx(vfx2);
		}
	}

	public override void DataInconsistencyBattleEndOperation()
	{
		JudgeEndTypeToLose(_receivedData.judgeEndType);
	}

	public override void TouchOperation()
	{
		if (_lastIndex != _receivedData.idx)
		{
			bool isSelf = _receivedData.isSelf;
			_lastIndex = _receivedData.idx;
			BattleCardBase battleCardBase = _networkBattleMgr.GetBattlePlayer(isSelf).HandCardList.Find((BattleCardBase c) => c.Index == _lastIndex);
			if (battleCardBase != null && battleCardBase.BattleCardView.GameObject != null)
			{
				_networkBattleMgr.VfxMgr.RegisterSequentialVfx(_networkBattleMgr.GetBattlePlayer(isSelf).BattleView.HandView.AsyncTouchCard(battleCardBase.BattleCardView.GameObject));
				_networkBattleMgr.VfxMgr.RegisterSequentialVfx(WaitVfx.Create(0.3f));
			}
		}
	}

	public override void SelectSkillOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
		ReceivePlayActionsReflectionBase receivePlayActionsReflectionBase = ((!_receivedData._isEvolveTargetSelect) ? ((ReceivePlayActionsReflectionBase)networkPlayCardAction) : ((ReceivePlayActionsReflectionBase)networkInPlayAction));
		switch (_receivedData._selectSkillOperation)
		{
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartSelect:
			CheckStateAndCancel(networkPlayCardAction, networkInPlayAction, _receivedData.isSelf);
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.SELECT;
			_watchBattleMgr.SlideObjectReceiveCtrl.CancelSlide();
			receivePlayActionsReflectionBase.StartSelect(_receivedData.idx, _receivedData.isSelf);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectCard:
			receivePlayActionsReflectionBase.SelectCard(_receivedData._selectedCardIndex, IsTargetSelf(), _receivedData._isEvolveTargetSelect, _receivedData.isSelf, _receivedData._isBurialRiteSelect, isChoiceBrave: false, isComplete: false);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteSelect:
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
			receivePlayActionsReflectionBase.CompleteSelectCard(_receivedData._selectedCardIndex, IsTargetSelf(), _receivedData._isEvolveTargetSelect, _receivedData.isSelf, _receivedData._isBurialRiteSelect, _receivedData.IsChoiceBraveSelect);
			_watchBattleMgr.GetBattlePlayer(_isPlayer).BattleView.ClearSelectSkillActCard();
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CancelSelect:
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
			receivePlayActionsReflectionBase.CancelSelect(_receivedData.isSelf);
			_watchBattleMgr.GetBattlePlayer(_isPlayer).BattleView.ClearSelectSkillActCard();
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartChoiceSelect:
			CheckStateAndCancel(networkPlayCardAction, networkInPlayAction, _receivedData.isSelf);
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.CHOICE;
			_watchBattleMgr.SlideObjectReceiveCtrl.CancelSlide();
			receivePlayActionsReflectionBase.StartChoiceSelect(_receivedData.idx, _receivedData.isSelf);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectChoiceCard:
			receivePlayActionsReflectionBase.WatchSelectChoiceCards(_receivedData._selectedChoiceCardIdList, _receivedData._isEvolveTargetSelect, _receivedData.isSelf);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteChoiceSelect:
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
			receivePlayActionsReflectionBase.CompleteChoiceCard(_receivedData._selectedChoiceCardIdList, _receivedData._isEvolveTargetSelect, _receivedData.isSelf);
			receivePlayActionsReflectionBase.WatchSelectChoiceCards(_receivedData._selectedChoiceCardIdList, _receivedData._isEvolveTargetSelect, _receivedData.isSelf, isComplete: true);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CancelChoiceSelect:
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
			receivePlayActionsReflectionBase.CancelChoiceSelect(_receivedData.isSelf);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartFusionSelect:
			CheckStateAndCancel(networkPlayCardAction, networkInPlayAction, _receivedData.isSelf);
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.FUSION;
			_watchBattleMgr.SlideObjectReceiveCtrl.CancelSlide();
			receivePlayActionsReflectionBase.StartSelectFusion(_receivedData.idx, _receivedData.isSelf);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectFusionIngredient:
			receivePlayActionsReflectionBase.SelectFusionIngredientCard(_receivedData._selectedCardIndex, IsTargetSelf());
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteFusionSelect:
			receivePlayActionsReflectionBase.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
			receivePlayActionsReflectionBase.CompleteSelectFusionIngredientCard(_isPlayer);
			_watchBattleMgr.GetBattlePlayer(_isPlayer).BattleView.ClearSelectSkillActCard();
			break;
		default:
			Debug.LogError("Invalid Select Skill Operation");
			break;
		}
	}

	protected virtual void CheckStateAndCancel(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction, bool isPlayer)
	{
		if (networkPlayCardAction.CompleteSelectDataIns == null && networkPlayCardAction.CompleteChoiceDataIns == null && networkInPlayAction.CompleteSelectDataIns == null && networkInPlayAction.CompleteChoiceDataIns == null)
		{
			switch (networkPlayCardAction.CurrentState)
			{
			case ReceivePlayActionsReflectionBase.SelectChoiceState.SELECT:
			case ReceivePlayActionsReflectionBase.SelectChoiceState.FUSION:
				networkPlayCardAction.CancelSelect(isPlayer);
				break;
			case ReceivePlayActionsReflectionBase.SelectChoiceState.CHOICE:
				networkPlayCardAction.CancelChoiceSelect(isPlayer);
				break;
			}
			switch (networkInPlayAction.CurrentState)
			{
			case ReceivePlayActionsReflectionBase.SelectChoiceState.SELECT:
				networkInPlayAction.CancelSelect(isPlayer);
				break;
			case ReceivePlayActionsReflectionBase.SelectChoiceState.CHOICE:
				networkInPlayAction.CancelChoiceSelect(isPlayer);
				break;
			}
			networkPlayCardAction.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
			networkInPlayAction.CurrentState = ReceivePlayActionsReflectionBase.SelectChoiceState.NONE;
		}
	}

	private bool IsTargetSelf()
	{
		if (_receivedData.isSelf)
		{
			if (_receivedData._isPlayerCard)
			{
				return true;
			}
			return false;
		}
		if (_receivedData._isPlayerCard)
		{
			return false;
		}
		return true;
	}

	public override void SelectObjectOperation()
	{
		bool flag = _receivedData._isPlayerCard;
		if (!_receivedData.isSelf)
		{
			flag = !flag;
		}
		BattlePlayerBase battlePlayer = _networkBattleMgr.GetBattlePlayer(flag);
		switch (_receivedData._selectObjectTargetType)
		{
		case NetworkBattleSender.SELECT_OBJECT_TARGET_TYPE.Deselect:
			_watchBattleMgr.ToggleSelectHandCardMove(null, _receivedData.isSelf);
			break;
		case NetworkBattleSender.SELECT_OBJECT_TARGET_TYPE.Select:
		{
			BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_networkBattleMgr, battlePlayer, _receivedData.idx);
			if (indexToCardBase != null && battlePlayer.HandCardList.Contains(indexToCardBase) && !_networkBattleMgr.IsSkillSelectTiming)
			{
				_watchBattleMgr.ToggleSelectHandCardMove(indexToCardBase, _receivedData.isSelf);
			}
			break;
		}
		default:
			Debug.LogError("Invalid Select Object Target Type: " + _receivedData._selectObjectTargetType);
			break;
		}
	}

	public override void TurnEndReady()
	{
		if (!_receivedData._isNotTurnEndReady)
		{
			TurnEndTimeController turnEndTimeController = _networkBattleMgr.turnEndTimeController;
			turnEndTimeController.EndCountDown("WatchTurnEndReady");
			turnEndTimeController.StartCountDown("WatchTurnEndReady");
			float timeLeftLong = PlayerStaticData.UserTime.GetTimeLeftLong(_receivedData._timeSent);
			turnEndTimeController.SetExtendTime(20f - timeLeftLong - turnEndTimeController.GetMaxSecond());
		}
	}

	public override void SlideObject()
	{
		_watchBattleMgr.SlideObjectReceiveCtrl.SlideObjectReceiveAction(_receivedData);
	}

	public override void BattleFinishOperation()
	{
		NetworkBattleReceiver.RESULT_CODE rESULT_CODE = _receivedData.result;
		if (IsResultNotFinish(rESULT_CODE))
		{
			rESULT_CODE = _receivedData.opponentResult;
			switch (rESULT_CODE)
			{
			case NetworkBattleReceiver.RESULT_CODE.LifeWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.LifeLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.DeckoutWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.DeckoutLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.RetireWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.RetireLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.SpecialWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.SpecialLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.DisconnectWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.DisconnectLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.FirstcardWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.FirstcardLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.TurnendWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.TurnendLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.TurnstartWin:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.TurnstartLose;
				break;
			case NetworkBattleReceiver.RESULT_CODE.LifeLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.LifeWin;
				break;
			case NetworkBattleReceiver.RESULT_CODE.DeckoutLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.DeckoutWin;
				break;
			case NetworkBattleReceiver.RESULT_CODE.RetireLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.RetireWin;
				break;
			case NetworkBattleReceiver.RESULT_CODE.SpecialLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.SpecialWin;
				break;
			case NetworkBattleReceiver.RESULT_CODE.DisconnectLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.DisconnectWin;
				break;
			case NetworkBattleReceiver.RESULT_CODE.FirstcardLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.FirstcardWin;
				break;
			case NetworkBattleReceiver.RESULT_CODE.TurnendLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.TurnendWin;
				break;
			case NetworkBattleReceiver.RESULT_CODE.TurnstartLose:
				rESULT_CODE = NetworkBattleReceiver.RESULT_CODE.TurnstartWin;
				break;
			}
		}
		_networkBattleMgr.JudgeResultReceive(rESULT_CODE, isNotStopCoroutine: true);
		_networkBattleMgr.BattleFinishReceiveAfterFinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.WatchJudgeResult, _isPlayer);
		LocalLog.SendLastTraceLog(null);
	}

	public override void MaintenanceOperation()
	{
		_networkBattleMgr.InstanceNetworkAgent.StopNetworkBattle();
		_networkBattleMgr.InstanceNetworkAgent.CallMaintenanceError();
		_networkBattleMgr.InstanceNetworkAgent.DestroyObj(RealTimeNetworkAgent.DESTROY_OBJECT_LOG.WatchMaintenance);
	}

	public override void JudgeResultOperation()
	{
		if (!IsResultNotFinish(_receivedData.result) || !IsResultNotFinish(_receivedData.opponentResult))
		{
			BattleFinishOperation();
		}
	}

	private bool IsResultNotFinish(NetworkBattleReceiver.RESULT_CODE result)
	{
		if (result == NetworkBattleReceiver.RESULT_CODE.NotFinish || result == NetworkBattleReceiver.RESULT_CODE.Error)
		{
			return true;
		}
		return false;
	}

	protected override int GetPlayedCardIndex()
	{
		return _networkBattleData.GetPlayCardIndex();
	}

	public override void SendEcho()
	{
		_networkBattleMgr.ClearRegisterCardList();
	}
}

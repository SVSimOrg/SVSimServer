using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;

public class NetworkBattleSender
{
	public class EmitData
	{
		public string Uri { get; private set; }

		public Dictionary<string, object> Data { get; private set; }

		public int SeqNumber { get; private set; }

		public EmitData(string uri, Dictionary<string, object> data, int seqNumber)
		{
			Uri = uri;
			Data = data;
			SeqNumber = seqNumber;
		}
	}

	public enum JUDGE_RESULT_STATUS
	{
		BattleFinishToJudge = 100,
		RetrySend = 200,
		FailedToRetryJudgeResult = 201,
		OppoDisconnectVictory = 300,
		DisconnectLose = 301,
		ServerSendDisconnectCheck = 302,
		OpponentNotTurnStartVictory = 400,
		TurnStartLose = 401,
		OpponentNotTurnEndVictory = 500,
		TurnEndLose = 501,
		OppoNotMulliganVictory = 600,
		MulliganLose = 601,
		BattleStopToJudgeResult = 700,
		ReceiveRetire = 800,
		ReceiveConsistencyLose = 900,
		Invalid = 901,
		WatchJudgeResult = 1000}

	public enum SELECT_SKILL_OPERATION
	{
		StartSelect,
		SelectCard,
		CancelSelect,
		StartChoiceSelect,
		SelectChoiceCard,
		CancelChoiceSelect,
		CompleteChoiceSelect,
		CompleteSelect,
		StartFusionSelect,
		SelectFusionIngredient,
		CompleteFusionSelect
	}

	public enum SELECT_OBJECT_TARGET_TYPE
	{
		Deselect,
		Select
	}

	public enum SLIDE_OBJECT_TYPE
	{
		Cancel,
		Attack,
		Evolve
	}

	public enum HAND_URI_TYPE
	{
		SELECT_OBJECT_URI = 3,
		TURN_END_READY_URI = 4,
		SLIDE_OBJECT_URI = 5	}

	private NetworkBattleManagerBase _battleMgr;

	private NetworkConsistency _networkConsistency;

	private SendCardDataMaker _sendCardDataMaker;

	private List<EmitData> _alreadyEmitList = new List<EmitData>();

	private bool _isEmitStop_OutsideJudgeResult;

	private BattleCardBase _lastSelectedCard;

	private BattleCardBase _selectedSlideCard;

	private List<object> _emitHandParameterList = new List<object>();

	private DateTime _oldSlideCurrentLocalTime;

	public NetworkBattleSender(NetworkBattleManagerBase battlemgr, RegisterActionManager registerList, List<RegisterUnapproved> unapprovedList, NetworkConsistency consistency)
	{
		_battleMgr = battlemgr;
		_networkConsistency = consistency;
		_sendCardDataMaker = new SendCardDataMaker(battlemgr, registerList, unapprovedList);
	}

	public void SendDeal()
	{
		if (!_battleMgr.IsRecovery)
		{
			EmitMsg(NetworkBattleDefine.NetworkBattleURI.Deal);
		}
	}

	public void SendSwapInfo(List<int> swapIndexList)
	{
		if (!_battleMgr.IsRecovery)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("idxList", swapIndexList);
			EmitMsg(NetworkBattleDefine.NetworkBattleURI.Swap, dictionary);
		}
	}

	public void SendTurnStart()
	{
		Dictionary<string, object> dictionary = _sendCardDataMaker.MakePlayActionsSendCardData(null, null, null, isEvol: false, null, isTurnStart: true);
		dictionary.Add("actionSeq", _battleMgr.InstanceNetworkAgent.GetTurnSequence());
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.TurnStart, dictionary);
	}

	public void SendPlayCard(BattleCardBase playCard, List<BattleCardBase> targetCardList, List<NetworkBattleManagerBase.ValidateSkillData> validateSkillIndexList, SendKeyActionDataManager sendKeyActionDataManager, List<int> selectTypeSkillIndexList)
	{
		if (playCard == null)
		{
			LocalLog.AccumulateLastTraceLog("SendPlayCard Err playCard Null");
		}
		Dictionary<string, object> dictionary = _sendCardDataMaker.MakePlayActionsSendCardData(playCard, targetCardList, validateSkillIndexList, isEvol: false, sendKeyActionDataManager, isTurnStart: false, selectTypeSkillIndexList);
		if (targetCardList != null && targetCardList.Count >= 1)
		{
			dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.type]] = 31;
		}
		else
		{
			dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.type]] = 30;
		}
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.PlayActions, dictionary);
	}

	public void SendTurnEndAction()
	{
		Dictionary<string, object> dataList = _sendCardDataMaker.MakePlayActionsSendCardData();
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.TurnEndActions, dataList);
	}

	public void SendTurnEnd(bool isNextTurnTimeDecrement, bool isNowTurnTimeDecrement, bool final)
	{
		if (final || _battleMgr.InstanceNetworkAgent.GetTurnState())
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			_networkConsistency.SetupConsistency();
			dictionary.Add(NetworkBattleSenderDefine.SendCommonParameter.battleCode.ToString(), _networkConsistency.GetConsistency());
			dictionary.Add("type", (isNextTurnTimeDecrement ? 1 : 0) + (isNowTurnTimeDecrement ? 10 : 0));
			List<int> list = new List<int>();
			list.Add(_battleMgr.GetBattlePlayer(isPlayer: true).CemeteryList.Count);
			list.Add(_battleMgr.GetBattlePlayer(isPlayer: false).CemeteryList.Count);
			dictionary.Add("actionSeq", _battleMgr.InstanceNetworkAgent.GetTurnSequence());
			dictionary.Add("cemetery", list);
			if (final)
			{
				EmitMsg(NetworkBattleDefine.NetworkBattleURI.TurnEndFinal, dictionary);
				SetEmitStopOutsideJudgeResult();
			}
			else
			{
				EmitMsg(NetworkBattleDefine.NetworkBattleURI.TurnEnd, dictionary);
			}
		}
	}

	public void SendTurnEndFinish()
	{
		SendTurnEnd(isNextTurnTimeDecrement: false, isNowTurnTimeDecrement: false, final: true);
	}

	public void SendJudge()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		_networkConsistency.SetupConsistency();
		dictionary.Add(NetworkBattleSenderDefine.SendCommonParameter.battleCode.ToString(), _networkConsistency.GetConsistency());
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.Judge, dictionary);
	}

	public void SendChatStamp(string stamp)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.stamp]] = stamp;
		_battleMgr.InstanceNetworkAgent.EmitMsgPack(NetworkBattleDefine.NetworkBattleURI.ChatStamp, dictionary, null, isGetableAck: false);
	}

	public void SendSelectObject(BattleCardBase selectedCard)
	{
		if (selectedCard != _lastSelectedCard)
		{
			_lastSelectedCard = selectedCard;
			string empty = string.Empty;
			if (selectedCard == null)
			{
				empty = "0";
			}
			else
			{
				string text = (selectedCard.IsPlayer ? "1" : "0");
				string text2 = selectedCard.Index.ToString();
				empty = "1" + text + text2;
			}
			_emitHandParameterList.Clear();
			_emitHandParameterList.Add(empty);
			_battleMgr.InstanceNetworkAgent.EmitHandData(_emitHandParameterList, HAND_URI_TYPE.SELECT_OBJECT_URI);
		}
	}

	public void SendTurnEndReady(bool isShortenedTurn)
	{
		_emitHandParameterList.Clear();
		_emitHandParameterList.Add(isShortenedTurn);
		_battleMgr.InstanceNetworkAgent.EmitHandData(_emitHandParameterList, HAND_URI_TYPE.TURN_END_READY_URI);
	}

	public void SendSlideObject(SLIDE_OBJECT_TYPE slideObjectType, BattleCardBase selectedCard, BattleCardBase attackingCard)
	{
		if (_selectedSlideCard == selectedCard)
		{
			return;
		}
		if (slideObjectType != SLIDE_OBJECT_TYPE.Cancel)
		{
			DateTime now = DateTime.Now;
			TimeSpan elapsedTimeByTimeSpan = TimeUtil.GetElapsedTimeByTimeSpan(now, _oldSlideCurrentLocalTime);
			float num = (float)elapsedTimeByTimeSpan.Milliseconds / 1000f;
			if (elapsedTimeByTimeSpan.Seconds == 0 && num < 0.4f)
			{
				return;
			}
			_oldSlideCurrentLocalTime = now;
		}
		_selectedSlideCard = selectedCard;
		string item = string.Empty;
		string item2 = string.Empty;
		switch (slideObjectType)
		{
		case SLIDE_OBJECT_TYPE.Cancel:
			item = "0";
			break;
		case SLIDE_OBJECT_TYPE.Attack:
			item = "1" + attackingCard.Index;
			item2 = selectedCard.Index.ToString();
			break;
		case SLIDE_OBJECT_TYPE.Evolve:
			item = "2";
			item2 = selectedCard.Index.ToString();
			break;
		}
		_emitHandParameterList.Clear();
		_emitHandParameterList.Add(item);
		_emitHandParameterList.Add(item2);
		_battleMgr.InstanceNetworkAgent.EmitHandData(_emitHandParameterList, HAND_URI_TYPE.SLIDE_OBJECT_URI);
	}

	public void EmitRetry(NetworkBattleDefine.NetworkBattleURI uri)
	{
		BattleCoroutine.GetInstance().StartCoroutine(EmitRetryWait(uri));
	}

	private IEnumerator EmitRetryWait(NetworkBattleDefine.NetworkBattleURI uri)
	{
		long oldTimer = TimeUtil.GetAbsoluteTime().Ticks;
		while ((float)NetworkUtility.GetTimeSpanSecond(oldTimer) < 0.5f)
		{
			yield return null;
		}
		EmitData emitData = _alreadyEmitList.Find((EmitData x) => x.Uri == uri.ToString());
		_alreadyEmitList.Remove(emitData);
		LocalLog.AccumulateLastTraceLog("EmitRetryWait " + emitData.Uri);
		EmitMsg((NetworkBattleDefine.NetworkBattleURI)Enum.Parse(typeof(NetworkBattleDefine.NetworkBattleURI), emitData.Uri), emitData.Data);
	}

	public void SendAtkData(BattleCardBase attackCard, BattleCardBase targetCard)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.Add(targetCard);
		Dictionary<string, object> dictionary = _sendCardDataMaker.MakePlayActionsSendCardData(attackCard, list);
		if (dictionary.Count >= 1)
		{
			dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.type]] = 10;
			EmitMsg(NetworkBattleDefine.NetworkBattleURI.PlayActions, dictionary);
		}
	}

	public void SendEvolData(BattleCardBase evolCard, List<BattleCardBase> targetCardList, List<NetworkBattleManagerBase.ValidateSkillData> validateSkillIndexList, SendKeyActionDataManager sendKeyActionDataManager, List<int> selectTypeSkillIndexList)
	{
		Dictionary<string, object> dictionary = _sendCardDataMaker.MakePlayActionsSendCardData(evolCard, targetCardList, validateSkillIndexList, isEvol: true, sendKeyActionDataManager, isTurnStart: false, selectTypeSkillIndexList);
		if (targetCardList != null && targetCardList.Count >= 1)
		{
			dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.type]] = 21;
		}
		else
		{
			dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.type]] = 20;
		}
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.PlayActions, dictionary);
	}

	public void SendRetire()
	{
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.Retire);
		SetEmitStopOutsideJudgeResult();
	}

	public void SendEcho(int idx, NetworkBattleDefine.PlayActionType actionType, SendKeyActionDataManager sendKeyActionDataManager, bool isNotActiveSeq = false, bool isTurnStart = false)
	{
		Dictionary<string, object> dictionary = _sendCardDataMaker.MakeEchoData(idx, actionType, sendKeyActionDataManager);
		if (dictionary.Count >= 1 || isTurnStart)
		{
			EmitMsg(NetworkBattleDefine.NetworkBattleURI.Echo, dictionary, isDisconnectIgnoring: false, isGetableAck: true, isNotActiveSeq);
		}
		else
		{
			LocalLog.AccumulateLastTraceLog("NotEchoData isVirtual" + _battleMgr.IsVirtualBattle + " " + StackTraceUtility.ExtractStackTrace());
		}
	}

	public void SendJudgeResult(JUDGE_RESULT_STATUS judgeResultStatus)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[NetworkBattleDefine.NetworkParameter.log.ToString()] = (int)judgeResultStatus;
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.JudgeResult, dictionary, isDisconnectIgnoring: true, isGetableAck: false);
	}

	public void SetEmitStopOutsideJudgeResult()
	{
		_isEmitStop_OutsideJudgeResult = true;
	}

	private bool EmitMsg(NetworkBattleDefine.NetworkBattleURI uri, Dictionary<string, object> dataList = null, bool isDisconnectIgnoring = false, bool isGetableAck = true, bool isNotActiveSeq = false)
	{
		if (_battleMgr.IsNetworkBattleEnd)
		{
			LocalLog.AccumulateLastTraceLog("networkbattleEnd:" + uri);
			return false;
		}
		if (!isDisconnectIgnoring && _battleMgr.disconnectToLoseChecker.IsSelfDisConnectOnTimeout())
		{
			LocalLog.AccumulateLastTraceLog("EmitMsgDisConnect:" + uri);
			return false;
		}
		if (_isEmitStop_OutsideJudgeResult && uri != NetworkBattleDefine.NetworkBattleURI.JudgeResult)
		{
			LocalLog.AccumulateLastTraceLog("EmitStop Outside JudgeResult:" + uri);
			return false;
		}
		if (uri == NetworkBattleDefine.NetworkBattleURI.PlayActions && !_battleMgr.GetBattlePlayer(isPlayer: true).IsSelfTurn)
		{
			LocalLog.AccumulateTraceLog("588157NotMyturnPlay");
			return false;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (dataList != null)
		{
			foreach (KeyValuePair<string, object> data in dataList)
			{
				dictionary.Add(data.Key, data.Value);
			}
		}
		_alreadyEmitList.Add(new EmitData(uri.ToString(), dictionary, _battleMgr.InstanceNetworkAgent.LastEmitSeqNumber));
		_battleMgr.InstanceNetworkAgent.EmitMsgPack(uri, dataList, null, isGetableAck, -1, isStockData: true, isNotActiveSeq);
		_battleMgr.SendIntervalTriggerMain.SendDataCheck(_battleMgr, uri);
		return true;
	}

	public void SendFusionData(BattleCardBase fusionCard, List<BattleCardBase> ingredientCards, SendKeyActionDataManager sendKeyActionDataManager, List<int> selectTypeSkillIndexList)
	{
		Dictionary<string, object> dictionary = _sendCardDataMaker.MakePlayActionsSendCardData(fusionCard, ingredientCards, null, isEvol: false, sendKeyActionDataManager, isTurnStart: false, selectTypeSkillIndexList);
		dictionary[NetworkBattleDefine.NetworkParameter.type.ToString()] = 40;
		EmitMsg(NetworkBattleDefine.NetworkBattleURI.PlayActions, dictionary);
	}
}

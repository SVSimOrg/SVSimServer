using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.RoomMatch;

public class NetworkBattleReceiver
{
	public enum JudgeEndType
	{
		JUDGE = 1,
		VALIDATE,
		FIRSTCARD
	}

	public class ReceiveData
	{
		public long _timeSent;

		public NetworkBattleDefine.NetworkBattleURI dataUri;

		public NetworkBattleDefine.NetworkBattleURI receiveUri;

		public int idx;

		public bool isSelf;

		public bool _isPlayerCard;

		public NetworkBattleDefine.PlayActionType actionType;

		public JudgeEndType judgeEndType;

		public int playCardIndex;

		public List<CardDataModel> unapprovedList = new List<CardDataModel>();

		public List<CardDataModel> SkillConditionCheckList = new List<CardDataModel>();

		public List<TargetData> OpponentTargetDataList = new List<TargetData>();

		public List<TargetData> PlayerTargetDataList = new List<TargetData>();

		public int oppoChatStamp;

		public int playChatStamp;

		public bool isWin;

		public bool _isBurialRiteSelect;

		public bool IsChoiceBraveSelect;

		public List<CardDataModel> knownCardList = new List<CardDataModel>();

		public List<CardDataModel> watchCardList = new List<CardDataModel>();

		public RESULT_CODE result;

		public NetworkBattleDefine.ReceiveNodeResultCode NodeResultCode;

		public RESULT_CODE opponentResult;

		public List<int> validateSkillIndexList = new List<int>();

		public NetworkBattleSender.SELECT_SKILL_OPERATION _selectSkillOperation;

		public int _selectedCardIndex;

		public List<int> ActiveSelectSkillIndexList = new List<int>();

		public List<int> _selectedChoiceCardIdList = new List<int>();

		public bool _isEvolveTargetSelect;

		public NetworkBattleSender.SELECT_OBJECT_TARGET_TYPE _selectObjectTargetType;

		public bool _isShortenedTurn;

		public bool _isNotTurnEndReady;

		public NetworkBattleSender.SLIDE_OBJECT_TYPE _slideObjectType;

		public bool IsTransformChoice;

		public List<int> choiceIdList = new List<int>();

		public List<int> burialRateList = new List<int>();

		public int mutationAfterCardId;

		public bool _isSettingMutationAfterCardId;

		public int transformBeforeCardId;

		public int mutationAfterCost;

		public List<SendKeyActionDataManager.KeyActionType> keyActionType = new List<SendKeyActionDataManager.KeyActionType>();

		public List<int> selfIdxList = new List<int>();

		public List<int> oppoIdxList = new List<int>();

		public int spin;

		public bool IsChoice => keyActionType.Exists((SendKeyActionDataManager.KeyActionType x) => x == SendKeyActionDataManager.KeyActionType.Choice || x == SendKeyActionDataManager.KeyActionType.HaveBeforeSkillChoice);

		public bool IsChoiceBrave => keyActionType.Exists((SendKeyActionDataManager.KeyActionType x) => x == SendKeyActionDataManager.KeyActionType.ChoiceBrave);

		public bool IsBurialRate => keyActionType.Exists((SendKeyActionDataManager.KeyActionType x) => x == SendKeyActionDataManager.KeyActionType.BurialRate);

		public bool IsAcceleratedOrCrystallize => keyActionType.Exists((SendKeyActionDataManager.KeyActionType x) => x == SendKeyActionDataManager.KeyActionType.Accelerated || x == SendKeyActionDataManager.KeyActionType.Crystallize);

		public bool IsFusion => keyActionType.Exists((SendKeyActionDataManager.KeyActionType x) => x == SendKeyActionDataManager.KeyActionType.Fusion);

		public List<CardDataModel> GetReceiveCardList()
		{
			// Pre-Phase-5b: gated on mgr.IsRecovery to choose between known vs. watch card lists.
			// Headless PVP relay stays in the "recovery" state throughout (per Phase 3 notes on
			// HeadlessNetworkBattleMgr.IsRecovery = true), so the else-branch is the safe default.
			return watchCardList;
		}
	}

	public class ReplayReceiveData : ReceiveData
	{

		public CardInfo CardInfo;

		public int Life;

		public int MaxLife;

		public int Cost;

		public CardBasePrm.ClanType Clan;

		public CardBasePrm.TribeInfo Tribe;
	}

	public class CardInfo
	{

		public int Cost;

		public List<InplaySkillEffect> InplaySkillEffectList;

		public int InductionNumber;

		public int AttackableCount;

		public List<int> Tribe = new List<int>();

		public int ChantCount;

		public int MaxAttackableCount = -1;
	}

	public class ReplayBuffInfo
	{
	}

	public class BuffCardInfo
	{

		public int CardId;
	}

	public class EffectInfo
	{
	}

	public class SideLogSkillInfo
	{

		public int CardId;

		public CardInfo CardInfo;
	}

	public class StatusPanelInfo
	{
	}

	public class ClassInfoUiInfo
	{

		public int Life;
	}

	public class MyRotationBonusInfo
	{
	}

	public class AvatarBattleDescriptionValueInfo
	{
	}

	public enum DestroyType
	{
		None,
		ChantCount	}

	public enum InplaySkillEffect
	{
		Guard,
		Killer,
		Untouchable,
		NotBeAttacked,
		Sneak,
		SkillCantAtkAll,
		Independent,
		DamageCutProtection,
		Indestructible,
		ReflectionClass,
		Drain,
		Destroy,
		WhiteRitual,
		StackWhiteRitual,
		Induction,
		InductionNumber,
		Geton,
		GetonAfter
	}

	public enum ReplayReceiveDataType
	{
		CardInfo,
		Cost,
		Life,
		MaxLife,
		CardId,
		Clan,
		Tribe	}

	public enum ReplayOperationType
	{
		TurnStart,
		GainPowerDown,
		Necromance	}

	public enum ReplayItemType
	{
		Turn,
		Cost,
		Atk,
		Life,
		MaxLife,
		MaxAttackableCount,
		AttackableCount,
		Player,
		Enemy,
		Tribe,
		ChantCount}

	public enum ReplayBuffType
	{
		None	}

	public enum ReplayBuffInfoTextType
	{
		Cost	}

	public enum ReplayParameterModifierType
	{
	}

	public class TargetData
	{
		public int TargetIndex { get; private set; }

		public int ViewId { get; private set; }

		public bool IsSelf { get; private set; }

		public List<int> SelectSkillIndexList { get; private set; }

		public TargetData(int target, int viewId, bool isSelf, List<int> selectSkillIndexList)
		{
			TargetIndex = target;
			ViewId = viewId;
			IsSelf = isSelf;
			SelectSkillIndexList = selectSkillIndexList;
		}
	}

	public struct ReplayBuffInfoLabel
	{
		public ReplayBuffInfoTextType Type { get; set; }

		public int Value { get; set; }

		public ReplayBuffInfoLabel(ReplayBuffInfoTextType type, int value = -1)
		{
			Type = type;
			Value = value;
		}
	}

	public enum RESULT_CODE
	{
		NotFinish = 0,
		NoContest = 1,
		Invalid = 2,
		LifeWin = 101,
		LifeLose = 102,
		DeckoutWin = 103,
		DeckoutLose = 104,
		RetireWin = 105,
		RetireLose = 106,
		SpecialWin = 107,
		SpecialLose = 108,
		DisconnectWin = 201,
		DisconnectLose = 202,
		FirstcardWin = 203,
		FirstcardLose = 204,
		TurnendWin = 205,
		TurnendLose = 206,
		TurnstartWin = 207,
		TurnstartLose = 208,
		MaxTurnLose = 210,
		Error = 999
	}

	protected NetworkBattleManagerBase networkBattleMgr;

	protected ReceiveData _receiveData;

	protected bool receiveStop;

	public NetworkBattleReceiver(NetworkBattleManagerBase battlemgr)
	{
		networkBattleMgr = battlemgr;
		_receiveData = new ReceiveData();
	}

	public bool ReceivedMessage(NetworkBattleDefine.NetworkBattleURI uri, bool isHaveSequence, Dictionary<string, object> data, bool isPlayer = false, WatchDataHandler handler = null, bool checkBreakData = true)
	{
		if (!ConvertReceiveDataToMakeData(uri, isHaveSequence, data, isPlayer, handler) && !RealTimeNetworkAgent.IsFromResumeData(data) && checkBreakData)
		{
			return false;
		}
		if (receiveStop)
		{
			return true;
		}
		if (isHaveSequence)
		{
			networkBattleMgr.ConductReceiveData(_receiveData, isPlayer);
		}
		else
		{
			networkBattleMgr.ConductReceiveData_NotHaveSequence(_receiveData, isPlayer);
		}
		return true;
	}

	public virtual void ReceivedTouchData(NetworkBattleDefine.NetworkBattleURI uri, bool isHaveSequence, Dictionary<string, object> data, bool isPlayer = false, WatchDataHandler handler = null)
	{
	}

	protected virtual bool ConvertReceiveDataToMakeData(NetworkBattleDefine.NetworkBattleURI uri, bool isHaveSequence, Dictionary<string, object> datas, bool isPlayer = false, WatchDataHandler handler = null)
	{
		if (isHaveSequence)
		{
			_receiveData = new ReceiveData();
		}
		_receiveData.dataUri = uri;
		string text = "URI : " + uri;
		try
		{
			foreach (KeyValuePair<string, object> data in datas)
			{
				if (!Enum.IsDefined(typeof(NetworkBattleDefine.NetworkParameter), data.Key.ToString()))
				{
					continue;
				}
				text = text + ", " + data.Key;
				if (data.Value is List<object> list)
				{
					text += "[";
					for (int i = 0; i < list.Count; i++)
					{
						if (!(list[i] is Dictionary<string, object> dictionary))
						{
							continue;
						}
						foreach (KeyValuePair<string, object> item in dictionary)
						{
							text = text + item.Key + " : " + item.Value.ToString() + ",";
						}
					}
					text += "]";
				}
				else
				{
					text = text + " : " + data.Value.ToString();
				}
				switch ((NetworkBattleDefine.NetworkParameter)Enum.Parse(typeof(NetworkBattleDefine.NetworkParameter), data.Key.ToString()))
				{
				case NetworkBattleDefine.NetworkParameter.time:
					_receiveData._timeSent = long.Parse(data.Value.ToString());
					break;
				case NetworkBattleDefine.NetworkParameter.idx:
					_receiveData.idx = ConvertToInt(data.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.self:
					_receiveData.selfIdxList = MakeReceiveCardIndexSortList(data.Value as List<object>);
					break;
				case NetworkBattleDefine.NetworkParameter.oppo:
					_receiveData.oppoIdxList = MakeReceiveCardIndexSortList(data.Value as List<object>);
					break;
				case NetworkBattleDefine.NetworkParameter.isSelf:
					_receiveData.isSelf = ConvertToInt(data.Value) == 1;
					break;
				case NetworkBattleDefine.NetworkParameter.type:
					_receiveData.actionType = (NetworkBattleDefine.PlayActionType)ConvertToInt(data.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.endType:
					_receiveData.judgeEndType = (JudgeEndType)ConvertToInt(data.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.targetUri:
					_receiveData.receiveUri = (NetworkBattleDefine.NetworkBattleURI)Enum.Parse(typeof(NetworkBattleDefine.NetworkBattleURI), data.Value.ToString());
					break;
				case NetworkBattleDefine.NetworkParameter.playIdx:
					_receiveData.playCardIndex = ConvertToInt(data.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.cards:
					_receiveData.watchCardList = MakeReceiveCardDataList(data.Value as List<object>, networkBattleMgr, handler);
					switch (uri)
					{
					case NetworkBattleDefine.NetworkBattleURI.Deal:
						_receiveData.selfIdxList = (from model in _receiveData.watchCardList
							where !model.isOpponent
							orderby model.RedrawCardPosition
							select model.Index).ToList();
						_receiveData.oppoIdxList = (from model in _receiveData.watchCardList
							where model.isOpponent
							orderby model.RedrawCardPosition
							select model.Index).ToList();
						break;
					case NetworkBattleDefine.NetworkBattleURI.Swap:
					case NetworkBattleDefine.NetworkBattleURI.Ready:
						_receiveData.selfIdxList = GetSwappedIndexList(isPlayer: true, _receiveData.watchCardList);
						_receiveData.oppoIdxList = GetSwappedIndexList(isPlayer: false, _receiveData.watchCardList);
						break;
					}
					break;
				case NetworkBattleDefine.NetworkParameter.idxChangeSeed:
					networkBattleMgr.CreateXorShift(ConvertToInt(data.Value));
					break;
				case NetworkBattleDefine.NetworkParameter.oppoIdxChangeSeed:
					networkBattleMgr.CreateXorShift(-1, ConvertToInt(data.Value));
					break;
				case NetworkBattleDefine.NetworkParameter.targetList:
				{
					List<TargetData> list3 = CreateTargetList(data, isWatch: true, handler);
					if (isPlayer)
					{
						_receiveData.PlayerTargetDataList = list3;
					}
					else
					{
						_receiveData.OpponentTargetDataList = list3;
					}
					break;
				}
				case NetworkBattleDefine.NetworkParameter.oppoTargetList:
					_receiveData.OpponentTargetDataList = CreateTargetList(data);
					break;
				case NetworkBattleDefine.NetworkParameter.chatStamp:
					if (data.Value is Dictionary<string, object> dictionary3)
					{
						int num2 = ConvertToInt(dictionary3[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.stamp]]);
						if (isPlayer)
						{
							_receiveData.playChatStamp = num2;
						}
						else
						{
							_receiveData.oppoChatStamp = num2;
						}
					}
					break;
				case NetworkBattleDefine.NetworkParameter.isWin:
					_receiveData.isWin = ((ConvertToInt(data.Value) != 0) ? true : false);
					break;
				case NetworkBattleDefine.NetworkParameter.knownList:
					_receiveData.knownCardList = MakeReceiveCardDataList(data.Value as List<object>, networkBattleMgr, handler);
					break;
				case NetworkBattleDefine.NetworkParameter.uList:
					_receiveData.unapprovedList = MakeReceiveCardDataList(data.Value as List<object>, networkBattleMgr, handler);
					break;
				case NetworkBattleDefine.NetworkParameter.result:
					_receiveData.result = (RESULT_CODE)ConvertToInt(data.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.resultCode:
					_receiveData.NodeResultCode = (NetworkBattleDefine.ReceiveNodeResultCode)Convert.ToInt32(data.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.keyAction:
				{
					List<object> obj = data.Value as List<object>;
					bool flag = false;
					foreach (object item2 in obj)
					{
						Dictionary<string, object> dictionary2 = item2 as Dictionary<string, object>;
						SendKeyActionDataManager.KeyActionType keyActionType = (SendKeyActionDataManager.KeyActionType)Enum.Parse(typeof(SendKeyActionDataManager.KeyActionType), dictionary2[SendKeyActionDataManager.KeyActionParameter.type.ToString()].ToString());
						_receiveData.keyActionType.Add(keyActionType);
						if (dictionary2.ContainsKey(SendKeyActionDataManager.KeyActionParameter.cardId.ToString()) && (!flag || keyActionType != SendKeyActionDataManager.KeyActionType.Choice))
						{
							_receiveData.transformBeforeCardId = ConvertToInt(dictionary2[SendKeyActionDataManager.KeyActionParameter.cardId.ToString()]);
							if (keyActionType == SendKeyActionDataManager.KeyActionType.Accelerated)
							{
								flag = true;
							}
						}
						switch (keyActionType)
						{
						case SendKeyActionDataManager.KeyActionType.Choice:
						case SendKeyActionDataManager.KeyActionType.HaveBeforeSkillChoice:
						case SendKeyActionDataManager.KeyActionType.ChoiceEvolution:
						{
							_receiveData.choiceIdList = new List<int>();
							if (dictionary2 != null && dictionary2.ContainsKey(SendKeyActionDataManager.KeyActionParameter.selectCard.ToString()))
							{
								_receiveData.choiceIdList = ConvertToListInt(dictionary2[SendKeyActionDataManager.KeyActionParameter.selectCard.ToString()]);
							}
							CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(_receiveData.transformBeforeCardId);
							BattleCardBase battleCardBase = networkBattleMgr.CreateBattleCard(cardParameterFromId.BaseCardId, isPlayer: false, null, cardParameterFromId, networkBattleMgr.BattlePlayer, 0);
							UnityEngine.Object.DestroyImmediate(battleCardBase.BattleCardView.GameObject);
							bool isEvol = _receiveData.actionType == NetworkBattleDefine.PlayActionType.EVOLUTION || _receiveData.actionType == NetworkBattleDefine.PlayActionType.EVOLUTION_SELECT;
							if (!(NetworkBattleGenericTool.GetChoiceSkill(battleCardBase, isEvol) is Skill_transform))
							{
								break;
							}
							_receiveData.IsTransformChoice = true;
							if (_receiveData.choiceIdList.Count <= 0)
							{
								break;
							}
							string text2 = BattleLogManager.ConvertPremiumIdToNormalId(_receiveData.choiceIdList[0]).ToString();
							if (!(battleCardBase.Skills.FirstOrDefault((SkillBase s) => s is Skill_choice) as Skill_choice).OptionValue.GetString(SkillFilterCreator.ContentKeyword.card_id).Contains(text2))
							{
								LocalLog.AccumulateTraceLog("Illegal Choice Id Received !：" + text2);
								List<int> list2 = new List<int>();
								for (int num = 0; num < _receiveData.choiceIdList.Count; num++)
								{
									list2.Add((_receiveData.choiceIdList[num] == 100011010) ? 100011020 : 100011010);
								}
								_receiveData.choiceIdList = list2;
							}
							break;
						}
						case SendKeyActionDataManager.KeyActionType.ChoiceBrave:
							_receiveData.choiceIdList = new List<int>();
							if (dictionary2 != null && dictionary2.ContainsKey(SendKeyActionDataManager.KeyActionParameter.selectCard.ToString()))
							{
								_receiveData.choiceIdList = ConvertToListInt(dictionary2[SendKeyActionDataManager.KeyActionParameter.selectCard.ToString()]);
							}
							break;
						case SendKeyActionDataManager.KeyActionType.BurialRate:
							_receiveData.burialRateList = ConvertToListInt(dictionary2[SendKeyActionDataManager.KeyActionParameter.cardIdx.ToString()]);
							break;
						}
					}
					break;
				}
				case NetworkBattleDefine.NetworkParameter.spin:
					_receiveData.spin = GetSpinCount(data.Value);
					break;
				}
			}
			if (networkBattleMgr.IsRecovery && uri != NetworkBattleDefine.NetworkBattleURI.OppoDisconnect && uri != NetworkBattleDefine.NetworkBattleURI.RecoveryStart && uri != NetworkBattleDefine.NetworkBattleURI.RecoveryEnd)
			{
				networkBattleMgr._lastReceivedData = _receiveData;
			}
			return true;
		}
		catch
		{
			LocalLog.AccumulateLastTraceLog("ConvertReciveDataToMakeData Error\n" + text);
			Debug.LogError("ConvertReciveDataToMakeData Error");
			return false;
		}
	}

	protected void CreateSlideObjectReceiveData(string startPoint, string endPoint)
	{
		_receiveData._slideObjectType = (NetworkBattleSender.SLIDE_OBJECT_TYPE)Enum.Parse(typeof(NetworkBattleSender.SLIDE_OBJECT_TYPE), startPoint[0].ToString());
		switch (_receiveData._slideObjectType)
		{
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Attack:
			_receiveData.idx = int.Parse(startPoint.Substring(1, startPoint.Length - 1));
			_receiveData._selectedCardIndex = int.Parse(endPoint);
			break;
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Evolve:
			_receiveData._selectedCardIndex = int.Parse(endPoint);
			break;
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Cancel:
			break;
		}
	}

	private List<TargetData> CreateTargetList(KeyValuePair<string, object> data, bool isWatch = false, WatchDataHandler handler = null)
	{
		List<object> obj = data.Value as List<object>;
		List<TargetData> list = new List<TargetData>();
		_receiveData.validateSkillIndexList = new List<int>();
		foreach (object item2 in obj)
		{
			int target = 0;
			int num = 0;
			bool isSelf = false;
			Dictionary<string, object> dictionary = item2 as Dictionary<string, object>;
			string key = NetworkBattleDefine.NetworkParameter.targetIdx.ToString();
			if (dictionary.ContainsKey(key))
			{
				target = ConvertToInt(dictionary[key]);
			}
			if (isWatch)
			{
				key = NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.vid];
				if (dictionary.ContainsKey(key))
				{
					num = ConvertToInt(dictionary[key].ToString());
					// Phase-5 chunk 39: dropped ambient PlayerStaticData.UserViewerID fallback in favor
					// of the mgr's InstanceViewerId — recovery branch compares wire vid to the mgr's
					// bound viewer id, live branch stays on handler.isOwner.
					isSelf = ((!networkBattleMgr.IsRecovery) ? (!handler.isOwner(dictionary[key].ToString())) : ((num != networkBattleMgr.InstanceViewerId) ? true : false));
				}
			}
			else
			{
				key = NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isSelf];
				if (dictionary.ContainsKey(key))
				{
					isSelf = ConvertToInt(dictionary[key]) == 1;
				}
			}
			key = NetworkBattleDefine.NetworkParameter.skillIndex.ToString();
			if (dictionary.ContainsKey(key))
			{
				foreach (int item3 in ConvertToListInt(dictionary[key]))
				{
					_receiveData.validateSkillIndexList.Add(item3);
				}
			}
			List<int> selectSkillIndexList = new List<int>();
			key = NetworkBattleDefine.NetworkParameter.selectSkillIndex.ToString();
			if (dictionary.ContainsKey(key))
			{
				selectSkillIndexList = ConvertToListInt(dictionary[key]);
			}
			TargetData item = new TargetData(target, num, isSelf, selectSkillIndexList);
			list.Add(item);
		}
		return list;
	}

	public void ReceiveStop()
	{
		LocalLog.AccumulateLastTraceLog("ReceiveBattleFinish StopReceiveData");
		receiveStop = true;
	}

	protected virtual void CheckDuplicateDataAddtoList(List<CardDataModel> list, CardDataModel cardDataModel)
	{
		list.Add(cardDataModel);
	}

	private List<int> MakeReceiveCardIndexSortList(List<object> mulliganList)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (object mulligan in mulliganList)
		{
			Dictionary<string, object> dictionary2 = mulligan as Dictionary<string, object>;
			int key = ConvertToInt(dictionary2[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.pos]]);
			int value = ConvertToInt(dictionary2[NetworkBattleDefine.NetworkParameter.idx.ToString()]);
			dictionary.Add(key, value);
		}
		return (from data in dictionary
			orderby data.Key
			select data.Value).ToList();
	}

	private List<int> GetSwappedIndexList(bool isPlayer, List<CardDataModel> cardList)
	{
		List<CardDataModel> list = (from param in cardList
			where param.isOpponent == !isPlayer
			where param.ToStateList.First() == NetworkBattleDefine.NetworkCardPlaceState.Hand
			select param).ToList();
		List<int> list2 = (isPlayer ? new List<int>(networkBattleMgr.MulliganMgr.PlayerMlgCtrl.DealIdxList) : new List<int>(networkBattleMgr.MulliganMgr.OpponentMlgCtrl.DealIdxList));
		for (int num = 0; num < list.Count; num++)
		{
			int redrawCardPosition = list[num].RedrawCardPosition;
			list2[redrawCardPosition] = list[num].Index;
		}
		return list2;
	}

	private List<CardDataModel> MakeReceiveCardDataList(List<object> cardData, BattleManagerBase mgr, WatchDataHandler handler)
	{
		List<CardDataModel> list = new List<CardDataModel>();
		foreach (object cardDatum in cardData)
		{
			if (!(cardDatum as Dictionary<string, object>).ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.activate]) && !(cardDatum as Dictionary<string, object>).ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.count]) && !(cardDatum as Dictionary<string, object>).ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.callCount]) && !(cardDatum as Dictionary<string, object>).ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.param]))
			{
				foreach (CardDataModel item in MakeReceiveCardData(cardDatum, mgr, handler))
				{
					CheckDuplicateDataAddtoList(list, item);
				}
				continue;
			}
			foreach (CardDataModel item2 in MakeReceiveCardData(cardDatum, mgr, handler))
			{
				_receiveData.SkillConditionCheckList.Add(item2);
			}
		}
		List<CardDataModel> list2 = new List<CardDataModel>();
		foreach (CardDataModel skillConditionCheck in _receiveData.SkillConditionCheckList)
		{
			if (list2.Contains(skillConditionCheck))
			{
				continue;
			}
			int num = 0;
			foreach (CardDataModel skillConditionCheck2 in _receiveData.SkillConditionCheckList)
			{
				if (skillConditionCheck.Index == skillConditionCheck2.Index && skillConditionCheck.publishedActiveSkillCount == skillConditionCheck2.publishedActiveSkillCount)
				{
					skillConditionCheck2.skillMovementNum = num;
					num++;
					list2.Add(skillConditionCheck2);
				}
			}
		}
		if (!_receiveData._isSettingMutationAfterCardId && _receiveData.IsAcceleratedOrCrystallize)
		{
			_receiveData._isSettingMutationAfterCardId = true;
			_receiveData.mutationAfterCardId = list[0].CardId;
			_receiveData.mutationAfterCost = list[0].playCardCost;
			list[0].CardId = _receiveData.transformBeforeCardId;
		}
		return list;
	}

	protected virtual List<CardDataModel> MakeReceiveCardData(object cardData, BattleManagerBase mgr, WatchDataHandler handler)
	{
		Dictionary<string, object> dictionary = cardData as Dictionary<string, object>;
		List<CardDataModel> list = new List<CardDataModel>();
		List<int> list2 = new List<int>();
		if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idx]))
		{
			list2.Add(ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idx]]));
		}
		else if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idxList]))
		{
			list2 = ConvertToListInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idxList]]);
		}
		else
		{
			Debug.LogError("BaseMakeReceiveCardData Not idx ");
		}
		for (int i = 0; i < list2.Count; i++)
		{
			CardDataModel cardDataModel = new CardDataModel();
			cardDataModel.Index = list2[i];
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				if (!Enum.IsDefined(typeof(NetworkBattleDefine.NetworkParameter), item.Key))
				{
					continue;
				}
				switch ((NetworkBattleDefine.NetworkParameter)Enum.Parse(typeof(NetworkBattleDefine.NetworkParameter), item.Key))
				{
				case NetworkBattleDefine.NetworkParameter.cardId:
					cardDataModel.CardId = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.to:
					cardDataModel.ToStateList.Add((NetworkBattleDefine.NetworkCardPlaceState)ConvertToInt(item.Value));
					break;
				case NetworkBattleDefine.NetworkParameter.isSelf:
					cardDataModel.isOpponent = ConvertToInt(item.Value) != 1;
					break;
				case NetworkBattleDefine.NetworkParameter.skillKeyCardIdx:
				{
					cardDataModel.skillKeyCardIdxList = new List<int>();
					if (item.Value is List<object>)
					{
						cardDataModel.skillKeyCardIdxList = ConvertToListInt(item.Value);
						break;
					}
					JsonData jsonData = item.Value as JsonData;
					for (int j = 0; j < jsonData.Count; j++)
					{
						cardDataModel.skillKeyCardIdxList.Add(jsonData[j].ToInt());
					}
					break;
				}
				case NetworkBattleDefine.NetworkParameter.pos:
					cardDataModel.RedrawCardPosition = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.cost:
					cardDataModel.playCardCost = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.activate:
					cardDataModel.activate = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.addLife:
					cardDataModel.AddLife = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.setLife:
					cardDataModel.SetLife = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.addAtk:
					cardDataModel.AddAtk = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.setAtk:
					cardDataModel.SetAtk = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.clan:
					cardDataModel.Clan = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.tribe:
					cardDataModel.Tribe = item.Value.ToString();
					break;
				case NetworkBattleDefine.NetworkParameter.skillIdx:
					cardDataModel.SkillIndex = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.skillCount:
					cardDataModel.publishedActiveSkillCount = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.count:
					cardDataModel.activate = 1;
					cardDataModel.SkillValueCount = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.callCount:
					cardDataModel.activate = 1;
					cardDataModel.SkillCallCount = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.param:
					cardDataModel.activate = 1;
					cardDataModel.SkillValueParameter = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.attachTarget:
					cardDataModel.SetAttachTarget(item.Value.ToString());
					break;
				case NetworkBattleDefine.NetworkParameter.highlander:
					cardDataModel.IsHighlander = ConvertToInt(item.Value) == 1;
					break;
				case NetworkBattleDefine.NetworkParameter.spellboost:
					cardDataModel.Spellboost = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.addChantCount:
					cardDataModel.AddChantCount = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.setChantCount:
					cardDataModel.SetChantCount = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.unionburst:
					cardDataModel.UnionBurstCount = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.skyboundArt:
					cardDataModel.SkyboundArtCount = ConvertToInt(item.Value);
					break;
				case NetworkBattleDefine.NetworkParameter.isInvoke:
					cardDataModel.IsInvoked = ConvertToInt(item.Value) == 1;
					break;
				case NetworkBattleDefine.NetworkParameter.skill:
				{
					string[] array = item.Value.ToString().Split('|');
					cardDataModel.skillCardIndex = int.Parse(array[0]);
					cardDataModel.publishedActiveSkillCount = int.Parse(array[1]);
					cardDataModel.skillMovementNum = int.Parse(array[2]);
					break;
				}
				case NetworkBattleDefine.NetworkParameter.is_open:
					cardDataModel.IsOpen = ConvertToInt(item.Value) == 1;
					break;
				case NetworkBattleDefine.NetworkParameter.randomTargetIdx:
				{
					List<int> list3 = ConvertToListInt(item.Value);
					if (i < list3.Count)
					{
						cardDataModel.RandomTargetIndex = list3[i];
					}
					break;
				}
				case NetworkBattleDefine.NetworkParameter.fusion:
					cardDataModel.FusionIngredientList = ConvertToListInt(item.Value);
					break;
				}
			}
			if (mgr != null)
			{
				if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.from]))
				{
					cardDataModel.fromState = (NetworkBattleDefine.NetworkCardPlaceState)ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.from]]);
				}
				else
				{
					cardDataModel.fromState = NetworkBattleGenericTool.GetCardPlaceState(mgr.GetBattlePlayer(cardDataModel.isOpponent), cardDataModel.Index);
				}
			}
			list.Add(cardDataModel);
		}
		return list;
	}

	protected virtual int ConvertToInt(object intStr)
	{
		return Convert.ToInt32(intStr.ToString());
	}

	protected int GetSpinCount(object receivedObject)
	{
		string s = receivedObject.ToString();
		int result = 0;
		if (int.TryParse(s, out result))
		{
			return ConvertToInt(receivedObject);
		}
		if (receivedObject is Dictionary<string, object> dictionary && dictionary.Keys.Contains("vid") && dictionary.Keys.Contains("value"))
		{
			int num = int.Parse(dictionary["vid"].ToString());
			if (networkBattleMgr.GameMgr.GetNetworkUserInfoData().GetSelfViewerId() == num)
			{
				return int.Parse(dictionary["value"].ToString());
			}
		}
		return 0;
	}

	private List<int> ConvertToListInt(object listObject)
	{
		List<object> obj = listObject as List<object>;
		List<int> list = new List<int>();
		foreach (object item in obj)
		{
			list.Add(ConvertToInt(item));
		}
		return list;
	}
}

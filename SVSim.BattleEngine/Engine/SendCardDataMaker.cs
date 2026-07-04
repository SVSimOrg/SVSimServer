using System.Collections.Generic;
using System.Linq;
using Wizard;

public class SendCardDataMaker
{
	private NetworkBattleManagerBase _battleMgr;

	private RegisterActionManager _registerActionManager;

	private List<RegisterUnapproved> _registerUnapprovedList;

	public SendCardDataMaker(NetworkBattleManagerBase battlemgr, RegisterActionManager registerList, List<RegisterUnapproved> unapprovedList)
	{
		_battleMgr = battlemgr;
		_registerActionManager = registerList;
		_registerUnapprovedList = unapprovedList;
	}

	public Dictionary<string, object> MakePlayActionsSendCardData(BattleCardBase playCardObj = null, List<BattleCardBase> targetCardList = null, List<NetworkBattleManagerBase.ValidateSkillData> validateSkillIndexList = null, bool isEvol = false, SendKeyActionDataManager sendKeyActionDataManager = null, bool isTurnStart = false, List<int> selectTypeSkillIndexList = null)
	{
		int nowCardIndex = -1;
		if (playCardObj != null)
		{
			nowCardIndex = playCardObj.Index;
		}
		SwapTransformMetamorphoseData();
		Dictionary<string, object> result = MakeCommonSendAndEchoCardData(nowCardIndex, playCardObj, targetCardList, validateSkillIndexList, isEvol, sendKeyActionDataManager, isEcho: false, isTurnStart, selectTypeSkillIndexList);
		ClearRegisterCards();
		return result;
	}

	public Dictionary<string, object> MakeEchoData(int playCardIdx, NetworkBattleDefine.PlayActionType actionType, SendKeyActionDataManager sendKeyActionDataManager)
	{
		BattleCardBase playCardObj = null;
		if (playCardIdx != -1)
		{
			playCardObj = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattleEnemy, playCardIdx);
		}
		InsertionTransformData(playCardObj);
		Dictionary<string, object> dictionary = MakeCommonSendAndEchoCardData(playCardIdx, playCardObj, null, null, isEvol: false, sendKeyActionDataManager, isEcho: true);
		if (actionType != NetworkBattleDefine.PlayActionType.NONE)
		{
			dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.type]] = (int)actionType;
		}
		ClearRegisterCards();
		return dictionary;
	}

	private Dictionary<string, object> MakeCommonSendAndEchoCardData(int nowCardIndex = -1, BattleCardBase playCardObj = null, List<BattleCardBase> targetCardObj = null, List<NetworkBattleManagerBase.ValidateSkillData> validateSkillIndexList = null, bool isEvol = false, SendKeyActionDataManager sendKeyActionDataManager = null, bool isEcho = false, bool isTurnStart = false, List<int> selectTypeSkillIndexList = null)
	{
		Dictionary<string, object> dictionary = MakeBasicSendCardData(nowCardIndex, targetCardObj, validateSkillIndexList, isEvol, sendKeyActionDataManager, selectTypeSkillIndexList);
		List<object> list = null;
		if (!isEcho && _registerUnapprovedList.Count >= 1)
		{
			DisCardCheckAndRemoveUlist();
			if (_registerUnapprovedList.Count >= 1)
			{
				list = MakeUList(_registerUnapprovedList);
			}
		}
		GatheredRegisterCard();
		SettingStateChangeCardToSkillTarget();
		InsertionTokenAfterStateChange();
		InsertionExtractAfterValidate();
		List<object> list2 = OrderListCreate(isTurnStart);
		if (list2.Count >= 1)
		{
			dictionary.Add(NetworkBattleSenderDefine.SendCommonParameter.orderList.ToString(), list2);
		}
		if (!isEcho && list != null)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameter.uList.ToString(), list);
		}
		return dictionary;
	}

	private void SwapTransformMetamorphoseData()
	{
		List<RegisterActionBase> registerDataList = _registerActionManager.RegisterDataList;
		RegisterMetamorphoseData registerMetamorphoseData = null;
		foreach (RegisterActionBase item in registerDataList)
		{
			if (item is RegisterMetamorphoseData && ((RegisterMetamorphoseData)item).IsChoice)
			{
				registerMetamorphoseData = (RegisterMetamorphoseData)item;
			}
		}
		if (registerMetamorphoseData != null)
		{
			registerDataList.Remove(registerMetamorphoseData);
			registerDataList.Insert(0, registerMetamorphoseData);
		}
	}

	private Dictionary<string, object> MakeBasicSendCardData(int nowCardIndex, List<BattleCardBase> targetCardList, List<NetworkBattleManagerBase.ValidateSkillData> validateDataList, bool isEvol, SendKeyActionDataManager sendKeyActionDataManager, List<int> selectTypeSkillIndexList)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (nowCardIndex == -1)
		{
			return dictionary;
		}
		BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattlePlayer, nowCardIndex);
		dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.playIdx]] = nowCardIndex;
		if (targetCardList != null && targetCardList.Count >= 1)
		{
			List<object> list = new List<object>();
			int num = 0;
			foreach (BattleCardBase targetCard in targetCardList)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.targetIdx]] = targetCard.Index;
				dictionary2[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isSelf]] = (targetCard.IsPlayer ? 1 : 0);
				if (selectTypeSkillIndexList != null && selectTypeSkillIndexList.Count() > 0)
				{
					dictionary2[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.selectSkillIndex]] = new List<int>(selectTypeSkillIndexList);
				}
				if (validateDataList != null && validateDataList.Exists((NetworkBattleManagerBase.ValidateSkillData x) => x.isPlayer == targetCard.IsPlayer && x.CardIndex == targetCard.Index))
				{
					List<NetworkBattleManagerBase.ValidateSkillData> list2 = validateDataList.FindAll((NetworkBattleManagerBase.ValidateSkillData x) => x.CardIndex == targetCard.Index && x.isPlayer == targetCard.IsPlayer);
					List<int> list3 = new List<int>();
					foreach (NetworkBattleManagerBase.ValidateSkillData item in list2)
					{
						list3.Add(item.SkillIndex);
					}
					dictionary2[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.skillIndex]] = list3;
					num++;
				}
				else if (validateDataList != null && indexToCardBase.IsHaveBurialRiteJudgeBothFlag)
				{
					List<int> list4 = new List<int>();
					foreach (NetworkBattleManagerBase.ValidateSkillData validateData in validateDataList)
					{
						list4.Add(validateData.SkillIndex);
					}
					dictionary2[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.skillIndex]] = list4;
				}
				list.Add(dictionary2);
			}
			dictionary[NetworkBattleDefine.NetworkParameter.targetList.ToString()] = list;
		}
		if (sendKeyActionDataManager != null)
		{
			dictionary = sendKeyActionDataManager.MakeSendData(dictionary, nowCardIndex);
		}
		return dictionary;
	}

	private void ClearRegisterCards()
	{
		_battleMgr.ClearRegisterCardList();
	}

	private List<object> MakeUList(List<RegisterUnapproved> moveObjs)
	{
		int num = moveObjs.Count;
		for (int i = 0; i < num - 1; i++)
		{
			RegisterUnapproved registerUnapproved = moveObjs[i];
			RegisterUnapproved registerUnapproved2 = moveObjs[i + 1];
			if (registerUnapproved.FromPlaceState == registerUnapproved2.FromPlaceState && registerUnapproved.ToPlaceState == registerUnapproved2.ToPlaceState && registerUnapproved.IsPlayer == registerUnapproved2.IsPlayer && registerUnapproved.SkillCardIdx == registerUnapproved2.SkillCardIdx && registerUnapproved.PublishedActiveSkillCount == registerUnapproved2.PublishedActiveSkillCount && registerUnapproved.Movement == registerUnapproved2.Movement && registerUnapproved.CardId == registerUnapproved2.CardId && registerUnapproved.AttachedSkillsPublishCount.SequenceEqual(registerUnapproved2.AttachedSkillsPublishCount))
			{
				registerUnapproved.IndexList.Add(registerUnapproved2.IndexList[0]);
				if (registerUnapproved2.RandomTargetIdx.Count > 0)
				{
					registerUnapproved.RandomTargetIdx.Add(registerUnapproved2.RandomTargetIdx[0]);
				}
				moveObjs.Remove(registerUnapproved2);
				num--;
				i--;
			}
		}
		List<object> list = new List<object>();
		for (int j = 0; j < moveObjs.Count; j++)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (moveObjs[j].CardId != 0)
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cardId], moveObjs[j].CardId);
			}
			if (moveObjs[j].Clan != -1)
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.clan], moveObjs[j].Clan);
			}
			if (moveObjs[j].Cost != -1)
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cost], moveObjs[j].Cost);
			}
			if (moveObjs[j].IsShortageDeck)
			{
				moveObjs[j].IndexList.Add(-99);
			}
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idxList], moveObjs[j].IndexList);
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.from], (int)moveObjs[j].FromPlaceState);
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.to], (int)moveObjs[j].ToPlaceState);
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isSelf], moveObjs[j].IsPlayer ? 1 : 0);
			string text = "";
			text = text + moveObjs[j].SkillCardIdx + "|";
			text = text + moveObjs[j].PublishedActiveSkillCount + "|";
			text += moveObjs[j].Movement;
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.skill], text);
			if (moveObjs[j].SkillKeyCardIdxList.Count >= 1)
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.skillKeyCardIdx], moveObjs[j].SkillKeyCardIdxList);
			}
			if (moveObjs[j].RandomTargetIdx.Count > 0)
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.randomTargetIdx], moveObjs[j].RandomTargetIdx);
			}
			if (moveObjs[j].IsInvoked)
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isInvoke], 1);
			}
			if (moveObjs[j].AttachedSkillsPublishCount.Count > 0)
			{
				string text2 = "";
				for (int k = 0; k < moveObjs[j].AttachedSkillsPublishCount.Count; k++)
				{
					if (k != 0)
					{
						text2 += ",";
					}
					text2 += moveObjs[j].AttachedSkillsPublishCount[k];
				}
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.attachTarget], text2);
			}
			list.Add(dictionary);
		}
		return list;
	}

	private List<object> OrderListCreate(bool isTurnStart)
	{
		List<object> list = new List<object>();
		if (isTurnStart)
		{
			List<object> list2 = new List<object>();
			List<object> list3 = new List<object>();
			foreach (RegisterActionBase registerData in _registerActionManager.RegisterDataList)
			{
				Dictionary<string, object> cardData = new Dictionary<string, object>();
				Dictionary<string, object> dictionary = MakeRegisterData(cardData, registerData);
				if (dictionary.Keys.Any((string key) => key == RegisterTool.OrderListParameter.skillConditionCheck.ToString()))
				{
					list2.Add(dictionary);
				}
				else
				{
					list3.Add(dictionary);
				}
			}
			list.AddRange(list2);
			list.AddRange(list3);
		}
		else
		{
			foreach (RegisterActionBase registerData2 in _registerActionManager.RegisterDataList)
			{
				Dictionary<string, object> cardData2 = new Dictionary<string, object>();
				list.Add(MakeRegisterData(cardData2, registerData2));
			}
		}
		return list;
	}

	private Dictionary<string, object> MakeRegisterData(Dictionary<string, object> cardData, RegisterActionBase registerCard)
	{
		string uriMsg = registerCard.GetUriMsg();
		Dictionary<string, object> dictionary = registerCard.MakeSendData();
		if (dictionary == null)
		{
			return cardData;
		}
		cardData.Add(uriMsg, dictionary);
		return cardData;
	}

	private void SettingStateChangeCardToSkillTarget()
	{
		List<RegisterActionBase> registerDataList = _registerActionManager.RegisterDataList;
		foreach (RegisterActionBase item in registerDataList)
		{
			if (!(item is RegisterSkillConditionCheck))
			{
				continue;
			}
			RegisterSkillConditionCheck registerSkillConditionCheck = item as RegisterSkillConditionCheck;
			if (registerSkillConditionCheck.SkillTargetId == 0)
			{
				continue;
			}
			foreach (int targetCardIndex in registerSkillConditionCheck.TargetCardIndexs)
			{
				foreach (RegisterActionBase item2 in registerDataList)
				{
					if (item2 is RegisterStateChangeCard)
					{
						RegisterStateChangeCard registerStateChangeCard = item2 as RegisterStateChangeCard;
						if (registerStateChangeCard.StateCard.Index == targetCardIndex && registerStateChangeCard.StateCard.IsPlayer == registerSkillConditionCheck.IsSelf && (registerStateChangeCard.FromPlaceState != NetworkBattleDefine.NetworkCardPlaceState.Hand || registerStateChangeCard.ToPlaceState != NetworkBattleDefine.NetworkCardPlaceState.Banish))
						{
							registerStateChangeCard.SkillTargetList.Add(registerSkillConditionCheck.SkillTargetId.ToString());
						}
					}
				}
			}
		}
	}

	private void GatheredRegisterCard()
	{
		List<RegisterActionBase> registerDataList = _registerActionManager.RegisterDataList;
		int num = registerDataList.Count;
		if (num <= 1)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			if (registerDataList[i] is RegisterStateChangeCard)
			{
				(registerDataList[i] as RegisterStateChangeCard).SetOpenIfWhenDraw();
				int num2 = i + 1;
				while (num2 < num && registerDataList[num2] is RegisterStateChangeCard)
				{
					RegisterStateChangeCard registerStateChangeCard = registerDataList[i] as RegisterStateChangeCard;
					RegisterStateChangeCard registerStateChangeCard2 = registerDataList[num2] as RegisterStateChangeCard;
					if (registerStateChangeCard.StateCard == null || registerStateChangeCard2.StateCard == null || registerStateChangeCard.StateCard.IsPlayer != registerStateChangeCard2.StateCard.IsPlayer || registerStateChangeCard.FromPlaceState != registerStateChangeCard2.FromPlaceState || registerStateChangeCard.Skill != registerStateChangeCard2.Skill || registerStateChangeCard.ToPlaceState != registerStateChangeCard2.ToPlaceState || !registerStateChangeCard.SkillTargetList.SequenceEqual(registerStateChangeCard2.SkillTargetList) || !(registerStateChangeCard.PrivateGroupIndexMsg == registerStateChangeCard2.PrivateGroupIndexMsg) || registerStateChangeCard.IsFlood != registerStateChangeCard2.IsFlood)
					{
						break;
					}
					registerStateChangeCard.IndexList.AddRange(registerStateChangeCard2.IndexList);
					registerStateChangeCard2.SetOpenIfWhenDraw();
					if (registerStateChangeCard2.OpenCardIndexList.Count > 0)
					{
						int num3 = registerStateChangeCard.IndexList.IndexOf(registerStateChangeCard2.IndexList[0]);
						if (num3 >= 0)
						{
							registerStateChangeCard.OpenCardIndexList.Add(num3);
						}
					}
					registerStateChangeCard.HasGuardList.AddRange(registerStateChangeCard2.HasGuardList);
					registerDataList.Remove(registerStateChangeCard2);
					num2--;
					num--;
					num2++;
				}
			}
			else if (registerDataList[i] is RegisterToken)
			{
				RegisterToken registerToken = registerDataList[i] as RegisterToken;
				int num4 = i + 1;
				while (num4 < num && registerDataList[num4] is RegisterToken)
				{
					RegisterToken registerToken2 = registerDataList[num4] as RegisterToken;
					if (!registerToken.IsMatchData(registerToken, registerToken2))
					{
						break;
					}
					registerToken.IndexList.AddRange(registerToken2.IndexList);
					registerDataList.Remove(registerToken2);
					num4--;
					num--;
					num4++;
				}
			}
			else
			{
				if (!(registerDataList[i] is RegisterMetamorphoseData))
				{
					continue;
				}
				int num5 = i + 1;
				while (num5 < num && registerDataList[num5] is RegisterMetamorphoseData)
				{
					RegisterMetamorphoseData registerMetamorphoseData = registerDataList[i] as RegisterMetamorphoseData;
					RegisterMetamorphoseData registerMetamorphoseData2 = registerDataList[num5] as RegisterMetamorphoseData;
					if (registerMetamorphoseData.IsSelf != registerMetamorphoseData2.IsSelf || registerMetamorphoseData.AfterId != registerMetamorphoseData2.AfterId)
					{
						break;
					}
					registerMetamorphoseData.IndexList.AddRange(registerMetamorphoseData2.IndexList);
					registerDataList.Remove(registerMetamorphoseData2);
					num5--;
					num--;
					num5++;
				}
			}
		}
	}

	private void DisCardCheckAndRemoveUlist()
	{
		List<RegisterActionBase> registerDataList = _registerActionManager.RegisterDataList;
		if (registerDataList.Count == 0)
		{
			return;
		}
		for (int i = 0; i < registerDataList.Count; i++)
		{
			if (!(registerDataList[i] is RegisterStateChangeCard { FromPlaceState: NetworkBattleDefine.NetworkCardPlaceState.Hand, ToPlaceState: NetworkBattleDefine.NetworkCardPlaceState.Cemetery } registerStateChangeCard))
			{
				continue;
			}
			for (int j = 0; j < _registerUnapprovedList.Count; j++)
			{
				RegisterUnapproved registerUnapproved = _registerUnapprovedList[j];
				if (registerUnapproved.FromPlaceState != NetworkBattleDefine.NetworkCardPlaceState.Hand || registerUnapproved.ToPlaceState != NetworkBattleDefine.NetworkCardPlaceState.Cemetery)
				{
					continue;
				}
				for (int k = 0; k < registerUnapproved.IndexList.Count; k++)
				{
					if (registerStateChangeCard.IndexList.Contains(registerUnapproved.IndexList[k]) && registerUnapproved.IsSelfDiscard())
					{
						registerStateChangeCard.AddOpenCardIndex(0);
						registerUnapproved.RemoveIndexList(registerUnapproved.IndexList[k]);
						k--;
						if (registerUnapproved.IndexList.Count == 0)
						{
							_registerUnapprovedList.Remove(registerUnapproved);
							j--;
						}
					}
				}
			}
		}
	}

	private void InsertionTokenAfterStateChange()
	{
		List<RegisterActionBase> registerDataList = _registerActionManager.RegisterDataList;
		int num = registerDataList.Count;
		for (int i = 0; i < num; i++)
		{
			if (!(registerDataList[i] is RegisterToken))
			{
				continue;
			}
			RegisterToken registerToken = registerDataList[i] as RegisterToken;
			RegisterStateChangeCard registerStateChangeCard = new RegisterStateChangeCard(null, NetworkBattleDefine.NetworkCardPlaceState.None, registerToken.ToPlaceState, registerToken.Skill, isOpen: false, registerToken.ToPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Cemetery);
			registerStateChangeCard.IsSelf = registerToken.IsSelf;
			List<int> list = registerToken.IndexList;
			RegisterChoiceAdd choiceAdd = registerToken as RegisterChoiceAdd;
			if (choiceAdd != null && choiceAdd.OverFlowIndexList.Count > 0)
			{
				list = list.Where((int index) => !choiceAdd.OverFlowIndexList.Contains(index)).ToList();
			}
			registerStateChangeCard.SetIndexList(list);
			_registerActionManager.RegisterDataList.Insert(i + 1, registerStateChangeCard);
			num++;
			if (choiceAdd != null && choiceAdd.OverFlowIndexList.Count > 0)
			{
				registerStateChangeCard = new RegisterStateChangeCard(null, NetworkBattleDefine.NetworkCardPlaceState.None, NetworkBattleDefine.NetworkCardPlaceState.Cemetery, registerToken.Skill, isOpen: false, isFlood: true);
				registerStateChangeCard.IsSelf = registerToken.IsSelf;
				registerStateChangeCard.SetIndexList(choiceAdd.OverFlowIndexList);
				_registerActionManager.RegisterDataList.Insert(i + 2, registerStateChangeCard);
				num++;
			}
		}
	}

	private void InsertionExtractAfterValidate()
	{
		List<RegisterActionBase> registerDataList = _registerActionManager.RegisterDataList;
		int count = registerDataList.Count;
		for (int i = 0; i < count; i++)
		{
			if (registerDataList[i] is RegisterExtract item)
			{
				int num = registerDataList.FindLastIndex((RegisterActionBase o) => o is RegisterValidate && !(o is RegisterScan));
				if (num != -1)
				{
					_registerActionManager.RegisterDataList.Remove(item);
					_registerActionManager.RegisterDataList.Insert(num + 1, item);
				}
				break;
			}
		}
	}

	private void InsertionTransformData(BattleCardBase playCardObj)
	{
		NetworkBattleReceiver.ReceiveData receiveData = _battleMgr.networkBattleData.GetReceiveData();
		if (receiveData != null && receiveData.keyActionType.Count >= 1 && !receiveData.IsFusion && (!receiveData.IsChoice || receiveData.IsTransformChoice || receiveData.IsAcceleratedOrCrystallize) && (receiveData.keyActionType.Count != 1 || !receiveData.IsBurialRate))
		{
			int transformBeforeCardId = receiveData.transformBeforeCardId;
			if (transformBeforeCardId != playCardObj.CardId)
			{
				RegisterValidate registerValidate = new RegisterValidate();
				registerValidate.AddIncludeList(CardMaster.GetInstanceForBattle().GetCardParameterFromId(transformBeforeCardId).BaseCardId, playCardObj.Index);
				_registerActionManager.RegisterDataList.Insert(0, registerValidate);
				int num = 0;
				num = ((!receiveData.IsChoice || receiveData.IsAcceleratedOrCrystallize) ? playCardObj.CardId : receiveData.choiceIdList[0]);
				_registerActionManager.RegisterDataList.Insert(1, new RegisterMetamorphoseData(num, playCardObj.Index, playCardObj.IsPlayer, null, receiveData.IsChoice ? true : false));
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.RoomMatch;

public class NetworkWatchBattleReceiver : NetworkBattleReceiver
{
	private List<CardDataModel> _lastUnapprovedList;

	public NetworkWatchBattleReceiver(NetworkBattleManagerBase battlemgr)
		: base(battlemgr)
	{
	}

	public override void ReceivedTouchData(NetworkBattleDefine.NetworkBattleURI uri, bool isHaveSequence, Dictionary<string, object> data, bool isPlayer = false, WatchDataHandler handler = null)
	{
		ConvertReciveTouchDataToMakeData(uri, isHaveSequence, data, isPlayer, handler);
		if (!receiveStop)
		{
			if (isHaveSequence)
			{
				networkBattleMgr.ConductReceiveData(_receiveData, isPlayer);
			}
			else
			{
				networkBattleMgr.ConductReceiveData_NotHaveSequence(_receiveData, isPlayer);
			}
		}
	}

	protected override bool ConvertReceiveDataToMakeData(NetworkBattleDefine.NetworkBattleURI uri, bool isHaveSequence, Dictionary<string, object> datas, bool isPlayer = false, WatchDataHandler handler = null)
	{
		bool result = base.ConvertReceiveDataToMakeData(uri, isHaveSequence, datas, isPlayer, handler);
		try
		{
			_receiveData.result = RESULT_CODE.NotFinish;
			foreach (KeyValuePair<string, object> data in datas)
			{
				if (!Enum.IsDefined(typeof(NetworkBattleDefine.NetworkParameter), data.Key.ToString()))
				{
					continue;
				}
				switch ((NetworkBattleDefine.NetworkParameter)Enum.Parse(typeof(NetworkBattleDefine.NetworkParameter), data.Key.ToString()))
				{
				case NetworkBattleDefine.NetworkParameter.vid:
					if (handler != null)
					{
						_receiveData.isSelf = (IsSelfVid(ConvertToInt(data.Value), handler) ? true : false);
					}
					break;
				case NetworkBattleDefine.NetworkParameter.finishData:
					foreach (object item in data.Value as List<object>)
					{
						Dictionary<string, object> resultData = item as Dictionary<string, object>;
						SetResultData(resultData, handler);
					}
					break;
				case NetworkBattleDefine.NetworkParameter.result:
					if (handler != null)
					{
						SetResultData(datas, handler);
					}
					break;
				case NetworkBattleDefine.NetworkParameter.uList:
					_lastUnapprovedList = _receiveData.unapprovedList;
					break;
				case NetworkBattleDefine.NetworkParameter.value:
				{
					List<object> list = data.Value as List<object>;
					switch (uri)
					{
					case NetworkBattleDefine.NetworkBattleURI.SelectSkill:
						ParseSelectSkillData(list);
						break;
					case NetworkBattleDefine.NetworkBattleURI.SelectObject:
					{
						string text = list[0].ToString();
						_receiveData._selectObjectTargetType = (NetworkBattleSender.SELECT_OBJECT_TARGET_TYPE)Enum.Parse(typeof(NetworkBattleSender.SELECT_OBJECT_TARGET_TYPE), text[0].ToString());
						if (_receiveData._selectObjectTargetType == NetworkBattleSender.SELECT_OBJECT_TARGET_TYPE.Select)
						{
							_receiveData._isPlayerCard = text[1].Equals('1');
							_receiveData.idx = int.Parse(text.Substring(2, text.Length - 2));
						}
						break;
					}
					case NetworkBattleDefine.NetworkBattleURI.TurnEndReady:
						_receiveData._isNotTurnEndReady = handler.IsIncludedUri(NetworkBattleDefine.NetworkURINames[NetworkBattleDefine.NetworkBattleURI.TurnEnd]);
						_receiveData._isShortenedTurn = bool.Parse(list[0].ToString());
						break;
					case NetworkBattleDefine.NetworkBattleURI.SlideObject:
					{
						string startPoint = list[0].ToString();
						string endPoint = list[1].ToString();
						CreateSlideObjectReceiveData(startPoint, endPoint);
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
			return result;
		}
		catch
		{
			LocalLog.AccumulateLastTraceLog("ConvertReciveDataToMakeData Watch Error");
			return false;
		}
	}

	private void SetResultData(Dictionary<string, object> resultData, WatchDataHandler handler)
	{
		if (IsSelfVid(ConvertToInt(resultData[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.vid]]), handler))
		{
			_receiveData.result = (RESULT_CODE)ConvertToInt(resultData[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.result]]);
		}
		else
		{
			_receiveData.opponentResult = (RESULT_CODE)ConvertToInt(resultData[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.result]]);
		}
	}

	private void ParseSelectSkillData(List<object> selectSkillData)
	{
		_receiveData._selectSkillOperation = (NetworkBattleSender.SELECT_SKILL_OPERATION)Enum.Parse(typeof(NetworkBattleSender.SELECT_SKILL_OPERATION), selectSkillData[0].ToString());
		_receiveData._isEvolveTargetSelect = bool.Parse(selectSkillData[1].ToString());
		_receiveData._isBurialRiteSelect = bool.Parse(selectSkillData[2].ToString());
		string text = selectSkillData[3].ToString();
		switch (_receiveData._selectSkillOperation)
		{
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartSelect:
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartChoiceSelect:
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartFusionSelect:
			_receiveData.idx = int.Parse(text);
			if (selectSkillData.Count > 5)
			{
				_receiveData.ActiveSelectSkillIndexList.AddRange((selectSkillData[5] as List<object>).Select((object d) => int.Parse(d.ToString())));
			}
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectCard:
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteSelect:
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectFusionIngredient:
		{
			string text2 = text.Substring(0, 1);
			string s = text.Substring(1, 3);
			_receiveData._isPlayerCard = text2.Equals("1");
			_receiveData._selectedCardIndex = int.Parse(s);
			_receiveData.unapprovedList = _lastUnapprovedList;
			_lastUnapprovedList = null;
			if (selectSkillData.Count > 4)
			{
				_receiveData.IsChoiceBraveSelect = selectSkillData[4].ToString() == "1";
			}
			break;
		}
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectChoiceCard:
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteChoiceSelect:
			if (text.Contains(","))
			{
				string[] array = text.Split(',');
				for (int i = 0; i < array.Count(); i++)
				{
					_receiveData._selectedChoiceCardIdList.Add(int.Parse(array[i]));
				}
			}
			else if (text != string.Empty)
			{
				_receiveData._selectedChoiceCardIdList.Add(int.Parse(text));
			}
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CancelSelect:
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CancelChoiceSelect:
			break;
		}
	}

	private void ConvertReciveTouchDataToMakeData(NetworkBattleDefine.NetworkBattleURI uri, bool isHaveSequence, Dictionary<string, object> datas, bool isPlayer = false, WatchDataHandler handler = null)
	{
		if (isHaveSequence)
		{
			_receiveData = new ReceiveData();
		}
		_receiveData.dataUri = uri;
		try
		{
			foreach (KeyValuePair<string, object> data in datas)
			{
				if (Enum.IsDefined(typeof(NetworkBattleDefine.NetworkParameter), data.Key.ToString()) && (NetworkBattleDefine.NetworkParameter)Enum.Parse(typeof(NetworkBattleDefine.NetworkParameter), data.Key.ToString()) == NetworkBattleDefine.NetworkParameter.touch)
				{
					List<object> list = data.Value as List<object>;
					_receiveData.idx = ConvertToInt(list[1]);
					_receiveData.isSelf = isPlayer;
				}
			}
		}
		catch
		{
			LocalLog.AccumulateLastTraceLog("ConvertReciveTouchDataToMakeData Error");
			Debug.LogError("ConvertReciveTouchDataToMakeData Error");
		}
	}

	protected override void CheckDuplicateDataAddtoList(List<CardDataModel> list, CardDataModel cardDataModel)
	{
		CardDataModel cardDataModel2 = list.Find((CardDataModel x) => x.Index == cardDataModel.Index && x.CardId == cardDataModel.CardId && x.isOpponent == cardDataModel.isOpponent && x.publishedActiveSkillCount == cardDataModel.publishedActiveSkillCount && x.IsOpen == cardDataModel.IsOpen && x.skillMovementNum == cardDataModel.skillMovementNum);
		if (cardDataModel2 == null)
		{
			list.Add(cardDataModel);
			return;
		}
		SkillBase skillBase = networkBattleMgr.PublishedSkillList.FirstOrDefault((SkillBase s) => s.PublishedActiveSkillCount == cardDataModel.publishedActiveSkillCount);
		if (skillBase != null && skillBase.OnWhenDrawOtherStart != 0)
		{
			list.Add(cardDataModel);
		}
		else
		{
			cardDataModel2.ToStateList.Add(cardDataModel.ToStateList.First());
		}
	}

	protected override List<CardDataModel> MakeReceiveCardData(object cardData, BattleManagerBase mgr, WatchDataHandler handler)
	{
		Dictionary<string, object> dictionary = cardData as Dictionary<string, object>;
		List<CardDataModel> list = base.MakeReceiveCardData(cardData, mgr, handler);
		foreach (CardDataModel item in list)
		{
			if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cardId]))
			{
				item.CardId = ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cardId]]);
				item.playCardCost = CardMaster.GetInstanceForBattle().GetCardParameterFromId(item.CardId).Cost;
			}
			else if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cardId]))
			{
				item.CardId = ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cardId]]);
				item.playCardCost = CardMaster.GetInstanceForBattle().GetCardParameterFromId(item.CardId).Cost;
			}
			if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cost]))
			{
				item.playCardCost = ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.cost]]);
			}
			if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.pos]))
			{
				item.RedrawCardPosition = ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.pos]]);
			}
			if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.vid]))
			{
				item.isOpponent = ((handler != null) ? (!handler.isOwner(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.vid]].ToString())) : (ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.vid]].ToString()) != networkBattleMgr.InstanceViewerId));
			}
			if (dictionary.ContainsKey(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.is_open]))
			{
				item.IsOpen = ConvertToInt(dictionary[NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.is_open]]) == 1;
			}
		}
		return list;
	}

	private bool IsSelfVid(int vid, WatchDataHandler handler = null)
	{
		return handler.isOwner(vid.ToString());
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIPlayptnRecorder
{
	private List<AISinglePlayptnRecord> _allPlayPtnList;

	private List<AIVirtualField> _referenceFieldList;

	private bool _isRegisteringPlayPtn;

	private bool _isCalculatingPriority;

	public List<AISinglePlayptnRecord> ValidPlayPtnList { get; private set; }

	public AIPlayptnRecorder()
	{
		ValidPlayPtnList = new List<AISinglePlayptnRecord>();
		_allPlayPtnList = new List<AISinglePlayptnRecord>();
		_referenceFieldList = new List<AIVirtualField>();
		_isRegisteringPlayPtn = false;
	}

	public static void BuildSinglePlayptnRecord(List<int> playPtn, AISinglePlayptnRecord currentRecord, AIVirtualField field, AIVirtualFieldRollBackBasicProcessor rollBackProcessor)
	{
		List<int> list = new List<int>();
		int num = 0;
		for (int i = 0; i < currentRecord.PlayedCardList.Count; i++)
		{
			if (!currentRecord.PlayedCardList[i].IsProcessed)
			{
				num = i;
				break;
			}
			list.Add(playPtn[i]);
		}
		for (int j = num; j < playPtn.Count; j++)
		{
			int num2 = playPtn[j];
			list.Add(num2);
			PlayedCardInfo playedCardInfo = currentRecord.PlayedCardList[j];
			AIVirtualCard aIVirtualCard = field.AllyHandCards[num2];
			AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.PLAY);
			PlaySimulationType playType;
			int num3 = (playedCardInfo.UsedCost = aIVirtualCard.GetUseCost(currentRecord.LastRestPp, list, aIVirtualTargetSelectAction, out playType));
			if (num3 >= 0)
			{
				int num4 = currentRecord.LastRestPp - num3;
				playedCardInfo.PlayType = playType;
				num4 += AIEvaluateBonusFromOhterUtility.GetTotalRecoverPp(aIVirtualCard, playPtn, aIVirtualTargetSelectAction);
				playedCardInfo.RestPp = num4;
				playedCardInfo.DrawCount = aIVirtualCard.GetPlayDrawCount(list, aIVirtualTargetSelectAction);
				SetTransformCardToPlayedInfo(playedCardInfo, aIVirtualCard, field, list, aIVirtualTargetSelectAction, num3, playType);
			}
			else
			{
				playedCardInfo.RestPp = -1;
				playedCardInfo.PlayType = PlaySimulationType.Undefined;
			}
			if (num == 0)
			{
				playedCardInfo.IsPassedPlayCondition = aIVirtualCard.BaseCard.BaseMovable;
			}
			else
			{
				playedCardInfo.IsPassedPlayCondition = true;
			}
			currentRecord.UpdatePlayPtnRecord(playedCardInfo);
			if (!currentRecord.IsValid)
			{
				break;
			}
			AIPreprocessSimulationUtility.SimulatePreprocess(aIVirtualTargetSelectAction.Actor, aIVirtualTargetSelectAction, field, AIScriptTokenArgType.WHEN_PLAY, isPseudo: true);
			currentRecord.RegisterPreprocess(playedCardInfo, aIVirtualTargetSelectAction, field);
			PseudoExecuteCardPlayProcess(field, currentRecord, playedCardInfo, aIVirtualTargetSelectAction);
		}
		rollBackProcessor.ResetVirtualFieldToStart();
	}

	public void CreateValidPlayPtnList(AIVirtualField field)
	{
		AIVirtualFieldRollBackBasicProcessor rollBackProcessor = new AIVirtualFieldRollBackBasicProcessor(field);
		int fieldIndex = RegisterFieldAndGetFieldIndex(field);
		List<Tuple<int, List<int>>> list = AIHandPtnCalculator.CreateSortedPlayPtnList(field.AI);
		_isRegisteringPlayPtn = true;
		for (int i = 0; i < list.Count; i++)
		{
			AISinglePlayptnRecord aISinglePlayptnRecord = new AISinglePlayptnRecord(list[i].second, field, fieldIndex);
			_allPlayPtnList.Add(aISinglePlayptnRecord);
			ProcessPlayPtnRecord(aISinglePlayptnRecord, field, rollBackProcessor);
		}
		_isRegisteringPlayPtn = false;
	}

	private void ProcessPlayPtnRecord(AISinglePlayptnRecord currentRecord, AIVirtualField field, AIVirtualFieldRollBackBasicProcessor rollBackProcessor)
	{
		List<int> playPtn = currentRecord.PlayPtn;
		AISinglePlayptnRecord aISinglePlayptnRecord = FindMostlyMatchedPlayPtn(currentRecord);
		if (aISinglePlayptnRecord != null)
		{
			if (!aISinglePlayptnRecord.IsValid)
			{
				_allPlayPtnList.Remove(currentRecord);
				return;
			}
			currentRecord = RegisterMatchedRecordToNewRecord(aISinglePlayptnRecord, field, currentRecord);
		}
		if (!currentRecord.IsValid)
		{
			rollBackProcessor.ResetVirtualFieldToStart();
			return;
		}
		BuildSinglePlayptnRecord(playPtn, currentRecord, field, rollBackProcessor);
		if (currentRecord.IsToBeRegister() && currentRecord.IsValid && !CheckSimilarValidPlayPtnRecord(currentRecord.PlayPtnHash))
		{
			ValidPlayPtnList.Add(currentRecord);
		}
	}

	public AISinglePlayptnRecord FindMatchedPlayPtnRecord(List<int> playPtn, AIVirtualField playPtnField)
	{
		ulong hash = playPtnField.GetHash();
		int num = GetFieldIndex(hash);
		for (int i = 0; i < _allPlayPtnList.Count; i++)
		{
			AISinglePlayptnRecord aISinglePlayptnRecord = _allPlayPtnList[i];
			if (aISinglePlayptnRecord.PlayPtn.Count != playPtn.Count || num != aISinglePlayptnRecord.ReferenceFieldIndex)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < playPtn.Count; j++)
			{
				if (!playPtnField.AllyHandCards[playPtn[j]].IsSameCard(aISinglePlayptnRecord.PlayedCardList[j].Card))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return aISinglePlayptnRecord;
			}
		}
		if (_isRegisteringPlayPtn)
		{
			AIConsoleUtility.LogError("FindMatchedPlayPtnRecordで無限ループが発生しそうです！！！！！");
			return null;
		}
		_isRegisteringPlayPtn = true;
		if (num < 0)
		{
			_referenceFieldList.Add(playPtnField);
			num = _referenceFieldList.Count - 1;
		}
		AISinglePlayptnRecord aISinglePlayptnRecord2 = new AISinglePlayptnRecord(playPtn, playPtnField, num);
		_allPlayPtnList.Add(aISinglePlayptnRecord2);
		AIVirtualFieldRollBackBasicProcessor rollBackProcessor = new AIVirtualFieldRollBackBasicProcessor(playPtnField);
		ProcessPlayPtnRecord(aISinglePlayptnRecord2, playPtnField, rollBackProcessor);
		_isRegisteringPlayPtn = false;
		return aISinglePlayptnRecord2;
	}

	public void StartCalculatingPriority()
	{
		_isCalculatingPriority = true;
	}

	public void StopCalculatingPriority()
	{
		_isCalculatingPriority = false;
	}

	public int GetRestPp(List<int> playPtn, AIVirtualField field)
	{
		if (_isCalculatingPriority)
		{
			AIConsoleUtility.LogWarning("AIPlayptnRecorder.GetRestPp() warning!! Calling GetRestPp() during calculating priority!!!!!");
			return 0;
		}
		int allyPp = field.AllyPp;
		if (playPtn == null || playPtn.Count <= 0)
		{
			return allyPp;
		}
		AISinglePlayptnRecord playptnRecordOnSim = field.GetPlayptnRecordOnSim(playPtn);
		if (playptnRecordOnSim == null)
		{
			return allyPp;
		}
		allyPp = playptnRecordOnSim.LastRestPp;
		return Mathf.Max(0, allyPp);
	}

	public int GetFixedCostWithCheckingPlayType(AIVirtualCard card, AIVirtualField field, List<int> playPtn, out PlaySimulationType playType)
	{
		int restPp = (card.IsAlly ? field.AllyPp : field.EnemyPp);
		if (playPtn == null || playPtn.Count <= 0 || !card.IsAlly)
		{
			return card.GetUseCost(restPp, playPtn, null, out playType);
		}
		int cardPlayPtnIndex = GetCardPlayPtnIndex(card, field, playPtn);
		if (cardPlayPtnIndex == -1)
		{
			return card.GetUseCost(restPp, playPtn, null, out playType);
		}
		return GetCardCostAndTypeFromPlayPtnRecord(card, cardPlayPtnIndex, field, playPtn, out playType);
	}

	public bool IsCardPlayingSimulationType(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation, PlaySimulationType targetPlayType)
	{
		List<int> list = (card.IsAlly ? playPtn : null);
		int restPp = (card.IsAlly ? field.AllyPp : field.EnemyPp);
		int num = -1;
		if (card.IsAlly && list != null && list.Count > 0)
		{
			num = field.AI.PlayPtnRecorder.GetCardPlayPtnIndex(card, field, list);
		}
		if (num != -1)
		{
			field.AI.PlayPtnRecorder.GetCardCostAndTypeFromPlayPtnRecord(card, num, field, list, out var playType);
			return playType == targetPlayType;
		}
		return card.IsPlayingSimulationType(restPp, playPtn, situation, targetPlayType);
	}

	public int GetCardPlaySimulationTypeCost(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation, PlaySimulationType targetPlayType)
	{
		List<int> list = (card.IsAlly ? playPtn : null);
		int restPp = (card.IsAlly ? field.AllyPp : field.EnemyPp);
		int num = -1;
		if (card.IsAlly && list != null && list.Count > 0)
		{
			num = field.AI.PlayPtnRecorder.GetCardPlayPtnIndex(card, field, list);
		}
		if (num != -1)
		{
			PlaySimulationType playType;
			int cardCostAndTypeFromPlayPtnRecord = field.AI.PlayPtnRecorder.GetCardCostAndTypeFromPlayPtnRecord(card, num, field, list, out playType);
			if (playType != targetPlayType)
			{
				return -1;
			}
			return cardCostAndTypeFromPlayPtnRecord;
		}
		return card.GetPlaySimulationTypeCost(restPp, playPtn, situation, targetPlayType);
	}

	public int GetCardPlayPtnIndex(AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		int item = -1;
		for (int i = 0; i < field.AllyHandCards.Count; i++)
		{
			if (field.AllyHandCards[i].IsSameCard(card))
			{
				item = i;
				break;
			}
		}
		int num = playPtn.IndexOf(item);
		if (num < 0)
		{
			AIConsoleUtility.LogError("AIPlayPtnRecorder.GetFixedCostWithCheckingPlayType() error!! card is not in playPtn!!!!!");
		}
		return num;
	}

	public int GetCardCostAndTypeFromPlayPtnRecord(AIVirtualCard card, int playPtnIndex, AIVirtualField field, List<int> playPtn, out PlaySimulationType playType)
	{
		AISinglePlayptnRecord aISinglePlayptnRecord = FindMatchedPlayPtnRecord(playPtn, field);
		playType = PlaySimulationType.Undefined;
		if (aISinglePlayptnRecord != null)
		{
			PlayedCardInfo playedCardInfo = aISinglePlayptnRecord.PlayedCardList[playPtnIndex];
			if (playedCardInfo.Card.IsSameCard(card))
			{
				playType = playedCardInfo.PlayType;
				return playedCardInfo.UsedCost;
			}
			AIConsoleUtility.LogError("AIPlayPtnRecorder.GetCardCostAndTypeInPlayPtnRecord() error!! Cannot find same card from matched playPtn record!!!!!");
			return -1;
		}
		AIConsoleUtility.LogError("AIPlayPtnRecorder.GetCardCostAndTypeInPlayPtnRecord() error!! Cannot find record!!!!!");
		return -1;
	}

	private AISinglePlayptnRecord RegisterMatchedRecordToNewRecord(AISinglePlayptnRecord matched, AIVirtualField field, AISinglePlayptnRecord newRecord)
	{
		for (int i = 0; i < matched.PlayedCardList.Count; i++)
		{
			PlayedCardInfo playedCardInfo = newRecord.PlayedCardList[i];
			playedCardInfo.CloneFromPartlyMatchedInfo(matched.PlayedCardList[i]);
			AIVirtualCard card = playedCardInfo.Card;
			AIVirtualCard obj = ((playedCardInfo.TransformCard == null) ? card : playedCardInfo.TransformCard);
			AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(obj, card, AIOperationType.PLAY);
			AIPreprocessSimulationUtility.SimulatePreprocess(obj, situation, field, AIScriptTokenArgType.WHEN_PLAY, isPseudo: true);
			newRecord.RegisterPreprocess(playedCardInfo, situation, field);
			newRecord.UpdatePlayPtnRecord(playedCardInfo);
			PseudoExecuteCardPlayProcess(field, newRecord, playedCardInfo, situation);
			if (!newRecord.IsValid)
			{
				break;
			}
		}
		return newRecord;
	}

	private AISinglePlayptnRecord FindMostlyMatchedPlayPtn(AISinglePlayptnRecord record)
	{
		AISinglePlayptnRecord result = null;
		int num = 0;
		List<int> playPtn = record.PlayPtn;
		for (int i = 0; i < _allPlayPtnList.Count; i++)
		{
			AISinglePlayptnRecord aISinglePlayptnRecord = _allPlayPtnList[i];
			int count = aISinglePlayptnRecord.PlayPtn.Count;
			if (record.ReferenceFieldIndex == aISinglePlayptnRecord.ReferenceFieldIndex && playPtn.Count > count && aISinglePlayptnRecord.IsMatchedPattern(playPtn) && count > num)
			{
				num = count;
				result = aISinglePlayptnRecord;
			}
		}
		return result;
	}

	private static void SetTransformCardToPlayedInfo(PlayedCardInfo info, AIVirtualCard playedCard, AIVirtualField field, List<int> playPtn, AIVirtualTargetSelectAction situation, int usedCost, PlaySimulationType type)
	{
		if (AIPlayCardSimulationUtility.SetRealActorToPlaySituation(situation, field, usedCost, type))
		{
			info.SetTransformCard(situation.Actor);
		}
	}

	private static void PseudoExecuteCardPlayProcess(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playedCardInfo, AIVirtualTargetSelectAction situation)
	{
		AIVirtualCard card = playedCardInfo.Card;
		if (card.IsSpell || card.IsAccelerated(field, null, situation))
		{
			AIPlayCardSimulationUtility.UpdateFieldWhenEvaluateSpellCard(card, field);
		}
		if (card.IsUnit)
		{
			record.FirstSummonedAllyFollower = card;
		}
		field.PlayedCardContainer.AddPlayedCard(card);
		card.PseudoExecuteWhenPlayTags(field, record, playedCardInfo, situation);
		field.AllActivateCountHolderIncrement(situation, AIPlayTagType.PlayActivateCount, card);
		field.IsNoInstantAttackRecheck();
	}

	private bool CheckSimilarValidPlayPtnRecord(ulong currentRecordHash)
	{
		for (int i = 0; i < ValidPlayPtnList.Count; i++)
		{
			if (ValidPlayPtnList[i].PlayPtnHash == currentRecordHash)
			{
				return true;
			}
		}
		return false;
	}

	private int RegisterFieldAndGetFieldIndex(AIVirtualField field)
	{
		ulong hash = field.GetHash();
		for (int i = 0; i < _referenceFieldList.Count; i++)
		{
			if (_referenceFieldList[i].GetHash() == hash)
			{
				return i;
			}
		}
		_referenceFieldList.Add(field);
		return _referenceFieldList.Count - 1;
	}

	private int GetFieldIndex(ulong fieldHash)
	{
		for (int i = 0; i < _referenceFieldList.Count; i++)
		{
			if (fieldHash == _referenceFieldList[i].GetHash())
			{
				return i;
			}
		}
		return -1;
	}

	public AISinglePlayptnRecord GetEmptyPlayPtnRecord()
	{
		AISinglePlayptnRecord aISinglePlayptnRecord = ValidPlayPtnList[0];
		if (aISinglePlayptnRecord.PlayedCardList.Count > 0)
		{
			AIConsoleUtility.LogError("AIPlayPtnRecorder.GetEmptyPlayPtnRecord() error!! first record is not empty!!!!!");
		}
		return aISinglePlayptnRecord;
	}

	public AISinglePlayptnRecord CreateRecordForNextPlayTargetSelectSimulation(AIVirtualTargetSelectAction action, AISinglePlayptnRecord previousRecord, AIVirtualField field)
	{
		bool num = action.ActionType == AIOperationType.PLAY;
		int fieldIndex = RegisterFieldAndGetFieldIndex(field);
		List<int> list = new List<int>();
		for (int i = (num ? 1 : 0); i < previousRecord.PlayedCardList.Count; i++)
		{
			AIVirtualCard card = previousRecord.PlayedCardList[i].Card;
			AIVirtualCard aIVirtualCard = field.SearchVirtualCard(card);
			if (aIVirtualCard == null)
			{
				AIConsoleUtility.LogError("AIPlayptnRecorder.CreateRecordForNextPlayTargetSelectSimulation() error!! Cannot find play card!!!!!");
			}
			else if (aIVirtualCard.IsInHand)
			{
				int num2 = field.AllyHandCards.IndexOf(aIVirtualCard);
				if (num2 < 0 || field.AllyHandCards.Count <= num2)
				{
					AIConsoleUtility.LogError($"CreateRecordForNextPlayTargetSelectSimulation() error!! handIndex: {num2}, CardName: {aIVirtualCard.CardName}");
					return null;
				}
				list.Add(num2);
			}
		}
		AISinglePlayptnRecord aISinglePlayptnRecord = new AISinglePlayptnRecord(list, field, fieldIndex);
		_isRegisteringPlayPtn = true;
		_allPlayPtnList.Add(aISinglePlayptnRecord);
		AIVirtualFieldRollBackBasicProcessor rollBackProcessor = new AIVirtualFieldRollBackBasicProcessor(field);
		ProcessPlayPtnRecord(aISinglePlayptnRecord, field, rollBackProcessor);
		_isRegisteringPlayPtn = false;
		return aISinglePlayptnRecord;
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AILethalSimulator
{
	private EnemyAI _ai;

	private AIOperationSimulatorAccessor _operator;

	private AILethalPlan _resultLethalPlan;

	private bool _isForceFailedSimulation;

	public AIVirtualField CurrentFieldForNewSimulator { get; private set; }

	public AILethalSimulator(EnemyAI ai)
	{
		_ai = ai;
		_operator = new AIOperationSimulatorAccessor(_ai);
		CurrentFieldForNewSimulator = new AIVirtualField(_ai.CurrentVirtualField, isLatestAction: false, isPlayptnSimulation: true);
	}

	public IEnumerator SimulateByAISimulator(AISinglePlayptnRecord playPtnRecord)
	{
		CurrentFieldForNewSimulator.BestPlayPtn = new List<int>(playPtnRecord.PlayPtn);
		CurrentFieldForNewSimulator.RegisterBestPlayPtnRecord(playPtnRecord);
		_resultLethalPlan = null;
		yield return AILethalSimulation(CurrentFieldForNewSimulator, playPtnRecord, null);
		if (_resultLethalPlan != null)
		{
			_ai.OutputLethalPlan = _resultLethalPlan;
		}
	}

	public IEnumerator AILethalSimulation(AIVirtualField field, AISinglePlayptnRecord playptnRecord, AILethalPlan lethalPlan)
	{
		AISinglePlayptnRecord nextPlayptnRecord = playptnRecord;
		List<PlaySkipInformation> list = null;
		AIVirtualCard aIVirtualCard = null;
		AILethalPlan aILethalPlan = ((lethalPlan != null) ? lethalPlan.Clone() : new AILethalPlan());
		List<AIVirtualCard> list2 = field.CreatePlayCardList(playptnRecord.PlayPtn);
		if (list2 != null && list2.Count > 0)
		{
			for (int i = 0; i < list2.Count; i++)
			{
				AIVirtualCard baseCard = list2[i];
				AIVirtualCard aIVirtualCard2 = field.SearchVirtualCard(baseCard);
				if (aIVirtualCard2 == null)
				{
					AIConsoleUtility.LogError("AILethalSimulation error!! cannot find virtualPlayCard!!!!!");
					yield break;
				}
				List<int> bestPlayPtn = field.BestPlayPtn;
				AIVirtualCard aIVirtualCard3 = aIVirtualCard2.FindRealActor(nextPlayptnRecord);
				AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard3, aIVirtualCard2, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
				aIVirtualTargetSelectAction.forceLethalMode = true;
				if (!AIPlayCardSimulationUtility.IsAbleToPlayCard(aIVirtualTargetSelectAction, field))
				{
					AIConsoleUtility.LogError("AILethalSimulation error!! cannot play card " + aIVirtualCard2.CardName);
					yield break;
				}
				list = aIVirtualCard2.GetPlaySkipInformation(bestPlayPtn, null, aIVirtualTargetSelectAction);
				for (int j = 0; j < field.CardListSet.BothClassAndInplayCards.Count; j++)
				{
					list = field.CardListSet.BothClassAndInplayCards[j].GetPlaySkipInformation(bestPlayPtn, list, aIVirtualTargetSelectAction);
				}
				if (list != null)
				{
					aIVirtualCard = aIVirtualCard2;
					break;
				}
				List<int> addedCardIdsWhenPlayout = aIVirtualCard3.GetAddedCardIdsWhenPlayout(field, field.BestPlayPtn, aIVirtualTargetSelectAction);
				if (AILethalTargetSelectUtility.LethalSelectTarget(field, aIVirtualTargetSelectAction, nextPlayptnRecord))
				{
					aILethalPlan.AddAction(aIVirtualTargetSelectAction);
				}
				ProcessAfterAction(field, nextPlayptnRecord, addedCardIdsWhenPlayout, out nextPlayptnRecord, out var addedPlayCardList);
				if (_isForceFailedSimulation)
				{
					_resultLethalPlan = null;
					yield break;
				}
				if (addedPlayCardList != null && addedPlayCardList.Count > 0)
				{
					int num = i + 1;
					if (num < list2.Count)
					{
						list2.InsertRange(num, addedPlayCardList);
					}
					else
					{
						list2.AddRange(addedPlayCardList);
					}
				}
				if (field.IsSuccess())
				{
					aILethalPlan.IsSuccess = true;
					_resultLethalPlan = aILethalPlan;
					yield break;
				}
				if (field.AllyClass.IsDead)
				{
					yield break;
				}
			}
		}
		List<AIVirtualCard> list3 = CreateEvolutionCardList(field, list);
		for (int k = 0; k <= list3.Count; k++)
		{
			AIVirtualField aIVirtualField = new AIVirtualField(field, isLatestAction: false, isPlayptnSimulation: true);
			AILethalPlan aILethalPlan2 = aILethalPlan.Clone();
			AIVirtualCard aIVirtualCard4 = ((k > 0) ? list3[k - 1] : null);
			List<AIVirtualCard> addedPlayCardList2;
			if (aIVirtualCard4 != null)
			{
				AIVirtualCard aIVirtualCard5 = aIVirtualField.SearchVirtualCard(aIVirtualCard4);
				if (aIVirtualCard5.IsAbleEvolution())
				{
					AIVirtualTargetSelectAction aIVirtualTargetSelectAction2 = new AIVirtualTargetSelectAction(aIVirtualCard5, aIVirtualCard5, AIOperationType.EVOLVE, (AISelectedTargetInfoSet)null);
					aIVirtualTargetSelectAction2.forceLethalMode = true;
					if (AILethalTargetSelectUtility.LethalSelectTarget(aIVirtualField, aIVirtualTargetSelectAction2, nextPlayptnRecord))
					{
						aILethalPlan2.AddAction(aIVirtualTargetSelectAction2);
					}
					ProcessAfterAction(aIVirtualField, nextPlayptnRecord, null, out nextPlayptnRecord, out addedPlayCardList2);
					if (_isForceFailedSimulation)
					{
						_resultLethalPlan = null;
						break;
					}
					if (aIVirtualField.IsSuccess())
					{
						aILethalPlan2.IsSuccess = true;
						_resultLethalPlan = aILethalPlan2;
						break;
					}
					if (aIVirtualField.AllyClass.IsDead)
					{
						continue;
					}
				}
			}
			do
			{
				AIVirtualAttackInfo aIVirtualAttackInfo = null;
				List<AIVirtualCard> allyInplayCards = aIVirtualField.AllyInplayCards;
				for (int l = 0; l < allyInplayCards.Count; l++)
				{
					AIVirtualCard aIVirtualCard6 = allyInplayCards[l];
					AIVirtualAttackInfo aIVirtualAttackInfo2 = new AIVirtualAttackInfo(aIVirtualCard6, aIVirtualField.EnemyClass);
					if (AIAttackSimulationUtility.IsAttackPossible(aIVirtualField, aIVirtualAttackInfo2) && !aIVirtualAttackInfo2.WillTargetDestroyByAttackTags(aIVirtualField, nextPlayptnRecord.PlayPtn, aIVirtualCard6))
					{
						aIVirtualAttackInfo = aIVirtualAttackInfo2;
						break;
					}
				}
				if (aIVirtualAttackInfo == null)
				{
					break;
				}
				AIVirtualAttackSimulator.Attack(aIVirtualAttackInfo, aIVirtualField);
				aILethalPlan2.AddAction(aIVirtualAttackInfo);
				ProcessAfterAction(aIVirtualField, nextPlayptnRecord, null, out nextPlayptnRecord, out addedPlayCardList2);
				if (_isForceFailedSimulation)
				{
					_resultLethalPlan = null;
					yield break;
				}
				if (aIVirtualField.IsSuccess())
				{
					aILethalPlan2.IsSuccess = true;
					_resultLethalPlan = aILethalPlan2;
					yield break;
				}
			}
			while (!aIVirtualField.AllyClass.IsDead);
			if (aIVirtualField.AllyClass.IsDead)
			{
				continue;
			}
			if (aIVirtualCard != null)
			{
				AIVirtualCard aIVirtualCard7 = aIVirtualField.SearchVirtualCard(aIVirtualCard);
				if (aIVirtualCard7 == null)
				{
					_isForceFailedSimulation = true;
					_resultLethalPlan = null;
					break;
				}
				AIVirtualTargetSelectAction aIVirtualTargetSelectAction3 = new AIVirtualTargetSelectAction(aIVirtualCard7.FindRealActor(nextPlayptnRecord), aIVirtualCard7, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
				aIVirtualTargetSelectAction3.forceLethalMode = true;
				List<int> bestPlayPtn2 = aIVirtualField.BestPlayPtn;
				List<int> addedCardIdsWhenPlayout2 = aIVirtualCard.GetAddedCardIdsWhenPlayout(field, bestPlayPtn2, aIVirtualTargetSelectAction3);
				if (AILethalTargetSelectUtility.LethalSelectTarget(aIVirtualField, aIVirtualTargetSelectAction3, nextPlayptnRecord))
				{
					aILethalPlan2.AddAction(aIVirtualTargetSelectAction3);
				}
				else
				{
					int num2 = aIVirtualField.AllyHandCards.IndexOf(aIVirtualCard7);
					if (num2 >= 0)
					{
						bestPlayPtn2.Remove(num2);
					}
				}
				if (aIVirtualField.IsSuccess())
				{
					aILethalPlan2.IsSuccess = true;
					_resultLethalPlan = aILethalPlan2;
					break;
				}
				ProcessAfterAction(aIVirtualField, nextPlayptnRecord, addedCardIdsWhenPlayout2, out var nextPlayptnRecord2, out addedPlayCardList2);
				if (_isForceFailedSimulation)
				{
					_resultLethalPlan = null;
					break;
				}
				yield return AILethalSimulation(aIVirtualField, nextPlayptnRecord2, aILethalPlan2);
				if (_isForceFailedSimulation)
				{
					_resultLethalPlan = null;
				}
				break;
			}
			AIVirtualTurnEndInfo turnEndSituation = new AIVirtualTurnEndInfo(aIVirtualField.AllyClass);
			BattleSequencer.ProcessAllyTurnEndToOpponentTurnStart(aIVirtualField, turnEndSituation);
			if (aIVirtualField.IsSuccess())
			{
				aILethalPlan2.IsSuccess = true;
				_resultLethalPlan = aILethalPlan2;
				break;
			}
		}
	}

	private void ProcessAfterAction(AIVirtualField afterActionField, AISinglePlayptnRecord currentPlayptnRecord, List<int> addCardToPlayoutPlayPtnIdList, out AISinglePlayptnRecord nextPlayptnRecord, out List<AIVirtualCard> addedPlayCardList)
	{
		addedPlayCardList = null;
		_isForceFailedSimulation = afterActionField.IsRemovedPlayPtnCard;
		if (_isForceFailedSimulation)
		{
			nextPlayptnRecord = null;
			return;
		}
		List<int> addedHandIndexList = null;
		if (addCardToPlayoutPlayPtnIdList != null && addCardToPlayoutPlayPtnIdList.Count > 0)
		{
			CreateAddCardToPlayoutPlayPtnList(afterActionField, addCardToPlayoutPlayPtnIdList, currentPlayptnRecord, out addedHandIndexList, out addedPlayCardList);
		}
		afterActionField.UpdateBestPlayptnRecordOnSim(addedHandIndexList);
		nextPlayptnRecord = afterActionField.BestPlayptnRecordOnSim;
		if (nextPlayptnRecord == null)
		{
			_isForceFailedSimulation = true;
		}
	}

	private void CreateAddCardToPlayoutPlayPtnList(AIVirtualField afterActionField, List<int> addCardToPlayoutPlayPtnIdList, AISinglePlayptnRecord currentRecord, out List<int> addedHandIndexList, out List<AIVirtualCard> addedHandCardList)
	{
		addedHandIndexList = null;
		addedHandCardList = null;
		List<AIVirtualCard> allyHandCards = afterActionField.AllyHandCards;
		int num = currentRecord.LastRestPp;
		List<int> list = null;
		for (int i = 0; i < addCardToPlayoutPlayPtnIdList.Count; i++)
		{
			int num2 = addCardToPlayoutPlayPtnIdList[i];
			for (int j = 0; j < allyHandCards.Count; j++)
			{
				AIVirtualCard aIVirtualCard = allyHandCards[j];
				if ((addedHandIndexList == null || !addedHandIndexList.Contains(j)) && (afterActionField.BestPlayPtn == null || !afterActionField.BestPlayPtn.Contains(j)) && aIVirtualCard.BaseId == num2)
				{
					AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.PLAY);
					aIVirtualTargetSelectAction.forceLethalMode = true;
					list = AIParamQuery.AddElementToList(j, list);
					PlaySimulationType playType;
					int useCost = aIVirtualCard.GetUseCost(num, list, aIVirtualTargetSelectAction, out playType);
					if (0 <= useCost && useCost <= num)
					{
						num -= useCost;
						addedHandIndexList = AIParamQuery.AddElementToList(j, addedHandIndexList);
						addedHandCardList = AIParamQuery.AddElementToList(aIVirtualCard, addedHandCardList);
						break;
					}
				}
			}
		}
	}

	private List<AIVirtualCard> CreateEvolutionCardList(AIVirtualField field, List<PlaySkipInformation> playSkipInfo)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (!aIVirtualCard.IsUnit || !aIVirtualCard.IsAbleEvolution())
			{
				continue;
			}
			if (playSkipInfo == null || !playSkipInfo.Any())
			{
				list.Add(aIVirtualCard);
				continue;
			}
			for (int j = 0; j < playSkipInfo.Count; j++)
			{
				if (playSkipInfo[j].IsEvoCardLegal(aIVirtualCard))
				{
					list.Add(aIVirtualCard);
					break;
				}
			}
		}
		return list;
	}
}

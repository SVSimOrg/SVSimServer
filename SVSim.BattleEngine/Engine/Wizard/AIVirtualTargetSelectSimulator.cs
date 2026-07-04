using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIVirtualTargetSelectSimulator
{
	public static IEnumerator ExecuteTargetSelect(AIVirtualTargetSelectSimulationInfo simulationInfo, AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		SimulationResult result = new SimulationResult();
		yield return EnemyAICoroutine.GetInstance().StartCoroutine(SimulateTargetSelect(simulationInfo, field, playPtnRecord, result, isBattleSim: false));
		AIVirtualCard originalActor = simulationInfo.OriginalActor;
		AIOperationType operationType = simulationInfo.OperationType;
		field.AI.OprTargetSelect(originalActor, result.SelectedTargets, operationType);
	}

	public static IEnumerator SimulateTargetSelect(AIVirtualTargetSelectSimulationInfo simulationInfo, AIVirtualField field, AISinglePlayptnRecord playPtnRecord, SimulationResult result, bool isBattleSim)
	{
		List<AIVirtualTargetSelectInfo> selectInfoList = simulationInfo.SelectInfoList;
		if (selectInfoList == null || selectInfoList.Count <= 0)
		{
			simulationInfo.ForceStopNextPlay();
			AIConsoleUtility.LogError("SimulateTargetSelect error!! no candidates!!!!!");
			yield break;
		}
		AIVirtualTargetSelectAction action = simulationInfo.CreateSituationForTargetSelectSimulation();
		AISelectedTargetInfoSet preDecidedTargets = field.AI.PreDecidedTargets;
		List<AISelectedTargetInfoSet> allTargetPattern = GetAllTargetSelectSimulationPattern(selectInfoList, preDecidedTargets, action, field, playPtnRecord);
		if (allTargetPattern == null || allTargetPattern.Count <= 0)
		{
			simulationInfo.ForceStopNextPlay();
			yield break;
		}
		bool isSecondTargetForbbidenSelectedTarget = selectInfoList.Count > 1 && selectInfoList[1].IsForbiddenSelectedTarget;
		for (int i = 0; i < allTargetPattern.Count; i++)
		{
			AISelectedTargetInfoSet currentPattern = allTargetPattern[i];
			AISelectedTargetInfoSet previousBestPattern;
			if (simulationInfo.IsFirstAction)
			{
				simulationInfo.FirstActionTargetTemp = currentPattern;
				previousBestPattern = result.SelectedTargets;
			}
			else
			{
				previousBestPattern = simulationInfo.CurrentSimulationBestTargetTemp;
			}
			if (!IsAbleToReplaceDummyTarget(currentPattern, previousBestPattern, isSecondTargetForbbidenSelectedTarget))
			{
				continue;
			}
			if (!currentPattern.IsTargetExist(0) && currentPattern.IsTargetExist(1))
			{
				AIConsoleUtility.LogError("currentPattern.firstTarget is null & secondTaget is not null");
				continue;
			}
			AIVirtualField aIVirtualField = new AIVirtualField(field)
			{
				BestPlayPtn = new List<int>(playPtnRecord.PlayPtn)
			};
			AIVirtualCard aIVirtualCard = aIVirtualField.SearchVirtualCard(action.OriginalCard);
			AIVirtualCard actor = action.Actor;
			AIVirtualTargetSelectAction action2 = new AIVirtualTargetSelectAction(actor.IsSameCard(aIVirtualCard) ? aIVirtualCard : new AIVirtualCard(actor, aIVirtualField), selectedTargetList: currentPattern.GetSimilarTargetInfoSet(aIVirtualField), original: aIVirtualCard, operationType: action.ActionType);
			yield return SimulateSinglePattern(action2, simulationInfo, aIVirtualField, result, playPtnRecord, isBattleSim);
			if (result.IsRecentlyUpdated)
			{
				result.SelectedTargets = simulationInfo.FirstActionTargetTemp;
				simulationInfo.CurrentSimulationBestTargetTemp = currentPattern;
				if (result.IsLethalPlan)
				{
					break;
				}
			}
			else if (simulationInfo.IsRecentlyUpdatedCurrentSimulationBestTargetTempValue)
			{
				simulationInfo.CurrentSimulationBestTargetTemp = currentPattern;
			}
		}
	}

	private static IEnumerator SimulateSinglePattern(AIVirtualTargetSelectAction action, AIVirtualTargetSelectSimulationInfo simulationInfo, AIVirtualField field, SimulationResult result, AISinglePlayptnRecord playPtnRecord, bool isBattleSim)
	{
		AIVirtualTargetSelectAction aIVirtualTargetSelectAction = null;
		switch (action.ActionType)
		{
		case AIOperationType.EVOLVE:
			if (!isBattleSim)
			{
				AIVirtualEvolutionSimulator.ManualEvolve(action, field);
			}
			else
			{
				aIVirtualTargetSelectAction = action;
			}
			break;
		case AIOperationType.PLAY:
		{
			PlaySimulationInfo playInfo = AIPlayCardSimulationUtility.CreatePlaySimulationInfo(action.OriginalCard, action, field);
			AIVirtualPlaySimulator.PlayCard(action, field, playInfo);
			break;
		}
		default:
			throw new NotImplementedException();
		}
		float removalBonus = GetRemovalBonus(action.SelectedTargets) + simulationInfo.PrevRemovalBonus;
		if (!isBattleSim && field.EnemyClass.Life <= 0)
		{
			result.MaxFieldValue = EnemyAI_Skill.PLAYOUT_VALUE;
			result.IsLethalPlan = true;
			result.MaxFieldValueAtSelfTurnEnd = result.MaxFieldValue;
			result.MaxFieldValueFirstSkill = result.MaxFieldValue;
			result.IsRecentlyUpdated = true;
			result.FirstActionInfo = new AIVirtualTargetSelectAction(action.Actor, action.OriginalCard, action.ActionType, action.SelectedTargets);
			result.BestResultField = field;
			yield break;
		}
		EnemyAI ai = field.AI;
		result.IsRecentlyUpdated = false;
		SimulationAdditionalActionInfoSet additionalActionInfoSet = new SimulationAdditionalActionInfoSet
		{
			ForceEvoAction = aIVirtualTargetSelectAction,
			ExtraActionBaseList = null,
			PlayPtnRecord = playPtnRecord
		};
		AIBattleSimulationLauncher battleSimLauncher = new AIBattleSimulationLauncher(field, ai, aIVirtualTargetSelectAction == null, null, additionalActionInfoSet);
		yield return battleSimLauncher.ExecuteBattleSimulationAI(null, checkTimeOverLogic: true);
		IBattleSimulationAI battleSimulationAI = battleSimLauncher.BattleSimAI;
		float valueWithBonusPoint = battleSimulationAI.Cr_MaxFieldValue + removalBonus;
		AIVirtualTargetSelectSimulationInfo nextPlayCardSimulationInfo = (field.AI.IsRunWeakLogic ? null : simulationInfo.CreateNextPlayCardTargetSelectSimulationInfo(field, removalBonus));
		if (nextPlayCardSimulationInfo != null)
		{
			nextPlayCardSimulationInfo.FieldValueAfterFirstSelectCardPlay = valueWithBonusPoint;
			AISinglePlayptnRecord aISinglePlayptnRecord = ai.PlayPtnRecorder.CreateRecordForNextPlayTargetSelectSimulation(action, playPtnRecord, field);
			if (aISinglePlayptnRecord == null)
			{
				simulationInfo.IsCurrentSimulationBestTargetTempNeedsToUpdated(valueWithBonusPoint);
				yield break;
			}
			yield return SimulateTargetSelect(nextPlayCardSimulationInfo, field, aISinglePlayptnRecord, result, isBattleSim: false);
			if (nextPlayCardSimulationInfo.IsInterruptedNextPlay)
			{
				UpdateSimulationResultAfterBattle(result, battleSimulationAI, valueWithBonusPoint, simulationInfo);
			}
			else
			{
				simulationInfo.IsCurrentSimulationBestTargetTempNeedsToUpdated(valueWithBonusPoint);
			}
		}
		else
		{
			UpdateSimulationResultAfterBattle(result, battleSimulationAI, valueWithBonusPoint, simulationInfo);
		}
	}

	private static void UpdateSimulationResultAfterBattle(SimulationResult result, IBattleSimulationAI battleSimulationAI, float valueWithBonus, AIVirtualTargetSelectSimulationInfo simulationInfo)
	{
		if (IsBetterThanCurrentBestResult(valueWithBonus, simulationInfo.FieldValueAfterFirstSelectCardPlay, battleSimulationAI, result))
		{
			result.IsLethalPlan = battleSimulationAI.Cr_MaxField.EnemyClass.IsDead;
			result.MaxFieldValue = valueWithBonus;
			result.MaxFieldValueAtSelfTurnEnd = battleSimulationAI.Cr_MaxFieldValueAtSelfTurnEnd;
			result.MaxFieldValueFirstSkill = simulationInfo.FieldValueAfterFirstSelectCardPlay;
			result.FirstActionInfo = battleSimulationAI.Cr_FirstActionInfo;
			result.BestResultField = battleSimulationAI.Cr_MaxField;
			result.IsRecentlyUpdated = true;
		}
		simulationInfo.IsCurrentSimulationBestTargetTempNeedsToUpdated(valueWithBonus);
	}

	private static bool IsBetterThanCurrentBestResult(float finalValue, float valueAfterFirstCardPlay, IBattleSimulationAI simulator, SimulationResult bestRecord)
	{
		if (EnemyAI.IsLargerThan(finalValue, bestRecord.MaxFieldValue))
		{
			return true;
		}
		if (EnemyAI.IsSameValue(finalValue, bestRecord.MaxFieldValue) && (EnemyAI.IsLargerThan(valueAfterFirstCardPlay, bestRecord.MaxFieldValueFirstSkill) || EnemyAI.IsLargerThan(simulator.Cr_MaxFieldValueAtSelfTurnEnd, bestRecord.MaxFieldValueAtSelfTurnEnd)))
		{
			return true;
		}
		return false;
	}

	public static List<AISelectedTargetInfoSet> GetAllTargetSelectSimulationPattern(List<AIVirtualTargetSelectInfo> selectInfoList, AISelectedTargetInfoSet preDecidedTargetSet, AIVirtualTargetSelectAction situation, AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		if (selectInfoList.Count > 2)
		{
			throw new Exception("GetAllTargetSelectSimulationPattern error!! selectInfoList.Count == " + selectInfoList.Count);
		}
		if (selectInfoList.Count == 1)
		{
			return GetAllSingleTargetSelectSimulationPattern(selectInfoList[0], preDecidedTargetSet, situation, field, playPtnRecord);
		}
		return GetAllMultipleTargetSelectSimulationpattern(selectInfoList[0], selectInfoList[1], preDecidedTargetSet, situation, field, playPtnRecord);
	}

	private static List<AISelectedTargetInfoSet> GetAllSingleTargetSelectSimulationPattern(AIVirtualTargetSelectInfo selectInfo, AISelectedTargetInfoSet preDecidedTargetSet, AIVirtualTargetSelectAction situation, AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		List<AISelectedTargetInfoSet> list = new List<AISelectedTargetInfoSet>();
		if (preDecidedTargetSet != null && preDecidedTargetSet.IsAnyTargetExists() && CheckWhenBurialRiteIsDefault(preDecidedTargetSet, selectInfo))
		{
			list.Add(preDecidedTargetSet);
		}
		else
		{
			List<AISelectedTargetInfo> allTargetPatternList = GetAllTargetPatternList(selectInfo, situation, field, playPtnRecord);
			if (allTargetPatternList == null || allTargetPatternList.Count <= 0)
			{
				return null;
			}
			bool isReplacedFirstInfoByBurialOrChoice = selectInfo.Type == TargetSelectType.BurialRite || selectInfo.Type == TargetSelectType.Choice;
			for (int i = 0; i < allTargetPatternList.Count; i++)
			{
				AISelectedTargetInfoSet item = CreateTargetInfoSet(allTargetPatternList[i], null, isReplacedFirstInfoByBurialOrChoice);
				list.Add(item);
			}
		}
		return list;
	}

	private static bool CheckWhenBurialRiteIsDefault(AISelectedTargetInfoSet targetInfo, AIVirtualTargetSelectInfo selectInfo)
	{
		if (targetInfo.PreprocessTarget != null && targetInfo.PreprocessTarget.Type == TargetSelectType.BurialRite && selectInfo.Type == TargetSelectType.Default)
		{
			return false;
		}
		return true;
	}

	private static List<AISelectedTargetInfoSet> GetAllMultipleTargetSelectSimulationpattern(AIVirtualTargetSelectInfo firstSelectInfo, AIVirtualTargetSelectInfo secondSelectInfo, AISelectedTargetInfoSet preDecidedTargetSet, AIVirtualTargetSelectAction situation, AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		List<AISelectedTargetInfo> list = null;
		List<AISelectedTargetInfo> list2 = null;
		bool flag = false;
		if (preDecidedTargetSet != null)
		{
			if (firstSelectInfo.Type == TargetSelectType.BurialRite && preDecidedTargetSet.PreprocessTarget != null && preDecidedTargetSet.PreprocessTarget.HasTarget)
			{
				list = AIParamQuery.AddElementToList(preDecidedTargetSet.PreprocessTarget, list);
				flag = true;
			}
			if (preDecidedTargetSet.HasChoiceTarget)
			{
				list = AIParamQuery.AddElementToList(preDecidedTargetSet.ChoiceTarget, list);
				flag = true;
			}
			if (preDecidedTargetSet.IsTargetExist(0))
			{
				AISelectedTargetInfo element = preDecidedTargetSet.Get(0);
				if (flag)
				{
					list2 = AIParamQuery.AddElementToList(element, list2);
				}
				else
				{
					list = AIParamQuery.AddElementToList(element, list);
					if (preDecidedTargetSet.IsTargetExist(1))
					{
						list2 = AIParamQuery.AddElementToList(preDecidedTargetSet.Get(1), list2);
					}
				}
			}
		}
		if (list == null)
		{
			list = GetAllTargetPatternList(firstSelectInfo, situation, field, playPtnRecord);
		}
		if (list2 == null)
		{
			list2 = GetAllTargetPatternList(secondSelectInfo, situation, field, playPtnRecord);
		}
		if (list == null || list.Count <= 0 || list2 == null || list2.Count <= 0)
		{
			return null;
		}
		List<AISelectedTargetInfoSet> list3 = new List<AISelectedTargetInfoSet>();
		AISelectedTargetInfo aISelectedTargetInfo = list2[0];
		List<AISelectedTargetInfo> list4 = new List<AISelectedTargetInfo>();
		for (int i = 0; i < list.Count; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo2 = list[i];
			if (secondSelectInfo.IsForbiddenSelectedTarget && aISelectedTargetInfo2.IsSelectedSameTarget(aISelectedTargetInfo))
			{
				list4.Add(aISelectedTargetInfo2);
				continue;
			}
			AISelectedTargetInfoSet item = CreateTargetInfoSet(aISelectedTargetInfo2, aISelectedTargetInfo, flag);
			list3.Add(item);
		}
		for (int j = 1; j < list2.Count; j++)
		{
			AISelectedTargetInfo aISelectedTargetInfo3 = list2[j];
			AISelectedTargetInfoSet item2 = CreateTargetInfoSet(null, aISelectedTargetInfo3, flag);
			list3.Add(item2);
			if (list4.Count <= 0)
			{
				continue;
			}
			for (int k = 0; k < list4.Count; k++)
			{
				AISelectedTargetInfo aISelectedTargetInfo4 = list4[k];
				if (!secondSelectInfo.IsForbiddenSelectedTarget || !aISelectedTargetInfo4.IsSelectedSameTarget(aISelectedTargetInfo3))
				{
					AISelectedTargetInfoSet item3 = CreateTargetInfoSet(aISelectedTargetInfo4, aISelectedTargetInfo3, flag);
					list3.Add(item3);
				}
			}
		}
		return list3;
	}

	private static List<AISelectedTargetInfo> ConvertTargetPatternArrayToInfoList(List<AIVirtualCard[]> targetPatternArray, TargetSelectType targetSelectType, AIRemovalType removalType)
	{
		List<AISelectedTargetInfo> list = new List<AISelectedTargetInfo>();
		for (int i = 0; i < targetPatternArray.Count; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = new AISelectedTargetInfo(targetSelectType, removalType);
			AIVirtualCard[] array = targetPatternArray[i];
			foreach (AIVirtualCard target in array)
			{
				aISelectedTargetInfo.AddTarget(target);
			}
			list.Add(aISelectedTargetInfo);
		}
		return list;
	}

	private static List<AISelectedTargetInfo> GetAllTargetPatternList(AIVirtualTargetSelectInfo selectInfo, AIVirtualTargetSelectAction situation, AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		AIVirtualCard actor = situation.Actor;
		List<int> playPtn = ((playPtnRecord == null) ? EnemyAI.EmptyPlayPtn : playPtnRecord.PlayPtn);
		List<AISelectedTargetInfo> list = new List<AISelectedTargetInfo>();
		switch (selectInfo.Type)
		{
		case TargetSelectType.Choice:
		{
			AISelectedTargetInfo choiceTargets = selectInfo.GetChoiceTargets(actor, field, playPtn, situation);
			if (choiceTargets != null)
			{
				list.Add(choiceTargets);
			}
			break;
		}
		case TargetSelectType.BurialRite:
		{
			bool isBreakPlayptn;
			AISelectedTargetInfo burialSelectTargets = selectInfo.GetBurialSelectTargets(situation, field, selectInfo, playPtnRecord, out isBreakPlayptn);
			if (burialSelectTargets != null)
			{
				list.Add(burialSelectTargets);
			}
			break;
		}
		case TargetSelectType.NormalRuleBase:
		{
			AISelectedTargetInfo ruleBaseTargets = selectInfo.GetRuleBaseTargets(situation, field);
			if (ruleBaseTargets != null)
			{
				list.Add(ruleBaseTargets);
			}
			break;
		}
		case TargetSelectType.Default:
		{
			List<AIVirtualCard[]> list2 = AIMathematicsLibrary.EnumerateCombinations(selectInfo.Candidates, selectInfo.Count).ToList();
			if (list2 != null && list2.Count > 0)
			{
				list = ConvertTargetPatternArrayToInfoList(list2, selectInfo.Type, selectInfo.RemovalType);
			}
			break;
		}
		}
		return list;
	}

	private static AISelectedTargetInfoSet CreateTargetInfoSet(AISelectedTargetInfo firstTarget, AISelectedTargetInfo secondTarget, bool isReplacedFirstInfoByBurialOrChoice)
	{
		AISelectedTargetInfoSet aISelectedTargetInfoSet = new AISelectedTargetInfoSet();
		if (isReplacedFirstInfoByBurialOrChoice)
		{
			if (firstTarget != null)
			{
				if (firstTarget.Type == TargetSelectType.BurialRite)
				{
					aISelectedTargetInfoSet.SetPreprocessTarget(firstTarget);
				}
				else if (firstTarget.Type == TargetSelectType.Choice)
				{
					aISelectedTargetInfoSet.SetChoiceTarget(firstTarget);
				}
			}
			aISelectedTargetInfoSet.Set(secondTarget, 0);
		}
		else
		{
			aISelectedTargetInfoSet.Set(firstTarget, 0);
			aISelectedTargetInfoSet.Set(secondTarget, 1);
		}
		return aISelectedTargetInfoSet;
	}

	public static bool IsAbleToReplaceDummyTarget(AISelectedTargetInfoSet currentPattern, AISelectedTargetInfoSet previousBestPattern, bool isSecondTargetForbbidenSelectedTarget)
	{
		if (previousBestPattern == null)
		{
			return true;
		}
		if (previousBestPattern.PreprocessTarget != null && currentPattern.PreprocessTarget == null)
		{
			currentPattern.SetPreprocessTarget(previousBestPattern.PreprocessTarget);
			return true;
		}
		if (previousBestPattern.ChoiceTarget != null && currentPattern.ChoiceTarget == null)
		{
			currentPattern.SetChoiceTarget(previousBestPattern.ChoiceTarget);
			return true;
		}
		if (!currentPattern.IsTargetExist(0))
		{
			AISelectedTargetInfo aISelectedTargetInfo = previousBestPattern.Get(0);
			if (!isSecondTargetForbbidenSelectedTarget || !currentPattern.Get(1).IsSelectedSameTarget(aISelectedTargetInfo))
			{
				currentPattern.Set(aISelectedTargetInfo, 0);
				return true;
			}
			return false;
		}
		return true;
	}

	private static float GetRemovalBonus(AISelectedTargetInfoSet selectedTargetInfoSet)
	{
		float num = 0f;
		for (int i = 0; i < AISelectedTargetInfoSet.LENGTH; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = selectedTargetInfoSet.Get(i);
			if (aISelectedTargetInfo != null)
			{
				AIRemovalType removalType = aISelectedTargetInfo.RemovalType;
				List<AIVirtualCard> targets = aISelectedTargetInfo.Targets;
				for (int j = 0; j < targets.Count; j++)
				{
					num += GetTargetRemovalBonus(targets[j], removalType);
				}
			}
		}
		return num;
	}

	private static float GetTargetRemovalBonus(AIVirtualCard target, AIRemovalType removalType)
	{
		float num = 0f;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		switch (removalType)
		{
		case AIRemovalType.Banish:
			num += (float)((!target.IsAlly) ? 1 : (-1)) * (target.EvaluateBreakValue(emptyPlayPtn, useIgnoreBreak: true) - target.GetAllBanishBonus(emptyPlayPtn, useIgnoreInBattle: true) - target.EvaluateLeaveValue(emptyPlayPtn, useIgnoreInBattle: true));
			break;
		case AIRemovalType.Bounce:
			num += (float)(target.IsAlly ? 1 : (-1)) * (target.GetBounceBonus() + target.EvaluateLeaveValue(emptyPlayPtn, useIgnoreInBattle: true));
			break;
		case AIRemovalType.Metamorphose:
			num += (float)((!target.IsAlly) ? 1 : (-1)) * (target.EvaluateBreakValue(emptyPlayPtn, useIgnoreBreak: true) + target.GetAllBanishBonus(emptyPlayPtn, useIgnoreInBattle: true));
			break;
		}
		return num;
	}
}

using System;
using System.Collections.Generic;

namespace Wizard;

public static class AITargetSelectUtility
{
	public static List<AIVirtualTargetSelectInfo> CreateAIVirtualSelectInfo(this AIVirtualCard owner, AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		List<AIVirtualTargetSelectInfo> resultList = null;
		AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo = CreatePreprocessSelectInfo(owner, field, situation);
		if (aIVirtualTargetSelectInfo != null)
		{
			resultList = AIParamQuery.AddElementToList(aIVirtualTargetSelectInfo, resultList);
		}
		switch (situation.ActionType)
		{
		case AIOperationType.FUSION:
		{
			AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo2 = owner.CreateFusionSelectInfo(field, situation);
			if (aIVirtualTargetSelectInfo2 != null)
			{
				resultList = AIParamQuery.AddElementToList(aIVirtualTargetSelectInfo2, resultList);
			}
			else
			{
				AIConsoleUtility.LogError("CreateAIVirtualSelectInfo error!! Cannot find fusion selectInfo of  " + owner.CardName + " !!!!!");
			}
			break;
		}
		case AIOperationType.EVOLVE:
			RegisterEvoSelectInfo(owner, field, situation, ref resultList);
			break;
		case AIOperationType.PLAY:
			RegisterWhenPlaySelectInfo(owner, field, situation, ref resultList);
			break;
		}
		AIPreprocessSimulationUtility.ResetPreprocess(situation, field);
		return resultList;
	}

	private static AIVirtualTargetSelectInfo CreatePreprocessSelectInfo(AIVirtualCard owner, AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		AIScriptTokenArgType aIScriptTokenArgType = AIPreprocessSimulationUtility.ConvertAIOperationTypeToTiming(situation.ActionType);
		if (aIScriptTokenArgType != AIScriptTokenArgType.NONE)
		{
			AIPreprocessSimulationUtility.SimulatePreprocess(situation.Actor, situation, field, aIScriptTokenArgType, isPseudo: true);
			if (situation.PreprocessRecorder.TotalBurialCount > 0)
			{
				AIVirtualTargetSelectInfo burialSelectInfo = owner.GetBurialSelectInfo(field, situation);
				if (burialSelectInfo != null)
				{
					return burialSelectInfo;
				}
			}
		}
		return null;
	}

	private static AIVirtualTargetSelectInfo CreateFusionSelectInfo(this AIVirtualCard owner, AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		if (owner.TagCollectionContainer.HasTag(AIPlayTagType.Fusion))
		{
			return owner.TagCollectionContainer.FusionTags.GetSelectInfo(owner, field, situation);
		}
		return null;
	}

	private static AIVirtualTargetSelectInfo CreateChoiceSelectInfoAndGetRealActor(AIVirtualCard owner, AIVirtualField field, AIVirtualTargetSelectAction situation, out AIVirtualCard realActor)
	{
		realActor = owner;
		AIVirtualTargetSelectInfo choiceSelectInfo = owner.GetChoiceSelectInfo(field, situation);
		if (choiceSelectInfo != null)
		{
			if (situation.IsChoiceAndChangeActor(field))
			{
				if (!situation.Actor.IsSameCard(owner))
				{
					realActor = situation.Actor;
					situation.SetChoicedTargetInInfo(situation.Actor);
				}
				else
				{
					AISelectedTargetInfo choiceTargets = choiceSelectInfo.GetChoiceTargets(owner, field, null, situation);
					if (choiceTargets != null && choiceTargets.HasTarget)
					{
						AIVirtualCard firstTarget = choiceTargets.FirstTarget;
						situation.SetActor(firstTarget);
						situation.SetChoicedMultipleTargetInInfo(choiceTargets.Targets);
						realActor = AITokenManager.ProcessToken(firstTarget.BaseCard, field);
					}
				}
			}
		}
		else
		{
			realActor = situation.Actor;
		}
		return choiceSelectInfo;
	}

	private static void RegisterEvoSelectInfo(AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualTargetSelectInfo> resultList)
	{
		AIVirtualCard realActor;
		AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo = CreateChoiceSelectInfoAndGetRealActor(actor, field, situation, out realActor);
		if (aIVirtualTargetSelectInfo != null)
		{
			resultList = AIParamQuery.AddElementToList(aIVirtualTargetSelectInfo, resultList);
		}
		if (realActor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenEvo))
		{
			((IAITargetSelectTagCollection)realActor.TagCollectionContainer.EvoTags).AddSelectInfoToSelectInfoList(realActor, field, (AISituationInfo)situation, ref resultList);
		}
	}

	private static void RegisterWhenPlaySelectInfo(AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualTargetSelectInfo> resultList)
	{
		AIVirtualCard realActor;
		AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo = CreateChoiceSelectInfoAndGetRealActor(actor, field, situation, out realActor);
		if (aIVirtualTargetSelectInfo != null)
		{
			resultList = AIParamQuery.AddElementToList(aIVirtualTargetSelectInfo, resultList);
		}
		if (realActor.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			realActor.TagCollectionContainer.FanfareTags.AddSelectInfoToSelectInfoList(realActor, field, situation, ref resultList);
		}
		if (realActor.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			realActor.TagCollectionContainer.PlayTags.AddSelectInfoToSelectInfoList(realActor, field, situation, ref resultList);
		}
	}

	public static List<AIVirtualCard> GetProspectedTargetWithPlayPtnUsableCardCheck(List<AIVirtualCard> candidates, AIVirtualField field, AIVirtualTargetSelectAction situation, AISinglePlayptnRecord playPtnRecord, int selectCount, Func<AIVirtualCard, AIVirtualField, List<int>, AIVirtualTargetSelectAction, float> skillValue, out bool isBreakPlayptn)
	{
		isBreakPlayptn = false;
		if (candidates == null || candidates.Count <= 0)
		{
			return null;
		}
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		List<float> list2 = new List<float>();
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.IsSameCard(situation.Actor) || aIVirtualCard.IsSameCard(situation.OriginalCard))
			{
				continue;
			}
			float num = skillValue(aIVirtualCard, field, playPtnRecord.PlayPtn, situation);
			if (playPtnRecord.IsUsableHandCard(aIVirtualCard))
			{
				num += 100f;
			}
			if (list.Count < selectCount)
			{
				list.Add(aIVirtualCard);
				list2.Add(num);
				continue;
			}
			for (int j = 0; j < list2.Count; j++)
			{
				float num2 = list2[j];
				AIVirtualCard aIVirtualCard2 = list[j];
				if (num > num2)
				{
					list2[j] = num;
					list[j] = aIVirtualCard;
					num = num2;
					aIVirtualCard = aIVirtualCard2;
				}
			}
		}
		if (list.Count < selectCount)
		{
			return null;
		}
		if (!playPtnRecord.IsAllTargetsUsableHandCard(list))
		{
			isBreakPlayptn = true;
		}
		return list;
	}
}

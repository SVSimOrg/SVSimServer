using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIVirtualTargetSelectInfo
{
	public int Count;

	public List<AIVirtualCard> Candidates;

	public TargetSelectType Type;

	public bool IsForbiddenSelectedTarget;

	public AIRemovalType RemovalType;

	public AIPlayTag SelectRule;

	public AIVirtualTargetSelectInfo(int count, List<AIVirtualCard> candidates, TargetSelectType type, bool isForbiddenSelectedTarget, AIPlayTag rule = null, AIRemovalType removalType = AIRemovalType.None)
	{
		Candidates = candidates;
		IsForbiddenSelectedTarget = isForbiddenSelectedTarget;
		Type = type;
		Count = count;
		SelectRule = rule;
		RemovalType = removalType;
	}

	public AISelectedTargetInfo GetChoiceTargets(AIVirtualCard actor, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> choiceTargets = actor.GetChoiceTargets(field, Candidates, playPtn, Count, situation);
		if (choiceTargets != null && choiceTargets.Count > 0)
		{
			return new AISelectedTargetInfo(choiceTargets, TargetSelectType.Choice);
		}
		return null;
	}

	public AISelectedTargetInfo GetBurialSelectTargets(AIVirtualTargetSelectAction situation, AIVirtualField field, AIVirtualTargetSelectInfo info, AISinglePlayptnRecord playPtnRecord, out bool isBreakPlayptn)
	{
		isBreakPlayptn = false;
		if (!situation.Actor.TagCollectionContainer.HasTag(AIPlayTagType.BurialRite))
		{
			return null;
		}
		List<AIVirtualCard> prospectedTargetWithPlayPtnUsableCardCheck = AITargetSelectUtility.GetProspectedTargetWithPlayPtnUsableCardCheck(info.Candidates, field, situation, playPtnRecord, info.Count, (AIVirtualCard card, AIVirtualField aIVirtualField, List<int> playPtn, AIVirtualTargetSelectAction aIVirtualTargetSelectAction) => Mathf.Abs(aIVirtualField.AllyPpTotal - card.Cost), out isBreakPlayptn);
		if (prospectedTargetWithPlayPtnUsableCardCheck != null && prospectedTargetWithPlayPtnUsableCardCheck.Count > 0)
		{
			return new AISelectedTargetInfo(prospectedTargetWithPlayPtnUsableCardCheck, TargetSelectType.BurialRite);
		}
		return null;
	}

	public AISelectedTargetInfo GetRuleBaseTargets(AIVirtualTargetSelectAction situation, AIVirtualField field)
	{
		if (SelectRule == null)
		{
			return null;
		}
		return AITargetSelectFilteringUtility.GetRuleBaseTargets(SelectRule, Candidates, field, situation, RemovalType);
	}

	public List<AISelectedTargetInfo> GetAllDefaultTargetPattern()
	{
		List<AISelectedTargetInfo> targetPatternList = null;
		GetDefaultTargetPatternRecursion(ref targetPatternList, 0);
		return targetPatternList;
	}

	private void GetDefaultTargetPatternRecursion(ref List<AISelectedTargetInfo> targetPatternList, int targetIndex, List<AIVirtualCard> selectedTargets = null)
	{
		for (int i = 0; i < Candidates.Count; i++)
		{
			if (targetIndex == 0)
			{
				selectedTargets = new List<AIVirtualCard>();
			}
			AIVirtualCard item = Candidates[i];
			if (IsForbiddenSelectedTarget || !selectedTargets.Contains(item))
			{
				selectedTargets.Add(item);
				if (targetIndex < Count - 1)
				{
					targetIndex++;
					GetDefaultTargetPatternRecursion(ref targetPatternList, targetIndex, selectedTargets);
				}
				else
				{
					AISelectedTargetInfo element = new AISelectedTargetInfo(selectedTargets, TargetSelectType.Default, RemovalType);
					targetPatternList = AIParamQuery.AddElementToList(element, targetPatternList);
				}
			}
		}
	}
}

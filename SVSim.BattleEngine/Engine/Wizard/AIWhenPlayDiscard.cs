using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayDiscard : AIWhenPlayTagArgument
{
	protected override bool _isSelectCountImplemented => true;

	public AIWhenPlayDiscard(string text)
		: base(text)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.TARGET_SELECT:
				AISkillSimulationUtility.ExecuteTargetSelectDiscard(targetsFromField, selectCount, base.SelectType, tagOwner, field, playPtn, situation);
				break;
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.DiscardAll(tagOwner, targetsFromField, field, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AISkillSimulationUtility.DiscardRandom(tagOwner, field, targetsFromField, selectCount, situation);
				break;
			}
		}
	}

	public override void PseudoExecute(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
		AIDiscardInfo aIDiscardInfo;
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			aIDiscardInfo = GetPseudoAllSelectDiscardInfo(record, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			aIDiscardInfo = GetPseudoRandomSelectDiscardInfo(record, field, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
			aIDiscardInfo = GetPseudoTargetSelectDiscardInfo(record, field, situation);
			break;
		default:
			AIConsoleUtility.LogError("AIWhenPlayDiscard error!! SelectType == " + base.SelectType);
			return;
		}
		playInfo.SetDiscardInfo(aIDiscardInfo, base.SelectType);
		record.CheckRegisteredDiscardInfo(aIDiscardInfo);
	}

	private AIDiscardInfo GetPseudoAllSelectDiscardInfo(AISinglePlayptnRecord record, AIVirtualTargetSelectAction situation)
	{
		List<AIVirtualCard> restHandCardList = record.RestHandCardList;
		AIVirtualCard actor = situation.Actor;
		List<int> playPtn = record.PlayPtn;
		List<AIVirtualCard> filteredTargets = GetFilteredTargets(restHandCardList, actor, playPtn, situation);
		if (filteredTargets == null || filteredTargets.Count <= 0)
		{
			return new AIDiscardInfo(actor, isSuccess: false, filteredTargets);
		}
		if (!record.IsAllTargetsUsableHandCard(filteredTargets))
		{
			AIDiscardInfo aIDiscardInfo = new AIDiscardInfo(actor, isSuccess: true, filteredTargets);
			aIDiscardInfo.MarkAsNG();
			return aIDiscardInfo;
		}
		return new AIDiscardInfo(actor, isSuccess: true, filteredTargets);
	}

	private AIDiscardInfo GetPseudoRandomSelectDiscardInfo(AISinglePlayptnRecord record, AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		List<AIVirtualCard> restHandCardList = record.RestHandCardList;
		AIVirtualCard actor = situation.Actor;
		List<int> playPtn = record.PlayPtn;
		List<AIVirtualCard> filteredTargets = GetFilteredTargets(restHandCardList, actor, playPtn, situation);
		if (filteredTargets == null || filteredTargets.Count <= 0)
		{
			return new AIDiscardInfo(actor, isSuccess: false, filteredTargets);
		}
		if (!record.IsAllTargetsUsableHandCard(filteredTargets))
		{
			AIDiscardInfo aIDiscardInfo = new AIDiscardInfo(actor, isSuccess: true, null);
			aIDiscardInfo.MarkAsNG();
			return aIDiscardInfo;
		}
		int selectCount = GetSelectCount(actor, actor.SelfField, playPtn, situation);
		List<AIVirtualCard> targets = AIDiscardUtility.SelectWorstDiscardTarget(actor, field, filteredTargets, selectCount, playPtn, situation);
		return new AIDiscardInfo(actor, isSuccess: true, targets);
	}

	private AIDiscardInfo GetPseudoTargetSelectDiscardInfo(AISinglePlayptnRecord record, AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		List<AIVirtualCard> restHandCardList = record.RestHandCardList;
		AIVirtualCard actor = situation.Actor;
		List<int> playPtn = record.PlayPtn;
		List<AIVirtualCard> filteredTargets = GetFilteredTargets(restHandCardList, actor, playPtn, situation);
		int selectCount = GetSelectCount(actor, actor.SelfField, playPtn, situation);
		if (filteredTargets == null || filteredTargets.Count < selectCount)
		{
			return new AIDiscardInfo(actor, isSuccess: false, filteredTargets);
		}
		bool isBreakPlayptn;
		List<AIVirtualCard> prospectedTargetWithPlayPtnUsableCardCheck = AITargetSelectUtility.GetProspectedTargetWithPlayPtnUsableCardCheck(filteredTargets, field, situation, record, selectCount, (AIVirtualCard card, AIVirtualField field2, List<int> playPtn2, AIVirtualTargetSelectAction situation2) => AIDiscardUtility.EvaluateDiscardedBonus(card, playPtn2, situation2, field2, isIgnoreInBattle: false, isCalcCostDiff: true, isCalcTokenValue: true), out isBreakPlayptn);
		AIDiscardInfo aIDiscardInfo = new AIDiscardInfo(actor, isSuccess: true, prospectedTargetWithPlayPtnUsableCardCheck);
		if (isBreakPlayptn)
		{
			aIDiscardInfo.MarkAsNG();
		}
		return aIDiscardInfo;
	}
}

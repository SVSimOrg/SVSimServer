using System.Collections.Generic;

namespace Wizard;

public class AIOtherWhenPlayBounce : AIOtherWhenPlayTagArgument
{
	public AIOtherWhenPlayBounce(string text)
		: base(text)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIBounceSimulationUtility.BounceAll(targets, situation);
		}
		else
		{
			AIConsoleUtility.LogError($"AIOtherWhenPlayBounce.RunTagMethod() : Unsupported SelectType {SelectType}");
		}
	}

	public override void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			TargetLifePredictionForAllSelect(target, owner, field, playPtn, situation, targetLifeRecord);
		}
		else
		{
			AIConsoleUtility.LogError($"AIOtherWhenPlayBounce.TargetLifePrediction() : Unsupported SelectType {SelectType}");
		}
	}

	private void TargetLifePredictionForAllSelect(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (!target.IsLeader && !target.IsIndependent && AIFilteringUtility.CheckMatchTargetFiltering(target, GetCandidateRange(field), base.TargetFilters, playPtn, owner, situation))
		{
			targetLifeRecord.CurrentLife = 0;
		}
	}

	public override void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		if (SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			return;
		}
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetList[i];
			LifeRecord lifeRecord = lifeList[i];
			if (!aIVirtualCard.IsLeader && !aIVirtualCard.IsIndependent && AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, GetCandidateRange(field), base.TargetFilters, playPtn, owner, situation))
			{
				lifeRecord.CurrentLife = 0;
			}
		}
	}
}

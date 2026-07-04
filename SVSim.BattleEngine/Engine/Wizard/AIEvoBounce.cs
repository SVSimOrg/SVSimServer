using System.Collections.Generic;

namespace Wizard;

public class AIEvoBounce : AIEvoTagArgument
{
	public AIEvoBounce(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBounceSimulationUtility.BounceAll(targetsFromField, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIBounceSimulationUtility.BounceRandom(targetsFromField, tagOwner, field, playPtn, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AIBounceSimulationUtility.ExecuteTargetSelectBounce(tagOwner, targetsFromField, situation, field, playPtn, base.SelectType, GetRemovalType());
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (candidate.IsIndependent)
		{
			return false;
		}
		return IsCertainlyIncludeTarget(owner, candidate, situation);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Bounce;
	}
}

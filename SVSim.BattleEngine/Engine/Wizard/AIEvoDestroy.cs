using System.Collections.Generic;

namespace Wizard;

public class AIEvoDestroy : AIEvoTagArgument
{
	public AIEvoDestroy(string text)
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
				AISkillSimulationUtility.DestroyAll(targetsFromField, field, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AISkillSimulationUtility.DestroyRandom(targetsFromField, tagOwner, field, playPtn, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AIDestroySimulationUtility.ExecuteTargetSelectDestroy(tagOwner, targetsFromField, field, playPtn, situation, base.SelectType);
				break;
			default:
				AIConsoleUtility.LogError("AIEvoDestroy SelectType Error!! SelectType cannot be " + base.SelectType);
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (candidate.IsIndependent || candidate.IsIndestructible)
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
		return AIRemovalType.Destroy;
	}
}

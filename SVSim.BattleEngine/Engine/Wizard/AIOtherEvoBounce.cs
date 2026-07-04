using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoBounce : AIOtherEvoTagArgument
{
	public AIOtherEvoBounce(string text)
		: base(text)
	{
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

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIBounceSimulationUtility.BounceAll(targets, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIBounceSimulationUtility.BounceRandom(targets, tagOwner, field, playPtn, situation);
			break;
		}
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoBanish : AIOtherEvoTagArgument
{
	public AIOtherEvoBanish(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (candidate.IsIndependent || candidate.IsUnbanishable)
		{
			return false;
		}
		return IsCertainlyIncludeTarget(owner, candidate, situation);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIBanishSimulationUtility.BanishAll(targets, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIBanishSimulationUtility.BanishRandom(targets, tagOwner, field, playPtn, situation);
			break;
		}
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Banish;
	}
}

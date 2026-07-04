using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonEvo : AITriggerAndTargetFiltersTagBase
{
	public AIOtherSummonEvo(string text)
		: base(text)
	{
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		AIAutoEvolutionSimulationUtility.AutoEvolution(field, targets, playPtn, situation, AIScriptTokenArgType.ALL_SELECT);
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.AllyInplayCards;
	}
}

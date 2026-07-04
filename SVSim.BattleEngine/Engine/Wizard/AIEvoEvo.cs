using System.Collections.Generic;

namespace Wizard;

public class AIEvoEvo : AIEvoTagArgument
{
	public AIEvoEvo(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && targetsFromField != null && targetsFromField.Count > 0)
		{
			AIAutoEvolutionSimulationUtility.AutoEvolution(field, targetsFromField, playPtn, situation, base.SelectType);
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override List<AIVirtualCard> GetBaseFilteringCards(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		List<AIVirtualCard> list = AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
		list?.RemoveAll((AIVirtualCard c) => c.IsEvolution);
		return list;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

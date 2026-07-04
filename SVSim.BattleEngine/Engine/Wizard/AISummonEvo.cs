using System.Collections.Generic;

namespace Wizard;

public class AISummonEvo : AIFiltersArgument
{
	public AISummonEvo(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (situation != null && situation.Actor != null)
		{
			List<AIVirtualCard> targets = new List<AIVirtualCard> { tagOwner };
			AIAutoEvolutionSimulationUtility.AutoEvolution(field, targets, playPtn, situation, AIScriptTokenArgType.ALL_SELECT);
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.AllyInplayCards;
	}
}

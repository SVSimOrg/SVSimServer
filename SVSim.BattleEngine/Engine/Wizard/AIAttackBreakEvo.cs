using System.Collections.Generic;

namespace Wizard;

public class AIAttackBreakEvo : AIScriptArgumentExpressions
{
	public AIAttackBreakEvo(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		if (!situation.Actor.IsEvolution)
		{
			AIAutoEvolutionSimulationUtility.AutoEvolveSingle(situation.Actor, field, situation);
		}
	}
}

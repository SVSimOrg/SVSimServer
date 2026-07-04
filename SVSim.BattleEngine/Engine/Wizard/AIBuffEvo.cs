using System.Collections.Generic;

namespace Wizard;

public class AIBuffEvo : AIScriptArgumentExpressions
{
	public AIBuffEvo(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		if (!tagOwner.IsEvolution)
		{
			AIAutoEvolutionSimulationUtility.AutoEvolveSingle(tagOwner, field, situation);
		}
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIEvolvedAttackable : AIScriptArgumentExpressions
{
	public AIEvolvedAttackable(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		tagOwner.GiveAttackable();
	}
}

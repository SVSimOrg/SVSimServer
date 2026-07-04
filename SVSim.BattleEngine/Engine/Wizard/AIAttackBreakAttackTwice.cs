using System.Collections.Generic;

namespace Wizard;

public class AIAttackBreakAttackTwice : AIScriptArgumentExpressions
{
	public AIAttackBreakAttackTwice(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		tagOwner.GiveAttackableCount(2);
	}
}

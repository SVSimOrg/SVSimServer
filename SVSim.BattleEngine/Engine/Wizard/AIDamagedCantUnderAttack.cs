using System.Collections.Generic;

namespace Wizard;

public class AIDamagedCantUnderAttack : AIScriptArgumentExpressions
{
	public AIDamagedCantUnderAttack(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		tagOwner.ChangeIsCantUnderAttack(isCantUnderAttack: true);
	}
}

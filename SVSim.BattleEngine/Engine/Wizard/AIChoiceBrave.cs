using System.Collections.Generic;

namespace Wizard;

public class AIChoiceBrave : AIChoiceTagArgument
{
	public AIChoiceBrave()
		: base("")
	{
	}

	protected override void InitExpressions(string text)
	{
		_choiceCount = null;
		_choiceIds = null;
	}

	public override int GetChoiceCount(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation)
	{
		return 1;
	}

	public override List<AIVirtualCard> GetChoiceTargets(AIVirtualCard owner, AIVirtualField field)
	{
		return null;
	}
}

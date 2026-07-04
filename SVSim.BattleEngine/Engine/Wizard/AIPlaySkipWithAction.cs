using System.Collections.Generic;

namespace Wizard;

public class AIPlaySkipWithAction : AIPlaySkipTagArgument
{
	private AIScriptTokenArgType _action;

	public AIPlaySkipWithAction(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_action = AIScriptTokenArgType.NONE;
		if (_exprList.Count >= 1)
		{
			_action = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[0]);
		}
	}

	public override PlaySkipInformation CreatePlaySkipInformation(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return new PlaySkipWithActionInformation(_action);
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AINecromanceAddCemetery : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _addCountArg;

	public AINecromanceAddCemetery(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_addCountArg = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		if (_addCountArg != null)
		{
			int count = (int)_addCountArg.EvalArg(tagOwner, playPtn, field, situation);
			field.VirtualCemetery.AddCemetery(count, tagOwner.IsAlly);
		}
	}
}

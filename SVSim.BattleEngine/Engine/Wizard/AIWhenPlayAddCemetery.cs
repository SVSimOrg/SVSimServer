using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayAddCemetery : AIWhenPlayTagArgument
{
	private AIPolishConvertedExpression _addCount;

	private readonly int COUNT_ARG_INDEX;

	public AIWhenPlayAddCemetery(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		base.SelectType = AIScriptTokenArgType.NONE;
		base.Filters = null;
		_addCount = _exprList[COUNT_ARG_INDEX];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int count = (int)_addCount.EvalArg(tagOwner, playPtn, field, situation);
		field.VirtualCemetery.AddCemetery(count, tagOwner.IsAlly);
	}

	public override void ExecuteForPlayPtnEvaluation(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		Execute(owner, field, playPtn, situation);
	}

	public override void PseudoExecute(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
		Execute(playInfo.Card, field, record.PlayPtn, situation);
	}
}

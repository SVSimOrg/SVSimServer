using System.Collections.Generic;

namespace Wizard;

public class AIOtherWhenPlayRecoverPp : AIOtherWhenPlayTagArgument
{
	private AIPolishConvertedExpression _recoverPpValue;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	protected override int SELECT_TYPE_OFFSET => -1;

	public AIOtherWhenPlayRecoverPp(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeFilters();
		_recoverPpValue = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int recoverPpValue = GetRecoverPpValue(tagOwner, field, playPtn, situation);
		if (recoverPpValue >= 0)
		{
			if (tagOwner.IsAlly)
			{
				field.AllyPp += recoverPpValue;
			}
			else
			{
				field.EnemyPp += recoverPpValue;
			}
		}
	}

	public int GetRecoverPpValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_recoverPpValue == null)
		{
			AIConsoleUtility.LogError("AIOtherWhenPlayRecoverPp error!! _recoverPpValue is null");
			return 0;
		}
		return (int)_recoverPpValue.EvalArg(tagOwner, playPtn, field, situation);
	}

	public bool CheckValidPlayCard(AIVirtualCard tagOwner, AIVirtualCard playCard, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.CheckMatchTargetFiltering(playCard, null, base.TriggerFilters, playPtn, tagOwner, situation);
	}
}

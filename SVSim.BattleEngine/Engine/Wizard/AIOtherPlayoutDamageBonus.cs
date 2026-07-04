using System.Collections.Generic;

namespace Wizard;

public class AIOtherPlayoutDamageBonus : AIFiltersArgument
{
	private AIPolishConvertedExpression _value;

	private int VALUE_INDEX_OFFSET = 1;

	protected override int NON_FILTER_FIRST_OFFSET => VALUE_INDEX_OFFSET;

	public AIOtherPlayoutDamageBonus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_value = _exprList[_exprList.Count - VALUE_INDEX_OFFSET];
	}

	public int GetPlayoutDamageBonus(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, List<int> playPtn)
	{
		if (AIFilteringUtility.CheckMatchTargetFiltering(target, field.CardListSet.AllAllyCards, base.Filters, playPtn, tagOwner, null))
		{
			return (int)_value.EvalArg(tagOwner, playPtn, field);
		}
		return 0;
	}
}

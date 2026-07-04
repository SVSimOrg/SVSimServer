using System.Collections.Generic;

namespace Wizard;

public class AIOtherBreakBonus : AIBonusArgumentWithIgnoreInBattle
{
	private List<AIScriptTokenBase> _filters;

	public AIOtherBreakBonus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		List<AIPolishConvertedExpression> range = _exprList.GetRange(0, _exprList.Count - _valueIndexOffset);
		_filters = GetFilters(range);
	}

	public float GetBonusValue(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (useIgnoreInBattle && base.IsIgnoreInBattle)
		{
			return 0f;
		}
		if (AIFilteringUtility.CheckMatchTargetFiltering(target, field.CardListSet.AllReferableCards, _filters, playPtn, tagOwner, null))
		{
			return _bonusValueArg.EvalArg(tagOwner, playPtn, field);
		}
		return 0f;
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIBuffToken : AIFiltersArgument
{
	private AIPolishConvertedExpression _tokenCount;

	private int TOKEN_COUNT_INDEX_OFFSET = 1;

	protected override int NON_FILTER_FIRST_OFFSET => TOKEN_COUNT_INDEX_OFFSET;

	public AIBuffToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_tokenCount = _exprList[_exprList.Count - TOKEN_COUNT_INDEX_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AISummonTokenUtility.ExecuteSummonToken(GetTargetsFromField(tagOwner, field, playPtn, situation, isBlockDead: false), base.Filters, _tokenCount, AIScriptTokenArgType.ALLY, tagOwner, field, playPtn, situation);
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

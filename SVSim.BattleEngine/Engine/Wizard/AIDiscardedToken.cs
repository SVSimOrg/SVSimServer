using System.Collections.Generic;

namespace Wizard;

public class AIDiscardedToken : AIFiltersArgument
{
	private AIPolishConvertedExpression _tokenCount;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIDiscardedToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_tokenCount = _exprList[_exprList.Count - 1];
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AISummonTokenUtility.ExecuteSummonToken(GetTargetsFromField(tagOwner, field, playPtn, situation, isBlockDead: false), base.Filters, _tokenCount, AIScriptTokenArgType.ALLY, tagOwner, field, playPtn, situation);
	}

	public List<AITokenInformation> GetTokenIds(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return AISummonTokenUtility.GetOwnerSideTokenIds(GetTargetsFromField(tagOwner, field, playPtn, situation, isBlockDead: false), base.Filters, _tokenCount, AITokenType.Default, AIScriptTokenArgType.ALLY, tagOwner, field, playPtn, situation);
	}
}

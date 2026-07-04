using System.Collections.Generic;

namespace Wizard;

public class AIOtherAttackToken : AIWhenAttackSelfAndOtherTagArgument
{
	private AIPolishConvertedExpression _tokenCount;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherAttackToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_tokenCount = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		AISummonTokenUtility.ExecuteSummonToken(targets, base.TargetFilters, _tokenCount, AIScriptTokenArgType.ALLY, tagOwner, field, playPtn, situation);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override List<AIVirtualCard> GetTargets(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return GetCandidateRange(field);
	}
}

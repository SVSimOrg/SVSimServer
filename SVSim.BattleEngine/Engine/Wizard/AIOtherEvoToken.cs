using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoToken : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _tokenCount;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherEvoToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeFilters();
		_tokenCount = _exprList[_exprList.Count - 1];
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override List<AIVirtualCard> GetTargets(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return GetCandidateRange(field);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		AISummonTokenUtility.ExecuteSummonToken(targets, base.TargetFilters, _tokenCount, AIScriptTokenArgType.ALLY, tagOwner, field, playPtn, situation);
	}

	public int GetTokenCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_tokenCount == null)
		{
			AIConsoleUtility.LogError("AIOtherEvoToken error!! _tokenCount is null");
			return 0;
		}
		return (int)_tokenCount.EvalArg(tagOwner, playPtn, field, situation);
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}
}

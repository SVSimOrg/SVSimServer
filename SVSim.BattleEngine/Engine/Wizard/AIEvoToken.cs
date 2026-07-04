using System.Collections.Generic;

namespace Wizard;

public class AIEvoToken : AIEvoTagArgument
{
	private AIPolishConvertedExpression _tokenCount;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	protected override int SELECT_TYPE_OFFSET => -1;

	public AIEvoToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_tokenCount = _exprList[_exprList.Count - 1];
	}

	protected override void InitSelectType()
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AISummonTokenUtility.ExecuteSummonToken(GetTargetsFromField(tagOwner, field, playPtn, situation, isBlockDead: false), base.Filters, _tokenCount, AIScriptTokenArgType.ALLY, tagOwner, field, playPtn, situation);
	}

	public int GetTokenCount(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_tokenCount == null)
		{
			AIConsoleUtility.LogError("AIEvoToken error!! _tokenCount is null");
			return 0;
		}
		return (int)_tokenCount.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}
}

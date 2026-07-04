using System.Collections.Generic;

namespace Wizard;

public class AIOtherWhenPlayToken : AIOtherWhenPlayTagArgument
{
	private AIPolishConvertedExpression _tokenCountArg;

	private AIScriptTokenArgType _sideType;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherWhenPlayToken(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeFilters();
		_tokenCountArg = _exprList[_exprList.Count - 1];
		_sideType = AIScriptTokenArgType.ALLY;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int tokenCount = GetTokenCount(tagOwner, field, playPtn, situation);
			AISummonTokenUtility.GetBothSideTokenIdListFromFilter(tagOwner, field, targets, base.TargetFilters, AITokenType.Default, _sideType, AIScriptTokenArgType.ALL_SELECT, tokenCount, playPtn, situation).SummonAllTokenToField(field, tagOwner, situation);
		}
	}

	private int GetTokenCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_tokenCountArg == null)
		{
			AIConsoleUtility.LogError("AIOtherWhenPlayToken.GetTokenCount() error!! _tokenCountArg is null");
			return 0;
		}
		return (int)_tokenCountArg.EvalArg(tagOwner, playPtn, field, situation);
	}

	protected override List<AIVirtualCard> GetTargets(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return GetCandidateRange(field);
	}

	public List<AITokenInformation> GetAllySideTokenIds(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return GetBothSideTokenIdCollection(tagOwner, field, playPtn, situation)?.AllyTokenIdList;
	}

	public AITokenIdCollection GetBothSideTokenIdCollection(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> targets = GetTargets(tagOwner, field, playPtn, situation);
		if (targets == null || targets.Count <= 0)
		{
			return null;
		}
		int tokenCount = GetTokenCount(tagOwner, field, playPtn, situation);
		return AISummonTokenUtility.GetBothSideTokenIdListFromFilter(tagOwner, field, targets, base.TargetFilters, AITokenType.Default, _sideType, AIScriptTokenArgType.ALL_SELECT, tokenCount, playPtn, situation);
	}
}

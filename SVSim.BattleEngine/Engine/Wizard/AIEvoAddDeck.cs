using System.Collections.Generic;

namespace Wizard;

public class AIEvoAddDeck : AIEvoTagArgument
{
	private AIPolishConvertedExpression _tokenCount;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	protected override int SELECT_TYPE_OFFSET => -1;

	public AIEvoAddDeck(string text)
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
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AddDeckAction(tagOwner, field, playPtn, situation);
	}

	private void AddDeckAction(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null, bool isPseudo = false)
	{
		List<AITokenInformation> tokenIdFromTag = GetTokenIdFromTag(tagOwner, field, playPtn, situation);
		if (tokenIdFromTag != null)
		{
			int tokenCount = (int)_tokenCount.EvalArg(tagOwner, playPtn, field, situation);
			for (int i = 0; i < tokenIdFromTag.Count; i++)
			{
				field.AddDeckCard(tokenIdFromTag[i].TokenId, tokenCount, tagOwner, playPtn, situation, isPseudo);
			}
		}
	}

	private List<AITokenInformation> GetTokenIdFromTag(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation, isBlockDead: false);
		AITokenIdCollection bothSideTokenIdListFromFilter = AISummonTokenUtility.GetBothSideTokenIdListFromFilter(tagOwner, field, targetsFromField, base.Filters, AITokenType.Default, AIScriptTokenArgType.ALLY, AIScriptTokenArgType.ALL_SELECT, 1, playPtn, situation);
		if (bothSideTokenIdListFromFilter == null || !bothSideTokenIdListFromFilter.HasAllyToken)
		{
			return null;
		}
		return bothSideTokenIdListFromFilter.AllyTokenIdList;
	}
}

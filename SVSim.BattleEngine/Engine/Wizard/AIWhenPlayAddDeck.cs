using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayAddDeck : AIWhenPlayTagArgument
{
	private AIPolishConvertedExpression _addCount;

	private readonly int COUNT_ARG_INDEX = 1;

	public AIWhenPlayAddDeck(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeFilter();
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
		_addCount = _exprList[_exprList.Count - COUNT_ARG_INDEX];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AddDeckAction(tagOwner, field, playPtn, situation);
	}

	public override void ExecuteForPlayPtnEvaluation(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AddDeckAction(owner, field, playPtn, situation, isPseudo: true);
	}

	public override void PseudoExecute(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
		AddDeckAction(playInfo.Card, field, record.PlayPtn, situation, isPseudo: true);
	}

	private void AddDeckAction(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null, bool isPseudo = false)
	{
		List<AITokenInformation> tokenIdFromTag = GetTokenIdFromTag(tagOwner, field, playPtn, situation);
		if (tokenIdFromTag != null)
		{
			int tokenCount = (int)_addCount.EvalArg(tagOwner, playPtn, field, situation);
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

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}
}

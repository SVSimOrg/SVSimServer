using System.Collections.Generic;

namespace Wizard;

public class AIAttackDiscard : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _discardCountArg;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIAttackDiscard(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_discardCountArg = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int discardCount = GetDiscardCount(tagOwner, playPtn, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.RANDOM_SELECT:
				AISkillSimulationUtility.DiscardRandom(tagOwner, field, targetsFromField, discardCount, situation);
				break;
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.DiscardAll(tagOwner, targetsFromField, field, situation);
				break;
			}
		}
	}

	public bool IsAttackDiscardTargetInPlayPtn(AIVirtualCard tagOwner, List<AIVirtualCard> handCards, List<int> playPtn, AISituationInfo situation)
	{
		for (int i = 0; i < playPtn.Count; i++)
		{
			if (AIFilteringUtility.CheckMatchTargetFiltering(handCards[playPtn[i]], handCards, base.Filters, playPtn, tagOwner, situation))
			{
				return true;
			}
		}
		return false;
	}

	private int GetDiscardCount(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_discardCountArg == null)
		{
			return 0;
		}
		return (int)_discardCountArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}
}

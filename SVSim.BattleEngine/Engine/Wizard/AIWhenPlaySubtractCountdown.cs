using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlaySubtractCountdown : AIWhenPlayTagArgument
{
	private readonly int COUNT_CHANGE_ARG_OFFSET = 1;

	public AIPolishConvertedExpression CountChange { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlaySubtractCountdown(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		CountChange = _exprList[_exprList.Count - COUNT_CHANGE_ARG_OFFSET];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int num = (int)CountChange.EvalArg(tagOwner, playPtn, field, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISubtractCountdownSimulationUtility.SubtractCountdownAll(targetsFromField, num, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AISubtractCountdownSimulationUtility.ExecuteTargetSelectSubtractCountdown(tagOwner, targetsFromField, field, playPtn, situation, base.SelectType, num);
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForCountdownAmuletOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

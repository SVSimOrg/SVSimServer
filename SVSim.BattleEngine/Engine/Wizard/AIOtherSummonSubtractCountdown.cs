using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonSubtractCountdown : AITriggerAndTargetFiltersTagBase
{

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIPolishConvertedExpression SubtractValue { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIOtherSummonSubtractCountdown(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
		SubtractValue = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int value = (int)SubtractValue.EvalArg(tagOwner, playPtn, field, situation);
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AISubtractCountdownSimulationUtility.SubtractCountdownAll(targets, value, situation);
		}
		else
		{
			AIConsoleUtility.LogError("AIOtherSummonSubtractCountdown.RunTagMethod(): Unsupport select type!");
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForCountdownAmuletOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}

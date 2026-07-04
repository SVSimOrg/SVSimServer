using System.Collections.Generic;

namespace Wizard;

public class AIOtherDamagedSubtractCountdown : AITriggerAndTargetFiltersTagBase
{
	private AIScriptTokenArgType _selectType;

	private AIPolishConvertedExpression _count;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIOtherDamagedSubtractCountdown(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
		_count = _exprList[_exprList.Count - 1];
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForCountdownAmuletOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (targets != null && targets.Count > 0)
		{
			int value = (int)_count.EvalArg(tagOwner, playPtn, field, situation);
			if (_selectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISubtractCountdownSimulationUtility.SubtractCountdownAll(targets, value, situation);
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIAfterAttackEvo : AITriggerAndTargetFiltersTagBase
{

	public AIScriptTokenArgType SelectType { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIAfterAttackEvo(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (IsLegalSelectType(_exprList[_exprList.Count - 1], out var selectType))
		{
			SelectType = selectType;
		}
		else
		{
			LogSelectTypeError(selectType);
		}
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			AIAutoEvolutionSimulationUtility.AutoEvolution(field, targets, playPtn, situation, SelectType);
		}
	}

	private void LogSelectTypeError(AIScriptTokenArgType selectType)
	{
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.AllyInplayCards;
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIOtherDamagedHeal : AITriggerAndTargetFiltersTagBase
{

	private AIPolishConvertedExpression _healArg;

	public AIScriptTokenArgType SelectType { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIOtherDamagedHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
		_healArg = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (targets != null && targets.Count > 0)
		{
			int heal = (int)_healArg.EvalArg(tagOwner, playPtn, field, situation);
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targets, field, heal, playPtn, situation);
			}
			else
			{
				AIConsoleUtility.LogError("not implementation : AIOtherDamagedHeal other than ALL_SELECT");
			}
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

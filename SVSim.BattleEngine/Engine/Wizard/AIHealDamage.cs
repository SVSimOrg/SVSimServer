using System.Collections.Generic;

namespace Wizard;

public class AIHealDamage : AITriggerAndTargetFiltersTagBase
{

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIPolishConvertedExpression Damage { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIHealDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIPolishConvertedExpression arg = _exprList[_exprList.Count - 2];
		AIScriptTokenArgType selectType = AIScriptTokenArgType.NONE;
		if (!IsLegalSelectType(arg, out selectType))
		{
			SelectType = AIScriptTokenArgType.ALL_SELECT;
		}
		else
		{
			SelectType = selectType;
		}
		Damage = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int damage = (int)Damage.EvalArg(tagOwner, playPtn, field, situation);
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damage, situation);
			}
			else if (SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIDamageSimulationUtility.DamageRandom(targets, tagOwner, field, damage, situation);
			}
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

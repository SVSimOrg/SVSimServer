using System.Collections.Generic;

namespace Wizard;

public class AIBounceDamage : AITriggerAndTargetFiltersTagBase
{
	private readonly int DAMAGE_OFFSET = 1;

	private readonly int SELECT_TYPE_OFFSET = 2;

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIPolishConvertedExpression Damage { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIBounceDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (IsLegalSelectType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], out var selectType))
		{
			SelectType = selectType;
		}
		Damage = _exprList[_exprList.Count - DAMAGE_OFFSET];
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

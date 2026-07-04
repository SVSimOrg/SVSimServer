using System.Collections.Generic;

namespace Wizard;

public class AILeaveDamage : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _damageAmount;

	private readonly int DAMAGE_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 2;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AILeaveDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damageAmount = _exprList[_exprList.Count - DAMAGE_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int damage = (int)_damageAmount.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damage, situation);
			}
			else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damage, situation);
			}
			else
			{
				AIConsoleUtility.LogError("AILeaveDamage Error!! Unsupport SelectType=" + base.SelectType);
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

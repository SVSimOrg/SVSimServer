using System.Collections.Generic;

namespace Wizard;

public class AIDiscardDamage : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _damageArg;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIDiscardDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damageArg = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int damage = GetDamage(tagOwner, playPtn, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, tagOwner.SelfField, damage, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, tagOwner.SelfField, damage, situation);
				break;
			}
		}
	}

	private int GetDamage(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageArg == null)
		{
			return 0;
		}
		return (int)_damageArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

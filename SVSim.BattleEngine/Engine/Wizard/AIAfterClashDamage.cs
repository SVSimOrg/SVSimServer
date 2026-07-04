using System.Collections.Generic;

namespace Wizard;

public class AIAfterClashDamage : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _damage;

	private readonly int DAMAGE_AMOUNT_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIAfterClashDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damage = _exprList[_exprList.Count - DAMAGE_AMOUNT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int damage = (int)_damage.EvalArg(tagOwner, playPtn, field, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damage, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damage, situation);
				break;
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

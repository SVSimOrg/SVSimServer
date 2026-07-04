using System.Collections.Generic;

namespace Wizard;

public class AINecromanceDamage : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _damageAmount;

	protected override int SELECT_TYPE_OFFSET => 2;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AINecromanceDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damageAmount = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int damageAmount = GetDamageAmount(tagOwner, field, playPtn, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damageAmount, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damageAmount, situation);
				break;
			}
		}
	}

	private int GetDamageAmount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageAmount == null)
		{
			AIConsoleUtility.LogError("AINecromanceDamage.GetDamageAmount() error!! _damageAmount is null");
			return 0;
		}
		return (int)_damageAmount.EvalArg(tagOwner, playPtn, field, situation);
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}
}

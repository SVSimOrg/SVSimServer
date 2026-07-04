using System.Collections.Generic;

namespace Wizard;

public class AILeaveHeal : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _healValue;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AILeaveHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_healValue = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int healValue = GetHealValue(tagOwner, playPtn, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targetsFromField, field, healValue, playPtn, situation);
			}
			else
			{
				AIConsoleUtility.LogError("AILeaveHeal Error!! Unsupport SelectType=" + base.SelectType);
			}
		}
	}

	public int GetHealValue(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_healValue == null)
		{
			AIConsoleUtility.LogError("AILeaveHeal error!!! _healValue argument is null!!");
			return 0;
		}
		return (int)_healValue.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

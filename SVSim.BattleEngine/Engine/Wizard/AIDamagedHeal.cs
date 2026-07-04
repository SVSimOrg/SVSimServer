using System.Collections.Generic;

namespace Wizard;

public class AIDamagedHeal : AIFiltersAndSelectTypeArgument
{

	private AIPolishConvertedExpression _healAmount;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIDamagedHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_healAmount = _exprList[_exprList.Count - 1];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, owner, playPtn, situation, isBlockDead);
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int healAmount = GetHealAmount(tagOwner, field, playPtn, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targetsFromField, field, healAmount, playPtn, situation);
			}
		}
	}

	public int GetHealAmount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_healAmount == null)
		{
			AIConsoleUtility.LogError("AIDamagedHeal error!! _healAmount is null!");
			return 0;
		}
		return (int)_healAmount.EvalArg(tagOwner, playPtn, field, situation);
	}
}

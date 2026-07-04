using System.Collections.Generic;

namespace Wizard;

public class AIDamagedDamage : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _damageValue;

	private AIPolishConvertedExpression _damageCount;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIDamagedDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damageValue = _exprList[_exprList.Count - 2];
		_damageCount = _exprList[_exprList.Count - 1];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, owner, playPtn, situation, isBlockDead);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int damage = GetDamage(tagOwner, field, playPtn, situation);
		int count = GetCount(tagOwner, field, playPtn, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			for (int j = 0; j < count; j++)
			{
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damage, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			for (int i = 0; i < count; i++)
			{
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damage, situation);
			}
			break;
		}
		}
	}

	private int GetDamage(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageValue == null)
		{
			AIConsoleUtility.LogError("AIDamagedDamage error!! _damageValue is null");
			return 0;
		}
		return (int)_damageValue.EvalArg(tagOwner, playPtn, field, situation);
	}

	private int GetCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageCount == null)
		{
			AIConsoleUtility.LogError("AIDamagedDamage error!! _damageCount is null");
			return 0;
		}
		return (int)_damageCount.EvalArg(tagOwner, playPtn, field, situation);
	}
}

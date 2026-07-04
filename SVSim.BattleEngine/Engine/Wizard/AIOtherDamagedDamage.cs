using System.Collections.Generic;

namespace Wizard;

public class AIOtherDamagedDamage : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _damage;

	private AIPolishConvertedExpression _count;

	protected override int NON_FILTER_FIRST_OFFSET => 3;

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIOtherDamagedDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 3], base.LegalSelectTypes);
		_damage = _exprList[_exprList.Count - 2];
		_count = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets == null || targets.Count == 0)
		{
			return;
		}
		int damage = GetDamage(tagOwner, playPtn, situation);
		int times = GetTimes(tagOwner, playPtn, situation);
		switch (SelectType)
		{
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			for (int j = 0; j < times; j++)
			{
				AIDamageSimulationUtility.DamageRandom(targets, tagOwner, field, damage, situation);
			}
			break;
		}
		case AIScriptTokenArgType.ALL_SELECT:
		{
			for (int i = 0; i < times; i++)
			{
				AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damage, situation);
			}
			break;
		}
		}
	}

	public int GetDamage(AIVirtualCard tagOwner, List<int> playPtn = null, AISituationInfo situation = null)
	{
		return (int)_damage.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	public int GetTimes(AIVirtualCard tagOwner, List<int> playPtn = null, AISituationInfo situation = null)
	{
		return (int)_count.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
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

using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayDamage : AIWhenPlayTagArgument
{
	private AIPolishConvertedExpression _damageArg;

	private AIPolishConvertedExpression _countArg;

	private readonly int EXECUTE_COUNT_ARG_OFFSET = 1;

	private readonly int DAMAGE_ARG_OFFSET = 2;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIWhenPlayDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damageArg = _exprList[_exprList.Count - DAMAGE_ARG_OFFSET];
		_countArg = _exprList[_exprList.Count - EXECUTE_COUNT_ARG_OFFSET];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[6]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT,
			AIScriptTokenArgType.DIVIDED_SELECT,
			AIScriptTokenArgType.RANDOM_MULTI_SELECT
		};
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int num = (int)_damageArg.EvalArg(tagOwner, playPtn, field, situation);
		int num2 = (int)_countArg.EvalArg(tagOwner, playPtn, field, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			UpdateSituationRemovalType(situation);
			for (int j = 0; j < num2; j++)
			{
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, num, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			for (int k = 0; k < num2; k++)
			{
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, num, situation);
			}
			break;
		}
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			AIDamageSimulationUtility.ExecuteTargetSelectDamage(tagOwner, targetsFromField, field, playPtn, situation, base.SelectType, num, num2);
			break;
		case AIScriptTokenArgType.DIVIDED_SELECT:
		{
			for (int i = 0; i < num2; i++)
			{
				AIDamageSimulationUtility.DamageOldOrderedTargets(targetsFromField, num, tagOwner, field, playPtn, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			AIDamageSimulationUtility.DamageRandomMultiSelect(targetsFromField, tagOwner, field, num, num2, situation);
			break;
		default:
			AIConsoleUtility.LogError("AIPlayDamage.Execute() Error!! SelectType=" + base.SelectType);
			break;
		}
	}

	public override void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (base.SelectType != AIScriptTokenArgType.ALL_SELECT || (!target.IsLeader && !target.IsUnit) || !AIFilteringUtility.CheckMatchTargetFiltering(target, field.CardListSet.BothClassAndInplayCards, base.Filters, playPtn, owner, situation))
		{
			return;
		}
		int damageAmount = (int)_damageArg.EvalArg(owner, playPtn, field, situation);
		int num = (int)_countArg.EvalArg(owner, playPtn, field, situation);
		for (int i = 0; i < num; i++)
		{
			int num2 = target.SimulateDamageAmount(damageAmount, isSkillDamage: true, owner.IsSpell);
			targetLifeRecord.CurrentLife -= num2;
			if (targetLifeRecord.CurrentLife <= 0)
			{
				break;
			}
		}
	}

	public override void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		if (base.SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			return;
		}
		int damageAmount = (int)_damageArg.EvalArg(owner, playPtn, field, situation);
		int num = (int)_countArg.EvalArg(owner, playPtn, field, situation);
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetList[i];
			LifeRecord lifeRecord = lifeList[i];
			if ((!aIVirtualCard.IsLeader && !aIVirtualCard.IsUnit) || !AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, field.CardListSet.BothClassAndInplayCards, base.Filters, playPtn, owner, situation))
			{
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				int num2 = aIVirtualCard.SimulateDamageAmount(damageAmount, isSkillDamage: true, owner.IsSpell);
				lifeRecord.CurrentLife -= num2;
				if (lifeRecord.CurrentLife <= 0)
				{
					if (aIVirtualCard.IsDestroyByBanish)
					{
						situation.RegisterOwnBanishedCard(aIVirtualCard);
					}
					else
					{
						situation.RegisterOwnDestroyedCard(aIVirtualCard);
					}
					break;
				}
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Damage;
	}
}

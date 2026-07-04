using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIWhenPlayHeal : AIWhenPlayTagArgument
{
	private readonly int HEAL_ARG_OFFSET = 1;

	public AIPolishConvertedExpression Heal { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlayHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Heal = _exprList[_exprList.Count - HEAL_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int heal = (int)Heal.EvalArg(tagOwner, playPtn, field, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AISkillSimulationUtility.HealAll(targetsFromField, field, heal, playPtn, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			if (situation == null || !situation.IsTargetExists(base.SelectType))
			{
				AISkillSimulationUtility.HealTargetPrediction(situation, targetsFromField, tagOwner, field, playPtn, base.SelectType, heal);
			}
			else
			{
				AISkillSimulationUtility.HealTarget(situation, field, base.SelectType, heal);
			}
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			break;
		}
	}

	public bool IsDelayHeal(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, field.BestPlayPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int num = (int)Heal.EvalArg(tagOwner, field.BestPlayPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.TARGET_SELECT)
			{
				float num2 = float.MinValue;
				AIVirtualCard aIVirtualCard = null;
				float num3 = float.MinValue;
				AIVirtualCard aIVirtualCard2 = null;
				for (int i = 0; i < targetsFromField.Count; i++)
				{
					AIVirtualCard aIVirtualCard3 = targetsFromField[i];
					if (!aIVirtualCard3.IsDead)
					{
						float num4 = (float)Mathf.Min(num, aIVirtualCard3.MaxLife - aIVirtualCard3.Life) * (aIVirtualCard3.IsAlly ? 1f : (-1f));
						if (num2 < num4)
						{
							num2 = num4;
							aIVirtualCard = aIVirtualCard3;
						}
						float num5 = AIHealSimulationUtility.CalcEvalHealAfterAttack(aIVirtualCard3, num);
						if (num3 < num5)
						{
							num3 = num5;
							aIVirtualCard2 = aIVirtualCard3;
						}
					}
				}
				num2 = ((aIVirtualCard == null) ? 0f : num2);
				num3 = ((aIVirtualCard2 == null) ? 0f : num3);
				return num3 > num2;
			}
		}
		return false;
	}

	public override void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
	}

	public override void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[4]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

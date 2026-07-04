using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIWhenPlaySetMaxStatus : AIWhenPlayTagArgument
{
	private readonly int LIFE_ARG_OFFSET = 1;

	private readonly int ATTACK_ARG_OFFSET = 2;

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIWhenPlaySetMaxStatus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Attack = _exprList[_exprList.Count - ATTACK_ARG_OFFSET];
		Life = _exprList[_exprList.Count - LIFE_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int attackValue = GetAttackValue(tagOwner, playPtn, field, situation);
		int lifeValue = GetLifeValue(tagOwner, playPtn, field, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AISetStatusSimulationUtility.SetMaxStatusToAll(targetsFromField, attackValue, lifeValue, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			if (!situation.IsTargetExists(base.SelectType))
			{
				SetMaxStatusToPredictedTarget(targetsFromField, attackValue, lifeValue, situation);
			}
			else
			{
				AISetStatusSimulationUtility.SetMaxStatusToTarget(situation, attackValue, lifeValue, base.SelectType);
			}
			break;
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			break;
		}
	}

	private int GetAttackValue(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		if (Attack.IsCertainArgumentTypeExpress(AIScriptTokenArgType.NONE))
		{
			return -1;
		}
		return (int)Attack.EvalArg(tagOwner, playPtn, field, situation);
	}

	private int GetLifeValue(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		if (Life.IsCertainArgumentTypeExpress(AIScriptTokenArgType.NONE))
		{
			return -1;
		}
		return (int)Life.EvalArg(tagOwner, playPtn, field, situation);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !Attack.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	private void SetMaxStatusToPredictedTarget(List<AIVirtualCard> candidates, int attack, int life, AISituationInfo situation)
	{
		int num = int.MinValue;
		int num2 = int.MinValue;
		AIVirtualCard target = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			int num3 = attack - aIVirtualCard.Attack;
			int num4 = Mathf.Min(aIVirtualCard.Life, life) - aIVirtualCard.Life;
			int num5 = num3 + num4;
			if (num5 > num || (num5 == num && num3 > num2))
			{
				num = num5;
				num2 = num3;
				target = aIVirtualCard;
			}
		}
		situation.SetSingleTargetInInfo(target, TargetSelectType.Default, base.SelectType);
		AISetStatusSimulationUtility.SetMaxStatusToTarget(situation, attack, life, base.SelectType);
	}
}

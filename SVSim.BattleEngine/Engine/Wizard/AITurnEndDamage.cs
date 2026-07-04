using System.Collections.Generic;

namespace Wizard;

public class AITurnEndDamage : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private AIPolishConvertedExpression _damageArg;

	private AIPolishConvertedExpression _countArg;

	private readonly int DAMAGE_ARG_OFFSET = 3;

	private readonly int COUNT_ARG_OFFSET = 2;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 4;

	public AITurnEndDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		int count = _exprList.Count;
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		_damageArg = _exprList[count - DAMAGE_ARG_OFFSET];
		_countArg = _exprList[count - COUNT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int damage = GetDamage(tagOwner, playPtn);
		int count = GetCount(tagOwner, playPtn);
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
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			AIDamageSimulationUtility.DamageRandomMultiSelect(targetsFromField, tagOwner, field, damage, count, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
			break;
		}
	}

	public bool IsTarget(AIVirtualCard target, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!target.IsAmulet)
		{
			return AIFilteringUtility.CheckMatchTargetFiltering(target, field.CardListSet.BothClassAndInplayCards, base.Filters, playPtn, tagOwner, situation);
		}
		return false;
	}

	public int GetDamage(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (_damageArg == null)
		{
			return 0;
		}
		return (int)_damageArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField);
	}

	public int GetCount(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (_countArg == null)
		{
			return 0;
		}
		return (int)_countArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField);
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, selfField, EnemyAI.EmptyPlayPtn, selfField.CommonAllyTurnEndSituation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return 0f;
		}
		float num = 0f;
		int damage = GetDamage(tagOwner, EnemyAI.EmptyPlayPtn);
		int count = GetCount(tagOwner, EnemyAI.EmptyPlayPtn);
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			for (int i = 0; i < selfField.CardListSet.BothClassAndInplayCards.Count; i++)
			{
				AIVirtualCard aIVirtualCard = selfField.CardListSet.BothClassAndInplayCards[i];
				if (targetsFromField.Contains(aIVirtualCard))
				{
					Tuple<int, int> tuple = allInplayStatusList[i];
					if (tuple.second > 0)
					{
						int life = tuple.second;
						num += CalculateDamageThreatenToOneCard(aIVirtualCard, damage, count, tagOwner.IsAlly, ref life);
						allInplayStatusList[i].second = life;
					}
				}
			}
		}
		else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
		{
			num = float.MinValue;
			int num2 = -1;
			int second = -1;
			for (int j = 0; j < selfField.CardListSet.BothClassAndInplayCards.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = selfField.CardListSet.BothClassAndInplayCards[j];
				if (!targetsFromField.Contains(aIVirtualCard2))
				{
					continue;
				}
				Tuple<int, int> tuple2 = allInplayStatusList[j];
				if (tuple2.second > 0)
				{
					int life2 = tuple2.second;
					float num3 = CalculateDamageThreatenToOneCard(aIVirtualCard2, damage, count, tagOwner.IsAlly, ref life2);
					if (num3 >= num)
					{
						num = num3;
						num2 = j;
						second = life2;
					}
				}
			}
			if (num2 >= 0)
			{
				allInplayStatusList[num2].second = second;
			}
		}
		return num;
	}

	private float CalculateDamageThreatenToOneCard(AIVirtualCard target, int damage, int count, bool isTagOwnerAlly, ref int life)
	{
		int num = life;
		float num2 = 0f;
		for (int i = 0; i < count; i++)
		{
			if (num <= 0)
			{
				break;
			}
			int num3 = target.SimulateDamageAmount(damage, isSkillDamage: true);
			num -= num3;
		}
		if (target.IsLeader)
		{
			if (num < 0)
			{
				num = 0;
			}
			num2 += AILeaderLifeEvaluationUtility.Evaluate(num, life, target.IsAlly, isTagOwnerAlly);
		}
		else
		{
			num2 = ((num > 0) ? (num2 + (float)(life - num)) : (num2 + target.Value));
			num2 *= (float)((target.IsAlly != isTagOwnerAlly) ? 1 : (-1));
		}
		life = num;
		return num2;
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}

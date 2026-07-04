using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AITurnEndBuff : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private readonly int IS_ALLY_TURN_OFFSET = 1;

	private readonly int LIFE_OFFSET = 2;

	private readonly int ATTACK_OFFSET = 3;

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 4;

	public AITurnEndBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		Attack = _exprList[_exprList.Count - ATTACK_OFFSET];
		Life = _exprList[_exprList.Count - LIFE_OFFSET];
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
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			for (int j = 0; j < selfField.CardListSet.BothClassAndInplayCards.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = selfField.CardListSet.BothClassAndInplayCards[j];
				if (targetsFromField.Contains(aIVirtualCard2))
				{
					Tuple<int, int> tuple2 = allInplayStatusList[j];
					int attack2 = tuple2.first;
					int life2 = tuple2.second;
					num += CalculateBuffThreatenToOneCard(tagOwner, aIVirtualCard2, ref attack2, ref life2);
					allInplayStatusList[j].first = attack2;
					allInplayStatusList[j].second = life2;
				}
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			int num2 = -1;
			int first = -1;
			int second = -1;
			num = float.MinValue;
			for (int i = 0; i < selfField.CardListSet.BothClassAndInplayCards.Count; i++)
			{
				AIVirtualCard aIVirtualCard = selfField.CardListSet.BothClassAndInplayCards[i];
				if (targetsFromField.Contains(aIVirtualCard) && !aIVirtualCard.IsLeader && !aIVirtualCard.IsAmulet)
				{
					Tuple<int, int> tuple = allInplayStatusList[i];
					int attack = tuple.first;
					int life = tuple.second;
					float num3 = CalculateBuffThreatenToOneCard(tagOwner, aIVirtualCard, ref attack, ref life);
					if (num3 > num)
					{
						num = num3;
						num2 = i;
						first = attack;
						second = life;
					}
				}
			}
			if (num2 != -1)
			{
				allInplayStatusList[num2].first = first;
				allInplayStatusList[num2].second = second;
			}
			else
			{
				num = 0f;
			}
			break;
		}
		}
		return num;
	}

	private float CalculateBuffThreatenToOneCard(AIVirtualCard tagOwner, AIVirtualCard target, ref int attack, ref int life)
	{
		if (!target.IsUnit || life <= 0)
		{
			return 0f;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, selfField, selfField.CommonAllyTurnEndSituation, selfField.BestPlayPtn, Attack, Life);
		int num = Mathf.Max(0, attack + buffExecutingInfo_old.GetExpectedAttackBuffValue(attack));
		int num2 = Mathf.Max(0, life + buffExecutingInfo_old.GetExpectedLifeBuffValue(life));
		float result = ((num2 <= 0) ? target.Value : ((float)(life - num2 + (attack - num)))) * (float)(target.IsAlly ? 1 : (-1));
		attack = num;
		life = num2;
		return result;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBuffSimulationUtility.BuffAll_old(targetsFromField, field, buffExecutingInfo_old, isTemp: false, playPtn, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIBuffSimulationUtility.BuffRandom_old(targetsFromField, field, playPtn, situation, buffExecutingInfo_old, isTemp: false);
				break;
			}
		}
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
}

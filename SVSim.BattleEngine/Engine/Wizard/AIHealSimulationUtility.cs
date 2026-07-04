using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public static class AIHealSimulationUtility
{
	public static int CalcHealModifier(AIVirtualCard healTarget, List<int> playPtn, AISituationInfo situation, int originalHealValue)
	{
		if (!healTarget.TagCollectionContainer.HasTag(AIPlayTagType.ModifyHeal))
		{
			return originalHealValue;
		}
		return healTarget.TagCollectionContainer.ModifyHealTags.GetModifiedHealValue(healTarget, healTarget, originalHealValue, playPtn, situation);
	}

	public static float EvalTargetingHeal(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, int heal)
	{
		float num = float.MinValue;
		AIVirtualField selfField = tagOwner.SelfField;
		AIVirtualCard aIVirtualCard = null;
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(selfField.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null);
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard2 = list[i];
				if (aIVirtualCard2.IsAlly == tagOwner.IsAlly || (!aIVirtualCard2.IsUntouchable && !aIVirtualCard2.IsSneak))
				{
					int num2 = CalcHealModifier(aIVirtualCard2, playPtn, null, heal);
					float num3 = 0f;
					if (aIVirtualCard2.IsUnit)
					{
						num3 = (float)Mathf.Min(num2, aIVirtualCard2.MaxLife - aIVirtualCard2.Life) * (aIVirtualCard2.IsAlly ? 1f : (-1f));
					}
					else if (aIVirtualCard2.IsLeader)
					{
						num3 = AILeaderLifeEvaluationUtility.Evaluate(Mathf.Min(aIVirtualCard2.MaxLife, aIVirtualCard2.Life + num2), aIVirtualCard2.Life, aIVirtualCard2.IsAlly, tagOwner.IsAlly);
					}
					if (num < num3)
					{
						num = num3;
						aIVirtualCard = aIVirtualCard2;
					}
					float num4 = CalcEvalHealAfterAttack(aIVirtualCard2, num2);
					if (num < num4)
					{
						num = num4;
						aIVirtualCard = aIVirtualCard2;
					}
				}
			}
		}
		if (aIVirtualCard != null)
		{
			return num;
		}
		return 0f;
	}

	public static float EvalAllHeal(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, int heal)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(tagOwner.SelfField.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (aIVirtualCard.IsAlly == tagOwner.IsAlly)
			{
				int num2 = CalcHealModifier(aIVirtualCard, playPtn, null, heal);
				float num3 = 0f;
				if (aIVirtualCard.IsUnit)
				{
					num3 = (float)Mathf.Min(num2, aIVirtualCard.MaxLife - aIVirtualCard.Life) * (aIVirtualCard.IsAlly ? 1f : (-1f));
				}
				else if (aIVirtualCard.IsLeader)
				{
					num3 = AILeaderLifeEvaluationUtility.Evaluate(Mathf.Min(aIVirtualCard.MaxLife, aIVirtualCard.Life + num2), aIVirtualCard.Life, aIVirtualCard.IsAlly, tagOwner.IsAlly);
				}
				num += num3;
			}
		}
		return num;
	}

	public static float CalcEvalHealAfterAttack(AIVirtualCard healTarget, int heal)
	{
		AIVirtualField selfField = healTarget.SelfField;
		if (!healTarget.IsUnit || !healTarget.IsAlly)
		{
			return 0f;
		}
		float num = float.MinValue;
		bool flag = healTarget.IsAbleEvolution();
		for (int i = 0; i < selfField.EnemyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = selfField.EnemyInplayCards[i];
			if (aIVirtualCard.IsCantUnderAttack(selfField.ParamQuery, healTarget, EnemyAI.EmptyPlayPtn, selfField))
			{
				continue;
			}
			AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(healTarget, aIVirtualCard);
			if (!(AIAttackSimulationUtility.IsAttackPossible(selfField, aIVirtualAttackInfo) || flag))
			{
				continue;
			}
			int num2 = healTarget.SimulateDamageAmount(aIVirtualCard.SimulateAttackAmount(aIVirtualAttackInfo));
			if (healTarget.Life > num2)
			{
				int b = healTarget.MaxLife - (healTarget.Life - num2);
				float num3 = Mathf.Min(heal, b);
				if (num3 > num)
				{
					num = num3;
				}
			}
			else if (flag && healTarget.EvolutionLife > num2)
			{
				int b2 = healTarget.BaseCard.BaseParameter.EvoLife - (healTarget.EvolutionLife - num2);
				float num4 = Mathf.Min(heal, b2);
				if (num4 > num)
				{
					num = num4;
				}
			}
		}
		return num;
	}

	public static int GetSelfTurnHealCountAll(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType durationType)
	{
		return GetSelfTurnHealCountPlayed(tagOwner, field, filters, playPtn, durationType, situation) + GetSelfTurnHealCountPlayPtn(tagOwner, field, filters, playPtn, situation);
	}

	public static int GetSelfTurnHealCountAtCountType(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType durationType, AIScriptTokenArgType countType)
	{
		return countType switch
		{
			AIScriptTokenArgType.PLAYED => GetSelfTurnHealCountPlayed(tagOwner, field, filters, playPtn, durationType, situation), 
			AIScriptTokenArgType.PLAYPTN => GetSelfTurnHealCountPlayPtn(tagOwner, field, filters, playPtn, situation), 
			AIScriptTokenArgType.BEFORE_PLAYPTN => GetSelfTurnHealCountBeforePlayPtn(tagOwner, field, filters, playPtn, situation, durationType), 
			_ => 0, 
		};
	}

	private static int GetSelfTurnHealCountPlayed(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AIScriptTokenArgType durationType, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		int turn = (tagOwner.IsAlly ? field.AllyTurnCount : field.EnemyTurnCount);
		return 0 + field.HealRecorderCollection.GetTurnHealCount(turn, list, tagOwner.IsAlly);
	}

	private static int GetSelfTurnHealCountPlayPtn(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		if (playPtn == null || !tagOwner.IsAlly)
		{
			return 0;
		}
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[playPtn[i]];
			if (aIVirtualCard.IsSameCard(tagOwner))
			{
				break;
			}
			num += aIVirtualCard.GetWhenPlayHealCount(list, field, playPtn, situation);
		}
		return num;
	}

	private static int GetSelfTurnHealCountBeforePlayPtn(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType durationType)
	{
		if (playPtn == null || !tagOwner.IsAlly)
		{
			return 0;
		}
		return GetSelfTurnHealCountPlayPtn(tagOwner, field, filters, playPtn, situation) + GetSelfTurnHealCountPlayed(tagOwner, field, filters, playPtn, durationType, situation);
	}

	public static AIVirtualCard SelectBestTarget(List<AIVirtualCard> targets, int healPoint)
	{
		float num = -1f;
		AIVirtualCard result = null;
		for (int i = 0; i < targets.Count; i++)
		{
			if (!targets[i].IsIndependent)
			{
				float num2 = Mathf.Min(healPoint, targets[i].MaxLife - targets[i].Life);
				if (num2 > num)
				{
					num = num2;
					result = targets[i];
				}
				if (num2 >= (float)healPoint)
				{
					break;
				}
			}
		}
		return result;
	}
}

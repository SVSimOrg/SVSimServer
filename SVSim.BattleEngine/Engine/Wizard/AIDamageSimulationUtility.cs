using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public static class AIDamageSimulationUtility
{
	public static float EvalTargetingDamageAndRandomMultiSelectDamage(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> targetFilters, List<AIScriptTokenBase> randomFilters, List<int> playPtn, AISituationInfo situation, int targetDamage, int randomDamage, int randomDamageCount)
	{
		if (tagOwner == null || field == null || targetFilters == null)
		{
			return 0f;
		}
		int num = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, targetDamage);
		int damageAmount = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, randomDamage);
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, targetFilters, tagOwner, playPtn, situation);
		list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		list = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(list, tagOwner, playPtn);
		float num2 = float.MinValue;
		int count = list.Count;
		AIVirtualCard target = null;
		for (int num3 = 0; num3 < count; num3++)
		{
			AIVirtualCard aIVirtualCard = list[num3];
			if (aIVirtualCard.IsAlly == tagOwner.IsAlly || (!aIVirtualCard.IsUntouchable && !aIVirtualCard.IsSneak))
			{
				float num4 = 0f;
				num4 = ((!aIVirtualCard.IsLeader) ? EvalDamageToCertainUnit(tagOwner, aIVirtualCard, field, num, playPtn, situation, field.AllyHandCards.Contains(tagOwner)) : AILeaderLifeEvaluationUtility.Evaluate(CalcLifeAfterDamage(aIVirtualCard, situation, num, tagOwner.IsSpell), aIVirtualCard.Life, aIVirtualCard.IsAlly, tagOwner.IsAlly));
				if (num4 > num2)
				{
					num2 = num4;
					target = aIVirtualCard;
				}
			}
		}
		if (target == null)
		{
			return AIRandomMultiDamageEvaluator.EvaluateRandomDamageAverage(tagOwner, field, playPtn, situation, targetFilters, damageAmount, randomDamageCount);
		}
		int num5 = target.SimulateDamageAmount(num, tagOwner.IsUnit, tagOwner.IsSpell);
		float num6 = 0f;
		int num7 = CalcLifeAfterDamage(target, situation, num, tagOwner.IsSpell);
		if (num7 <= 0)
		{
			int num8 = target.Life - num7;
			float num9 = target.EvaluateValueOnField(playPtn, situation, useStyle: true) - (target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) + target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false)) + (float)(num8 - target.Life) * -0.1f;
			num6 = (target.IsAlly ? (0f - num9) : num9);
			new List<AIVirtualCard>(field.CardListSet.BothClassAndInplayCards).RemoveAll((AIVirtualCard c) => c.Equals(target));
		}
		else
		{
			_ = field.CardListSet.BothClassAndInplayCards;
			num6 = target.EvaluateValueOnField(playPtn, situation, useStyle: true);
			target.Life -= num5;
			num6 -= target.EvaluateValueOnField(playPtn, situation, useStyle: true);
			target.Life += num5;
		}
		target.Life -= num5;
		num6 += AIRandomMultiDamageEvaluator.EvaluateRandomDamageAverage(tagOwner, field, playPtn, situation, targetFilters, damageAmount, randomDamageCount);
		target.Life += num5;
		return num6;
	}

	public static float EvalTargetingDamage(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, int damage)
	{
		if (tagOwner == null || field == null || filters == null)
		{
			return 0f;
		}
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		list = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(list, tagOwner, playPtn);
		int damage2 = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damage);
		float num = float.MinValue;
		int count = list.Count;
		if (count <= 0)
		{
			return 0f;
		}
		for (int num2 = 0; num2 < count; num2++)
		{
			AIVirtualCard aIVirtualCard = list[num2];
			if (aIVirtualCard.IsAlly == tagOwner.IsAlly || (!aIVirtualCard.IsUntouchable && !aIVirtualCard.IsSneak))
			{
				float num3 = 0f;
				num3 = ((!aIVirtualCard.IsLeader) ? EvalDamageToCertainUnit(tagOwner, aIVirtualCard, field, damage2, playPtn, situation, field.AllyHandCards.Contains(tagOwner)) : AILeaderLifeEvaluationUtility.Evaluate(CalcLifeAfterDamage(aIVirtualCard, situation, damage2, tagOwner.IsSpell), aIVirtualCard.Life, aIVirtualCard.IsAlly, tagOwner.IsAlly));
				if (num3 > num)
				{
					num = num3;
				}
			}
		}
		if (num == float.MinValue)
		{
			num = 0f;
		}
		return num;
	}

	private static float EvalDamageToCertainUnit(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, int damage, List<int> playPtn, AISituationInfo situation, bool isWhenPlaySkill)
	{
		float num = target.EvaluateValueOnField(playPtn, situation, useStyle: true) - (target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) + target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false));
		int life = target.Life;
		float num2 = 0f;
		int num3 = CalcLifeAfterDamage(target, situation, damage, tagOwner.IsSpell);
		if (num3 <= 0)
		{
			int num4 = life - num3;
			float num5 = num + (float)(num4 - life) * -0.1f;
			num2 = (target.IsAlly ? (0f - num5) : num5);
		}
		else
		{
			if (target.IsAlly)
			{
				return 0f;
			}
			if (target.IsCantUnderAnyAttack())
			{
				num2 += 0f;
			}
			else if (field.ParamQuery.GetEnemyGuardiansCount() > 0 && !target.IsGuard && !tagOwner.IsIgnoreGuard)
			{
				num2 += 0f;
			}
			else
			{
				AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(null, target);
				List<int> list = new List<int>();
				List<int> list2 = new List<int>();
				List<float> list3 = new List<float>();
				if (tagOwner.IsUnit && isWhenPlaySkill && tagOwner.IsAbleEvolution())
				{
					aIVirtualAttackInfo.SetActor(tagOwner);
					list.Add(tagOwner.SimulateAttackAmount(tagOwner.Attack + tagOwner.EvoAttackPlus, aIVirtualAttackInfo));
					list2.Add(tagOwner.Life + tagOwner.EvoLifePlus);
					list3.Add(tagOwner.EvaluateValueOnField(playPtn, situation, useStyle: true));
				}
				for (int i = 0; i < field.AllyInplayCards.Count; i++)
				{
					AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
					if (aIVirtualCard.IsUnit && !aIVirtualCard.IsDead && aIVirtualCard.IsAttackable(EnemyAI.EmptyPlayPtn))
					{
						aIVirtualAttackInfo.SetActor(aIVirtualCard);
						list.Add(aIVirtualCard.SimulateAttackAmount(aIVirtualAttackInfo));
						list2.Add(aIVirtualCard.Life);
						list3.Add(aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true));
					}
				}
				int num6 = (int)Mathf.Pow(2f, list.Count);
				for (int j = 0; j < num6; j++)
				{
					int num7 = j;
					int num8 = 0;
					int num9 = 0;
					float num10 = 0f;
					float num11 = 0f;
					while (num7 > 0)
					{
						if (num7 % 2 == 1)
						{
							num9 += list[num8];
							if (list2[num8] <= target.Attack)
							{
								num10 += list3[num8];
							}
						}
						num8++;
						num7 /= 2;
					}
					num9 = target.SimulateDamageAmount(num9);
					num11 = ((num9 >= life) ? (num11 + 0f) : ((num9 >= num3) ? (num11 + (num - num10)) : (num11 + 0f)));
					if (num2 < num11)
					{
						num2 = num11;
					}
				}
			}
		}
		return num2;
	}

	public static float EvalRandomMultiSelectDamage(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, int damage, int selectCount)
	{
		if (tagOwner == null || field == null || filters == null)
		{
			return 0f;
		}
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null);
		if (list != null && list.Count > 0)
		{
			list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
			if (list.Count <= selectCount)
			{
				return CalculateMultiAllDamage(tagOwner, list, field, playPtn, situation, damage, 1);
			}
			int num = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damage);
			List<float> list2 = new List<float>();
			List<int> list3 = new List<int>();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				list2.Add(list[num2].EvaluateValueOnField(playPtn, situation, useStyle: true) - list[num2].GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - list[num2].GetAllLeaveBonus(playPtn, useIgnoreInBattle: false));
				list3.Add(num2);
			}
			List<int[]> list4 = AIMathematicsLibrary.EnumerateCombinations(list3, selectCount).ToList();
			if (list4 == null || list4.Count == 0)
			{
				return 0f;
			}
			float num3 = 0f;
			for (int num4 = 0; num4 < list4.Count; num4++)
			{
				int[] array = list4[num4];
				float num5 = 0f;
				for (int num6 = 0; num6 < array.Length; num6++)
				{
					int num7 = CalcLifeAfterDamage(list[array[num6]], situation, num, tagOwner.IsSpell);
					num5 += ((num7 <= 0) ? list2[array[num6]] : ((float)num));
				}
				num3 += num5;
			}
			return num3 / (float)list4.Count;
		}
		return 0f;
	}

	public static int CalcLifeAfterDamage(AIVirtualCard target, AISituationInfo situation, int damage, bool isSpell)
	{
		int num = target.SimulateDamageAmount(damage, isSkillDamage: true, isSpell);
		int num2 = target.Life - num;
		int atkBuff = 0;
		int lifeBuff = 0;
		if (target.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenDamaged))
		{
			target.TagCollectionContainer.DamagedTags.GetDamagedBuffValue(target, target.SelfField, target.SelfField.BestPlayPtn, situation, out atkBuff, out lifeBuff);
			num2 += lifeBuff;
		}
		return num2;
	}

	public static float EvalEchoDamage(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, int damage, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			float item = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true) - aIVirtualCard.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
			List<AIVirtualCard> list2 = new List<AIVirtualCard> { aIVirtualCard };
			List<float> list3 = new List<float> { item };
			List<AIVirtualCard> list4 = (aIVirtualCard.IsAlly ? field.AllyInplayCards : field.EnemyInplayCards);
			for (int j = 0; j < list4.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = list4[j];
				if (!aIVirtualCard2.IsSameCard(aIVirtualCard) && aIVirtualCard2.BaseId == aIVirtualCard.BaseId)
				{
					list2.Add(aIVirtualCard2);
					float item2 = aIVirtualCard2.EvaluateValueOnField(playPtn, situation, useStyle: true) - aIVirtualCard2.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - aIVirtualCard2.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
					list3.Add(item2);
				}
			}
			float num2 = 0f;
			for (int k = 0; k < list2.Count; k++)
			{
				AIVirtualCard aIVirtualCard3 = list2[k];
				int damageAmount = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damage);
				damageAmount = aIVirtualCard3.SimulateDamageAmount(damageAmount, isSkillDamage: true, tagOwner.IsSpell);
				if (damageAmount >= aIVirtualCard3.Life)
				{
					num2 += list3[k];
				}
			}
			if (EnemyAI.IsLargerThan(num2, num))
			{
				num = num2;
			}
		}
		return num;
	}

	public static AIVirtualCard SelectDamageTarget(List<AIVirtualCard> candidates, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int damage, bool isSpell, AISelectTargetPattern bestOrWorst)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			AIConsoleUtility.LogError("AISimulationDamageUtility.SelectDamageTarget() : candidates is null ");
			return null;
		}
		AIVirtualCard result = null;
		float num = ((bestOrWorst == AISelectTargetPattern.Worst) ? float.MaxValue : float.MinValue);
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (!aIVirtualCard.IsDead)
			{
				float damageValueToCertainTarget = GetDamageValueToCertainTarget(aIVirtualCard, damage, situation, playPtn, isSpell);
				bool flag = false;
				switch (bestOrWorst)
				{
				case AISelectTargetPattern.Worst:
					flag = num > damageValueToCertainTarget;
					break;
				case AISelectTargetPattern.Best:
					flag = damageValueToCertainTarget > num;
					break;
				}
				if (flag)
				{
					num = damageValueToCertainTarget;
					result = aIVirtualCard;
				}
			}
		}
		return result;
	}

	public static float GetDamageValueToCertainTarget(AIVirtualCard target, int damage, AISituationInfo situation, List<int> playPtn, bool isSpell)
	{
		float num = 0f;
		int num2 = CalcLifeAfterDamage(target, situation, damage, isSpell);
		float num3 = target.EvaluateValueOnField(playPtn, situation, useStyle: true);
		if (num2 <= 0)
		{
			num = num3 - target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
		}
		else
		{
			float num4 = 0.01f;
			num = (float)(target.Life - num2) + num3 * num4;
		}
		return num * (target.IsAlly ? (-1f) : 1f);
	}

	public static float EvalOldestDamage(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, AIVirtualField field, int damage, List<int> playPtn, AISituationInfo situation)
	{
		float num = 0f;
		if (candidates == null || candidates.Count <= 0)
		{
			return 0f;
		}
		int num2 = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damage);
		AIVirtualCard aIVirtualCard = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = candidates[i];
			if (aIVirtualCard2.IsLeader)
			{
				if (aIVirtualCard == null)
				{
					aIVirtualCard = aIVirtualCard2;
				}
				else
				{
					AIConsoleUtility.LogError("EvalOldestDamage(): Already selected leader card! Candidate cards is illegal.");
				}
			}
			else if (aIVirtualCard2.IsUnit)
			{
				int life = aIVirtualCard2.Life;
				int num3 = ((num2 < life) ? num2 : life);
				num += EvalDamageToCertainUnit(tagOwner, aIVirtualCard2, field, num3, playPtn, situation, field.AllyHandCards.Contains(tagOwner));
				num2 -= num3;
				if (num2 <= 0)
				{
					break;
				}
			}
		}
		if (aIVirtualCard != null && num2 > 0)
		{
			AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = new AIBarrierPseudoSimulationInfo(aIVirtualCard);
			bool isSpell = tagOwner.IsSpell;
			int num4 = aIBarrierPseudoSimulationInfo.SimulateDamageAmount(aIVirtualCard.SimulateDamageShield(damage, isSkillDamage: true, isSpell), isSpell);
			num += AILeaderLifeEvaluationUtility.Evaluate(aIVirtualCard.Life - num4, aIVirtualCard.Life, aIVirtualCard.IsAlly, tagOwner.IsAlly);
		}
		return num;
	}

	public static float EvalMultiAllDamage(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, int damage, int damageCount)
	{
		if (tagOwner == null || field == null || filters == null)
		{
			return 0f;
		}
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null);
		list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		if (!list.IsNotNullOrEmpty())
		{
			return 0f;
		}
		AIFunctionResultContainer funcResultContainer = field.AI.FuncResultContainer;
		ulong hash = AIFunctionResultHashCalculator.GetHash(tagOwner, field, playPtn, null, GetArgumentHash(list, damage, damageCount));
		if (funcResultContainer.GetContainsResultValue(AIScriptTokenFuncType.EVAL_ALL_MULTI_DAMAGE, hash, out var getResult))
		{
			return getResult;
		}
		float result = CalculateMultiAllDamage(tagOwner, list, field, playPtn, situation, damage, damageCount);
		funcResultContainer.AddRecord(AIScriptTokenFuncType.EVAL_ALL_MULTI_DAMAGE, hash, result);
		return result;
	}

	private static float CalculateMultiAllDamage(AIVirtualCard tagOwner, List<AIVirtualCard> targetList, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int damage, int damageCount)
	{
		float num = 0f;
		List<AIVirtualCardStatusInfo> list = new List<AIVirtualCardStatusInfo>();
		List<AIVirtualCardStatusInfo> list2 = new List<AIVirtualCardStatusInfo>();
		int damageAmount = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damage);
		bool isSpell = tagOwner.IsSpell;
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetList[i];
			int num2 = aIVirtualCard.Life;
			AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = new AIBarrierPseudoSimulationInfo(aIVirtualCard);
			for (int j = 0; j < damageCount; j++)
			{
				int num3 = aIBarrierPseudoSimulationInfo.SimulateDamageAmount(aIVirtualCard.SimulateDamageShield(damageAmount, isSkillDamage: true, isSpell), isSpell);
				num2 -= num3;
				if (num2 <= 0)
				{
					break;
				}
				aIBarrierPseudoSimulationInfo.DepriveBarrier(AIBarrierStopTiming.AfterDamage);
			}
			if (aIVirtualCard.IsLeader)
			{
				num += AILeaderLifeEvaluationUtility.Evaluate(num2, aIVirtualCard.Life, aIVirtualCard.IsAlly, tagOwner.IsAlly);
			}
			else if (num2 <= 0)
			{
				float num4 = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true) - aIVirtualCard.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
				num += num4 * (aIVirtualCard.IsAlly ? (-1f) : 1f);
			}
			else if (!aIVirtualCard.IsAlly || aIVirtualCard.IsAttackable(playPtn))
			{
				(aIVirtualCard.IsAlly ? list : list2).Add(new AIVirtualCardStatusInfo(aIVirtualCard, aIVirtualCard.Attack, num2));
			}
		}
		if (list2.Count > 0 && list.Count > 0)
		{
			num += AISimulationUtility.EvaluateAttackValueAfterAllSkill(field, situation, list, list2, playPtn);
		}
		return num;
	}

	public static void DamageAll(List<AIVirtualCard> targets, AIVirtualCard damageOwner, AIVirtualField field, int damage, AISituationInfo situation)
	{
		int baseDamage = field.DamageModifierCollection.CalcModifiedDamage(field, field.BestPlayPtn, situation, damageOwner, damage);
		for (int i = 0; i < targets.Count; i++)
		{
			if ((targets[i].IsUnit || targets[i].IsLeader) && !targets[i].IsDead && !targets[i].IsIndependent)
			{
				targets[i].AddDamage(situation, baseDamage, isSkillDamage: true);
				if (targets[i].IsLeader && targets[i].IsAlly)
				{
					field.AllyDamageCountInGame++;
					field.AllyDamageCountInTurn++;
				}
			}
		}
		for (int j = 0; j < targets.Count; j++)
		{
			if (targets[j].IsDead)
			{
				targets[j].RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
			}
		}
	}

	public static void DamageRandom(List<AIVirtualCard> targets, AIVirtualCard damageOwner, AIVirtualField field, int damage, AISituationInfo situation)
	{
		List<int> bestPlayPtn = field.BestPlayPtn;
		AIVirtualCard aIVirtualCard = SelectDamageTarget(targets, field, bestPlayPtn, situation, damage, damageOwner.IsSpell, AISelectTargetPattern.Worst);
		if (aIVirtualCard != null && !aIVirtualCard.IsDead)
		{
			int baseDamage = field.DamageModifierCollection.CalcModifiedDamage(field, bestPlayPtn, situation, damageOwner, damage);
			aIVirtualCard.AddDamage(situation, baseDamage, isSkillDamage: true);
			if (aIVirtualCard.IsDead)
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
			}
		}
	}

	public static void DamageTarget(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualCard damageOwner, AIVirtualField field, AIScriptTokenArgType whichTarget, int damage, int count)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("DamageTarget error!! No target!!!!!");
			return;
		}
		int baseDamage = field.DamageModifierCollection.CalcModifiedDamage(field, field.BestPlayPtn, situation, damageOwner, damage);
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			bool flag = candidates.Contains(aIVirtualCard);
			for (int j = 0; j < count; j++)
			{
				if (aIVirtualCard.Life <= 0)
				{
					break;
				}
				if (flag)
				{
					aIVirtualCard.AddDamage(situation, baseDamage, isSkillDamage: true);
				}
			}
		}
		for (int k = 0; k < targets.Count; k++)
		{
			if (targets[k].IsDead)
			{
				targets[k].RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
			}
		}
	}

	public static void ExecuteTargetSelectDamage(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType selectType, int damageAmount, int selectCount = 1)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("ExecuteTargetSelectDamage() Error!! situation is null!!!!!");
		}
		else if (situation.IsTargetExists(selectType))
		{
			DamageTarget(situation, candidates, tagOwner, field, selectType, damageAmount, selectCount);
		}
		else
		{
			DamageTargetPrediction(situation, candidates, tagOwner, field, playPtn, selectType, damageAmount, selectCount);
		}
	}

	private static void DamageTargetPrediction(AISituationInfo situation, List<AIVirtualCard> candidates, AIVirtualCard damageOwner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType whichTarget, int damage, int count)
	{
		if (count > 0)
		{
			List<AIVirtualCard> candidates2 = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(candidates, damageOwner, playPtn);
			if (count == 1)
			{
				AIVirtualCard target = SelectDamageTarget(candidates2, field, playPtn, situation, damage, damageOwner.IsSpell, AISelectTargetPattern.Best);
				situation.SetSingleTargetInInfo(target, TargetSelectType.Default, whichTarget);
				DamageTarget(situation, candidates2, damageOwner, field, whichTarget, damage, count);
			}
			else
			{
				AIConsoleUtility.LogError("DamageTargetPrediction(): 複数選択ダメージのPrediction未対応");
			}
		}
	}

	public static void DamageRandomMultiSelect(List<AIVirtualCard> targets, AIVirtualCard owner, AIVirtualField field, int damage, int selectCount, AISituationInfo situation)
	{
		List<AIVirtualCard> list = null;
		List<int> bestPlayPtn = field.BestPlayPtn;
		for (int i = 0; i < selectCount; i++)
		{
			AIVirtualCard aIVirtualCard = SelectDamageTarget(targets, field, bestPlayPtn, situation, damage, owner.IsSpell, AISelectTargetPattern.Worst);
			if (aIVirtualCard == null || aIVirtualCard.IsDead)
			{
				break;
			}
			int baseDamage = field.DamageModifierCollection.CalcModifiedDamage(field, bestPlayPtn, situation, owner, damage);
			aIVirtualCard.AddDamage(situation, baseDamage, isSkillDamage: true);
			targets.Remove(aIVirtualCard);
			if (targets.Count <= 0)
			{
				break;
			}
			if (aIVirtualCard.IsDead)
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				list[j].RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
			}
		}
	}

	public static void DamageOldOrderedTargets(List<AIVirtualCard> targets, int damage, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		int num = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damage);
		AIVirtualCard aIVirtualCard = null;
		List<AIVirtualCard> list = null;
		for (int i = 0; i < targets.Count; i++)
		{
			if (num <= 0)
			{
				break;
			}
			AIVirtualCard aIVirtualCard2 = targets[i];
			if (aIVirtualCard2.IsAlly != tagOwner.IsAlly && aIVirtualCard2.IsLeader)
			{
				aIVirtualCard = aIVirtualCard2;
				continue;
			}
			int num2 = 0;
			num2 = ((aIVirtualCard != null || i != targets.Count - 1) ? Math.Min(aIVirtualCard2.Life, num) : num);
			num -= num2;
			aIVirtualCard2.AddDamage(situation, num2, isSkillDamage: true);
			if (aIVirtualCard2.IsDead)
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard2, list);
			}
		}
		if (aIVirtualCard != null && num > 0)
		{
			aIVirtualCard.AddDamage(situation, num, isSkillDamage: true);
			if (aIVirtualCard.IsDead)
			{
				return;
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				list[j].RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: true);
			}
		}
	}

	private static ulong GetArgumentHash(List<AIVirtualCard> targets, int damage, int damageCount)
	{
		ulong[] array = new ulong[12]
		{
			907uL, 911uL, 919uL, 929uL, 937uL, 941uL, 947uL, 953uL, 967uL, 971uL,
			977uL, 983uL
		};
		ulong num = 0uL;
		for (int i = 0; i < targets.Count; i++)
		{
			num += targets[i].GetHash() * array[i];
		}
		num += (ulong)((long)damage * 3L);
		return num + (ulong)((long)damageCount * 313L);
	}
}

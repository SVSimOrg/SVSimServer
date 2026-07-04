using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public static class AIInstantAttackUtility
{
	public static float EvalInstantAttack(int attack, int life, int count, List<int> playPtn, AIVirtualCard owner, AISituationInfo situation, bool isRush = false)
	{
		AIVirtualField selfField = owner.SelfField;
		if (selfField.IsNoInstantAttack)
		{
			return 0f;
		}
		AIFunctionResultContainer funcResultContainer = selfField.AI.FuncResultContainer;
		AIScriptTokenFuncType funcType = (isRush ? AIScriptTokenFuncType.EVAL_RUSH : AIScriptTokenFuncType.EVAL_INSTANT_ATTACK);
		ulong hash = AIFunctionResultHashCalculator.GetHash(owner, selfField, playPtn, null, GetArgumentHash(attack, life, count));
		if (funcResultContainer.GetContainsResultValue(funcType, hash, out var getResult))
		{
			return getResult;
		}
		List<AIVirtualCard> list = selfField.CardListSet.EnemyClassAndInplayCards.FindAll((AIVirtualCard c) => c.IsUnit && !c.IsCantUnderAnyAttack());
		Dictionary<int, float> savedEvaluations = CacheCardsEvaluation(list, playPtn, situation);
		Dictionary<int, float> savedBreakBonus = CacheCardsBreakBonus(list);
		Dictionary<int, float> savedLeaveBonus = CachedCardsLeaveBonus(list);
		int count2 = list.Count;
		List<AIVirtualCard> list2 = new List<AIVirtualCard>();
		List<AIVirtualCard> list3 = new List<AIVirtualCard>();
		for (int num = 0; num < count2; num++)
		{
			AIVirtualCard aIVirtualCard = list[num];
			if (aIVirtualCard.IsGuard)
			{
				list2.Add(aIVirtualCard);
			}
			else
			{
				list3.Add(aIVirtualCard);
			}
		}
		int count3 = list2.Count;
		int count4 = list3.Count;
		AIVirtualCard aIVirtualCard2 = CreateDummyAttacker(selfField, owner, attack, life, count, isRush);
		float num2 = 0f;
		if (count3 > 0)
		{
			int num3 = (int)Mathf.Pow(2f, count3) - 1;
			bool isAllGuardDestroyed = false;
			float num4 = EvalInstantAttackToCertainBreakOrLeavePattern(aIVirtualCard2, list2, selfField, num3, playPtn, ref isAllGuardDestroyed, savedEvaluations, savedBreakBonus, savedLeaveBonus);
			if (!isAllGuardDestroyed)
			{
				float num5 = 0f;
				for (int num6 = num3 - 1; num6 >= 0; num6--)
				{
					aIVirtualCard2.AttackableCount = count;
					aIVirtualCard2.Life = life;
					aIVirtualCard2.Attack = attack;
					float num7 = EvalInstantAttackToCertainBreakOrLeavePattern(aIVirtualCard2, list2, selfField, num6, playPtn, ref isAllGuardDestroyed, savedEvaluations, savedBreakBonus, savedLeaveBonus);
					if (num7 > num5)
					{
						num5 = num7;
					}
				}
				if (num5 == 0f)
				{
					num5 = EvalInstantAttackAgainstFollower(list2, owner, selfField, playPtn, aIVirtualCard2);
				}
				funcResultContainer.AddRecord(funcType, hash, num5);
				RemoveDummyCardFromField(selfField);
				return num5;
			}
			num2 = num4;
			count = aIVirtualCard2.AttackableCount;
			attack = aIVirtualCard2.Attack;
			life = aIVirtualCard2.Life;
		}
		if (count > 0)
		{
			int num8 = (int)Mathf.Pow(2f, count4);
			float num9 = 0f;
			for (int num10 = 0; num10 < num8; num10++)
			{
				aIVirtualCard2.AttackableCount = count;
				aIVirtualCard2.Attack = attack;
				aIVirtualCard2.Life = life;
				bool isAllGuardDestroyed2 = true;
				float num11 = EvalInstantAttackToCertainBreakOrLeavePattern(aIVirtualCard2, list3, selfField, num10, playPtn, ref isAllGuardDestroyed2, savedEvaluations, savedBreakBonus, savedLeaveBonus);
				if (isRush)
				{
					if (num11 > num9)
					{
						num9 = num11;
					}
					continue;
				}
				float num12 = num11 + EvalInstantAttackAgainstLeader(aIVirtualCard2, playPtn);
				if (num12 > num9)
				{
					num9 = num12;
				}
			}
			num2 += num9;
		}
		funcResultContainer.AddRecord(funcType, hash, num2);
		RemoveDummyCardFromField(selfField);
		return num2;
	}

	private static float EvalInstantAttackAgainstFollower(List<AIVirtualCard> targetCards, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AIVirtualCard attacker)
	{
		float num = 0f;
		int num2 = attacker.AttackableCount;
		int attack = attacker.Attack;
		int num3 = attacker.Life;
		for (int i = 0; i < targetCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetCards[i];
			AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(attacker, aIVirtualCard);
			EvalInstantAttackInformation evalInstantAttackInformation = new EvalInstantAttackInformation(aIVirtualAttackInfo);
			aIVirtualAttackInfo.PseudoSimulateForEvalInstantAttack(field, playPtn, evalInstantAttackInformation);
			if (evalInstantAttackInformation.IsAttackerDestroyWhenAttack)
			{
				continue;
			}
			int targetLifeBuff = evalInstantAttackInformation.TargetLifeBuff;
			int attackerAttackBuff = evalInstantAttackInformation.AttackerAttackBuff;
			int attackerLifeBuff = evalInstantAttackInformation.AttackerLifeBuff;
			AIBarrierPseudoSimulationInfo targetBarrierInfo = evalInstantAttackInformation.TargetBarrierInfo;
			int damage = aIVirtualCard.SimulateDamageShield(attack + attackerAttackBuff);
			damage = targetBarrierInfo.SimulateDamageAmount(damage, isSpellDamage: false, isSkillDamage: false);
			float num4 = 0f;
			int attackerTotalDamage = evalInstantAttackInformation.AttackerTotalDamage;
			while (num2 > 0)
			{
				num3 += attackerLifeBuff - attackerTotalDamage;
				if (num3 <= 0)
				{
					break;
				}
				damage -= targetLifeBuff;
				num4 += (float)damage;
				num2--;
			}
			if (num4 > num)
			{
				num = num4;
			}
		}
		attacker.AttackableCount = num2;
		attacker.Life = num3;
		attacker.Attack = attack;
		return num;
	}

	private static float EvalInstantAttackAgainstLeader(AIVirtualCard dummyAttacker, List<int> playPtn)
	{
		if (dummyAttacker.Life <= 0 || dummyAttacker.AttackableCount <= 0)
		{
			return 0f;
		}
		AIVirtualAttackInfo attackLeaderSituation = dummyAttacker.AttackLeaderSituation;
		AIVirtualCard attackTarget = attackLeaderSituation.AttackTarget;
		AIVirtualField selfField = dummyAttacker.SelfField;
		if (attackLeaderSituation == null)
		{
			AIConsoleUtility.LogError("EvalInstantAttackAgainstLeader(): Leader attack situation is null!!");
			return 0f;
		}
		int num = attackTarget.Life;
		for (int i = 0; i < dummyAttacker.AttackableCount; i++)
		{
			EvalInstantAttackInformation evalInstantAttackInformation = new EvalInstantAttackInformation(attackLeaderSituation);
			attackLeaderSituation.PseudoSimulateForEvalInstantAttack(selfField, playPtn, evalInstantAttackInformation);
			int targetLifeBuff = evalInstantAttackInformation.TargetLifeBuff;
			int attackerAttackBuff = evalInstantAttackInformation.AttackerAttackBuff;
			int attackerLifeBuff = evalInstantAttackInformation.AttackerLifeBuff;
			int attackerTotalDamage = evalInstantAttackInformation.AttackerTotalDamage;
			AIBarrierPseudoSimulationInfo targetBarrierInfo = evalInstantAttackInformation.TargetBarrierInfo;
			int damage = attackTarget.SimulateDamageShield(dummyAttacker.Attack + attackerAttackBuff);
			damage = targetBarrierInfo.SimulateDamageAmount(damage, isSpellDamage: false, isSkillDamage: false);
			dummyAttacker.Life += attackerLifeBuff - attackerTotalDamage;
			if (dummyAttacker.Life <= 0)
			{
				break;
			}
			damage -= targetLifeBuff;
			num -= damage;
			if (num <= 0)
			{
				break;
			}
		}
		return AILeaderLifeEvaluationUtility.Evaluate(num, attackTarget.Life, attackTarget.IsAlly, dummyAttacker.IsAlly);
	}

	private static Dictionary<int, float> CacheCardsEvaluation(List<AIVirtualCard> targets, List<int> playPtn, AISituationInfo situation, bool useStyle = true)
	{
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			float value = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true);
			try
			{
				dictionary.Add(aIVirtualCard.CardIndex, value);
			}
			catch (ArgumentException)
			{
			}
		}
		return dictionary;
	}

	private static Dictionary<int, float> CacheCardsBreakBonus(List<AIVirtualCard> targets)
	{
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			float value = aIVirtualCard.EvaluateBreakValue(EnemyAI.EmptyPlayPtn, useIgnoreBreak: false);
			try
			{
				dictionary.Add(aIVirtualCard.CardIndex, value);
			}
			catch (ArgumentException)
			{
			}
		}
		return dictionary;
	}

	private static Dictionary<int, float> CachedCardsLeaveBonus(List<AIVirtualCard> targets)
	{
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			float value = aIVirtualCard.EvaluateLeaveValue(EnemyAI.EmptyPlayPtn, useIgnoreInBattle: true);
			try
			{
				dictionary.Add(aIVirtualCard.CardIndex, value);
			}
			catch (ArgumentException)
			{
			}
		}
		return dictionary;
	}

	private static float EvalInstantAttackToCertainBreakOrLeavePattern(AIVirtualCard attacker, List<AIVirtualCard> targetCards, AIVirtualField field, int breakPtnIndex, List<int> playPtn, ref bool isAllGuardDestroyed, Dictionary<int, float> savedEvaluations, Dictionary<int, float> savedBreakBonus, Dictionary<int, float> savedLeaveBonus)
	{
		float num = 0f;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		int count = targetCards.Count;
		for (int i = 0; i < count; i++)
		{
			int num2 = (int)Mathf.Pow(2f, count - i - 1);
			if (breakPtnIndex / num2 <= 0)
			{
				list.Add(i);
				continue;
			}
			list2.Add(i);
			breakPtnIndex -= num2;
		}
		int count2 = list2.Count;
		int num3 = attacker.AttackableCount;
		int attack = attacker.Attack;
		int num4 = attacker.Life;
		for (int j = 0; j < count2; j++)
		{
			if (num3 <= 0)
			{
				break;
			}
			if (num4 <= 0)
			{
				break;
			}
			int index = list2[j];
			AIVirtualCard aIVirtualCard = targetCards[index];
			AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(attacker, aIVirtualCard);
			EvalInstantAttackInformation evalInstantAttackInformation = new EvalInstantAttackInformation(aIVirtualAttackInfo);
			aIVirtualAttackInfo.PseudoSimulateForEvalInstantAttack(field, playPtn, evalInstantAttackInformation);
			if (evalInstantAttackInformation.IsAttackerDestroyWhenAttack)
			{
				continue;
			}
			int num5 = aIVirtualCard.Life;
			int targetLifeBuff = evalInstantAttackInformation.TargetLifeBuff;
			int attackerAttackBuff = evalInstantAttackInformation.AttackerAttackBuff;
			int attackerLifeBuff = evalInstantAttackInformation.AttackerLifeBuff;
			AIBarrierPseudoSimulationInfo targetBarrierInfo = evalInstantAttackInformation.TargetBarrierInfo;
			int damage = aIVirtualCard.SimulateDamageShield(attack + attackerAttackBuff);
			damage = targetBarrierInfo.SimulateDamageAmount(damage, isSpellDamage: false, isSkillDamage: false);
			int attackerTotalDamage = evalInstantAttackInformation.AttackerTotalDamage;
			while (num5 > 0 && num3 > 0)
			{
				num4 += attackerLifeBuff - attackerTotalDamage;
				if (num4 <= 0)
				{
					break;
				}
				num5 += targetLifeBuff;
				num5 -= damage;
				num3--;
				if (num5 <= 0)
				{
					savedEvaluations.TryGetValue(aIVirtualCard.CardIndex, out var value);
					savedBreakBonus.TryGetValue(aIVirtualCard.CardIndex, out var value2);
					savedLeaveBonus.TryGetValue(aIVirtualCard.CardIndex, out var value3);
					num += value - value2 - value3;
					isAllGuardDestroyed = j == count2 - 1;
				}
			}
			attacker.AttackableCount = num3;
			attacker.Life = num4;
			attacker.Attack = attack;
		}
		return num;
	}

	public static AIVirtualCard CreateDummyAttacker(AIVirtualField field, AIVirtualCard owner, int attack, int life, int attackableCount, bool isEvalRush)
	{
		AIVirtualCardParameter baseParameter = new AIVirtualCardParameter(attack, life, attackableCount);
		AIVirtualCard aIVirtualCard = new AIVirtualCard(field, baseParameter, owner, isEvalRush);
		if (aIVirtualCard.IsAlly)
		{
			field.AllyInplayCards.Add(aIVirtualCard);
			field.CardListSet.AddAllyInplayCard(aIVirtualCard);
		}
		else
		{
			field.EnemyInplayCards.Add(aIVirtualCard);
			field.CardListSet.AddEnemyInplayCard(aIVirtualCard);
		}
		return aIVirtualCard;
	}

	public static void RemoveDummyCardFromField(AIVirtualField field)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (IsDummyCard(aIVirtualCard))
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		if (list != null && list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = list[j];
				field.AllyInplayCards.Remove(aIVirtualCard2);
				field.CardListSet.RemoveAllyInplayCard(aIVirtualCard2);
			}
		}
		List<AIVirtualCard> list2 = null;
		for (int k = 0; k < field.EnemyInplayCards.Count; k++)
		{
			AIVirtualCard aIVirtualCard3 = field.EnemyInplayCards[k];
			if (IsDummyCard(aIVirtualCard3))
			{
				list2 = AIParamQuery.AddElementToList(aIVirtualCard3, list2);
			}
		}
		if (list2 != null && list2.Count > 0)
		{
			for (int l = 0; l < list2.Count; l++)
			{
				AIVirtualCard aIVirtualCard4 = list2[l];
				field.EnemyInplayCards.Remove(aIVirtualCard4);
				field.CardListSet.RemoveEnemyInplayCard(aIVirtualCard4);
			}
		}
	}

	private static bool IsDummyCard(AIVirtualCard card)
	{
		return card.CardParameter.CardName == "Dummy";
	}

	private static ulong GetArgumentHash(int damage, int life, int count)
	{
		return (ulong)(0 + (long)damage * 53L + (long)life * 3L + (long)count * 313L);
	}
}

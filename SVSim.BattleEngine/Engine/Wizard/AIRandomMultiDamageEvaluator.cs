using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public static class AIRandomMultiDamageEvaluator
{
	private struct DamageSituationInfo
	{
		public AIVirtualCard TagOwner;

		public AIVirtualField Field;

		public List<int> PlayPtn;

		public bool IsAlly;

		public bool IsSpell;

		public int DamageAmount;

		public int DamageCount;

		public List<AIBarrierPseudoSimulationInfo> BarrierSimList;
	}

	private class RandomDamageSimulationResult
	{
		public float BreakValue;

		public List<int> RestLifeList;

		public List<AIBarrierPseudoSimulationInfo> BarrierSimulationList;
	}

	private class CardValueInformationForEvalRandomMultiDamage
	{
		public float BreakValue;

		public float BattleBonusRate;
	}

	public static void CreateEvalRandomDamageArgList(List<AIScriptTokenBase> src, out List<AIScriptTokenBase> filters, out int damage, out int count)
	{
		filters = src.GetRange(0, src.Count - 2);
		damage = (int)src[src.Count - 2].Value;
		count = (int)src[src.Count - 1].Value;
	}

	private static float CalcSingleTargetEvaluation(AIVirtualCard target, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool ownerIsAlly, int damageAmount, bool isSpellDamage)
	{
		float num = 0f;
		int num2 = target.SimulateDamageAmount(damageAmount, isSkillDamage: true, isSpellDamage);
		int num3 = target.Life - num2;
		if (target.IsLeader)
		{
			return AILeaderLifeEvaluationUtility.Evaluate(num3, target.Life, target.IsAlly, ownerIsAlly);
		}
		if (num3 > 0)
		{
			float num4 = field.StyleQuery.GetUnitRate(field, target, playPtn) * target.EvaluateAllBattleBonusRate(playPtn, useOthersTag: true, useIgnoreInBattle: false, situation);
			num = (float)num2 * num4;
		}
		else
		{
			num = target.EvaluateValueOnField(playPtn, situation, useStyle: true) - target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false, situation);
		}
		return num * (target.IsAlly ? (-1f) : 1f);
	}

	public static float EvaluateRandomDamageAverage(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> filters, int damageAmount, int damageCount)
	{
		if (damageCount < 0)
		{
			AIConsoleUtility.LogError("EvaluateRandomMultiDamageAverage() error!! damageCount < 0");
			return 0f;
		}
		if (damageCount == 1)
		{
			return EvaluateSingleTargetAverage(tagOwner, field, playPtn, situation, filters, damageAmount);
		}
		return EvaluateMultiTargetsAverage(tagOwner, playPtn, field, filters, situation, damageAmount, damageCount);
	}

	private static float EvaluateSingleTargetAverage(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> filters, int damageAmount)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		list.RemoveAll((AIVirtualCard c) => !c.IsUnit && !c.IsLeader);
		if (list.Count <= 0)
		{
			return 0f;
		}
		bool isSpellDamage = tagOwner.IsSpell || tagOwner.IsAccelerated(field, playPtn, situation);
		float num = 0f;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			float num3 = CalcSingleTargetEvaluation(list[num2], field, playPtn, situation, tagOwner.IsAlly, damageAmount, isSpellDamage);
			num += num3;
		}
		return num / (float)list.Count;
	}

	private static float EvaluateMultiTargetsAverage(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, List<AIScriptTokenBase> filters, AISituationInfo situation, int damageAmount, int damageCount)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null);
		list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		if (!list.IsNotNullOrEmpty())
		{
			return 0f;
		}
		int count = list.Count;
		SeparateTargetCardBySide(list, out var allyInplays, out var enemyInplays, out var allyLeader, out var enemyLeader);
		int num = damageCount / count;
		int num2 = ((allyLeader != null) ? num : 0);
		int num3 = ((enemyLeader != null) ? num : 0);
		int num4 = ((allyInplays != null && allyInplays.Count > 0) ? ((damageCount - num2 - num3) / 2) : 0);
		int num5 = ((enemyInplays != null && enemyInplays.Count > 0) ? ((damageCount - num2 - num3) / 2) : 0);
		int num6 = damageCount - (num2 + num4 + num3 + num5);
		if (num6 > 0)
		{
			if (enemyLeader != null)
			{
				num3 += num6;
			}
			else if (enemyInplays.IsNotNullOrEmpty())
			{
				num5 += num6;
			}
			else if (allyInplays.IsNotNullOrEmpty())
			{
				num4 += num6;
			}
			else if (allyLeader != null)
			{
				num2 += num6;
			}
		}
		num6 = 0;
		DamageSituationInfo damageSituation = new DamageSituationInfo
		{
			DamageAmount = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damageAmount),
			TagOwner = tagOwner,
			Field = field,
			PlayPtn = playPtn,
			IsSpell = (tagOwner.IsSpell || tagOwner.IsAccelerated(field, playPtn))
		};
		Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> cardValueList = CreateCardValueInformationDictionary(allyInplays, field, playPtn, situation);
		Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> cardValueList2 = CreateCardValueInformationDictionary(enemyInplays, field, playPtn, situation);
		List<AIBarrierPseudoSimulationInfo> barrierSimList = new List<AIBarrierPseudoSimulationInfo>(field.CardListSet.BothClassAndInplayCards.Select((AIVirtualCard c) => new AIBarrierPseudoSimulationInfo(c)));
		List<AIBarrierPseudoSimulationInfo> barrierSimList2 = new List<AIBarrierPseudoSimulationInfo>(field.CardListSet.BothClassAndInplayCards.Select((AIVirtualCard c) => new AIBarrierPseudoSimulationInfo(c)));
		damageSituation.IsAlly = false;
		float num7 = 0f;
		RandomDamageSimulationResult randomDamageSimulationResult = new RandomDamageSimulationResult();
		RandomDamageSimulationResult randomDamageSimulationResult2 = new RandomDamageSimulationResult();
		if (enemyInplays.IsNotNullOrEmpty())
		{
			damageSituation.DamageCount = num5;
			damageSituation.BarrierSimList = barrierSimList2;
			List<int> targetLifeList = GetTargetLifeList(enemyInplays);
			randomDamageSimulationResult = EvalWorstInplayRandomDamage(enemyInplays, targetLifeList, cardValueList2, damageSituation, situation, ref num6);
			if (randomDamageSimulationResult.RestLifeList.Sum() == 0)
			{
				randomDamageSimulationResult2.BreakValue = randomDamageSimulationResult.BreakValue;
			}
			else
			{
				randomDamageSimulationResult2 = EvalBestInplayRandomDamage(enemyInplays, targetLifeList, cardValueList2, damageSituation, situation, ref num6);
			}
		}
		float num8 = 0f;
		int restLife = enemyLeader?.Life ?? 0;
		if (enemyLeader != null)
		{
			damageSituation.DamageCount = num3 + num6;
			damageSituation.BarrierSimList = barrierSimList;
			num8 = EvalRandomMultiDamageToLeader(enemyLeader, damageSituation, tagOwner.IsAlly, ref restLife, out num6);
		}
		damageSituation.IsAlly = true;
		float num9 = 0f;
		if (allyInplays.IsNotNullOrEmpty())
		{
			damageSituation.DamageCount = num4 + num6;
			damageSituation.BarrierSimList = barrierSimList2;
			List<int> targetLifeList2 = GetTargetLifeList(allyInplays);
			RandomDamageSimulationResult randomDamageSimulationResult3 = EvalWorstInplayRandomDamage(allyInplays, targetLifeList2, cardValueList, damageSituation, situation, ref num6);
			float breakValue = randomDamageSimulationResult3.BreakValue;
			num9 = ((randomDamageSimulationResult3.RestLifeList.Sum() != 0) ? ((EvalBestInplayRandomDamage(allyInplays, targetLifeList2, cardValueList, damageSituation, situation, ref num6).BreakValue + breakValue) / 2f) : breakValue);
		}
		float num10 = 0f;
		if (allyLeader != null)
		{
			int restLife2 = allyLeader.Life;
			damageSituation.DamageCount = num2 + num6;
			damageSituation.BarrierSimList = barrierSimList;
			num10 = EvalRandomMultiDamageToLeader(allyLeader, damageSituation, tagOwner.IsAlly, ref restLife2, out num6);
		}
		if (num6 > 0)
		{
			damageSituation.IsAlly = false;
			if (enemyInplays.IsNotNullOrEmpty())
			{
				damageSituation.DamageCount = num6;
				damageSituation.BarrierSimList = randomDamageSimulationResult.BarrierSimulationList;
				RandomDamageSimulationResult randomDamageSimulationResult4 = EvalWorstInplayRandomDamage(enemyInplays, randomDamageSimulationResult.RestLifeList, cardValueList2, damageSituation, situation, ref num6);
				randomDamageSimulationResult.BreakValue += randomDamageSimulationResult4.BreakValue;
				if (randomDamageSimulationResult4.RestLifeList.Sum() == 0)
				{
					randomDamageSimulationResult2.BreakValue = randomDamageSimulationResult.BreakValue;
				}
				else
				{
					damageSituation.BarrierSimList = randomDamageSimulationResult2.BarrierSimulationList;
					randomDamageSimulationResult2.BreakValue += EvalBestInplayRandomDamage(enemyInplays, randomDamageSimulationResult2.RestLifeList, cardValueList2, damageSituation, situation, ref num6).BreakValue;
				}
			}
			if (num6 > 0 && enemyLeader != null && restLife > 0)
			{
				damageSituation.DamageCount = num6;
				damageSituation.BarrierSimList = barrierSimList;
				num8 += EvalRandomMultiDamageToLeader(enemyLeader, damageSituation, tagOwner.IsAlly, ref restLife, out num6);
			}
		}
		num7 = (randomDamageSimulationResult2.BreakValue + randomDamageSimulationResult.BreakValue) / 2f;
		return num8 + num7 + num10 + num9;
	}

	public static float EvaluateRandomDamageMax(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, List<AIScriptTokenBase> filters, AISituationInfo situation, int damageAmount, int damageCount)
	{
		if (damageCount < 0)
		{
			AIConsoleUtility.LogError("EvaluateRandomMultiDamageAverage() error!! damageCount < 0");
			return 0f;
		}
		if (damageCount == 1)
		{
			return EvaluateSingleTargetMax(tagOwner, field, playPtn, situation, filters, damageAmount);
		}
		return EvaluateMultiTargetsMax(tagOwner, playPtn, field, filters, situation, damageAmount, damageCount);
	}

	private static float EvaluateSingleTargetMax(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> filters, int damageAmount)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		list.RemoveAll((AIVirtualCard c) => !c.IsUnit && !c.IsLeader);
		if (list.Count <= 0)
		{
			return 0f;
		}
		bool flag = false;
		bool isSpellDamage = tagOwner.IsSpell || tagOwner.IsAccelerated(field, playPtn, situation);
		float num = float.MinValue;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			float num3 = CalcSingleTargetEvaluation(list[num2], field, playPtn, situation, tagOwner.IsAlly, damageAmount, isSpellDamage);
			if (num3 > num)
			{
				num = num3;
			}
			flag = true;
		}
		if (!flag)
		{
			num = 0f;
		}
		return num;
	}

	private static float EvaluateMultiTargetsMax(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, List<AIScriptTokenBase> filters, AISituationInfo situation, int damageAmount, int damageCount)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null);
		list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		if (!list.IsNotNullOrEmpty())
		{
			return 0f;
		}
		_ = list.Count;
		SeparateTargetCardBySide(list, out var allyInplays, out var enemyInplays, out var allyLeader, out var enemyLeader);
		DamageSituationInfo damageSituation = new DamageSituationInfo
		{
			DamageAmount = field.DamageModifierCollection.CalcModifiedDamage(field, playPtn, situation, tagOwner, damageAmount),
			TagOwner = tagOwner,
			Field = field,
			PlayPtn = playPtn,
			IsSpell = (tagOwner.IsSpell || tagOwner.IsAccelerated(field, playPtn))
		};
		Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> cardValueList = CreateCardValueInformationDictionary(allyInplays, field, playPtn, situation);
		Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> cardValueList2 = CreateCardValueInformationDictionary(enemyInplays, field, playPtn, situation);
		List<AIBarrierPseudoSimulationInfo> barrierSimList = new List<AIBarrierPseudoSimulationInfo>(field.CardListSet.BothClassAndInplayCards.Select((AIVirtualCard c) => new AIBarrierPseudoSimulationInfo(c)));
		List<AIBarrierPseudoSimulationInfo> barrierSimList2 = new List<AIBarrierPseudoSimulationInfo>(field.CardListSet.BothClassAndInplayCards.Select((AIVirtualCard c) => new AIBarrierPseudoSimulationInfo(c)));
		damageSituation.IsAlly = false;
		float num = 0f;
		int restDamageCount = damageCount;
		if (enemyLeader != null)
		{
			int restLife = enemyLeader.Life;
			damageSituation.DamageCount = restDamageCount;
			damageSituation.BarrierSimList = barrierSimList;
			num = EvalRandomMultiDamageToLeader(enemyLeader, damageSituation, tagOwner.IsAlly, ref restLife, out restDamageCount);
			if (restLife <= 0)
			{
				return num;
			}
		}
		float num2 = 0f;
		int restDamageCount2 = damageCount;
		if (enemyInplays != null && enemyInplays.Count > 0)
		{
			damageSituation.DamageCount = restDamageCount2;
			damageSituation.BarrierSimList = barrierSimList2;
			List<int> targetLifeList = GetTargetLifeList(enemyInplays);
			RandomDamageSimulationResult randomDamageSimulationResult = EvalBestInplayRandomDamage(enemyInplays, targetLifeList, cardValueList2, damageSituation, situation, ref restDamageCount2);
			num2 = randomDamageSimulationResult.BreakValue;
			if (restDamageCount2 > 0 && enemyLeader != null)
			{
				int restLife2 = enemyLeader.Life;
				damageSituation.DamageCount = restDamageCount2;
				damageSituation.BarrierSimList = randomDamageSimulationResult.BarrierSimulationList;
				num2 += EvalRandomMultiDamageToLeader(enemyLeader, damageSituation, tagOwner.IsAlly, ref restLife2, out restDamageCount);
				if (restLife2 <= 0)
				{
					return num2;
				}
			}
		}
		float num3 = 0f;
		if (enemyLeader != null && enemyInplays.IsNotNullOrEmpty())
		{
			num3 = Mathf.Max(num2, num);
		}
		else if (enemyLeader != null && !enemyInplays.IsNotNullOrEmpty())
		{
			num3 = num;
		}
		else if (enemyLeader == null && enemyInplays.IsNotNullOrEmpty())
		{
			num3 = num2;
		}
		if (restDamageCount2 <= 0)
		{
			return num3;
		}
		damageSituation.IsAlly = true;
		int restDamageCount3 = restDamageCount2;
		float num4 = 0f;
		if (allyLeader != null)
		{
			int restLife3 = allyLeader.Life;
			damageSituation.DamageCount = restDamageCount3;
			damageSituation.BarrierSimList = barrierSimList;
			num4 = EvalRandomMultiDamageToLeader(allyLeader, damageSituation, tagOwner.IsAlly, ref restLife3, out restDamageCount3);
		}
		float num5 = 0f;
		int restDamageCount4 = restDamageCount2;
		if (allyInplays != null && allyInplays.Count > 0)
		{
			damageSituation.DamageCount = restDamageCount4;
			damageSituation.BarrierSimList = barrierSimList2;
			List<int> targetLifeList2 = GetTargetLifeList(allyInplays);
			RandomDamageSimulationResult randomDamageSimulationResult2 = EvalWorstInplayRandomDamage(allyInplays, targetLifeList2, cardValueList, damageSituation, situation, ref restDamageCount4);
			num5 = randomDamageSimulationResult2.BreakValue;
			if (restDamageCount4 > 0 && allyLeader != null)
			{
				int restLife4 = allyLeader.Life;
				damageSituation.DamageCount = restDamageCount4;
				damageSituation.BarrierSimList = randomDamageSimulationResult2.BarrierSimulationList;
				num4 += EvalRandomMultiDamageToLeader(allyLeader, damageSituation, tagOwner.IsAlly, ref restLife4, out restDamageCount4);
			}
		}
		float num6 = 0f;
		if (allyLeader != null && allyInplays.IsNotNullOrEmpty())
		{
			num6 = Mathf.Max(num5, num4);
		}
		else if (allyLeader != null && !allyInplays.IsNotNullOrEmpty())
		{
			num6 = num4;
		}
		else if (allyLeader == null && allyInplays.IsNotNullOrEmpty())
		{
			num6 = num5;
		}
		return num3 + num6;
	}

	private static RandomDamageSimulationResult EvalWorstInplayRandomDamage(List<AIVirtualCard> targetCards, List<int> targetLifeList, Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> cardValueList, DamageSituationInfo damageSituation, AISituationInfo situation, ref int restDamageCount)
	{
		restDamageCount = damageSituation.DamageCount;
		RandomDamageSimulationResult simResult = new RandomDamageSimulationResult
		{
			BreakValue = 0f,
			RestLifeList = new List<int>(targetLifeList),
			BarrierSimulationList = new List<AIBarrierPseudoSimulationInfo>(damageSituation.BarrierSimList.Select((AIBarrierPseudoSimulationInfo info) => new AIBarrierPseudoSimulationInfo(info)))
		};
		List<AIVirtualCard> list = targetCards.OrderBy((AIVirtualCard c) => cardValueList[c].BreakValue).ToList();
		List<int> list2 = list.Select((AIVirtualCard sortedCard) => targetCards.FindIndex((AIVirtualCard originalCard) => originalCard.IsSameCard(sortedCard))).ToList();
		List<int> list3 = list2.Select((int index2) => targetLifeList[index2]).ToList();
		List<int> list4 = new List<int>(list.Select((AIVirtualCard c) => 0));
		List<AIBarrierPseudoSimulationInfo> list5 = list.Select((AIVirtualCard c) => simResult.BarrierSimulationList.Find((AIBarrierPseudoSimulationInfo info) => info.Owner.IsSameCard(c))).ToList();
		bool isSpell = damageSituation.IsSpell;
		for (int num = 0; num < damageSituation.DamageCount; num++)
		{
			int num2 = int.MaxValue;
			int num3 = -1;
			int num4 = 0;
			if (list3.Count((int life) => life > 0) == 0)
			{
				simResult.BreakValue *= (damageSituation.IsAlly ? (-1f) : 1f);
				return simResult;
			}
			int num5 = -1;
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				if (list3[num6] > 0)
				{
					if (num5 < 0)
					{
						num5 = num6;
					}
					int damage = list[num6].SimulateDamageShield(damageSituation.DamageAmount, isSkillDamage: true, isSpell);
					damage = list5[num6].SimulateDamageAmount(damage, isSpell);
					if (damage > 0 && list3[num6] <= damage)
					{
						num4++;
					}
					else if (damage < num2)
					{
						num2 = damage;
						num3 = num6;
					}
				}
			}
			if (num4 == list3.Count((int life) => life > 0))
			{
				num3 = num5;
				num2 = list[num3].SimulateDamageShield(damageSituation.DamageAmount, isSkillDamage: true, isSpell);
				num2 = list5[num3].SimulateDamageAmount(num2, isSpell);
			}
			if (0 <= num3)
			{
				int index = list2[num3];
				AIVirtualCard key = list[num3];
				list3[num3] = Mathf.Max(0, list3[num3] - num2);
				simResult.RestLifeList[index] = list3[num3];
				list4[num3]++;
				AIBarrierPseudoSimulationInfo targetBarrierInfo = list5[num3];
				bool flag = list3[num3] <= 0;
				PseudoRemoveBarrierWhenTargetGetDamaged(targetBarrierInfo, simResult.BarrierSimulationList, flag);
				if (flag)
				{
					simResult.BreakValue += cardValueList[key].BreakValue;
				}
			}
			restDamageCount--;
		}
		for (int num7 = 0; num7 < targetCards.Count; num7++)
		{
			AIVirtualCard aIVirtualCard = targetCards[num7];
			int num8 = list4[num7];
			if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.DamagedBuff) && num8 > 0 && simResult.RestLifeList[num7] > 0)
			{
				DamagedTagCollection damagedTags = aIVirtualCard.TagCollectionContainer.DamagedTags;
				for (int num9 = 0; num9 < num8; num9++)
				{
					int atkBuff = 0;
					int lifeBuff = 0;
					damagedTags.GetDamagedBuffValue(aIVirtualCard, aIVirtualCard.SelfField, damageSituation.PlayPtn, situation, out atkBuff, out lifeBuff);
					simResult.RestLifeList[num7] += lifeBuff;
					if (simResult.RestLifeList[num7] <= 0)
					{
						simResult.BreakValue += cardValueList[aIVirtualCard].BreakValue;
						break;
					}
				}
			}
			if (simResult.RestLifeList[num7] > 0)
			{
				simResult.BreakValue += aIVirtualCard.Life - simResult.RestLifeList[num7];
			}
		}
		simResult.BreakValue *= (damageSituation.IsAlly ? (-1f) : 1f);
		return simResult;
	}

	private static RandomDamageSimulationResult EvalBestInplayRandomDamage(List<AIVirtualCard> targetCards, List<int> targetLifeList, Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> cardValueList, DamageSituationInfo damageSituation, AISituationInfo situation, ref int restDamageCount)
	{
		RandomDamageSimulationResult randomDamageSimulationResult = new RandomDamageSimulationResult();
		randomDamageSimulationResult.BreakValue = float.MinValue;
		int num = (int)Mathf.Pow(2f, targetCards.Count);
		for (int i = 0; i < num; i++)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			int num2 = i;
			for (int j = 0; j < targetCards.Count; j++)
			{
				int num3 = (int)Mathf.Pow(2f, targetCards.Count - j - 1);
				if (num2 / num3 <= 0)
				{
					list.Add(j);
					continue;
				}
				list2.Add(j);
				num2 -= num3;
			}
			float num4 = 0f;
			int damageCount = damageSituation.DamageCount;
			List<AIBarrierPseudoSimulationInfo> list3 = new List<AIBarrierPseudoSimulationInfo>(damageSituation.BarrierSimList.Select((AIBarrierPseudoSimulationInfo info) => new AIBarrierPseudoSimulationInfo(info)));
			List<int> list4 = new List<int>(targetLifeList);
			if (0 < list2.Count)
			{
				num4 += EvalRandomMultiDamageBreakValue(damageSituation.DamageAmount, ref damageCount, list2, damageSituation.IsSpell, targetCards, damageSituation.PlayPtn, situation, list4, cardValueList, list3);
			}
			if (0 < damageCount && 0 < list.Count)
			{
				num4 += EvalRandomMultiDamageBreakValue(damageSituation.DamageAmount, ref damageCount, list, damageSituation.IsSpell, targetCards, damageSituation.PlayPtn, situation, list4, cardValueList, list3);
			}
			num4 *= (damageSituation.IsAlly ? (-1f) : 1f);
			if (randomDamageSimulationResult.BreakValue < num4)
			{
				randomDamageSimulationResult.BreakValue = num4;
				randomDamageSimulationResult.BarrierSimulationList = list3;
				randomDamageSimulationResult.RestLifeList = list4;
				restDamageCount = damageCount;
			}
		}
		if (randomDamageSimulationResult.RestLifeList == null)
		{
			randomDamageSimulationResult.RestLifeList = new List<int>(targetLifeList);
		}
		if (randomDamageSimulationResult.BarrierSimulationList == null)
		{
			randomDamageSimulationResult.BarrierSimulationList = new List<AIBarrierPseudoSimulationInfo>(damageSituation.BarrierSimList.Select((AIBarrierPseudoSimulationInfo info) => new AIBarrierPseudoSimulationInfo(info)));
		}
		return randomDamageSimulationResult;
	}

	private static float EvalRandomMultiDamageBreakValue(int damageAmount, ref int damageCount, List<int> targetIdxList, bool isSpell, List<AIVirtualCard> candidates, List<int> playPtn, AISituationInfo situation, List<int> lifeList, Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> cardValueList, List<AIBarrierPseudoSimulationInfo> removeBarrierSimList)
	{
		if (lifeList.Sum() == 0)
		{
			return 0f;
		}
		float num = 0f;
		int num2 = damageCount;
		float num3 = 0f;
		if (targetIdxList.Count > 0)
		{
			int num4 = -1;
			for (int i = 0; i < targetIdxList.Count; i++)
			{
				if (lifeList[targetIdxList[i]] > 0)
				{
					num4 = i;
					break;
				}
			}
			if (num4 < 0 || num4 >= targetIdxList.Count)
			{
				return 0f;
			}
			int index = targetIdxList[num4];
			int num5 = 0;
			int num6 = lifeList[index];
			AIVirtualCard target = candidates[index];
			AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = removeBarrierSimList.Find((AIBarrierPseudoSimulationInfo info) => info.Owner.IsSameCard(target));
			float battleBonusRate = cardValueList[target].BattleBonusRate;
			while (true)
			{
				if (num2 <= 0)
				{
					damageCount = num2;
					return num + num3;
				}
				int num7 = target.SimulateDamageShield(damageAmount, isSkillDamage: true, isSpell);
				if (num7 > 0)
				{
					num7 = aIBarrierPseudoSimulationInfo.SimulateDamageAmount(damageAmount, isSpell);
				}
				num5++;
				num3 += (float)num7 * battleBonusRate;
				lifeList[index] -= num7;
				aIBarrierPseudoSimulationInfo.DepriveBarrier(AIBarrierStopTiming.AfterDamage);
				num2--;
				if (target.TagCollectionContainer.HasTag(AIPlayTagType.DamagedBuff) && num5 >= num6)
				{
					DamagedTagCollection damagedTags = target.TagCollectionContainer.DamagedTags;
					for (int num8 = 0; num8 < num5; num8++)
					{
						int atkBuff = 0;
						int lifeBuff = 0;
						damagedTags.GetDamagedBuffValue(target, target.SelfField, playPtn, situation, out atkBuff, out lifeBuff);
						lifeList[index] += lifeBuff;
						if (lifeList[index] <= 0)
						{
							break;
						}
					}
				}
				if (lifeList[index] <= 0)
				{
					lifeList[index] = 0;
					num3 = 0f;
					num5 = 0;
					num += cardValueList[target].BreakValue;
					num4 = targetIdxList.FindIndex((int idx) => lifeList[idx] > 0);
					if (num4 < 0 || num4 >= targetIdxList.Count)
					{
						break;
					}
					index = targetIdxList[num4];
					num6 = lifeList[index];
					target = candidates[index];
					battleBonusRate = cardValueList[target].BattleBonusRate;
					aIBarrierPseudoSimulationInfo = removeBarrierSimList.Find((AIBarrierPseudoSimulationInfo info) => info.Owner.IsSameCard(target));
				}
			}
		}
		damageCount = num2;
		return num + num3;
	}

	private static float EvalRandomMultiDamageToLeader(AIVirtualCard leader, DamageSituationInfo damageSituation, bool isAllyOwner, ref int restLife, out int restDamageCount)
	{
		restDamageCount = damageSituation.DamageCount;
		int defaultLife = restLife;
		AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = damageSituation.BarrierSimList.Find((AIBarrierPseudoSimulationInfo c) => c.Owner.IsSameCard(leader));
		bool isSpell = damageSituation.IsSpell;
		for (int num = 0; num < damageSituation.DamageCount; num++)
		{
			if (restLife <= 0)
			{
				restLife = 0;
				break;
			}
			int num2 = aIBarrierPseudoSimulationInfo.SimulateDamageAmount(leader.SimulateDamageShield(damageSituation.DamageAmount, isSkillDamage: true, isSpell), isSpell);
			restLife -= num2;
			aIBarrierPseudoSimulationInfo.DepriveBarrier(AIBarrierStopTiming.AfterDamage);
			restDamageCount--;
		}
		return AILeaderLifeEvaluationUtility.Evaluate(restLife, defaultLife, leader.IsAlly, isAllyOwner);
	}

	private static void SeparateTargetCardBySide(List<AIVirtualCard> allTargets, out List<AIVirtualCard> allyInplays, out List<AIVirtualCard> enemyInplays, out AIVirtualCard allyLeader, out AIVirtualCard enemyLeader)
	{
		allyInplays = null;
		enemyInplays = null;
		allyLeader = null;
		enemyLeader = null;
		if (allTargets == null)
		{
			return;
		}
		for (int i = 0; i < allTargets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = allTargets[i];
			if (aIVirtualCard.IsAlly)
			{
				if (aIVirtualCard.IsLeader)
				{
					allyLeader = aIVirtualCard;
				}
				else
				{
					allyInplays = AIParamQuery.AddElementToList(aIVirtualCard, allyInplays);
				}
			}
			else if (aIVirtualCard.IsLeader)
			{
				enemyLeader = aIVirtualCard;
			}
			else
			{
				enemyInplays = AIParamQuery.AddElementToList(aIVirtualCard, enemyInplays);
			}
		}
	}

	private static List<int> GetTargetLifeList(List<AIVirtualCard> targets)
	{
		if (targets == null)
		{
			return null;
		}
		List<int> list = null;
		for (int i = 0; i < targets.Count; i++)
		{
			list = AIParamQuery.AddElementToList(targets[i].Life, list);
		}
		return list;
	}

	private static Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> CreateCardValueInformationDictionary(List<AIVirtualCard> cardList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (cardList == null)
		{
			return null;
		}
		Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage> dictionary = new Dictionary<AIVirtualCard, CardValueInformationForEvalRandomMultiDamage>();
		for (int i = 0; i < cardList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = cardList[i];
			float breakValue = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true) - (aIVirtualCard.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) + aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false));
			float battleBonusRate = field.StyleQuery.GetUnitRate(field, aIVirtualCard, playPtn) * aIVirtualCard.EvaluateAllBattleBonusRate(playPtn, useOthersTag: true, useIgnoreInBattle: false, situation);
			CardValueInformationForEvalRandomMultiDamage value = new CardValueInformationForEvalRandomMultiDamage
			{
				BreakValue = breakValue,
				BattleBonusRate = battleBonusRate
			};
			dictionary.Add(aIVirtualCard, value);
		}
		return dictionary;
	}

	private static void PseudoRemoveBarrierWhenTargetGetDamaged(AIBarrierPseudoSimulationInfo targetBarrierInfo, List<AIBarrierPseudoSimulationInfo> otherDamageCandidateBarrierInfoList, bool isTargetLeaveFromField)
	{
		targetBarrierInfo.DepriveBarrier(AIBarrierStopTiming.AfterDamage);
		if (isTargetLeaveFromField)
		{
			AIVirtualCard owner = targetBarrierInfo.Owner;
			owner.SelfField.TagPreprocessContainer.LeaveStopInfoContainer?.PseudoSimulateForEvalRandomMultiDamage(otherDamageCandidateBarrierInfoList, owner);
		}
	}
}

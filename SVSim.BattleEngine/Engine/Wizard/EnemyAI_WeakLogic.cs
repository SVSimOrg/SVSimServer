using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public class EnemyAI_WeakLogic
{
	private EnemyAI _ai;

	public EnemyAI_WeakLogic(EnemyAI ai)
	{
		_ai = ai;
	}

	public bool BattleAI_AttackWeak()
	{
		float[] dstValues = new float[_ai.CurrentVirtualField.AllyInplayCards.Count];
		float[] dstValues2 = new float[_ai.CurrentVirtualField.EnemyInplayCards.Count];
		bool dstBGuard = false;
		if (_EvaluateAllyFieldUnits(ref dstValues, bSelf: true) < 0f)
		{
			return false;
		}
		int dstMaxIndex = 0;
		_EvaluateEnemyFieldUnits(ref dstValues2, ref dstBGuard, ref dstMaxIndex, bSelf: true);
		if (dstMaxIndex < 0)
		{
			return false;
		}
		int dstAllyIndex = 0;
		int dstEnemyIndex = 0;
		float dstAdvantage = -1f;
		int dstEnemyRemainLife = 0;
		_CalcMostValuableUnitBattle_OneVsOne(dstValues, dstValues2, dstBGuard, ref dstAllyIndex, ref dstEnemyIndex, ref dstAdvantage, ref dstEnemyRemainLife);
		if (dstAdvantage >= 0f)
		{
			_ai.OprAttack(new AIVirtualAttackInfo(_ai.CurrentVirtualField.AllyInplayCards[dstAllyIndex], _ai.CurrentVirtualField.EnemyInplayCards[dstEnemyIndex]));
			return true;
		}
		if (_ai.AIStableRandom(100) >= 50)
		{
			int num = _CalcMinClassAttackableAlly(dstValues);
			if (num >= 0)
			{
				_ai.OprAttack(new AIVirtualAttackInfo(_ai.CurrentVirtualField.AllyInplayCards[num], _ai.CurrentVirtualField.EnemyInplayCards[dstMaxIndex]));
				return true;
			}
		}
		return false;
	}

	public bool BattleAI_EvoWeak()
	{
		if (!_ai.IsAbleEvo())
		{
			return false;
		}
		float num = _ai.CalcFieldAdvantage();
		int num2 = ((num <= -8f) ? 100 : 0);
		if (_ai.AIStableRandom(100) >= num2)
		{
			return false;
		}
		int num3 = _ai.CurrentVirtualField.EnemyInplayCards.Count();
		float[] dstValues = new float[num3];
		bool dstBGuard = false;
		int dstMaxIndex = 0;
		_EvaluateEnemyFieldUnits(ref dstValues, ref dstBGuard, ref dstMaxIndex, bSelf: true);
		AIVirtualField currentVirtualField = _ai.CurrentVirtualField;
		AIVirtualCard aIVirtualCard = null;
		float num4 = 0f;
		for (int i = 0; i < currentVirtualField.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = currentVirtualField.AllyInplayCards[i];
			BattleCardBase baseCard = aIVirtualCard2.BaseCard;
			if (!aIVirtualCard2.IsAbleEvolution() || !aIVirtualCard2.IsFirstTurn || (aIVirtualCard2.IsQuick && !aIVirtualCard2.IsAttackable(EnemyAI.EmptyPlayPtn)) || aIVirtualCard2.IsNoNormalEvo(currentVirtualField))
			{
				continue;
			}
			float num5 = baseCard.BaseParameter.EvoAtk + baseCard.BaseParameter.EvoLife;
			num5 *= aIVirtualCard2.GetBattleBonusRate(_ai.BestPlayPtn);
			num5 += aIVirtualCard2.GetFieldBonus(_ai.BestPlayPtn);
			float num6 = -1f;
			for (int j = 0; j < num3; j++)
			{
				AIVirtualCard aIVirtualCard3 = _ai.CurrentVirtualField.EnemyInplayCards[j];
				float num7 = -1f;
				float num8 = dstValues[j];
				if (num8 <= 0f)
				{
					continue;
				}
				if (baseCard.BaseParameter.EvoAtk >= aIVirtualCard3.Life)
				{
					num7 = num8;
					if (baseCard.BaseParameter.EvoLife <= aIVirtualCard3.Attack && num5 > 0f)
					{
						num7 -= num5;
					}
				}
				if (num7 > num6)
				{
					num6 = num7;
				}
			}
			if (num >= 0f)
			{
				num6 -= _ai.GetEvoPenalty();
			}
			if (num6 >= num4)
			{
				aIVirtualCard = aIVirtualCard2;
				num4 = num6;
			}
		}
		if (aIVirtualCard != null)
		{
			_ai.OprEvolution(aIVirtualCard);
			return true;
		}
		return false;
	}

	private void _CalcMostValuableUnitBattle_OneVsOne(float[] allyValues, float[] enemyValues, bool bGuard, ref int dstAllyIndex, ref int dstEnemyIndex, ref float dstAdvantage, ref int dstEnemyRemainLife)
	{
		int num = allyValues.Length;
		int num2 = enemyValues.Length;
		int num3 = 0;
		int num4 = 0;
		int num5 = int.MinValue;
		float num6 = float.MinValue;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				int dstEnemyRemainLife2 = 0;
				float num7 = _Atk_EvaluateAtk_OneVsOne_Weak(i, j, bGuard, allyValues[i], enemyValues[j], ref dstEnemyRemainLife2);
				if (num7 > num6)
				{
					num3 = i;
					num4 = j;
					num5 = dstEnemyRemainLife2;
					num6 = num7;
				}
			}
		}
		dstAllyIndex = num3;
		dstEnemyIndex = num4;
		dstAdvantage = num6;
		dstEnemyRemainLife = num5;
	}

	private float _Atk_EvaluateAtk_OneVsOne_Weak(int allyIndex, int enemyIndex, bool bGuard, float allyValue, float enemyValue, ref int dstEnemyRemainLife)
	{
		if (enemyValue < 0f || allyValue < 0f)
		{
			return -1f;
		}
		BattleCardBase baseCard = _ai.CurrentVirtualField.AllyInplayCards[allyIndex].BaseCard;
		BattleCardBase baseCard2 = _ai.CurrentVirtualField.EnemyInplayCards[enemyIndex].BaseCard;
		dstEnemyRemainLife = baseCard2.Life;
		float num = 0f;
		if (baseCard.SkillApplyInformation.IsSkillCantAtkUnit)
		{
			return -1f;
		}
		if (bGuard && !baseCard2.SkillApplyInformation.IsGuard && !baseCard.SkillApplyInformation.IsIgnoreGuard)
		{
			return -1f;
		}
		if (baseCard2.SkillApplyInformation.IsKiller || baseCard.Life <= baseCard.CalculateFinalDamageAmount(baseCard2.DamageCalculationAtkTypeBeAttacked.Damage))
		{
			num -= allyValue;
		}
		int num2 = baseCard2.CalculateFinalDamageAmount(baseCard.DamageCalculationAtkTypeAttack.Damage);
		if (baseCard.SkillApplyInformation.IsKiller || baseCard2.Life <= num2)
		{
			num += enemyValue;
			dstEnemyRemainLife = 0;
		}
		else
		{
			dstEnemyRemainLife = baseCard2.Life - num2;
		}
		return num;
	}

	private float _EvaluateAllyFieldUnits(ref float[] dstValues, bool bSelf)
	{
		int num = dstValues.Length;
		List<AIVirtualCard> list = ((!bSelf) ? _ai.CurrentVirtualField.EnemyInplayCards : _ai.CurrentVirtualField.AllyInplayCards);
		float num2 = float.MinValue;
		for (int i = 0; i < list.Count && i < num; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			dstValues[i] = aIVirtualCard.EvaluateValueOnField(_ai.BestPlayPtn, null, useStyle: true);
			if (bSelf)
			{
				if (!aIVirtualCard.IsAttackable(EnemyAI.EmptyPlayPtn))
				{
					dstValues[i] = -1f;
				}
			}
			else if (!aIVirtualCard.IsUnit)
			{
				dstValues[i] = -1f;
			}
			if (dstValues[i] > num2)
			{
				num2 = dstValues[i];
			}
		}
		return num2;
	}

	private int _CalcMinClassAttackableAlly(float[] allyValues)
	{
		float num = float.MaxValue;
		int result = -1;
		int num2 = allyValues.Length;
		for (int i = 0; i < num2; i++)
		{
			if (!(allyValues[i] < 0f))
			{
				AIVirtualCard aIVirtualCard = _ai.CurrentVirtualField.AllyInplayCards[i];
				if (!aIVirtualCard.IsCantAttackClass() && aIVirtualCard.Attack > 0 && num > allyValues[i])
				{
					num = allyValues[i];
					result = i;
				}
			}
		}
		return result;
	}

	private float _EvaluateEnemyFieldUnits(ref float[] dstValues, ref bool dstBGuard, ref int dstMaxIndex, bool bSelf, bool bSimulateAdv = true)
	{
		int length = dstValues.GetLength(0);
		float[] dstEnemyUnitAdvantages = new float[length];
		for (int i = 0; i < length; i++)
		{
			dstEnemyUnitAdvantages[i] = float.MinValue;
		}
		List<AIVirtualCard> list;
		if (bSelf)
		{
			list = _ai.CurrentVirtualField.EnemyInplayCards;
			if (bSimulateAdv)
			{
				float dstEnemyMaxAdvantage = 0f;
				EvaluateEnemyBattleAdvantage(ref dstEnemyUnitAdvantages, ref dstEnemyMaxAdvantage);
			}
		}
		else
		{
			list = _ai.CurrentVirtualField.AllyInplayCards;
		}
		float num = float.MinValue;
		float num2 = float.MinValue;
		bool flag = false;
		int num3 = -1;
		int num4 = -1;
		for (int j = 0; j < list.Count && j < length; j++)
		{
			AIVirtualCard aIVirtualCard = list[j];
			if (!aIVirtualCard.IsUnit)
			{
				dstValues[j] = -1f;
				continue;
			}
			if (aIVirtualCard.IsCantUnderAnyAttack())
			{
				dstValues[j] = -1f;
				continue;
			}
			dstValues[j] = aIVirtualCard.EvaluateValueOnField(EnemyAI.EmptyPlayPtn, null, useStyle: true);
			if (dstEnemyUnitAdvantages[j] > 0f)
			{
				dstValues[j] += dstEnemyUnitAdvantages[j];
			}
			if (aIVirtualCard.IsGuard)
			{
				flag = true;
				if (dstValues[j] > num2)
				{
					num2 = dstValues[j];
					num4 = j;
				}
			}
			else if (!(dstValues[j] < 0f) && dstValues[j] > num)
			{
				num = dstValues[j];
				num3 = j;
			}
		}
		dstBGuard = flag;
		if (flag)
		{
			dstMaxIndex = num4;
			return num2;
		}
		dstMaxIndex = num3;
		return num;
	}

	public void EvaluateEnemyBattleAdvantage(ref float[] dstEnemyUnitAdvantages, ref float dstEnemyMaxAdvantage)
	{
		int count = _ai.CurrentVirtualField.EnemyInplayCards.Count;
		float[] dstValues = new float[_ai.CurrentVirtualField.AllyInplayCards.Count];
		float[] dstValues2 = new float[count];
		if (dstEnemyUnitAdvantages.Length == count)
		{
			for (int i = 0; i < count; i++)
			{
				dstEnemyUnitAdvantages[i] = float.MinValue;
			}
			bool dstBGuard = false;
			if (!(_EvaluateAllyFieldUnits(ref dstValues2, bSelf: false) < 0f))
			{
				int dstMaxIndex = 0;
				_EvaluateEnemyFieldUnits(ref dstValues, ref dstBGuard, ref dstMaxIndex, bSelf: false, bSimulateAdv: false);
				int dstAllyPtnIndex = 0;
				int dstEnemyIndex = 0;
				int dstEnemyRemainLife = 0;
				_CalcMostValuableUnitBattle(dstValues2, dstValues, dstBGuard, ref dstAllyPtnIndex, ref dstEnemyIndex, ref dstEnemyMaxAdvantage, ref dstEnemyRemainLife, ref dstEnemyUnitAdvantages, isSelf: false);
			}
		}
	}

	private void _CalcMostValuableUnitBattle(float[] allyValues, float[] enemyValues, bool bGuard, ref int dstAllyPtnIndex, ref int dstEnemyIndex, ref float dstAdvantage, ref int dstEnemyRemainLife, ref float[] dstUnitMaxAdvantages, bool isSelf)
	{
		int num = allyValues.Length;
		int num2 = enemyValues.Length;
		if (dstUnitMaxAdvantages.GetLength(0) != num)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			dstUnitMaxAdvantages[i] = float.MinValue;
		}
		int num3 = 0;
		int num4 = 0;
		int num5 = int.MinValue;
		int num6 = 0;
		float num7 = float.MinValue;
		int num8 = (int)Mathf.Pow(2f, num);
		for (int j = 0; j < num8; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				int dstEnemyRemainLife2 = 0;
				int dstAllyCount = 0;
				float num9 = _Atk_EvaluateAtkPattern(j, k, bGuard, enemyValues[k], allyValues, ref dstEnemyRemainLife2, ref dstAllyCount, isSelf);
				if (num9 > num7)
				{
					num3 = j;
					num4 = k;
					num5 = dstEnemyRemainLife2;
					num6 = dstAllyCount;
					num7 = num9;
				}
				else if (num9 == num7 && dstAllyCount < num6)
				{
					num3 = j;
					num4 = k;
					num5 = dstEnemyRemainLife2;
					num6 = dstAllyCount;
				}
				int num10 = j;
				for (int l = 0; l < num; l++)
				{
					int num11 = (int)Mathf.Pow(2f, num - l - 1);
					if (num10 / num11 > 0)
					{
						num10 -= num11;
						if (num9 > dstUnitMaxAdvantages[l])
						{
							dstUnitMaxAdvantages[l] = num9;
						}
					}
				}
			}
		}
		dstAllyPtnIndex = num3;
		dstEnemyIndex = num4;
		dstAdvantage = num7;
		dstEnemyRemainLife = num5;
	}

	private float _Atk_EvaluateAtkPattern(int ptnIndex, int enemyIndex, bool bGuard, float enemyValue, float[] allyValues, ref int dstEnemyRemainLife, ref int dstAllyCount, bool isSelf)
	{
		List<AIVirtualCard> list;
		AIVirtualCard aIVirtualCard;
		if (isSelf)
		{
			list = _ai.CurrentVirtualField.AllyInplayCards;
			aIVirtualCard = _ai.CurrentVirtualField.EnemyInplayCards[enemyIndex];
		}
		else
		{
			list = _ai.CurrentVirtualField.EnemyInplayCards;
			aIVirtualCard = _ai.CurrentVirtualField.AllyInplayCards[enemyIndex];
		}
		int count = list.Count;
		bool flag = !list.Any((AIVirtualCard c) => !c.IsIgnoreGuard || c.BaseCard.SkillApplyInformation.IsSkillCantAtkUnit);
		dstEnemyRemainLife = aIVirtualCard.Life;
		if (enemyValue < 0f)
		{
			return enemyValue;
		}
		if (bGuard && !aIVirtualCard.IsGuard && !flag)
		{
			return -1f;
		}
		int num = 0;
		float num2 = 0f;
		int num3 = ptnIndex;
		int num4 = 0;
		bool flag2 = false;
		for (int num5 = 0; num5 < count; num5++)
		{
			int num6 = (int)Mathf.Pow(2f, count - num5 - 1);
			if (num3 / num6 > 0)
			{
				num3 -= num6;
				num4++;
				if (allyValues[num5] < 0f)
				{
					return -1f;
				}
				num += list[num5].Attack;
				num2 = (aIVirtualCard.IsKiller ? (num2 + allyValues[num5]) : ((list[num5].Life > aIVirtualCard.Attack) ? (num2 + (float)aIVirtualCard.Attack) : (num2 + allyValues[num5])));
				if (list[num5].IsKiller)
				{
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			dstEnemyRemainLife = 0;
		}
		else
		{
			if (num < aIVirtualCard.Life)
			{
				return -1f;
			}
			dstEnemyRemainLife = aIVirtualCard.Life - num;
		}
		dstAllyCount = num4;
		return enemyValue - num2;
	}
}

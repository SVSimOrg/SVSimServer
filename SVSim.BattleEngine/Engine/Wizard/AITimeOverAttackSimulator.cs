using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class AITimeOverAttackSimulator : IBattleSimulationAI
{

	private AIRuleBaseBattleSimulator _ruleBaseSimulator;

	private SimulationResult _bestResult;

	private int _originalFieldInplayCount;

	private ulong[] _defaultPlayptnHashArray;

	private SimulationAdditionalActionInfoSet _simAdditionalOptions;

	public AIVirtualField Cr_MaxField
	{
		get
		{
			if (_bestResult == null)
			{
				return null;
			}
			return _bestResult.BestResultField;
		}
	}

	public float Cr_MaxFieldValue
	{
		get
		{
			if (_bestResult == null)
			{
				return 0f;
			}
			return _bestResult.MaxFieldValue;
		}
	}

	public float Cr_MaxFieldValueAtSelfTurnEnd
	{
		get
		{
			if (_bestResult == null)
			{
				return 0f;
			}
			return _bestResult.MaxFieldValueAtSelfTurnEnd;
		}
	}

	public List<AIVirtualActionInfo> Cr_MaxActionInfoSequence => _bestResult.ActionSequence;

	public PlayPtnWithToken Cr_BestPlayPtnWithToken { get; private set; }

	public AIVirtualActionInfo Cr_FirstActionInfo { get; private set; }

	public IEnumerator _CrSimulate(AIVirtualField originalField, bool doesUseEvo, bool checkTimeOverLogic, PlayPtnWithToken[] bestPlayPtnWithToken = null, SimulationAdditionalActionInfoSet additionalActions = null)
	{
		originalField.AI.IsInVirutalSimulation = true;
		_ruleBaseSimulator = new AIRuleBaseBattleSimulator(originalField.AI, bestPlayPtnWithToken, canChangeToTimeOverLogic: true);
		_bestResult = new SimulationResult();
		_originalFieldInplayCount = originalField.AllyInplayCards.Count((AIVirtualCard c) => !c.IsDead);
		_defaultPlayptnHashArray = AISimulationUtility.CalculatePlayptnWithTokenHashs(originalField, bestPlayPtnWithToken);
		_simAdditionalOptions = additionalActions;
		ParallelJob job = ParallelJob.Dispatch(delegate
		{
			try
			{
				AIVirtualField field = new AIVirtualField(originalField);
				int[] attackerIdxList = SortByOneAttackBreak(field);
				if (additionalActions != null)
				{
					_ = additionalActions.ForceEvoAction;
				}
				SimulateEvoPatternBattle(field, attackerIdxList, doesUseEvo);
				if (bestPlayPtnWithToken != null && Cr_MaxField != null)
				{
					int num = Cr_MaxField.AllyInplayCards.Count((AIVirtualCard c) => !c.IsDead);
					int num2 = Mathf.Min(Mathf.Max(0, _originalFieldInplayCount - num), bestPlayPtnWithToken.Length - 1);
					Cr_BestPlayPtnWithToken = null;
					for (int num3 = num2; num3 >= 0; num3--)
					{
						if (bestPlayPtnWithToken[num3] != null)
						{
							Cr_BestPlayPtnWithToken = bestPlayPtnWithToken[num3];
							break;
						}
					}
				}
			}
			catch (Exception)
			{
			}
		});
		while (!job.isDone)
		{
			yield return null;
		}
		if (Cr_MaxField != null)
		{
			Cr_FirstActionInfo = BattleSequencer.GetFirstActionInfo(_bestResult.ActionSequence, Cr_MaxField, Cr_MaxField.BestPlayPtn);
		}
		originalField.AI.IsInVirutalSimulation = false;
	}

	private int[] SortByOneAttackBreak(AIVirtualField field)
	{
		List<int> sortedAttackerIndexList = GetSortedAttackerIndexList(field);
		int num = ((field.EnemyInplayCards != null) ? field.EnemyInplayCards.Count : 0);
		if (num == 0)
		{
			return sortedAttackerIndexList.ToArray();
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>(sortedAttackerIndexList);
		for (int i = 0; i < sortedAttackerIndexList.Count; i++)
		{
			int num2 = 0;
			bool flag = false;
			int num3 = 0;
			for (int j = 0; j < num; j++)
			{
				AIVirtualCard aIVirtualCard = field.AllyInplayCards[sortedAttackerIndexList[i]];
				AIVirtualCard aIVirtualCard2 = field.EnemyInplayCards[j];
				if (!aIVirtualCard2.IsUnit)
				{
					continue;
				}
				int num4 = aIVirtualCard2.SimulateDamageAmount(aIVirtualCard.Attack);
				if (aIVirtualCard2.Life <= num4)
				{
					num2++;
					if (!flag)
					{
						flag = aIVirtualCard2.IsBreakFirst(field.BestPlayPtn);
					}
					if (aIVirtualCard2.IsBreakLast(field.BestPlayPtn))
					{
						num3++;
					}
					break;
				}
			}
			if (num2 > 0 && flag)
			{
				int item = sortedAttackerIndexList[i];
				list.Add(item);
				list3.Remove(item);
			}
			else if (num2 == 0 || (num2 > 0 && num3 == num2))
			{
				int item2 = sortedAttackerIndexList[i];
				list2.Add(item2);
				list3.Remove(item2);
			}
		}
		list.AddRange(list3);
		list.AddRange(list2);
		return list.ToArray();
	}

	private void SimulateEvoPatternBattle(AIVirtualField field, int[] attackerIdxList, bool useEve)
	{
		List<int> sortedAttackTargetIndexList = GetSortedAttackTargetIndexList(field);
		if (((_simAdditionalOptions != null) ? _simAdditionalOptions.ForceEvoAction : null) != null)
		{
			SimulateTargetsBreakPattern(field, attackerIdxList, -1, sortedAttackTargetIndexList);
			return;
		}
		int num = (useEve ? field.AllyInplayCards.Count : 0);
		for (int i = -1; i < num; i++)
		{
			if (AISimulationUtility.IsValuableEvoIndex(i, field))
			{
				SimulateTargetsBreakPattern(field, attackerIdxList, i, sortedAttackTargetIndexList);
			}
		}
	}

	private void SimulateTargetsBreakPattern(AIVirtualField field, int[] attackrIndecies, int evoIndex, List<int> enemyAllTargets)
	{
		List<ulong> list = new List<ulong>();
		List<int> list2 = new List<int>();
		_ruleBaseSimulator.RunSequence(field, attackrIndecies, evoIndex, new List<int>(), _bestResult, _simAdditionalOptions, null, checkActFailure: false);
		list.Add(AISimulationUtility.CalculateTargetSequenceHash(list2.ToArray(), field));
		int num = enemyAllTargets.Count;
		while (num > 0 && !field.AI.IsBattleEnd)
		{
			int[] patternGroup = AISimulationUtility.GetPatternGroup(num);
			if (patternGroup != null)
			{
				for (int i = 0; i < patternGroup.Length; i++)
				{
					List<int> enemyTargets = AISimulationUtility.GetEnemyTargets(field.EnemyInplayCards, enemyAllTargets, patternGroup[i], field.BestPlayPtn);
					ulong item = AISimulationUtility.CalculateTargetSequenceHash(enemyTargets.ToArray(), field);
					if (!list.Contains(item))
					{
						AIOneMoreLastwordUtility.SortEnemyTargetByBreakBonus(field, enemyTargets);
						if (!AISimulationUtility.IsValidEnemyTargetIndexes(field.EnemyInplayCards, enemyTargets))
						{
							return;
						}
						_ruleBaseSimulator.RunSequence(field, attackrIndecies, evoIndex, enemyTargets, _bestResult, _simAdditionalOptions, null, checkActFailure: false);
						list.Add(item);
					}
				}
			}
			num--;
		}
	}

	private List<int> GetSortedAttackerIndexList(AIVirtualField field)
	{
		List<int> list = new List<int>();
		List<bool> list2 = new List<bool>();
		List<AIVirtualCard> allyInplayCards = field.AllyInplayCards;
		for (int i = 0; i < allyInplayCards.Count; i++)
		{
			list2.Add(AIAttackSimulationUtility.IsAttackPossible(field, allyInplayCards[i].AttackLeaderSituation));
		}
		for (int j = 0; j < allyInplayCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard = allyInplayCards[j];
			if (!aIVirtualCard.IsUnit)
			{
				continue;
			}
			int num = -1;
			for (int k = 0; k < list.Count; k++)
			{
				int index = list[k];
				AIVirtualCard aIVirtualCard2 = allyInplayCards[index];
				if (aIVirtualCard.Attack < aIVirtualCard2.Attack)
				{
					num = k;
					break;
				}
				if (aIVirtualCard.Attack == aIVirtualCard2.Attack && (!list2[j] || list2[index]) && ((!list2[j] && list2[index]) || aIVirtualCard.Life < aIVirtualCard2.Life))
				{
					num = k;
					break;
				}
			}
			if (num >= 0 && num < list.Count)
			{
				list.Insert(num, j);
			}
			else
			{
				list.Add(j);
			}
		}
		return list;
	}

	private List<int> GetSortedAttackTargetIndexList(AIVirtualField field)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<AIVirtualCard> enemyInplayCards = field.EnemyInplayCards;
		for (int i = 0; i < enemyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = enemyInplayCards[i];
			if (!aIVirtualCard.IsUnit)
			{
				continue;
			}
			int num = -1;
			List<int> list3 = (aIVirtualCard.IsGuard ? list2 : list);
			for (int j = 0; j < list3.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = enemyInplayCards[list3[j]];
				if (aIVirtualCard.Life < aIVirtualCard2.Life || (aIVirtualCard.Life == aIVirtualCard2.Life && aIVirtualCard.Attack > aIVirtualCard2.Attack))
				{
					num = j;
					break;
				}
			}
			if (num >= 0 && num < list3.Count)
			{
				list3.Insert(num, i);
			}
			else
			{
				list3.Add(i);
			}
		}
		list2.AddRange(list);
		return list2;
	}
}

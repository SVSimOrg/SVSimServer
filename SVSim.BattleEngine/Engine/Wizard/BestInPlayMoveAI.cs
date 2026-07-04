using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;

namespace Wizard;

public class BestInPlayMoveAI : IBattleSimulationAI
{
	private EnemyAI AI;

	private InplayMovePatternFilter _patternFilter;

	private AIRuleBaseBattleSimulator _ruleBaseSimulator;

	private SimulationResult BestRecord;

	private SimulationAdditionalActionInfoSet _simAdditionalOptions;

	public AIVirtualField Cr_MaxField => BestRecord.BestResultField;

	public float Cr_MaxFieldValue => BestRecord.MaxFieldValue;

	public float Cr_MaxFieldValueAtSelfTurnEnd => BestRecord.MaxFieldValueAtSelfTurnEnd;

	public AIVirtualActionInfo Cr_FirstActionInfo { get; private set; }

	public List<AIVirtualActionInfo> Cr_MaxActionInfoSequence => BestRecord.ActionSequence;

	public BestInPlayMoveAI(EnemyAI ai)
	{
		AI = ai;
		_patternFilter = new InplayMovePatternFilter();
	}

	public IEnumerator _CrSimulate(AIVirtualField originalField, bool doesUseEvo, bool checkTimeOverLogic, PlayPtnWithToken[] bestPlayPtnsWithToken = null, SimulationAdditionalActionInfoSet additionalActions = null)
	{
		AI.IsInVirutalSimulation = true;
		_ruleBaseSimulator = new AIRuleBaseBattleSimulator(AI, bestPlayPtnsWithToken, canChangeToTimeOverLogic: false);
		BestRecord = new SimulationResult();
		_simAdditionalOptions = additionalActions;
		ParallelJob job = ParallelJob.Dispatch(delegate
		{
			try
			{
				RunSimulation(originalField, doesUseEvo);
			}
			catch (Exception)
			{
			}
		});
		while (!job.isDone)
		{
			yield return null;
			if (checkTimeOverLogic && AI.IsRunWeakLogic)
			{
				AI.IsInVirutalSimulation = false;
				yield break;
			}
		}
		if (Cr_MaxField == null)
		{
			AI.IsInVirutalSimulation = false;
			yield break;
		}
		if (Cr_MaxActionInfoSequence.Count > 0)
		{
			Cr_FirstActionInfo = BattleSequencer.GetFirstActionInfo(Cr_MaxActionInfoSequence, Cr_MaxField, Cr_MaxField.BestPlayPtn);
		}
		AI.IsInVirutalSimulation = false;
	}

	private void RunSimulation(AIVirtualField originalField, bool doesUseEvo)
	{
		AIVirtualField originalField2 = new AIVirtualField(originalField);
		List<int> enemyFollowers = GetEnemyFollowers(originalField2);
		RunAllBreakPattern(originalField2, enemyFollowers, doesUseEvo);
	}

	private void RunAllBreakPattern(AIVirtualField originalField, List<int> enemyFollowers, bool doesUseEvo)
	{
		RunOnePattern(doesUseEvo, new List<int>(), originalField, enemyFollowers);
		List<ulong> list = new List<ulong>();
		int num = enemyFollowers.Count;
		while (num > 0 && !AI.IsBattleEnd && !AI.IsRunWeakLogic)
		{
			int[] patternGroup = AISimulationUtility.GetPatternGroup(num);
			if (patternGroup != null)
			{
				for (int i = 0; i < patternGroup.Length; i++)
				{
					if (AI.IsBattleEnd)
					{
						break;
					}
					if (AI.IsRunWeakLogic)
					{
						break;
					}
					List<int> enemyTargets = AISimulationUtility.GetEnemyTargets(originalField.EnemyInplayCards, enemyFollowers, patternGroup[i], originalField.BestPlayPtn);
					ulong item = AISimulationUtility.CalculateTargetSequenceHash(enemyTargets.ToArray(), originalField);
					if (!list.Contains(item))
					{
						_patternFilter.CheckBreakPatternCondition(originalField, enemyTargets);
						AIOneMoreLastwordUtility.SortEnemyTargetByBreakBonus(originalField, enemyTargets);
						RunOnePattern(doesUseEvo, enemyTargets, originalField, enemyFollowers);
						list.Add(item);
						if (_patternFilter.IsBestBreakPattern)
						{
							return;
						}
					}
				}
			}
			num--;
		}
	}

	private void RunOnePattern(bool doesUseEvo, List<int> enemyTargets, AIVirtualField originalField, List<int> enemyFollowers)
	{
		if (!AISimulationUtility.IsValidEnemyTargetIndexes(originalField.EnemyInplayCards, enemyTargets))
		{
			return;
		}
		int num = (doesUseEvo ? originalField.AllyInplayCards.Count : 0);
		for (int i = -1; i < num; i++)
		{
			if (AISimulationUtility.IsValuableEvoIndex(i, originalField))
			{
				MostValuableAttackPattern(originalField, BestRecord, i, enemyTargets);
			}
		}
	}

	private void MostValuableAttackPattern(AIVirtualField originalField, SimulationResult bestRecord, int evoIndex, List<int> enemyTargets)
	{
		List<int[]> attackerSequencePermLists = AIMathematicsLibrary.EnumeratePermutations(GetAllyInplayIndexLIst(originalField)).ToList();
		AIVirtualField dontUseHandAllRemovalFieldSub = null;
		if (originalField.IsHandAllRemovalWaiting())
		{
			dontUseHandAllRemovalFieldSub = new AIVirtualField(originalField);
		}
		ExecuteAttackSequenceForAllFields(originalField, dontUseHandAllRemovalFieldSub, enemyTargets, attackerSequencePermLists, evoIndex, bestRecord);
	}

	private void ExecuteAttackSequenceForAllFields(AIVirtualField originalField, AIVirtualField DontUseHandAllRemovalFieldSub, List<int> enemyTargets, List<int[]> attackerSequencePermLists, int evoIndex, SimulationResult bestRecord)
	{
		List<ulong> list = new List<ulong>();
		int count = attackerSequencePermLists.Count;
		for (int i = 0; i < count; i++)
		{
			if (AI.IsRunWeakLogic)
			{
				break;
			}
			int[] array = attackerSequencePermLists[i];
			ulong item = AISimulationUtility.CalculateAttackSequenceHash(array, originalField);
			if (!list.Contains(item))
			{
				_ruleBaseSimulator.RunSequence(originalField, array, evoIndex, enemyTargets, bestRecord, _simAdditionalOptions, _patternFilter);
				if (DontUseHandAllRemovalFieldSub != null)
				{
					_ruleBaseSimulator.RunSequence(DontUseHandAllRemovalFieldSub, array, evoIndex, enemyTargets, bestRecord, _simAdditionalOptions, _patternFilter);
				}
				list.Add(item);
			}
		}
	}

	private List<int> GetEnemyFollowers(AIVirtualField originalField)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < originalField.EnemyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = originalField.EnemyInplayCards[i];
			if (!aIVirtualCard.IsUnit)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = originalField.EnemyInplayCards[list[j]];
				if (EnemyAI.IsLargerThan(aIVirtualCard.Value, aIVirtualCard2.Value))
				{
					list.Insert(j, i);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(i);
			}
		}
		return list;
	}

	private List<int> GetAllyInplayIndexLIst(AIVirtualField originalField)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < originalField.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = originalField.AllyInplayCards[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsDead && aIVirtualCard.AttackableCount > 0)
			{
				list.Add(i);
			}
		}
		return list;
	}
}

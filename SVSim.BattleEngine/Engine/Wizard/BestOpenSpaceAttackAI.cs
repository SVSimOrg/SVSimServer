using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;

namespace Wizard;

public class BestOpenSpaceAttackAI
{
	private EnemyAI AI;

	public AIVirtualActionInfo Cr_FirstActionInfo;

	public PlayPtnWithToken Cr_BestPlayPtnWithToken;

	private ulong[] _defaultPlayptnHashArray;

	public BestOpenSpaceAttackAI(EnemyAI ai)
	{
		AI = ai;
	}

	public IEnumerator _CrSimulate(AIVirtualField originalField, PlayPtnWithToken[] bestPlayPtnsWithToken)
	{
		AI.IsInVirutalSimulation = true;
		float maxFieldValue = float.MinValue;
		int bestFirstDeadActionIndex = int.MaxValue;
		List<AIVirtualActionInfo> bestActionInfoSequence = null;
		AIVirtualField bestField = originalField;
		Cr_BestPlayPtnWithToken = null;
		Cr_FirstActionInfo = null;
		_defaultPlayptnHashArray = AISimulationUtility.CalculatePlayptnWithTokenHashs(originalField, bestPlayPtnsWithToken);
		ParallelJob job = ParallelJob.Dispatch(delegate
		{
			try
			{
				int count = originalField.AllyInplayCards.Count;
				List<int> list = new List<int>();
				for (int i = 0; i < originalField.AllyInplayCards.Count; i++)
				{
					AIVirtualCard aIVirtualCard = originalField.AllyInplayCards[i];
					int num = 0;
					for (num = 0; num < list.Count && !(aIVirtualCard.Value < originalField.AllyInplayCards[list[num]].Value); num++)
					{
					}
					list.Insert(num, i);
				}
				int num2 = (1 << originalField.AllyInplayCards.Count) - 1;
				List<ulong> list2 = new List<ulong>();
				for (int j = 0; j <= num2; j++)
				{
					if (IsValidBreakIndex(originalField.AllyInplayCards, list, j))
					{
						if (AI.IsBattleEnd || AI.IsRunWeakLogic)
						{
							break;
						}
						int[] allyAttackers = GetAllyAttackers(originalField, list, j);
						ulong item = AISimulationUtility.CalculateAttackSequenceHash(allyAttackers, originalField);
						if (!list2.Contains(item))
						{
							List<int> list3 = new List<int>();
							for (int k = 0; k < originalField.EnemyInplayCards.Count; k++)
							{
								if (originalField.EnemyInplayCards[k].IsUnit)
								{
									list3.Add(k);
								}
							}
							if (AI.IsRunWeakLogic)
							{
								break;
							}
							List<int[]> list4 = AIMathematicsLibrary.EnumeratePermutations(list3).ToList();
							List<ulong> list5 = new List<ulong>();
							for (int l = 0; l < list4.Count; l++)
							{
								List<int> enemyTargetIndexes = list4[l].ToList();
								if (AISimulationUtility.IsValidEnemyTargetIndexes(originalField.EnemyInplayCards, enemyTargetIndexes))
								{
									ulong item2 = AISimulationUtility.CalculateTargetSequenceHash(list4[l], originalField);
									if (!list5.Contains(item2))
									{
										int num3 = (1 << allyAttackers.Length) - 1;
										for (int m = 0; m <= num3; m++)
										{
											if (AI.IsRunWeakLogic)
											{
												return;
											}
											bool[] attackLeaderOrFollowerFlags = GetAttackLeaderOrFollowerFlags(m, allyAttackers.Length);
											AIVirtualField aIVirtualField = new AIVirtualField(originalField);
											List<AIVirtualActionInfo> actionInfoSequence = AISimulationUtility.GetActionInfoSequence(aIVirtualField, allyAttackers, attackLeaderOrFollowerFlags, -1);
											List<AIVirtualCard> enemyTargetSequence = AISimulationUtility.GetEnemyTargetSequence(aIVirtualField, enemyTargetIndexes);
											BattleSequencer.ExecSimulation(aIVirtualField, actionInfoSequence, enemyTargetSequence, new SimulationSetting(isHandRemovalValid: true, useLeaderAttackPreCheck: false, noSkipAttack: true));
											_ = aIVirtualField.EnemyClass.IsDead;
											if (AI.IsRunWeakLogic)
											{
												return;
											}
											float num4 = AISimulationUtility.EvaluateField(aIVirtualField, EvaluationType.SIMPLE);
											if (AI.IsRunWeakLogic)
											{
												return;
											}
											int openCount = count - aIVirtualField.AllyInplayCards.Count((AIVirtualCard c) => !c.IsDead);
											PlayPtnWithToken bestPlayPtnWithToken = null;
											int bestFirstDeadActionIndex2 = int.MaxValue;
											float num5 = AISimulationUtility.EvaluateLegalPlayPtnAfterSimulation(aIVirtualField, actionInfoSequence, bestPlayPtnsWithToken, openCount, _defaultPlayptnHashArray, out bestPlayPtnWithToken, out bestFirstDeadActionIndex2);
											float num6 = num4 + num5;
											if (AI.IsRunWeakLogic)
											{
												return;
											}
											if (EnemyAI.IsLargerThan(num6, maxFieldValue) || (EnemyAI.IsSameValue(num6, maxFieldValue) && bestFirstDeadActionIndex2 < bestFirstDeadActionIndex))
											{
												maxFieldValue = num6;
												bestFirstDeadActionIndex = bestFirstDeadActionIndex2;
												Cr_BestPlayPtnWithToken = bestPlayPtnWithToken;
												bestActionInfoSequence = actionInfoSequence;
												bestField = aIVirtualField;
											}
										}
										list5.Add(item2);
									}
								}
							}
							list2.Add(item);
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
			if (AI.IsRunWeakLogic)
			{
				AI.IsInVirutalSimulation = false;
				yield break;
			}
		}
		if (AI.IsBattleEnd)
		{
			AI.IsInVirutalSimulation = false;
			yield break;
		}
		if (bestActionInfoSequence.Count > 0)
		{
			Cr_FirstActionInfo = BattleSequencer.GetFirstActionInfo(bestActionInfoSequence, bestField, Cr_BestPlayPtnWithToken.PlayPtn);
		}
		AI.IsInVirutalSimulation = false;
	}

	private bool IsValidBreakIndex(List<AIVirtualCard> allyCards, List<int> sortedInplayIndex, int sortedBreakIndex)
	{
		for (int i = 0; i < allyCards.Count; i++)
		{
			if (((sortedBreakIndex >> i) & 1) > 0)
			{
				AIVirtualCard aIVirtualCard = allyCards[sortedInplayIndex[i]];
				if (aIVirtualCard.IsAmulet || aIVirtualCard.AttackableCount <= 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	private int[] GetAllyAttackers(AIVirtualField field, List<int> sortedInplayIndex, int sortedBreakIndex)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < sortedInplayIndex.Count; i++)
		{
			if (!field.AllyInplayCards[sortedInplayIndex[i]].IsAmulet && ((sortedBreakIndex >> i) & 1) > 0)
			{
				list.Add(sortedInplayIndex[i]);
			}
		}
		return list.ToArray();
	}

	private bool[] GetAttackLeaderOrFollowerFlags(int index, int attackerCount)
	{
		bool[] array = new bool[attackerCount];
		for (int i = 0; i < attackerCount; i++)
		{
			if (((index >> i) & 1) > 0)
			{
				array[i] = true;
			}
			else
			{
				array[i] = false;
			}
		}
		return array;
	}
}

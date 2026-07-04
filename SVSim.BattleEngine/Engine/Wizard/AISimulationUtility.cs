using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public static class AISimulationUtility
{
	public static int FULL_SIMULATION_MOVE_COUNT_TURESHOLD = 16;

	public static float ILLEGAL_PLAYPTN_VALUE = -1000f;

	private static readonly int[] PATTERN_GROUP_FOR_5 = new int[1] { 31 };

	private static readonly int[] PATTERN_GROUP_FOR_4 = new int[5] { 30, 29, 27, 23, 15 };

	private static readonly int[] PATTERN_GROUP_FOR_3 = new int[10] { 28, 26, 25, 22, 21, 19, 14, 13, 11, 7 };

	private static readonly int[] PATTERN_GROUP_FOR_2 = new int[10] { 24, 20, 18, 17, 12, 10, 9, 6, 5, 3 };

	private static readonly int[] PATTERN_GROUP_FOR_1 = new int[5] { 16, 8, 4, 2, 1 };

	private static ulong[] PRIME_NUMBERS_FOR_ENEMY = new ulong[6] { 67uL, 9221uL, 181uL, 271939uL, 3001uL, 12277uL };

	private static ulong[] PRIME_NUMBERS_FOR_ALLY = new ulong[6] { 4919uL, 11149uL, 187651uL, 139uL, 3011uL, 2003uL };

	public static List<AIVirtualActionInfo> GetActionInfoSequence(AIVirtualField field, int[] permList, bool[] isAttackFollowerFlags, int evoIndex)
	{
		List<AIVirtualActionInfo> list = new List<AIVirtualActionInfo>();
		if (permList != null && permList.Length <= field.AllyInplayCards.Count((AIVirtualCard c) => c.IsUnit) && permList.Length == isAttackFollowerFlags.Length)
		{
			for (int num = 0; num < permList.Length; num++)
			{
				int num2 = permList[num];
				AIVirtualCard originalActor = field.AllyInplayCards[num2];
				AIVirtualCard currentFieldActor = field.AllyInplayCards.FirstOrDefault((AIVirtualCard card) => card.CardIndex == originalActor.CardIndex);
				if (currentFieldActor == null)
				{
					break;
				}
				if (num2 == evoIndex && !currentFieldActor.IsEvolution)
				{
					field.AllyInplayCards[num2].IsUseEvo = true;
					if (field.CardListSet.AllyClassAndInplayCards.Any((AIVirtualCard c) => c.IsFirstEvo(currentFieldActor, field.BestPlayPtn)))
					{
						list.Insert(0, new AIVirtualTargetSelectAction(currentFieldActor, currentFieldActor, AIOperationType.EVOLVE));
					}
					else
					{
						list.Add(new AIVirtualTargetSelectAction(currentFieldActor, currentFieldActor, AIOperationType.EVOLVE));
					}
				}
				list.Add(new AIVirtualAttackInfo(currentFieldActor, isAttackFollowerFlags[num]));
			}
		}
		field.ActionLength = 0;
		return list;
	}

	public static List<int> GetEnemyTargets(List<AIVirtualCard> enemyInplayCards, List<int> targetCandidates, int breakPtnIndex, List<int> playPtn)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < targetCandidates.Count; i++)
		{
			int num = targetCandidates[i];
			if (!enemyInplayCards[num].IsAmulet && ((breakPtnIndex >> i) & 1) > 0)
			{
				AddTarget(enemyInplayCards, num, list, playPtn);
			}
		}
		return list;
	}

	private static void AddTarget(List<AIVirtualCard> targetCandidates, int targetIndex, List<int> virtualCardList, List<int> playPtn)
	{
		AIVirtualCard tagOwner = targetCandidates[targetIndex];
		int num = -1;
		for (int i = 0; i < virtualCardList.Count; i++)
		{
			if (targetCandidates[virtualCardList[i]].IsBreakLast(playPtn))
			{
				num = i;
				break;
			}
		}
		if (tagOwner.IsBreakFirst(playPtn))
		{
			virtualCardList.Insert(0, targetIndex);
		}
		else if (tagOwner.IsBreakLast(playPtn))
		{
			virtualCardList.Add(targetIndex);
		}
		else if (num >= 0)
		{
			virtualCardList.Insert(num, targetIndex);
		}
		else
		{
			virtualCardList.Add(targetIndex);
		}
	}

	public static List<AIVirtualCard> GetEnemyTargetSequence(AIVirtualField field, List<int> enemyTargetIndexes)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < enemyTargetIndexes.Count; i++)
		{
			if (enemyTargetIndexes[i] < field.EnemyInplayCards.Count && enemyTargetIndexes[i] >= 0)
			{
				list.Add(field.EnemyInplayCards[enemyTargetIndexes[i]]);
			}
		}
		return list;
	}

	public static bool IsValidEnemyTargetIndexes(List<AIVirtualCard> enemyCards, List<int> enemyTargetIndexes)
	{
		if (enemyTargetIndexes.Count > enemyCards.Count)
		{
			return false;
		}
		for (int i = 0; i < enemyTargetIndexes.Count; i++)
		{
			if (enemyCards[enemyTargetIndexes[i]].IsAmulet)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsValuableEvoIndex(int evoIndex, AIVirtualField originalField)
	{
		if (evoIndex >= 0 && !originalField.AllyInplayCards[evoIndex].IsUnit)
		{
			return false;
		}
		if (evoIndex >= 0)
		{
			AIVirtualCard aIVirtualCard = originalField.AllyInplayCards[evoIndex];
			AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.EVOLVE, (AISelectedTargetInfoSet)null);
			if (!aIVirtualCard.IsAbleEvolution() || aIVirtualCard.CreateAIVirtualSelectInfo(originalField, situation) != null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsBestRecord(AIVirtualField field, float currentFieldValue, float valueAtSelfTurnEnd, SimulationResult bestRecord)
	{
		if (field.EnemyClass.IsDead)
		{
			int actionLength = field.ActionLength;
			if (actionLength < bestRecord.MinActionLengthOfLethal || (actionLength == bestRecord.MinActionLengthOfLethal && EnemyAI.IsLargerThan(currentFieldValue, bestRecord.MaxFieldValue)))
			{
				return true;
			}
		}
		else
		{
			if (EnemyAI.IsLargerThan(currentFieldValue, bestRecord.MaxFieldValue))
			{
				return true;
			}
			if (EnemyAI.IsSameValue(currentFieldValue, bestRecord.MaxFieldValue) && EnemyAI.IsLargerThan(valueAtSelfTurnEnd, bestRecord.MaxFieldValueAtSelfTurnEnd))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ExecuteVirtualAction(AIVirtualActionInfo actionInfo, AIVirtualActionInfo evolutionInfo, List<AIVirtualCard> enemyTargetSequence, SimulationSetting setting)
	{
		AIVirtualField selfField = actionInfo.Actor.SelfField;
		if (actionInfo.IsAlreadyUsed)
		{
			_ = (actionInfo.Actor.IsPlayer ? "p" : "e") + actionInfo.Actor.CardIndex;
			return false;
		}
		if (actionInfo.Actor.IsDead)
		{
			actionInfo.IsAlreadyUsed = true;
			return false;
		}
		if (actionInfo.ActionType == AIOperationType.EVOLVE)
		{
			actionInfo.IsAlreadyUsed = true;
			if (actionInfo.Actor.IsEvolution)
			{
				return false;
			}
			if (!actionInfo.Actor.IsUseEvo)
			{
				return false;
			}
			AIVirtualEvolutionSimulator.ManualEvolve(actionInfo, selfField);
			selfField.EvoUsedCard = actionInfo.Actor;
		}
		else if (actionInfo is AIVirtualAttackInfo aIVirtualAttackInfo)
		{
			AIVirtualAttackInfo aIVirtualAttackInfo2 = aIVirtualAttackInfo;
			if (aIVirtualAttackInfo.AttackTarget != null)
			{
				aIVirtualAttackInfo2 = new AIVirtualAttackInfo(aIVirtualAttackInfo.Actor, aIVirtualAttackInfo.IsAttackFollower, aIVirtualAttackInfo.PremiseAction);
			}
			AIVirtualCard aIVirtualCard = BattleSequencer.FindAttackTarget(selfField, aIVirtualAttackInfo2, enemyTargetSequence, evolutionInfo, setting.IsHandRemovalValid, setting.NoSkipAttack, aIVirtualAttackInfo2.IsAttackFollower);
			bool isAttackerUsed = false;
			if (aIVirtualCard != null)
			{
				AIAttackSimulationUtility.SimulateAttackIfValuable(aIVirtualAttackInfo2, selfField, setting.UseLeaderAttackPreCheck, ref isAttackerUsed);
				if (aIVirtualCard.IsDead)
				{
					aIVirtualAttackInfo2.IsBreakTarget = true;
				}
			}
			else
			{
				aIVirtualAttackInfo2.SetAttackTarget(null);
				isAttackerUsed = true;
			}
			actionInfo.IsAlreadyUsed = isAttackerUsed;
			if (!aIVirtualAttackInfo2.IsAttackSuccessed)
			{
				return false;
			}
		}
		return true;
	}

	public static float EvaluateField(AIVirtualField field, EvaluationType type)
	{
		switch (type)
		{
		case EvaluationType.FULL:
			return field.EvaluateField() - (field.EnemyClass.IsDead ? 0f : field.EvaluateThreaten());
		case EvaluationType.SELF_TURN:
		case EvaluationType.SIMPLE:
			return field.EvaluateField();
		default:
			return 0f;
		}
	}

	public static float EvaluateLegalPlayPtnAfterSimulation(AIVirtualField fieldTemp, List<AIVirtualActionInfo> actionInfoSequence, PlayPtnWithToken[] playPtnsWithToken, int openCount, ulong[] defaultPlayptnHashs, out PlayPtnWithToken bestPlayPtnWithToken, out int bestFirstDeadActionIndex)
	{
		bestFirstDeadActionIndex = int.MaxValue;
		bestPlayPtnWithToken = null;
		ulong[] array = CalculatePlayptnWithTokenHashs(fieldTemp, playPtnsWithToken);
		PlayPtnWithToken[] array2 = new PlayPtnWithToken[playPtnsWithToken.Length];
		for (int i = 0; i < playPtnsWithToken.Length; i++)
		{
			PlayPtnWithToken playPtnWithToken = playPtnsWithToken[i];
			if (playPtnWithToken == null || array[i] != defaultPlayptnHashs[i])
			{
				array2[i] = null;
			}
			else
			{
				array2[i] = playPtnWithToken;
			}
		}
		if (array2.Any((PlayPtnWithToken p) => p != null))
		{
			return EvaluatePlayPtnValue(fieldTemp, actionInfoSequence, array2, openCount, out bestPlayPtnWithToken, out bestFirstDeadActionIndex);
		}
		return ILLEGAL_PLAYPTN_VALUE;
	}

	private static float EvaluatePlayPtnValue(AIVirtualField fieldTemp, List<AIVirtualActionInfo> actionInfoSequence, PlayPtnWithToken[] playPtnsWithToken, int openCount, out PlayPtnWithToken bestPlayPtnWithToken, out int bestFirstDeadActionIndex)
	{
		AIVirtualFieldRollBackBasicProcessor aIVirtualFieldRollBackBasicProcessor = new AIVirtualFieldRollBackBasicProcessor(fieldTemp);
		AIStyleQuery styleQuery = fieldTemp.ParamQuery.styleQuery;
		float num = float.MinValue;
		bestFirstDeadActionIndex = int.MaxValue;
		bestPlayPtnWithToken = null;
		for (int num2 = openCount; num2 >= 0; num2--)
		{
			if (playPtnsWithToken[num2] != null)
			{
				PlayPtnWithToken playPtnWithToken = playPtnsWithToken[num2];
				if (playPtnWithToken != null)
				{
					List<int> list = (fieldTemp.BestPlayPtn = playPtnWithToken.PlayPtn);
					fieldTemp.UpdateBestPlayptnRecordOnSim(list);
					List<TokenPlayPattern> tokenPtn = playPtnWithToken.TokenPtn;
					if (list.Count == tokenPtn.Count)
					{
						float num3 = float.MinValue;
						for (int i = 0; i < fieldTemp.AllyInplayCards.Count; i++)
						{
							AIVirtualCard aIVirtualCard = fieldTemp.AllyInplayCards[i];
							if (aIVirtualCard.IsUnit && !aIVirtualCard.IsDead && aIVirtualCard.IsAbleEvolution())
							{
								float num4 = aIVirtualCard.EvaluateAllEvoBonus(list, new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.EVOLVE, (AISelectedTargetInfoSet)null));
								if (num4 > num3)
								{
									num3 = num4;
								}
							}
						}
						float num5 = 0f;
						AISinglePlayptnRecord playptnRecordOnSim = fieldTemp.GetPlayptnRecordOnSim(list);
						for (int j = 0; j < tokenPtn.Count; j++)
						{
							TokenPlayPattern tokenPlayPattern = tokenPtn[j];
							AIVirtualCard aIVirtualCard2 = fieldTemp.AllyHandCards[list[j]];
							AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard2, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
							AIPreprocessSimulationUtility.SimulatePreprocess(aIVirtualCard2, situation, fieldTemp, AIScriptTokenArgType.WHEN_PLAY, isPseudo: true);
							bool flag = fieldTemp.StyleQuery.IsPlayBreak(aIVirtualCard2, list, situation);
							bool flag2 = playptnRecordOnSim.PlayedCardList[j].PlayType == PlaySimulationType.Accelerate;
							if (!flag && !flag2)
							{
								num5 += aIVirtualCard2.EvaluateValueOnField(list, null, useStyle: true);
							}
							num5 += aIVirtualCard2.EvaluatePlayValue(list, situation);
							num5 += (aIVirtualCard2.GetAllBreakBonus(list, useIgnoreInBattle: false) + aIVirtualCard2.GetAllLeaveBonus(list, useIgnoreInBattle: false)) * (flag ? 1f : EnemyAI.BREAKBONUS_RATE_IN_HAND);
							fieldTemp.UpdateVirtualFieldWhenEvaluation(aIVirtualCard2, situation);
							if (tokenPlayPattern.TokenInfoPairList != null && tokenPlayPattern.TokenInfoPairList.Count > 0)
							{
								for (int k = 0; k < tokenPlayPattern.TokenInfoPairList.Count; k++)
								{
									AITokenInformation aITokenInformation = tokenPlayPattern.TokenInfoPairList[k];
									int tokenId = aITokenInformation.TokenId;
									AIVirtualCard tokenFromId = fieldTemp.AI.tokenManager.GetTokenFromId(tokenId, aIVirtualCard2.IsAlly, fieldTemp);
									num5 += tokenFromId.EvaluateValueOnField(list, null, useStyle: true);
									num5 += (tokenFromId.GetAllBreakBonus(list, useIgnoreInBattle: false) + tokenFromId.GetAllLeaveBonus(list, useIgnoreInBattle: false)) * EnemyAI.BREAKBONUS_RATE_IN_HAND;
									if (aITokenInformation.TokenType == AITokenType.Reanimate)
									{
										num5 += tokenFromId.GetReanimateBonus(list);
									}
								}
							}
							if (j == 0 && aIVirtualCard2.IsFollower(list) && aIVirtualCard2.IsAbleEvolution())
							{
								float num6 = aIVirtualCard2.EvaluateAllEvoBonus(list, new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard2, AIOperationType.EVOLVE, (AISelectedTargetInfoSet)null));
								if (num6 > num3)
								{
									num5 += num6;
								}
							}
						}
						aIVirtualFieldRollBackBasicProcessor.ResetVirtualFieldToStart();
						num5 += AIEvaluateBonusFromOhterUtility.GetPlayPtnBonus(fieldTemp, list) + styleQuery.GetPlayptnBonus(fieldTemp, list);
						int num7 = int.MaxValue;
						for (int l = 0; l < actionInfoSequence.Count; l++)
						{
							if (actionInfoSequence[l].Actor != null && actionInfoSequence[l].Actor.IsDead)
							{
								num7 = l;
								break;
							}
						}
						if (EnemyAI.IsLargerThan(num5, num) || (EnemyAI.IsSameValue(num5, num) && num7 < bestFirstDeadActionIndex))
						{
							num = num5;
							bestFirstDeadActionIndex = num7;
							bestPlayPtnWithToken = playPtnWithToken;
						}
					}
				}
			}
		}
		return num;
	}

	public static float CalculateMoveFirstBonusValue(List<AIVirtualActionInfo> moves)
	{
		if (moves == null || !moves.Any())
		{
			return 0f;
		}
		AIVirtualCard actor = moves[0].Actor;
		return actor.SelfField.StyleQuery.GetFirstMoveBonus(actor, moves);
	}

	public static float CalculateOneOnOneBothDeadBonus(List<AIVirtualActionInfo> moves)
	{
		if (moves.Count <= 1 || !(moves[0] is AIVirtualAttackInfo))
		{
			return 0f;
		}
		AIVirtualAttackInfo aIVirtualAttackInfo = moves[0] as AIVirtualAttackInfo;
		if (aIVirtualAttackInfo.Actor.IsDead && aIVirtualAttackInfo.IsBreakTarget)
		{
			return 0.002f;
		}
		return 0f;
	}

	public static List<AIVirtualActionInfo> GetAllMovesForFullSimulation(AIVirtualField field, bool doesUseEvo, SimulationAdditionalActionInfoSet additionalActionInfoSet)
	{
		AIVirtualTargetSelectAction aIVirtualTargetSelectAction = null;
		List<AIVirtualActionInfo> extraActions = null;
		AISinglePlayptnRecord playPtnRecord = null;
		if (additionalActionInfoSet != null)
		{
			aIVirtualTargetSelectAction = additionalActionInfoSet.ForceEvoAction;
			extraActions = additionalActionInfoSet.ExtraActionBaseList;
			playPtnRecord = additionalActionInfoSet.PlayPtnRecord;
		}
		List<AIVirtualActionInfo> list = null;
		if (doesUseEvo)
		{
			list = GetAllVirtualInplayMove(field);
		}
		else
		{
			list = GetAllVirtualAttackMove(field);
			if (aIVirtualTargetSelectAction != null)
			{
				AIVirtualActionInfo aIVirtualActionInfo = CreateActionInfoOnFieldFromSituation(field, aIVirtualTargetSelectAction);
				if (aIVirtualActionInfo != null)
				{
					list.Add(aIVirtualActionInfo);
				}
			}
		}
		List<AIVirtualActionInfo> allExtraActionList = GetAllExtraActionList(field, extraActions, playPtnRecord);
		if (allExtraActionList != null && allExtraActionList.Count > 0)
		{
			list.AddRange(allExtraActionList);
		}
		list.Add(new AIVirtualTurnEndInfo(field.AllyClass));
		return list;
	}

	private static List<AIVirtualActionInfo> GetAllVirtualInplayMove(AIVirtualField field)
	{
		List<AIVirtualActionInfo> list = new List<AIVirtualActionInfo>();
		List<AIVirtualActionInfo> allVirtualEvolMove = GetAllVirtualEvolMove(field);
		List<AIVirtualActionInfo> allVirtualAttackMove = GetAllVirtualAttackMove(field);
		for (int i = 0; i < allVirtualEvolMove.Count; i++)
		{
			list.Add(allVirtualEvolMove[i]);
		}
		for (int j = 0; j < allVirtualAttackMove.Count; j++)
		{
			list.Add(allVirtualAttackMove[j]);
		}
		return list;
	}

	private static List<AIVirtualActionInfo> GetAllVirtualEvolMove(AIVirtualField field)
	{
		List<AIVirtualActionInfo> list = new List<AIVirtualActionInfo>();
		if (field.EvoUsedCard != null)
		{
			return list;
		}
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (!aIVirtualCard.IsDead)
			{
				AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.EVOLVE);
				if (aIVirtualCard.IsAbleEvolution() && aIVirtualCard.CreateAIVirtualSelectInfo(field, aIVirtualTargetSelectAction) == null)
				{
					list.Add(aIVirtualTargetSelectAction);
				}
			}
		}
		return list;
	}

	private static List<AIVirtualActionInfo> GetAllVirtualAttackMove(AIVirtualField field)
	{
		List<AIVirtualActionInfo> list = new List<AIVirtualActionInfo>();
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (aIVirtualCard.IsDead)
			{
				continue;
			}
			for (int j = 0; j < field.EnemyInplayCards.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = field.EnemyInplayCards[j];
				AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(aIVirtualCard, aIVirtualCard2);
				if (!aIVirtualCard2.IsDead && AIAttackSimulationUtility.IsAttackPossible(field, aIVirtualAttackInfo))
				{
					list.Add(aIVirtualAttackInfo);
				}
			}
		}
		for (int k = 0; k < field.AllyInplayCards.Count; k++)
		{
			AIVirtualCard aIVirtualCard3 = field.AllyInplayCards[k];
			if (!aIVirtualCard3.IsDead)
			{
				AIVirtualAttackInfo aIVirtualAttackInfo2 = new AIVirtualAttackInfo(aIVirtualCard3, field.EnemyClass);
				if (!field.EnemyClass.IsDead && AIAttackSimulationUtility.IsAttackPossible(field, aIVirtualAttackInfo2))
				{
					list.Add(aIVirtualAttackInfo2);
				}
			}
		}
		return list;
	}

	private static List<AIVirtualActionInfo> GetAllExtraActionList(AIVirtualField field, List<AIVirtualActionInfo> extraActions, AISinglePlayptnRecord playPtnRecord)
	{
		if (extraActions == null || extraActions.Count <= 0)
		{
			return null;
		}
		List<AIVirtualActionInfo> list = null;
		for (int i = 0; i < extraActions.Count; i++)
		{
			AIVirtualActionInfo aIVirtualActionInfo = extraActions[i];
			if (aIVirtualActionInfo.ActionType != AIOperationType.PLAY || !(CreateActionInfoOnFieldFromSituation(field, aIVirtualActionInfo) is AIVirtualTargetSelectAction aIVirtualTargetSelectAction))
			{
				continue;
			}
			List<AIVirtualTargetSelectInfo> list2 = aIVirtualTargetSelectAction.Actor.CreateAIVirtualSelectInfo(field, aIVirtualTargetSelectAction);
			if (list2 == null || list2.Count <= 0)
			{
				if (AIPlayCardSimulationUtility.IsAbleToPlayCard(aIVirtualTargetSelectAction, field, EnemyAI.EmptyPlayPtn) && field.AllyInplayCards.Count((AIVirtualCard c) => !c.IsDead) < 5)
				{
					list = AIParamQuery.AddElementToList(aIVirtualTargetSelectAction, list);
				}
				continue;
			}
			AISelectedTargetInfoSet preDecidedTargetSet = null;
			if (aIVirtualTargetSelectAction.SelectedTargets.IsAnyTargetExists())
			{
				preDecidedTargetSet = aIVirtualTargetSelectAction.SelectedTargets;
			}
			List<AISelectedTargetInfoSet> allTargetSelectSimulationPattern = AIVirtualTargetSelectSimulator.GetAllTargetSelectSimulationPattern(list2, preDecidedTargetSet, aIVirtualTargetSelectAction, field, playPtnRecord);
			if (allTargetSelectSimulationPattern == null || allTargetSelectSimulationPattern.Count <= 0)
			{
				continue;
			}
			for (int num = 0; num < allTargetSelectSimulationPattern.Count; num++)
			{
				AISelectedTargetInfoSet selectedTargetList = allTargetSelectSimulationPattern[num];
				AIVirtualTargetSelectAction aIVirtualTargetSelectAction2 = new AIVirtualTargetSelectAction(aIVirtualTargetSelectAction.Actor, aIVirtualTargetSelectAction.OriginalCard, AIOperationType.PLAY, selectedTargetList);
				if (AIPlayCardSimulationUtility.IsAbleToPlayCard(aIVirtualTargetSelectAction2, field, EnemyAI.EmptyPlayPtn) && field.AllyInplayCards.Count < 5)
				{
					list = AIParamQuery.AddElementToList(aIVirtualTargetSelectAction2, list);
				}
			}
		}
		return list;
	}

	public static float EvaluateAttackValueAfterAllSkill(AIVirtualField field, AISituationInfo situation, List<AIVirtualCardStatusInfo> allyAttackers, List<AIVirtualCardStatusInfo> enemyRemains, List<int> playPtn)
	{
		for (int i = 0; i < allyAttackers.Count; i++)
		{
			allyAttackers[i].CalculateCardValueWhenDestroyed(playPtn, situation);
		}
		for (int j = 0; j < enemyRemains.Count; j++)
		{
			enemyRemains[j].CalculateCardValueWhenDestroyed(playPtn, situation);
		}
		enemyRemains.OrderBy((AIVirtualCardStatusInfo info) => info.ValueWhenDestroyed);
		float num = 0f;
		AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(null, null);
		for (int num2 = 0; num2 < enemyRemains.Count; num2++)
		{
			AIVirtualCardStatusInfo aIVirtualCardStatusInfo = enemyRemains[num2];
			aIVirtualAttackInfo.SetAttackTarget(aIVirtualCardStatusInfo.BaseCard);
			float num3 = 0f;
			List<AIVirtualCardStatusInfo> usedAttackers = null;
			int num4 = 0;
			List<int> list = null;
			int num5 = (int)Mathf.Pow(2f, allyAttackers.Count);
			for (int num6 = 0; num6 < num5; num6++)
			{
				int num7 = num6;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				float num11 = 0f;
				float num12 = 0f;
				List<AIVirtualCardStatusInfo> list2 = new List<AIVirtualCardStatusInfo>();
				List<int> list3 = new List<int>();
				while (num7 > 0)
				{
					if (num7 % 2 == 1)
					{
						AIVirtualCardStatusInfo aIVirtualCardStatusInfo2 = allyAttackers[num8];
						if (!aIVirtualCardStatusInfo2.BaseCard.IsAttackable(EnemyAI.EmptyPlayPtn))
						{
							num8++;
							num7 /= 2;
							continue;
						}
						aIVirtualAttackInfo.SetActor(aIVirtualCardStatusInfo2.BaseCard);
						list2.Add(aIVirtualCardStatusInfo2);
						num9 += aIVirtualCardStatusInfo.BaseCard.SimulateDamageAmount(aIVirtualCardStatusInfo2.BaseCard.SimulateAttackAmount(aIVirtualCardStatusInfo2.Attack, aIVirtualAttackInfo));
						num10 += aIVirtualCardStatusInfo.BaseCard.SimulateDamageAmount(aIVirtualCardStatusInfo2.BaseCard.SimulateAttackAmount(aIVirtualAttackInfo));
						int num13 = aIVirtualCardStatusInfo2.BaseCard.SimulateDamageAmount(aIVirtualCardStatusInfo.BaseCard.SimulateAttackAmount(aIVirtualCardStatusInfo.Attack, aIVirtualAttackInfo));
						int num14 = aIVirtualCardStatusInfo2.BaseCard.SimulateDamageAmount(aIVirtualCardStatusInfo.BaseCard.SimulateAttackAmount(aIVirtualAttackInfo));
						list3.Add(num13);
						if (num13 >= aIVirtualCardStatusInfo2.Life)
						{
							num11 += aIVirtualCardStatusInfo2.ValueWhenDestroyed;
						}
						int num15 = Mathf.Max(0, aIVirtualCardStatusInfo2.Life - num13);
						int num16 = Mathf.Max(0, aIVirtualCardStatusInfo2.BaseCard.Life - num14);
						num12 += (float)(num15 - num16);
					}
					num8++;
					num7 /= 2;
				}
				float num17 = 0f;
				num17 = ((num10 >= aIVirtualCardStatusInfo.BaseCard.Life) ? 0f : ((num9 >= aIVirtualCardStatusInfo.Life) ? (aIVirtualCardStatusInfo.ValueWhenDestroyed - num11) : 0f));
				num17 += num12;
				if (num3 < num17)
				{
					num3 = num17;
					usedAttackers = list2;
					num4 = num9;
					list = list3;
				}
			}
			if (usedAttackers != null && list != null)
			{
				allyAttackers.RemoveAll((AIVirtualCardStatusInfo card) => usedAttackers.Contains(card));
				num += num3;
				for (int num18 = 0; num18 < usedAttackers.Count; num18++)
				{
					AIVirtualCardStatusInfo aIVirtualCardStatusInfo3 = usedAttackers[num18];
					int num19 = list[num18];
					aIVirtualCardStatusInfo3.ModifyStatus(aIVirtualCardStatusInfo3.Attack, aIVirtualCardStatusInfo3.Life - num19);
				}
				aIVirtualCardStatusInfo.ModifyStatus(aIVirtualCardStatusInfo.Attack, aIVirtualCardStatusInfo.Life - num4);
			}
		}
		return num;
	}

	public static int[] GetPatternGroup(int count)
	{
		return count switch
		{
			5 => PATTERN_GROUP_FOR_5, 
			4 => PATTERN_GROUP_FOR_4, 
			3 => PATTERN_GROUP_FOR_3, 
			2 => PATTERN_GROUP_FOR_2, 
			1 => PATTERN_GROUP_FOR_1, 
			_ => null, 
		};
	}

	public static int Factorial(int n)
	{
		if (n <= 1)
		{
			return 1;
		}
		return n * Factorial(n - 1);
	}

	public static ulong CalculateTargetSequenceHash(int[] permList, AIVirtualField field)
	{
		ulong num = 0uL;
		for (int i = 0; i < permList.Length; i++)
		{
			AIVirtualCard aIVirtualCard = field.EnemyInplayCards[permList[i]];
			num += aIVirtualCard.GetHash() * PRIME_NUMBERS_FOR_ENEMY[i];
		}
		return num;
	}

	public static ulong CalculateAttackSequenceHash(int[] permList, AIVirtualField field)
	{
		ulong num = 0uL;
		for (int i = 0; i < permList.Length; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[permList[i]];
			num += aIVirtualCard.GetHash() * PRIME_NUMBERS_FOR_ALLY[i];
		}
		return num;
	}

	public static ulong[] CalculatePlayptnWithTokenHashs(AIVirtualField field, PlayPtnWithToken[] bestPlayptnWithTokens)
	{
		if (bestPlayptnWithTokens == null)
		{
			return null;
		}
		ulong[] array = new ulong[bestPlayptnWithTokens.Length];
		for (int i = 0; i < bestPlayptnWithTokens.Length; i++)
		{
			PlayPtnWithToken playPtnWithToken = bestPlayptnWithTokens[i];
			if (playPtnWithToken == null)
			{
				array[i] = 0uL;
			}
			else
			{
				array[i] = field.CalculatePlayptnHash(playPtnWithToken.PlayPtn);
			}
		}
		return array;
	}

	public static AIVirtualActionInfo CreateActionInfoOnFieldFromSituation(AIVirtualField fieldTemp, AISituationInfo situation)
	{
		if (situation == null || situation.Actor == null)
		{
			return null;
		}
		switch (situation.ActionType)
		{
		case AIOperationType.ATTACK:
		{
			AIVirtualAttackInfo attackInfo = situation as AIVirtualAttackInfo;
			if (attackInfo != null)
			{
				AIVirtualCard sourceCard = fieldTemp.AllyInplayCards.Find((AIVirtualCard c) => c.IsSameCard(attackInfo.Actor));
				AIVirtualCard target = fieldTemp.CardListSet.EnemyClassAndInplayCards.Find((AIVirtualCard c) => c.IsSameCard(attackInfo.AttackTarget));
				return new AIVirtualAttackInfo(sourceCard, target);
			}
			return null;
		}
		case AIOperationType.EVOLVE:
		{
			AIVirtualCard aIVirtualCard2 = fieldTemp.AllyInplayCards.Find((AIVirtualCard c) => c.IsSameCard(situation.Actor));
			if (aIVirtualCard2.IsEvolution)
			{
				return null;
			}
			aIVirtualCard2.IsUseEvo = true;
			AISelectedTargetInfoSet similarTargetInfoSet = situation.SelectedTargets.GetSimilarTargetInfoSet(fieldTemp);
			return new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard2, AIOperationType.EVOLVE, similarTargetInfoSet);
		}
		case AIOperationType.PLAY:
		{
			AIVirtualCard aIVirtualCard = fieldTemp.AllyHandCards.Find((AIVirtualCard c) => c.IsSameCard(situation.OriginalCard));
			if (aIVirtualCard == null)
			{
				return null;
			}
			return new AIVirtualTargetSelectAction(aIVirtualCard.IsSameCard(situation.Actor) ? aIVirtualCard : new AIVirtualCard(situation.Actor, fieldTemp), aIVirtualCard, AIOperationType.PLAY);
		}
		default:
			return null;
		}
	}
}

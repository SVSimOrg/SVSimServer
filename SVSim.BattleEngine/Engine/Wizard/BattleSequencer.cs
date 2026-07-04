using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class BattleSequencer
{
	public static void ExecSimulation(AIVirtualField field, List<AIVirtualActionInfo> actionInfoSequence, List<AIVirtualCard> enemyTargetSequence, SimulationSetting setting)
	{
		AIVirtualActionInfo aIVirtualActionInfo = actionInfoSequence.FirstOrDefault((AIVirtualActionInfo a) => a.ActionType == AIOperationType.EVOLVE);
		AIVirtualActionInfo aIVirtualActionInfo2 = null;
		while (actionInfoSequence.Any((AIVirtualActionInfo a) => !a.IsAlreadyUsed))
		{
			field.EnemyTokenQueue.Clear();
			int num = -1;
			for (int num2 = 0; num2 < actionInfoSequence.Count; num2++)
			{
				AIVirtualActionInfo aIVirtualActionInfo3 = actionInfoSequence[num2];
				if (!aIVirtualActionInfo3.IsAlreadyUsed)
				{
					aIVirtualActionInfo2 = aIVirtualActionInfo3;
					num = num2;
					break;
				}
			}
			if (aIVirtualActionInfo2 != null)
			{
				if (aIVirtualActionInfo2 == aIVirtualActionInfo)
				{
					aIVirtualActionInfo = null;
				}
				bool flag = AISimulationUtility.ExecuteVirtualAction(aIVirtualActionInfo2, aIVirtualActionInfo, enemyTargetSequence, setting);
				if (num == 0 && !flag && setting.CheckFirstActionFailure)
				{
					return;
				}
			}
			if (field.EnemyClass.IsDead || field.AllyClass.IsDead)
			{
				return;
			}
			List<int> bestPlayPtn = field.BestPlayPtn;
			for (int num3 = 0; num3 < field.AllyInplayCards.Count; num3++)
			{
				AIVirtualCard aIVirtualCard = field.AllyInplayCards[num3];
				if (!aIVirtualCard.IsAttackable(bestPlayPtn))
				{
					continue;
				}
				bool flag2 = false;
				for (int num4 = 0; num4 < actionInfoSequence.Count; num4++)
				{
					if (actionInfoSequence[num4].Actor.CardIndex == aIVirtualCard.CardIndex)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					actionInfoSequence.Insert(num + 1, new AIVirtualAttackInfo(aIVirtualCard, isAttackFollower: true, aIVirtualActionInfo2));
				}
			}
			while (field.EnemyTokenQueue.Count > 0)
			{
				Tuple<AIVirtualCard, AIVirtualCard> tuple = field.EnemyTokenQueue.Dequeue();
				AIVirtualCard first = tuple.first;
				AIVirtualCard second = tuple.second;
				int num5 = enemyTargetSequence.IndexOf(first);
				if (num5 >= 0 && (IsToBeKilled(field) || second.IsGuard))
				{
					enemyTargetSequence.Insert(num5 + 1, second);
					if (first.IsMetamorphosed)
					{
						enemyTargetSequence.Remove(first);
					}
				}
			}
			if (aIVirtualActionInfo2 != null && aIVirtualActionInfo2 is AIVirtualAttackInfo { IsAttackSuccessed: not false } && field.AllyInplayCards.Count((AIVirtualCard card) => !card.IsDead) < 5 && aIVirtualActionInfo2.Actor.TagCollectionContainer.HasTag(AIPlayTagType.PuppetAttack))
			{
				aIVirtualActionInfo2.Actor.TagCollectionContainer.PuppetAttackTags.ExecutePuppetAttack(aIVirtualActionInfo2.Actor, aIVirtualActionInfo2, actionInfoSequence);
			}
		}
		field.CallAfterLeaderAttackSimulation();
	}

	public static AIVirtualCard FindAttackTarget(AIVirtualField field, AIVirtualAttackInfo attack, List<AIVirtualCard> enemyTargetSequence, AIVirtualActionInfo evolutionInfo, bool isHandAllRemovalValid, bool isNoSkipAttack, bool isAttackFollower)
	{
		AIVirtualCard aIVirtualCard = null;
		if (isAttackFollower)
		{
			for (int i = 0; i < enemyTargetSequence.Count; i++)
			{
				AIVirtualCard aIVirtualCard2 = enemyTargetSequence[i];
				attack.SetAttackTarget(aIVirtualCard2);
				if (AIAttackSimulationUtility.IsAttackPossible(field, attack))
				{
					AIVirtualCard nextTarget = ((enemyTargetSequence.Count > i + 1) ? enemyTargetSequence[i + 1] : field.EnemyClass);
					if (isNoSkipAttack || !IsSkipAttack(field, attack, nextTarget, evolutionInfo, isHandAllRemovalValid))
					{
						aIVirtualCard = aIVirtualCard2;
						break;
					}
				}
			}
		}
		if (aIVirtualCard == null)
		{
			attack.SetAttackTarget(field.EnemyClass);
			if (AIAttackSimulationUtility.IsAttackPossible(field, attack))
			{
				aIVirtualCard = field.EnemyClass;
			}
			else
			{
				attack.SetAttackTarget(null);
			}
		}
		else
		{
			attack.SetAttackTarget(aIVirtualCard);
		}
		return aIVirtualCard;
	}

	private static bool IsSkipAttack(AIVirtualField field, AIVirtualAttackInfo situation, AIVirtualCard nextTarget, AIVirtualActionInfo evolutionInfo, bool isHandAllRemovalValid)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (!attackTarget.IsGuard && AISimulationRemovalUtility.WillDieBySkillPrediction(attackTarget, field, field.BestPlayPtn, situation, isHandAllRemovalValid))
		{
			return true;
		}
		if (evolutionInfo != null && !attackTarget.IsGuard && !attackTarget.IsIndependent && !attackTarget.IsIndestructible && field.CheckDestroyByEvoTags(evolutionInfo, attackTarget))
		{
			return true;
		}
		AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(actor, nextTarget);
		if (!attackTarget.IsGuard && !attackTarget.IsIndependent && aIVirtualAttackInfo.WillTargetDestroyByAttackTags(field, field.BestPlayPtn, attackTarget))
		{
			return true;
		}
		if (situation.WillTargetDestroyByAttackTags(field, field.BestPlayPtn, attackTarget))
		{
			return true;
		}
		if (situation.WillAnyOtherAttackerDestroyed(field, field.BestPlayPtn))
		{
			return true;
		}
		return false;
	}

	public static void OnAfterExecuteAttackCommon(AIVirtualField field, AIVirtualTurnEndInfo turnEndSituation, ref List<AIVirtualActionInfo> moves, bool isAttackToLeaderAgain = false)
	{
		if (moves == null || moves.Count <= 0)
		{
			AIConsoleUtility.LogError("OnAfterExecuteAttackCommon error!! moves is empty!!!!!");
			return;
		}
		List<AIVirtualCard> beforeCards = new List<AIVirtualCard>(field.AllyInplayCards);
		AIPlayCardSimulationUtility.ExecuteRestPlayPtnAfterAttack(field, ref moves);
		if (isAttackToLeaderAgain)
		{
			AttackToLeaderByNewInplayCards(field, beforeCards);
		}
		ProcessAllyTurnEndToOpponentTurnStart(field, turnEndSituation);
	}

	public static void ProcessAllyTurnEndToOpponentTurnStart(AIVirtualField field, AIVirtualTurnEndInfo turnEndSituation)
	{
		AIVirtualTurnEndSimulator.TurnEnd(turnEndSituation, field);
		AIVirtualTurnStartInfo situation = new AIVirtualTurnStartInfo(field.EnemyClass);
		AIVirtualTurnStartSimulator.TurnStart(situation, field);
		field.DrawCard(isAlly: false, 1, EnemyAI.EmptyPlayPtn, situation);
	}

	private static void AttackToLeaderByNewInplayCards(AIVirtualField currentField, List<AIVirtualCard> beforeCards)
	{
		if (currentField.AllyInplayCards.Count <= beforeCards.Count)
		{
			return;
		}
		AIVirtualCard enemyClass = currentField.EnemyClass;
		int actionLength = currentField.ActionLength;
		for (int i = beforeCards.Count; i < currentField.AllyInplayCards.Count; i++)
		{
			AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(currentField.AllyInplayCards[i], enemyClass);
			if (AIAttackSimulationUtility.IsAttackPossible(currentField, aIVirtualAttackInfo))
			{
				bool isAttackerUsed = false;
				AIAttackSimulationUtility.SimulateAttackIfValuable(aIVirtualAttackInfo, currentField, useAttackLeaderPreCheck: false, ref isAttackerUsed);
				if (enemyClass.IsDead)
				{
					currentField.ActionLength -= actionLength;
					break;
				}
			}
		}
	}

	private static bool IsToBeKilled(AIVirtualField field)
	{
		int life = field.AllyClass.Life;
		int num = field.GetInplayAttackSumToLeader(isAlly: false);
		if (field.AI.OPPONENT.IsEvolve)
		{
			num += field.EnemyInplayCards.Max((AIVirtualCard card) => card.EvolutionAttack - card.Attack);
		}
		return num - life >= 0;
	}

	public static AIVirtualActionInfo GetFirstActionInfo(List<AIVirtualActionInfo> actionInfoSequence, AIVirtualField field, List<int> playPtn)
	{
		AIVirtualActionInfo aIVirtualActionInfo = null;
		int num = 0;
		while (num < actionInfoSequence.Count)
		{
			AIVirtualActionInfo aIVirtualActionInfo2 = actionInfoSequence[num];
			int nextIndexInFindingFirstAction = GetNextIndexInFindingFirstAction(actionInfoSequence, aIVirtualActionInfo2, num);
			if (aIVirtualActionInfo2.ActionType == AIOperationType.PLAY || aIVirtualActionInfo2.ActionType == AIOperationType.TURNEND)
			{
				aIVirtualActionInfo = aIVirtualActionInfo2;
				break;
			}
			AIVirtualCard actor = aIVirtualActionInfo2.Actor;
			List<AIVirtualCard> list = (actor.IsAlly ? field.AllyInplayCards : field.EnemyInplayCards);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard = list[i];
				if (actor.CardIndex == aIVirtualCard.CardIndex)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				num = nextIndexInFindingFirstAction;
				continue;
			}
			if (aIVirtualActionInfo2.ActionType == AIOperationType.EVOLVE)
			{
				aIVirtualActionInfo = aIVirtualActionInfo2;
				break;
			}
			if (aIVirtualActionInfo2.ActionType == AIOperationType.ATTACK)
			{
				if (!(aIVirtualActionInfo2 is AIVirtualAttackInfo { AttackTarget: var attackTarget } aIVirtualAttackInfo))
				{
					AIConsoleUtility.LogError("GetFirstAction() error!! candidate is not virtualAttackInfo");
					continue;
				}
				if (attackTarget == null)
				{
					num = nextIndexInFindingFirstAction;
					continue;
				}
				if (!aIVirtualAttackInfo.IsAttackSuccessed)
				{
					num = nextIndexInFindingFirstAction;
					continue;
				}
				if (attackTarget.IsBreakLast(playPtn) && !attackTarget.IsGuard)
				{
					if (aIVirtualActionInfo == null)
					{
						aIVirtualActionInfo = aIVirtualActionInfo2;
					}
					num = nextIndexInFindingFirstAction;
					continue;
				}
				List<AIVirtualCard> list2 = (attackTarget.IsAlly ? field.CardListSet.AllyClassAndInplayCards : field.CardListSet.EnemyClassAndInplayCards);
				bool flag2 = false;
				for (int j = 0; j < list2.Count; j++)
				{
					AIVirtualCard aIVirtualCard2 = list2[j];
					if (attackTarget.CardIndex == aIVirtualCard2.CardIndex)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					aIVirtualActionInfo = aIVirtualActionInfo2;
					break;
				}
			}
			num = nextIndexInFindingFirstAction;
		}
		return aIVirtualActionInfo;
	}

	private static int GetNextIndexInFindingFirstAction(List<AIVirtualActionInfo> actionInfoSequence, AIVirtualActionInfo candidate, int currentIndex)
	{
		int result = currentIndex + 1;
		for (int i = currentIndex + 1; i < actionInfoSequence.Count && actionInfoSequence[i].PremiseAction == candidate; i++)
		{
			result = i;
		}
		return result;
	}
}

using System.Collections.Generic;

namespace Wizard;

public static class AIVirtualFieldTagProcessExtension
{
	public static void ExecuteWhenChangeInplayTags(this AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!field.CardListSet.HasWhenChangeInplayHolder)
		{
			return;
		}
		List<AIVirtualCard> whenChangeInplayHolders = field.CardListSet.WhenChangeInplayHolders;
		for (int i = 0; i < whenChangeInplayHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = whenChangeInplayHolders[i];
			if (!aIVirtualCard.IsDead)
			{
				aIVirtualCard.ExecuteWhenChangeInplayTag(field, playPtn, situation);
			}
		}
	}

	public static void UpdateTagInformationFromLatestAction(this AIVirtualField currentField, AIRealActionInformation latestAction, AIVirtualField beforeLatestActionField)
	{
		AIVirtualField aIVirtualField = new AIVirtualField(beforeLatestActionField, isLatestAction: true);
		AISituationInfo aISituationInfo = latestAction.CreateSituationInfo(aIVirtualField);
		if (aISituationInfo != null)
		{
			EnemyAI aI = currentField.AI;
			List<AIVirtualCard> enemyHandCardList = currentField.GetEnemyHandCardList();
			for (int i = 0; i < enemyHandCardList.Count; i++)
			{
				AIVirtualCard aIVirtualCard = enemyHandCardList[i];
				aI.tokenManager.AddTokenFromId(aIVirtualCard.BaseId, aIVirtualCard.IsAlly);
			}
			aISituationInfo.SetLatestActionSimulationParameter();
			SimulateLatestAction(aISituationInfo, aIVirtualField);
			currentField.UpdateFieldParameterFromLatestActionField(aIVirtualField);
			AIGenerateTagUtility.ExecuteGenerateTag(aISituationInfo, currentField, aI.GenerateTagOwnerTable, aI.BattleInfoReceivedData.AttachedInfoReceiveCollection);
		}
	}

	private static AIVirtualCard FindSimilarCardFromSimulationField(AIVirtualCard card, AIVirtualField realField, AIVirtualField simulationField)
	{
		List<AIVirtualCard> list = null;
		int num = -1;
		if (card.IsInHand)
		{
			list = simulationField.AllyHandCards;
			num = realField.AllyHandCards.IndexOf(card);
		}
		else if (card.IsOnField)
		{
			if (card.IsAlly)
			{
				list = simulationField.CardListSet.AllyClassAndInplayCards;
				num = realField.CardListSet.AllyClassAndInplayCards.IndexOf(card);
			}
			else
			{
				list = simulationField.CardListSet.EnemyClassAndInplayCards;
				num = realField.CardListSet.EnemyClassAndInplayCards.IndexOf(card);
			}
		}
		if (list == null || num < 0 || num >= list.Count)
		{
			return null;
		}
		int num2 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (!aIVirtualCard.IsDead)
			{
				if (num2 < num)
				{
					num2++;
				}
				else if (num2 == num && aIVirtualCard.BaseId == card.BaseId)
				{
					return aIVirtualCard;
				}
			}
		}
		return null;
	}

	private static AIVirtualCard FindSimilarCardFromEnemyHand(AIVirtualCard card, AIVirtualField realField, AIVirtualField simulationField)
	{
		List<AIVirtualCard> list = null;
		int num = -1;
		if (card.IsInHand)
		{
			list = simulationField.GetEnemyHandCardList();
			num = realField.GetEnemyHandCardList().IndexOf(card);
		}
		if (list == null || num < 0 || num >= list.Count)
		{
			return null;
		}
		int num2 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (num2 < num && !aIVirtualCard.IsDead)
			{
				num2++;
			}
			else if (num2 == num && aIVirtualCard.BaseId == card.BaseId)
			{
				return aIVirtualCard;
			}
		}
		return null;
	}

	private static void SimulateLatestAction(AISituationInfo latestAction, AIVirtualField field)
	{
		switch (latestAction.ActionType)
		{
		case AIOperationType.PLAY:
		{
			PlaySimulationInfo playInfo = AIPlayCardSimulationUtility.CreatePlaySimulationInfo(latestAction.Actor, latestAction, field);
			AIVirtualPlaySimulator.PlayCard((AIVirtualTargetSelectAction)latestAction, field, playInfo);
			break;
		}
		case AIOperationType.EVOLVE:
			AIVirtualEvolutionSimulator.ManualEvolve((AIVirtualTargetSelectAction)latestAction, field);
			break;
		case AIOperationType.ATTACK:
			AIVirtualAttackSimulator.Attack((AIVirtualAttackInfo)latestAction, field);
			break;
		case AIOperationType.TURNEND:
		{
			AIVirtualTurnEndSimulator.TurnEnd((AIVirtualTurnEndInfo)latestAction, field);
			AIVirtualTurnStartInfo aIVirtualTurnStartInfo = new AIVirtualTurnStartInfo(latestAction.Actor.IsAlly ? field.EnemyClass : field.AllyClass);
			aIVirtualTurnStartInfo.SetLatestActionSimulationParameterFromPreAction(latestAction);
			AIVirtualTurnStartSimulator.TurnStart(aIVirtualTurnStartInfo, field);
			break;
		}
		case AIOperationType.FUSION:
			AIVirtualFusionSimulator.Fusion((AIVirtualTargetSelectAction)latestAction, field);
			break;
		}
	}

	private static void UpdateFieldParameterFromLatestActionField(this AIVirtualField field, AIVirtualField afterLatestActionField)
	{
		field.UpdateFieldInformationBeforeCardUpdating(afterLatestActionField);
		for (int i = 0; i < field.CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard card = field.CardListSet.AllReferableCards[i];
			AIVirtualCard aIVirtualCard = FindSimilarCardFromSimulationField(card, field, afterLatestActionField);
			if (aIVirtualCard != null)
			{
				card.UpdateCardInformation(aIVirtualCard);
			}
		}
		for (int j = 0; j < field.GetEnemyHandCardList().Count; j++)
		{
			AIVirtualCard card2 = field.GetEnemyHandCardList()[j];
			AIVirtualCard aIVirtualCard2 = FindSimilarCardFromEnemyHand(card2, field, afterLatestActionField);
			if (aIVirtualCard2 != null)
			{
				card2.UpdateCardInformation(aIVirtualCard2);
			}
		}
		field.UpdateFieldInformationAfterCardUpdating(afterLatestActionField);
	}

	private static void UpdateFieldInformationBeforeCardUpdating(this AIVirtualField currentField, AIVirtualField otherField)
	{
		currentField.SetParametersFromOtherField(otherField);
	}

	private static void UpdateFieldInformationAfterCardUpdating(this AIVirtualField currentField, AIVirtualField otherField)
	{
		currentField.TagPreprocessContainer = otherField.TagPreprocessContainer.Clone(currentField);
		currentField.DamageModifierCollection = otherField.DamageModifierCollection.Clone(currentField);
		currentField.IsNoInstantAttackRecheck();
	}

	public static void ApplyAttackBonus(this AIVirtualField field, AIVirtualAttackInfo situation)
	{
		if (situation.IsUsePreCheck)
		{
			field.SimulationExtraBonus += situation.PreCheckInformation.AttackBonus;
		}
		else
		{
			field.SimulationExtraBonus += situation.Actor.GetAttackBonus(field.BestPlayPtn, situation);
		}
	}

	public static void ApplyClashBonus(this AIVirtualField field, AIVirtualAttackInfo situation)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (attackTarget.IsUnit)
		{
			field.SimulationExtraBonus += actor.GetClashBonus();
			field.SimulationExtraBonus += attackTarget.GetClashBonus();
		}
	}

	public static void ApplyBuffBonus(this AIVirtualField field, AIVirtualCard buffedCard, List<int> playPtn, AISituationInfo situation)
	{
		field.SimulationExtraBonus += buffedCard.GetBuffBonus(playPtn, situation);
	}

	public static void ExecuteWhenDamageTags(this AIVirtualField field, AIVirtualCard damagedTarget, int defaultDamage, List<int> playPtn, AISituationInfo situation)
	{
		if (damagedTarget.Life <= 0)
		{
			return;
		}
		if (damagedTarget.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenDamaged))
		{
			damagedTarget.TagCollectionContainer.DamagedTags.RegisterPassedConditionTags(damagedTarget, field, playPtn, situation);
		}
		List<AIVirtualCard> otherDamagedTagHolders = field.CardListSet.OtherDamagedTagHolders;
		if (otherDamagedTagHolders != null)
		{
			for (int i = 0; i < otherDamagedTagHolders.Count; i++)
			{
				AIVirtualCard aIVirtualCard = otherDamagedTagHolders[i];
				aIVirtualCard.TagCollectionContainer.OtherDamagedTags.RegisterPassedConditionTags(aIVirtualCard, damagedTarget, defaultDamage, field, playPtn, situation);
			}
		}
	}
}

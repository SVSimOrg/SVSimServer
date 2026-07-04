using System;
using System.Collections.Generic;

namespace Wizard;

public static class AIVirtualTurnStartSimulator
{
	public static void TurnStart(AIVirtualTurnStartInfo situation, AIVirtualField field)
	{
		if (situation.ActionType != AIOperationType.TURNSTART)
		{
			AIConsoleUtility.LogError("AIVirtualTurnStartSimulator:TurnStart() error!! situation is not [TURNSTART] ActionType!!!!!");
			return;
		}
		field.TurnStartFieldProcess(situation);
		List<AIVirtualCard> whenChangeInplayHolders = field.CardListSet.WhenChangeInplayHolders;
		if (whenChangeInplayHolders != null)
		{
			for (int i = 0; i < whenChangeInplayHolders.Count; i++)
			{
				AIVirtualCard aIVirtualCard = whenChangeInplayHolders[i];
				if (!aIVirtualCard.IsDead && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenChangeInplay))
				{
					aIVirtualCard.TagCollectionContainer.ChangeInplayTags.ExecuteWhenTurnStart(aIVirtualCard, field, situation);
				}
			}
		}
		List<AIVirtualCard> turnStartTagHolders = field.CardListSet.TurnStartTagHolders;
		if (turnStartTagHolders != null && turnStartTagHolders.Count > 0)
		{
			for (int j = 0; j < turnStartTagHolders.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = turnStartTagHolders[j];
				if (!aIVirtualCard2.IsDead && aIVirtualCard2.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenTurnStart))
				{
					AIScriptTokenArgType turnStartSide = GetTurnStartSide(aIVirtualCard2, situation);
					aIVirtualCard2.TagCollectionContainer.TurnStartTags.RegisterConditionPassedTagActions(aIVirtualCard2, situation, turnStartSide);
				}
			}
		}
		field.TagPreprocessContainer.SimulateAllTurnStartInfo(situation.Actor.IsAlly, situation);
		situation.ExecuteAllSkillProcess();
	}

	private static AIScriptTokenArgType GetTurnStartSide(AIVirtualCard owner, AIVirtualTurnStartInfo situation)
	{
		if (owner.IsAlly == situation.Actor.IsAlly)
		{
			return AIScriptTokenArgType.ALLY;
		}
		return AIScriptTokenArgType.OPPONENT;
	}

	public static void TurnStartFieldProcess(this AIVirtualField field, AIVirtualTurnStartInfo situation)
	{
		field.ReverseAllCardIsSelfTurn();
		field.PlayedCardContainer.ClearInTurn(situation);
		if (situation.Actor.IsAlly)
		{
			field.AllyTurnCount++;
			field.AllyPpTotal = Math.Min(field.AllyPpTotal + 1, 10);
		}
		else
		{
			field.EnemyTurnCount++;
			field.EnemyPpTotal = Math.Min(field.EnemyPpTotal + 1, 10);
		}
		if (!field.IsLatestActionField)
		{
			return;
		}
		for (int i = 0; i < field.CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.AllReferableCards[i];
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.ActivateCount))
			{
				bool isOwnerTurn = aIVirtualCard.IsAlly == situation.Actor.IsAlly;
				aIVirtualCard.TagCollectionContainer.ActivateCountTags.ResetAllCounter(isOwnerTurn);
			}
		}
	}
}

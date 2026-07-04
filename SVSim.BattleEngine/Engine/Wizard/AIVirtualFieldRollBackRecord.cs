using System.Collections.Generic;

namespace Wizard;

public class AIVirtualFieldRollBackRecord
{
	public class CardParamRecord
	{
		public int Cost;

		public int SpellBoost;

		public bool IsDead;

		public int WhiteRitualCount;

		public List<AIActivateCounter> ActivateCounterList;

		public CardParamRecord(AIVirtualCard card)
		{
			Cost = card.Cost;
			SpellBoost = card.SpellboostCount;
			IsDead = card.IsDead;
			WhiteRitualCount = card.WhiteRitualCount;
			ActivateCounterList = null;
			if (card.TagCollectionContainer.HasTagCollection(TagCollectionType.ActivateCount))
			{
				ActivateCounterList = card.TagCollectionContainer.ActivateCountTags.CreateCloneCounterList();
			}
		}
	}

	public class CemeteryRecord
	{
		public int AllyCemetery;

		public int EnemyCemetery;

		public CemeteryRecord(AIVirtualCemetery cemetery)
		{
			AllyCemetery = cemetery.GetCemeteryCount(isAlly: true);
			EnemyCemetery = cemetery.GetCemeteryCount(isAlly: false);
		}
	}

	public class PlayedCardContainerRecord
	{
		public int AllyTurnPlayedCardCount;

		public int AllyInGamePlayedCardCount;

		public PlayedCardContainerRecord(AIPlayedCardContainer container)
		{
			AllyTurnPlayedCardCount = container.AllyTurnPlayedCardCount;
			AllyInGamePlayedCardCount = container.AllyInGamePlayedCardCount;
		}
	}

	public List<CardParamRecord> CardRecordList;

	public CemeteryRecord Cemetery;

	public bool IsNotInstantAttack;

	public int AllyNecromancedCountInGame;

	public int AllyAddedDeckCountInGame;

	public int AllySummonedCardCount;

	public PlayedCardContainerRecord PlayedCardContainer;

	public AIVirtualFieldRollBackRecord(AIVirtualField field)
	{
		CardRecordList = new List<CardParamRecord>();
		List<AIVirtualCard> allReferableCards = field.CardListSet.AllReferableCards;
		for (int i = 0; i < allReferableCards.Count; i++)
		{
			CardRecordList.Add(new CardParamRecord(allReferableCards[i]));
		}
		Cemetery = new CemeteryRecord(field.VirtualCemetery);
		PlayedCardContainer = new PlayedCardContainerRecord(field.PlayedCardContainer);
		IsNotInstantAttack = field.IsNoInstantAttack;
		AllyNecromancedCountInGame = field.AllyNecromancedCountInGame;
		AllyAddedDeckCountInGame = field.AllyGameAddUpdateDeckCards.Count;
		AllySummonedCardCount = field.SummonedCardContainer.AllyTurnSummonedListCount;
	}
}

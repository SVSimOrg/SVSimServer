using System.Collections.Generic;

namespace Wizard;

public class AISummonedCardContainer
{
	private class SummonedCardListSet
	{
		public List<AIVirtualCard> TurnSummonedCards { get; private set; }

		public List<AIVirtualCard> GameSummonedCards { get; private set; }

		public SummonedCardListSet()
		{
			TurnSummonedCards = new List<AIVirtualCard>();
			GameSummonedCards = new List<AIVirtualCard>();
		}

		private SummonedCardListSet(SummonedCardListSet clone)
		{
			TurnSummonedCards = new List<AIVirtualCard>(clone.TurnSummonedCards);
			GameSummonedCards = new List<AIVirtualCard>(clone.GameSummonedCards);
		}

		public SummonedCardListSet Clone()
		{
			return new SummonedCardListSet(this);
		}

		public void AddSummonedCard(AIVirtualCard card)
		{
			AddTurnSummonedCard(card);
			AddGameSummonedCard(card);
		}

		public void AddTurnSummonedCard(AIVirtualCard card)
		{
			TurnSummonedCards.Add(card);
		}

		private void AddGameSummonedCard(AIVirtualCard card)
		{
			GameSummonedCards.Add(card);
		}

		public void LoadListSet(AIVirtualField field, BattlePlayerBase player)
		{
			BattleManagerBase ins = player.BattleMgr;
			int currentTurn = ins.CurrentTurn;
			bool isSelfTurn = ins.BattlePlayer.IsSelfTurn;
			for (int i = 0; i < player.GameSummonCards.Count; i++)
			{
				BattlePlayerBase.TurnAndCard turnAndCard = player.GameSummonCards[i];
				SummonedVirtualCard card = new SummonedVirtualCard(turnAndCard.Card as BattleCardBase, field);
				AddGameSummonedCard(card);
				if (turnAndCard.Turn == currentTurn && turnAndCard.IsSelfTurn == isSelfTurn)
				{
					AddTurnSummonedCard(card);
				}
			}
		}
	}

	private SummonedCardListSet _allySummonedListSet = new SummonedCardListSet();

	private SummonedCardListSet _enemySummonedListSet = new SummonedCardListSet();

	public int AllyTurnSummonedListCount => _allySummonedListSet.TurnSummonedCards.Count;

	public AISummonedCardContainer()
	{
	}

	private AISummonedCardContainer(AISummonedCardContainer clone)
	{
		_allySummonedListSet = clone._allySummonedListSet.Clone();
		_enemySummonedListSet = clone._enemySummonedListSet.Clone();
	}

	public AISummonedCardContainer Clone()
	{
		return new AISummonedCardContainer(this);
	}

	public void LoadSummonedCardList(AIVirtualField field, BattlePlayerBase ally, BattlePlayerBase enemy)
	{
		_allySummonedListSet.LoadListSet(field, ally);
		_enemySummonedListSet.LoadListSet(field, enemy);
	}

	public List<AIVirtualCard> GetSummonedList(bool isAlly, bool isTurnList)
	{
		SummonedCardListSet summonedCardListSet = (isAlly ? _allySummonedListSet : _enemySummonedListSet);
		if (!isTurnList)
		{
			return summonedCardListSet.GameSummonedCards;
		}
		return summonedCardListSet.TurnSummonedCards;
	}

	public void AddSummonedCard(AIVirtualCard card)
	{
		(card.IsAlly ? _allySummonedListSet : _enemySummonedListSet).AddSummonedCard(card);
	}

	public void RollBackSummonedCard(int beforeTurnSummonedCount)
	{
		int num = _allySummonedListSet.TurnSummonedCards.Count - beforeTurnSummonedCount;
		if (0 < num)
		{
			_allySummonedListSet.TurnSummonedCards.RemoveRange(beforeTurnSummonedCount, num);
			_allySummonedListSet.GameSummonedCards.RemoveRange(beforeTurnSummonedCount, num);
		}
	}
}

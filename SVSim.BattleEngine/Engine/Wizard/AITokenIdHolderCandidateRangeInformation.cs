using System.Collections.Generic;

namespace Wizard;

public class AITokenIdHolderCandidateRangeInformation
{
	private enum CandidateRange
	{
		AllReferableCards,
		DestroyedCards,
		DestroyedInCurrentTurn
	}

	private AIScriptTokenArgType _side;

	private CandidateRange _range;

	public AITokenIdHolderCandidateRangeInformation(List<AIScriptTokenBase> filters)
	{
		_side = AIScriptTokenArgType.ALLY;
		_range = CandidateRange.AllReferableCards;
		InitializeFromFilters(filters);
	}

	private AITokenIdHolderCandidateRangeInformation(AIScriptTokenArgType side, CandidateRange range)
	{
		_side = side;
		_range = range;
	}

	public static AITokenIdHolderCandidateRangeInformation CreateReanimateRangeInformation(AIScriptTokenArgType side)
	{
		return new AITokenIdHolderCandidateRangeInformation(side, CandidateRange.DestroyedCards);
	}

	private void InitializeFromFilters(List<AIScriptTokenBase> filters)
	{
		if (filters == null || filters.Count <= 0)
		{
			return;
		}
		for (int num = filters.Count - 1; num >= 0; num--)
		{
			if (filters[num] is AIScriptArgumentToken aIScriptArgumentToken)
			{
				bool flag = true;
				switch (aIScriptArgumentToken.ArgumentType)
				{
				case AIScriptTokenArgType.ALLY:
				case AIScriptTokenArgType.OPPONENT:
					_side = aIScriptArgumentToken.ArgumentType;
					break;
				case AIScriptTokenArgType.DESTROYED_CARD:
					_range = CandidateRange.DestroyedCards;
					break;
				case AIScriptTokenArgType.DESTROYED_IN_CURRENT_TURN:
					_range = CandidateRange.DestroyedInCurrentTurn;
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					filters.RemoveAt(num);
				}
			}
		}
	}

	public List<AIVirtualCard> GetTokenIdHolderRange(AIVirtualField field)
	{
		return _range switch
		{
			CandidateRange.DestroyedCards => GetDestroyedCards(field), 
			CandidateRange.DestroyedInCurrentTurn => GetDestroyedCardsInCurrentTurn(field), 
			_ => field.CardListSet.AllReferableCards, 
		};
	}

	private List<AIVirtualCard> GetDestroyedCards(AIVirtualField field)
	{
		AIScriptTokenArgType side = _side;
		if (side != AIScriptTokenArgType.ALLY && side == AIScriptTokenArgType.OPPONENT)
		{
			return field.CardListSet.EnemyDestroyedCards;
		}
		return field.CardListSet.AllyDestroyedCards;
	}

	private List<AIVirtualCard> GetDestroyedCardsInCurrentTurn(AIVirtualField field)
	{
		List<AIVirtualCard> destroyedCards = GetDestroyedCards(field);
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < destroyedCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = destroyedCards[i];
			if (aIVirtualCard.DeadTurn)
			{
				list.Add(aIVirtualCard);
			}
		}
		return list;
	}
}

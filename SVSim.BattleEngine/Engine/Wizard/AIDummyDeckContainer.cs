using System.Collections.Generic;

namespace Wizard;

public class AIDummyDeckContainer
{
	private List<DeckVirtualCard> _allyDummyDeck;

	private List<DeckVirtualCard> _enemyDummyDeck;

	public AIDummyDeckContainer()
	{
		_allyDummyDeck = new List<DeckVirtualCard>();
		_enemyDummyDeck = new List<DeckVirtualCard>();
	}

	public AIDummyDeckContainer Clone(AIVirtualField field)
	{
		AIDummyDeckContainer aIDummyDeckContainer = new AIDummyDeckContainer();
		CopyDeck(_allyDummyDeck, aIDummyDeckContainer._allyDummyDeck, field);
		CopyDeck(_enemyDummyDeck, aIDummyDeckContainer._enemyDummyDeck, field);
		return aIDummyDeckContainer;
	}

	private void CopyDeck(List<DeckVirtualCard> srcDeck, List<DeckVirtualCard> dstDeck, AIVirtualField field)
	{
		for (int i = 0; i < srcDeck.Count; i++)
		{
			dstDeck.Add(new DeckVirtualCard(srcDeck[i], field));
		}
	}

	public List<DeckVirtualCard> GetDeck(bool isAlly)
	{
		if (!isAlly)
		{
			return _enemyDummyDeck;
		}
		return _allyDummyDeck;
	}

	public void AppendDummyCard(DeckVirtualCard card, bool isAlly)
	{
		List<DeckVirtualCard> deck = GetDeck(isAlly);
		if (!deck.Contains(card))
		{
			deck.Add(card);
		}
	}
}

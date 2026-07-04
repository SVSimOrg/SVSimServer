using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class PlaySkipIfEvoInformation : PlaySkipInformation
{
	private List<AIVirtualCard> _evolutionPermittedCards;

	public bool HasEvolutionPermittedCards
	{
		get
		{
			if (_evolutionPermittedCards != null)
			{
				return _evolutionPermittedCards.Any();
			}
			return false;
		}
	}

	public PlaySkipIfEvoInformation()
	{
		TagType = AIPlayTagType.PlaySkipIfEvo;
		base.IsEvolutionPermittedTag = true;
		_evolutionPermittedCards = null;
	}

	public override bool IsEvoCardLegal(AIVirtualCard evoCard)
	{
		if (evoCard == null)
		{
			return false;
		}
		if (HasEvolutionPermittedCards)
		{
			return _evolutionPermittedCards.Any((AIVirtualCard c) => c.IsSameCard(evoCard));
		}
		return false;
	}

	public void AddEvolutionPermittedCards(List<AIVirtualCard> cards)
	{
		_evolutionPermittedCards = AIParamQuery.AddRangeToList(cards, _evolutionPermittedCards);
	}
}

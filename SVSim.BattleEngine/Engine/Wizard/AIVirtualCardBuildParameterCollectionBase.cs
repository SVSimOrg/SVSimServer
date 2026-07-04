namespace Wizard;

public abstract class AIVirtualCardBuildParameterCollectionBase
{
	protected int _cardIndex;

	public AIVirtualCardBuildParameterCollectionBase(AIVirtualCard card)
	{
		_cardIndex = card.CardIndex;
	}

	public bool IsMatch(AIVirtualCard card)
	{
		return card.CardIndex == _cardIndex;
	}
}

public class TokenDrawModifier
{
	private int _cardId;

	private int _multiplyCount;

	public int CardId => _cardId;

	public int MultiplyCount => _multiplyCount;

	public TokenDrawModifier(int cardId, int multiplyCount)
	{
		_cardId = cardId;
		_multiplyCount = multiplyCount;
	}

	public bool Equals(TokenDrawModifier modifier)
	{
		if (_cardId == modifier.CardId)
		{
			return _multiplyCount == modifier._multiplyCount;
		}
		return false;
	}
}

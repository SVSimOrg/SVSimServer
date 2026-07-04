using System.Collections.Generic;

namespace Wizard;

public class AIDeckData
{
	private Dictionary<int, AICardData> cardDic;

	public Dictionary<int, AICardData> CardDic => cardDic;

	public AIDeckData()
	{
		cardDic = new Dictionary<int, AICardData>();
	}

	public AICardData SearchCardData(int card_id)
	{
		AICardData value = null;
		if (CardDic.TryGetValue(card_id, out value))
		{
			return value;
		}
		return null;
	}
}

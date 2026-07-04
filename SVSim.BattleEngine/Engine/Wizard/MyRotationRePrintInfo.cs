using System.Collections.Generic;

namespace Wizard;

public class MyRotationRePrintInfo
{
	private List<string> _cardPackId = new List<string>();

	public MyRotationRePrintInfo(int baseCardId, List<CardParameter> allCardParam)
	{
		foreach (CardParameter item in allCardParam)
		{
			if (item.BaseCardId == baseCardId)
			{
				_cardPackId.Add(item.CardSetId);
			}
		}
	}

	public bool IsRePrintCardAvailablePack(string cardPackId)
	{
		return _cardPackId.Contains(cardPackId);
	}
}

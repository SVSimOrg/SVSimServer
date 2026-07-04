using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIReverseEvaluateValueListOrder
{
	private List<float> evalvalues = new List<float>();

	private List<AIVirtualCard> cards = new List<AIVirtualCard>();

	public void AddData(float value, AIVirtualCard card)
	{
		if (card == null)
		{
			AIConsoleUtility.LogError("CompValue:AddData() Target card is null");
		}
		else if (evalvalues.Any())
		{
			bool flag = false;
			for (int i = 0; i < evalvalues.Count; i++)
			{
				if (evalvalues[i] > value)
				{
					evalvalues.Insert(i, value);
					cards.Insert(i, card);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				evalvalues.Add(value);
				cards.Add(card);
			}
		}
		else
		{
			evalvalues.Add(value);
			cards.Add(card);
		}
	}

	public List<AIVirtualCard> GetCardList(int takeCount)
	{
		if (takeCount <= 0 || cards.Count < takeCount)
		{
			return null;
		}
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < takeCount; i++)
		{
			list.Add(cards[i]);
		}
		return list;
	}

	public AIVirtualCard GetFirst()
	{
		if (cards == null || cards.Count <= 0)
		{
			return null;
		}
		return cards[0];
	}
}

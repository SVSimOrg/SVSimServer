using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class SpotCardData
{
	private Dictionary<int, int> _spotCardDict = new Dictionary<int, int>();

	public bool ExistsSpotCard(int cardId)
	{
		return _spotCardDict.ContainsKey(cardId);
	}

	public int GetSpotCardNum(int cardId)
	{
		int value = 0;
		_spotCardDict.TryGetValue(cardId, out value);
		return value;
	}

	public Dictionary<int, int> CreateDictionaryIncludingSpotCard(IDictionary<int, int> srcDict)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(srcDict);
		foreach (KeyValuePair<int, int> item in _spotCardDict)
		{
			int value = 0;
			if (dictionary.TryGetValue(item.Key, out value))
			{
				dictionary[item.Key] = value + item.Value;
			}
			else
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		return dictionary;
	}
}

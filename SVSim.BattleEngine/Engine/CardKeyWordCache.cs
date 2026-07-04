using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wizard;

public class CardKeyWordCache
{
	public enum Option
	{
		None,
		OnlyCardNames,
		OnlyCardNamesHiranaga
	}

	private Dictionary<int, IList<string>> _cache = new Dictionary<int, IList<string>>();

	private Option _option;

	public CardKeyWordCache(Option option = Option.None)
	{
		_option = option;
	}

	public IList<string> Get(CardParameter param, CardKeyWordCommonCache commonCache)
	{
		if (_cache.TryGetValue(param.CardId, out var value))
		{
			return value;
		}
		IList<string> cloneList = commonCache.GetCloneList(param);
		if (_option == Option.OnlyCardNames)
		{
			foreach (string item in new List<string>(cloneList))
			{
				if (Data.Master.BattleKeyWordDic.ContainsKey(item) && commonCache.GetCardListFromKeyWord(item).Count == 0)
				{
					cloneList.Remove(item);
				}
			}
		}
		else if (_option == Option.OnlyCardNamesHiranaga)
		{
			foreach (string item2 in new List<string>(cloneList))
			{
				if (!Data.Master.BattleKeyWordDic.ContainsKey(item2))
				{
					continue;
				}
				if (commonCache.GetCardListFromKeyWord(item2).Count == 0)
				{
					cloneList.Remove(item2);
					continue;
				}
				cloneList.Remove(item2);
				foreach (int item3 in commonCache.GetCardListFromKeyWord(item2))
				{
					CardParameter cardParameterFromId = CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetCardParameterFromId(item3);
					if (Regex.Replace(cardParameterFromId.CardName, "(\\[[a-zA-Z0-9\\/\\-]*(rub\\<[^\\>]*\\>)*\\])", "") == item2)
					{
						cloneList.Add(cardParameterFromId.CardHiragana);
						break;
					}
				}
			}
		}
		_cache[param.CardId] = cloneList;
		return cloneList;
	}
}

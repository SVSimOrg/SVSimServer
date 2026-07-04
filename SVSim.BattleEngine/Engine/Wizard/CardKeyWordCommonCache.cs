using System.Collections.Generic;

namespace Wizard;

public class CardKeyWordCommonCache
{
	private Dictionary<int, IList<string>> _keyWordCache = new Dictionary<int, IList<string>>();

	private Dictionary<string, IList<int>> _keywordCardListCache = new Dictionary<string, IList<int>>();

	public IList<int> GetCardListFromKeyWord(string keyWord)
	{
		if (_keywordCardListCache.TryGetValue(keyWord, out var value))
		{
			return value;
		}
		List<int> cardIdsInDesc = BattleKeywordInfoListMgr.GetCardIdsInDesc(Data.Master.BattleKeyWordDic[keyWord]);
		_keywordCardListCache[keyWord] = cardIdsInDesc;
		return cardIdsInDesc;
	}

	public IList<string> GetCloneList(CardParameter param)
	{
		return new List<string>(Get(param));
	}

	public void CacheKeyWord(CardParameter param)
	{
		Get(param);
	}

	private IList<string> Get(CardParameter param)
	{
		if (_keyWordCache.TryGetValue(param.CardId, out var value))
		{
			return value;
		}
		IList<string> keywords = BattleKeywordInfoListMgr.GetKeywords(param);
		_keyWordCache[param.CardId] = keywords;
		return keywords;
	}
}

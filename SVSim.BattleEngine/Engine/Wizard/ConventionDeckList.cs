using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class ConventionDeckList
{
	public IDictionary<int, DeckData> DeckList { get; private set; }

	public IDictionary<int, int> CardPool { get; private set; }

	public IList<int> DeckIdList { get; private set; }

	public ConventionInfo Conventioninfo { get; private set; }

	public ConventionDeckList()
	{
		DeckList = new Dictionary<int, DeckData>();
		DeckIdList = new List<int>();
	}

	public void Parse(JsonData responseData, ConventionInfo conventionInfo)
	{
		Conventioninfo = conventionInfo;
		if (responseData["data"].Keys.Contains("tournament_card_list"))
		{
			JsonData jsonData = responseData["data"]["tournament_card_list"];
			CardPool = new Dictionary<int, int>(jsonData.Count);
			for (int i = 0; i < jsonData.Count; i++)
			{
				CardPool.Add(jsonData[i].ToInt(), 3);
			}
		}
		ParseDeckListJson(responseData["data"]["user_deck_list"]);
	}

	public ConventionDeckList(JsonData responseData, ConventionInfo conventionInfo)
	{
		Parse(responseData, conventionInfo);
	}

	public void ParseDeckListJson(JsonData userDeck)
	{
		for (int i = 0; i < userDeck.Count; i++)
		{
			JsonData jsonData = userDeck[i];
			int num = jsonData["deck_no"].ToInt();
			if (!DeckIdList.Contains(num))
			{
				DeckIdList.Add(num);
				DeckList.Add(num, new DeckData(Conventioninfo.BattleParameterInstance.DeckFormat, DeckAttributeType.CustomDeck));
			}
			DeckList[num].Initialize(jsonData);
		}
	}
}

using System.Collections.Generic;

namespace Wizard;

public class CardSetNameMgr
{

	private List<CardSetName> _list = new List<CardSetName>();

	private List<CardSetName> _listBasicAndPack;

	public static bool IsPrizeSetId(int setId)
	{
		if (70000 <= setId)
		{
			return setId <= 79999;
		}
		return false;
	}

	public static bool IsBasicSetId(int setId)
	{
		return setId == 10000;
	}

	public static bool IsTokenSetId(int setId)
	{
		return setId == 90000;
	}

	public static bool IsCollaboSetId(int setId)
	{
		if (20001 <= setId)
		{
			return setId <= 24999;
		}
		return false;
	}

	public List<CardSetName> GetListBasicAndPack()
	{
		return _listBasicAndPack;
	}

	public CardSetName Get(string id)
	{
		CardSetName cardSetName = _list.Find((CardSetName x) => x.ID == id);
		if (cardSetName == null)
		{
			return new CardSetName(id, string.Empty, string.Empty);
		}
		return cardSetName;
	}
}

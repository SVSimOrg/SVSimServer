using System;
using System.Collections.Generic;
using Wizard;

public static class CardBasePrm
{
	public enum CharaType
	{
		CLASS = 0,
		NORMAL = 1,
		FIELD = 2,
		CHANT_FIELD = 3,
		SPELL = 4,
		EVOLUTION = 5,
		MAX = 6,
		NONE = 6
	}

	public enum ClanType
	{
		ALL = 0,
		MIN = 1,
		ELF = 1,
		ROYAL = 2,
		WITCH = 3,
		DRAGON = 4,
		NECRO = 5,
		VAMPIRE = 6,
		BISHOP = 7,
		NEMESIS = 8,
		MAX = 9,
		NONE = 10,
		SHADOW = 99
	}

	public enum TribeType
	{
		ALL = 0,
		LORD = 1,
		LEGION = 2,
		WHITE_RITUAL = 3,
		MANARIA = 4,
		ARTIFACT = 5,
		LOOTING = 6,
		MACHINE = 7,
		FOOD = 8,
		LEVIN = 9,
		NATURE = 10,
		BANQUET = 11,
		HERO = 12,
		ARMED = 13,
		CHESS = 14,
		HELLBOUND = 15,
		SCHOOL = 16,
		MAX = 17,
		NONE = 17
	}

	public enum TribeChangeType
	{
		CHANGE,
		ADD
	}

	public class TribeInfo
	{
		public List<TribeType> TribeTypeList { get; private set; }

		public TribeChangeType ChangeType { get; private set; }

		public TribeInfo(List<TribeType> tribe, TribeChangeType type)
		{
			TribeTypeList = tribe;
			ChangeType = type;
		}
	}

	private static List<TribeType> _defaultType;

	public static List<TribeType> DefaultType
	{
		get
		{
			if (_defaultType == null)
			{
				_defaultType = new List<TribeType> { TribeType.ALL };
			}
			return _defaultType;
		}
	}

	public static CharaType ToStrCharaType(string str)
	{
		CharaType result = CharaType.MAX;
		try
		{
			result = (CharaType)Enum.Parse(typeof(CharaType), str);
		}
		catch
		{
		}
		return result;
	}

	public static ClanType ToStrClanType(string str)
	{
		int value = int.Parse(str);
		ClanType result = ClanType.NONE;
		try
		{
			result = (ClanType)Enum.ToObject(typeof(ClanType), value);
		}
		catch
		{
		}
		return result;
	}

	public static bool ClanTypeIsUseable(ClanType type)
	{
		if (type >= ClanType.MIN)
		{
			return type < ClanType.MAX;
		}
		return false;
	}

	public static TribeType ToStrTribeType(string str)
	{
		TribeType result = TribeType.MAX;
		try
		{
			result = (TribeType)Enum.Parse(typeof(TribeType), str);
		}
		catch
		{
		}
		return result;
	}

	public static ClanType GetClanType(string clan)
	{
		using (IEnumerator<string> enumerator = ((IEnumerable<string>)clan.Split('.', '=')).GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case "all":
					return ClanType.ALL;
				case "elf":
					return ClanType.MIN;
				case "royal":
					return ClanType.ROYAL;
				case "witch":
					return ClanType.WITCH;
				case "dragon":
					return ClanType.DRAGON;
				case "necro":
					return ClanType.NECRO;
				case "vampire":
					return ClanType.VAMPIRE;
				case "bishop":
					return ClanType.BISHOP;
				case "nemesis":
					return ClanType.NEMESIS;
				}
			}
		}
		return ClanType.NONE;
	}

	public static List<TribeType> CreateTribeTypeList(string tribe, bool isTribeCheck = false, bool notEqual = false)
	{
		string text = "";
		List<TribeType> list = new List<TribeType>();
		if (isTribeCheck)
		{
			text = (notEqual ? "tribe!=" : "tribe=");
		}
		if (tribe.Contains(text + "all"))
		{
			list.Add(TribeType.ALL);
		}
		if (tribe.Contains(text + "legion"))
		{
			list.Add(TribeType.LEGION);
		}
		if (tribe.Contains(text + "lord"))
		{
			list.Add(TribeType.LORD);
		}
		if (tribe.Contains(text + "white_ritual"))
		{
			list.Add(TribeType.WHITE_RITUAL);
		}
		if (tribe.Contains(text + "manaria"))
		{
			list.Add(TribeType.MANARIA);
		}
		if (tribe.Contains(text + "artifact"))
		{
			list.Add(TribeType.ARTIFACT);
		}
		if (tribe.Contains(text + "looting"))
		{
			list.Add(TribeType.LOOTING);
		}
		if (tribe.Contains(text + "machine"))
		{
			list.Add(TribeType.MACHINE);
		}
		if (tribe.Contains(text + "food"))
		{
			list.Add(TribeType.FOOD);
		}
		if (tribe.Contains(text + "levin"))
		{
			list.Add(TribeType.LEVIN);
		}
		if (tribe.Contains(text + "nature"))
		{
			list.Add(TribeType.NATURE);
		}
		if (tribe.Contains(text + "banquet"))
		{
			list.Add(TribeType.BANQUET);
		}
		if (tribe.Contains(text + "hero"))
		{
			list.Add(TribeType.HERO);
		}
		if (tribe.Contains(text + "armed"))
		{
			list.Add(TribeType.ARMED);
		}
		if (tribe.Contains(text + "chess"))
		{
			list.Add(TribeType.CHESS);
		}
		if (tribe.Contains(text + "hellbound"))
		{
			list.Add(TribeType.HELLBOUND);
		}
		if (tribe.Contains(text + "school"))
		{
			list.Add(TribeType.SCHOOL);
		}
		if (list.Count == 0)
		{
			list.Add(TribeType.MAX);
		}
		return list;
	}

	public static bool IsFollowerCard(CharaType type)
	{
		return type == CharaType.NORMAL;
	}

	public static string GetCardTypeName(CharaType type)
	{
		string result = string.Empty;
		switch (type)
		{
		case CharaType.NORMAL:
		case CharaType.EVOLUTION:
			result = Data.SystemText.Get("Card_0044");
			break;
		case CharaType.SPELL:
			result = Data.SystemText.Get("Card_0045");
			break;
		case CharaType.FIELD:
		case CharaType.CHANT_FIELD:
			result = Data.SystemText.Get("Card_0046");
			break;
		}
		return result;
	}
}

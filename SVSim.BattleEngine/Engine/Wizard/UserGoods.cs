using System;
using System.Collections.Generic;

namespace Wizard;

public class UserGoods
{
	public enum Type
	{
		RedEther = 1,
		Crystal = 2,
		Item = 4,
		Card = 5,
		Sleeve = 6,
		Emblem = 7,
		Degree = 8,
		Rupy = 9,
		Skin = 10,
		SpotCard = 11,
		SpotCardPoint = 12,
		SpotCardOnlyLatestCardPack = 13,
		FreeGachaCount = 14,
		MyPageBG = 15
	}

	private static readonly Dictionary<Type, Func<long, string>> UserGoodsNameFuncTable = new Dictionary<Type, Func<long, string>>
	{
		[Type.RedEther] = (long id) => Data.SystemText.Get("Common_0205"),
		[Type.Crystal] = (long id) => Data.SystemText.Get("Common_0201"),
		[Type.Item] = GetItemName,
		[Type.Card] = GetCardName,
		[Type.Sleeve] = GetSleeveName,
		[Type.Emblem] = GetEmblemName,
		[Type.Degree] = GetDegreeName,
		[Type.Rupy] = (long id) => Data.SystemText.Get("Common_0115"),
		[Type.Skin] = GetSkinName,
		[Type.SpotCard] = GetSpotCardName,
		[Type.SpotCardOnlyLatestCardPack] = GetSpotCardName,
		[Type.SpotCardPoint] = (long id) => Data.SystemText.Get("Common_0161"),
		[Type.MyPageBG] = GetMyPageBGName
	};

	public Type GoodsType { get; private set; }

	public long Id { get; private set; }

	public string Thumbnail => GetUserGoodsImageName(GoodsType, Id);

	public UserGoods(Type type, long userGoodsId)
	{
		GoodsType = type;
		Id = userGoodsId;
	}

	public static string GetUserGoodsImageName(Type userGoodsType, long userGoodsId = 0L)
	{
		switch (userGoodsType)
		{
		case Type.Crystal:
			return "thumbnail_crystal";
		case Type.RedEther:
			return "thumbnail_liquid";
		case Type.Item:
		{
			string result = "";
			Item itemData = Item.GetItemData(userGoodsType, (int)userGoodsId);
			if (itemData != null)
			{
				result = itemData.thumbnail;
			}
			return result;
		}
		case Type.Sleeve:
			return "thumbnail_card";
		case Type.Card:
		case Type.SpotCard:
		case Type.SpotCardOnlyLatestCardPack:
			return "thumbnail_card";
		case Type.Emblem:
			return "thumbnail_emblem";
		case Type.Degree:
			return "thumbnail_title";
		case Type.Rupy:
			return "thumbnail_rupy";
		case Type.Skin:
			return "thumbnail_leader";
		case Type.SpotCardPoint:
			return "thumbnail_spotpoint";
		case Type.MyPageBG:
			return "thumbnail_mypage_custom_bg";
		default:
			return "";
		}
	}

	public string GetUserGoodsIndividualImageName()
	{
		return GoodsType switch
		{
			Type.Sleeve => "thumbnail_sleeve_" + Id, 
			Type.Emblem => "thumbnail_emblem_" + Id, 
			Type.Skin => "thumbnail_leader_" + Id, 
			_ => Thumbnail, 
		};
	}

	public static string getUserGoodsName(Type userGoodsType, long userGoodsId)
	{
		string text = null;
		if (UserGoodsNameFuncTable.TryGetValue(userGoodsType, out var value))
		{
			text = value(userGoodsId);
		}
		if (text == null)
		{
			text = string.Empty;
		}
		return text;
	}

	private static string GetItemName(long id)
	{
		return Item.GetItemData(Type.Item, (int)id)?.name;
	}

	private static string GetMyPageBGName(long id)
	{
		return Data.Master.MyPageCustomBGMaster[id.ToString()].Name;
	}

	private static string GetCardName(long id)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetCardParameterFromId((int)id);
		if (!cardParameterFromId.IsFoil)
		{
			return cardParameterFromId.CardName;
		}
		return cardParameterFromId.CardName + " " + Data.SystemText.Get("Mail_0054");
	}

	private static string GetSleeveName(long id)
	{
		string text = (Data.Master.SleeveMgr.Get(id).IsPremiumSleeve ? Data.SystemText.Get("Mail_0061") : Data.SystemText.Get("Mail_0055"));
		if (!Data.Master.SleeveMgr.IsContainsInMaster(id))
		{
			return Data.SystemText.Get("Common_0203");
		}
		return Data.Master.SleeveMgr.Get(id).sleeve_name + " " + text;
	}

	private static string GetEmblemName(long id)
	{
		if (!Data.Master.EmblemMgr.IsContainsInMaster(id))
		{
			return Data.SystemText.Get("Mail_0036");
		}
		return Data.Master.EmblemMgr.Get(id)._name + " " + Data.SystemText.Get("Mail_0057");
	}

	private static string GetDegreeName(long id)
	{
		if (!Data.Master.DegreeMgr.IsContainsInMaster((int)id))
		{
			return Data.SystemText.Get("Mail_0039");
		}
		return Data.Master.DegreeMgr.Get((int)id)._name + " " + Data.SystemText.Get("Mail_0056");
	}

	private static string GetSkinName(long id)
	{
		ClassCharacterMasterData charaPrmBySkinId = null; // Pre-Phase-5b: no chara master headless
		if (charaPrmBySkinId == null)
		{
			return null;
		}
		return charaPrmBySkinId.chara_name + " " + Data.SystemText.Get("Mail_0058");
	}

	private static string GetSpotCardName(long id)
	{
		return CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetCardParameterFromId((int)id).CardName + " " + Data.SystemText.Get("Mail_0062");
	}
}

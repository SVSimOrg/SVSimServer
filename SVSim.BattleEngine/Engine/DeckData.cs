using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using Wizard;

public class DeckData
{
	public enum UnusableReason
	{
		None,
		MaintenanceCard,
		FormatRestrictCard,
		TooLittleCards,
		TooMuchCards,
		NonPossessionCard,
		ShortageMainClassCards,
		ShortageSubClassCards,
		ShortageBothClassCards,
		Unknown
	}

	private int _deckId;

	private string _deckName;

	private bool _isComplete;

	private int _deckClassId;

	private int _deckSubClassId = 10;

	private long _sleeveId;

	private int _skinId;

	private List<int> _cardIdList;

	public Format Format { get; private set; }

	public bool IsFormatRestrictError { get; set; }

	public bool IsMaintenanceDeck { get; set; }

	public Format DeckCopyFormat { get; private set; }

	public bool IsRecommend { get; private set; }

	public bool IsContainsNonPossessionCard { get; private set; }

	public bool IsSkinRandom { get; set; }

	public List<int> SelectRandomSkinIdList { get; set; }

	public bool IsReplaceDeckSkin { get; set; }

	public DeckAttributeType DeckAttributeType { get; private set; }

	public DateTime? CreatedTime { get; private set; }

	public string MyRotationId { get; set; }

	public string RotationId { get; set; }

	public DeckData(Format format = Format.Max, DeckAttributeType deckAttributeType = DeckAttributeType.Invalid)
	{
		_skinId = 0;
		Format = format;
		DeckCopyFormat = format;
		DeckAttributeType = deckAttributeType;
	}

	public DeckData Clone()
	{
		return (DeckData)MemberwiseClone();
	}

	public void SetDeckID(int deckId)
	{
		_deckId = deckId;
	}

	public void SetDeckName(string deckName)
	{
		_deckName = deckName;
	}

	public void SetDeckIsComplete(bool isComplete)
	{
		_isComplete = isComplete;
	}

	public void SetDeckClassID(int deckClassId)
	{
		_deckClassId = deckClassId;
	}

	public void SetDeckSubClassID(int deckSubClassId)
	{
		_deckSubClassId = deckSubClassId;
	}

	public void SetDeckSleeveID(long sleeveId)
	{
		_sleeveId = DataMgr.GetAbleSleeveId(sleeveId);
	}

	public void SetCardIdList(List<int> cardIdList)
	{
		_cardIdList = cardIdList;
	}

	public void SetEmptyCardIdList()
	{
		_cardIdList = new List<int>();
	}

	public void SetSkinId(int skinId)
	{
		_skinId = skinId;
	}

	public int GetDeckID()
	{
		return _deckId;
	}

	public string GetDeckName()
	{
		return _deckName;
	}

	public bool GetDeckIsComplete()
	{
		return _isComplete;
	}

	private UnusableReason GetUnusableReason()
	{
		if (IsMaintenanceDeck)
		{
			return UnusableReason.MaintenanceCard;
		}
		if (IsFormatRestrictError)
		{
			return UnusableReason.FormatRestrictCard;
		}
		if (!_isComplete)
		{
			int num = ((_cardIdList != null) ? _cardIdList.Count : 0);
			if (num < 40)
			{
				return UnusableReason.TooLittleCards;
			}
			if (num > 40)
			{
				return UnusableReason.TooMuchCards;
			}
			if (Format == Format.Crossover && _cardIdList != null)
			{
				CardMaster cardMaster = CardMaster.GetInstance(FormatBehaviorManager.GetDefaultBehaviour(Format).CardMasterId);
				CardBasePrm.ClanType mainClass = (CardBasePrm.ClanType)_deckClassId;
				CardBasePrm.ClanType subClass = (CardBasePrm.ClanType)_deckSubClassId;
				bool num2 = _cardIdList.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == mainClass) < 24;
				bool flag = _cardIdList.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == subClass) < 9;
				if (num2)
				{
					if (!flag)
					{
						return UnusableReason.ShortageMainClassCards;
					}
					return UnusableReason.ShortageBothClassCards;
				}
				if (flag)
				{
					return UnusableReason.ShortageSubClassCards;
				}
			}
			return UnusableReason.Unknown;
		}
		if (IsContainsNonPossessionCard)
		{
			return UnusableReason.NonPossessionCard;
		}
		return UnusableReason.None;
	}

	public static bool ContainsNonPossessionCard(IEnumerable<int> cardIdList, IFormatBehavior formatBehavior)
	{
		return cardIdList.Distinct().Any((int id) => cardIdList.Count((int i) => i == id) > formatBehavior.GetPossessionCardNum(id, isIncludingSpotCard: true));
	}

	public bool IsUsable(out UnusableReason reason, bool canUseNonPossessionCard = false)
	{
		reason = GetUnusableReason();
		if (canUseNonPossessionCard && reason == UnusableReason.NonPossessionCard)
		{
			reason = UnusableReason.None;
		}
		return reason == UnusableReason.None;
	}

	public bool IsUsable(bool canUseNonPossessionCard = false)
	{
		UnusableReason reason;
		return IsUsable(out reason, canUseNonPossessionCard);
	}

	public int GetDeckClassID()
	{
		return _deckClassId;
	}

	public int GetDeckSubClassID()
	{
		return _deckSubClassId;
	}

	public long GetDeckSleeveID()
	{
		return DataMgr.GetAbleSleeveId(_sleeveId);
	}

	public List<int> GetCardIdList()
	{
		return _cardIdList;
	}

	public void ExtractMainClassAndNeutralCards()
	{
		CardMaster instance = CardMaster.GetInstance(FormatBehaviorManager.GetDefaultBehaviour(Format).CardMasterId);
		CardBasePrm.ClanType deckClassId = (CardBasePrm.ClanType)_deckClassId;
		List<int> list = new List<int>();
		foreach (int cardId in _cardIdList)
		{
			CardBasePrm.ClanType clan = instance.GetCardParameterFromId(cardId).Clan;
			if (clan == deckClassId || clan == CardBasePrm.ClanType.ALL)
			{
				list.Add(cardId);
			}
		}
		_cardIdList = list;
	}

	public bool IsNoCard()
	{
		return _cardIdList == null;
	}

	public bool IsDefaultDeck()
	{
		return IsDeckAttributeMatch(DeckAttributeType.DefaultDeck);
	}

	public bool IsDeckAttributeMatch(DeckAttributeType deckAttributeType)
	{
		return deckAttributeType == DeckAttributeType;
	}

	public int GetRawSkinId()
	{
		return _skinId;
	}

	public int GetSkinId(bool isDefaultSkin = false)
	{
		// Pre-Phase-5b: fell back to the class's default chara prm skin_id when _skinId was
		// zero (or when the default was explicitly requested). Callers are all UI (deck-list
		// display, avatar-dialog, avatar-battle info) so returning 0 headless is safe.
		return _skinId;
	}

	private int GetJsonInt(JsonData deckData, string key, int defaultValue)
	{
		if (deckData.Keys.Contains(key))
		{
			return deckData[key].ToInt();
		}
		return defaultValue;
	}

	private bool GetJsonBool(JsonData deckData, string key, bool defaultValue)
	{
		if (deckData.Keys.Contains(key))
		{
			return deckData[key].ToBoolean();
		}
		return defaultValue;
	}

	public void Initialize(JsonData deckData)
	{
		SetDeckID(GetJsonInt(deckData, "deck_no", 0));
		if (deckData.Keys.Contains("format"))
		{
			Format = Data.ParseApiFormat(deckData["format"].ToInt());
		}
		SetDeckName(deckData["deck_name"].ToString());
		SetDeckIsComplete(GetJsonBool(deckData, "is_complete_deck", defaultValue: true));
		IsContainsNonPossessionCard = GetJsonBool(deckData, "is_include_un_possession_card", defaultValue: false);
		SetDeckClassID(deckData["class_id"].ToInt());
		if (FormatBehaviorManager.GetDefaultBehaviour(Format).UseSubClass)
		{
			int valueOrDefault = deckData.GetValueOrDefault("sub_class_id", 10);
			_deckSubClassId = ((valueOrDefault == 0) ? 10 : valueOrDefault);
		}
		if (deckData.TryGetValue("sleeve_id", out var value))
		{
			SetDeckSleeveID(value.ToLong());
		}
		else
		{
			_sleeveId = 3000011L;
		}
		if (deckData.Keys.Contains("leader_skin_id"))
		{
			SetSkinId(deckData["leader_skin_id"].ToInt());
		}
		if (deckData.Keys.Contains("restricted_card_exists"))
		{
			IsFormatRestrictError = deckData["restricted_card_exists"].ToBoolean();
		}
		if (deckData.Keys.Contains("current_format"))
		{
			DeckCopyFormat = Data.ParseApiFormat(deckData["current_format"].ToInt());
		}
		else
		{
			DeckCopyFormat = Format;
		}
		if (deckData.Keys.Contains("is_recommend"))
		{
			IsRecommend = deckData["is_recommend"].ToInt() == 1;
		}
		else
		{
			IsRecommend = false;
		}
		if (deckData.TryGetValue("create_deck_time", out var value2) && value2 != null)
		{
			CreatedTime = DateTime.Parse($"{value2}");
		}
		ParseCardIdList(deckData);
		MyRotationId = deckData.GetValueOrDefault("rotation_id", null);
		RotationId = MyRotationId;
		if (Data.MyRotationAllInfo.Get(MyRotationId) == null)
		{
			MyRotationId = null;
		}
		IsSkinRandom = deckData.GetValueOrDefault("is_random_leader_skin", 0) == 1;
		SelectRandomSkinIdList = new List<int>();
		if (deckData.Keys.Contains("leader_skin_id_list"))
		{
			JsonData jsonData = deckData["leader_skin_id_list"];
			for (int i = 0; i < jsonData.Count; i++)
			{
				SelectRandomSkinIdList.Add(jsonData[i].ToInt());
			}
			SelectRandomSkinIdList.Sort();
		}
		MaintenanceCardCheack();
	}

	public void ParseCardIdList(JsonData deckData)
	{
		JsonData jsonData = deckData["card_id_array"];
		List<int> cardIdList = null;
		int count = jsonData.Count;
		if (count > 0)
		{
			cardIdList = new List<int>();
			for (int i = 0; i < count; i++)
			{
				cardIdList.Add(jsonData[i].ToInt());
			}
			cardIdList = UIManager.GetInstance().getUIBase_CardManager().SortIDList(cardIdList, FormatBehaviorManager.GetDefaultBehaviour(Format).CardMasterId);
		}
		SetCardIdList(cardIdList);
	}

	public void MaintenanceCardCheack()
	{
		IsMaintenanceDeck = false;
		if (_cardIdList != null)
		{
			// Pre-Phase-5b: probed DataMgr.IsMaintenanceCard for each id; headless has no
			// maintenance card list so no card is ever considered under maintenance.
			IsMaintenanceDeck = false;
		}
	}

	public string GetMyRotationClassName()
	{
		MyRotationInfo info = Data.MyRotationAllInfo.Get(MyRotationId);
		return CreateMyRotationClassName(_deckClassId, info);
	}

	public static string CreateMyRotationClassName(int classType, MyRotationInfo info)
	{
		// Pre-Phase-5b: threaded DataMgr.GetClanNameByKey for the class-name substitution.
		// Headless has no class-name map; substitute the class type as a raw string.
		return Data.SystemText.Get("MyRotation_ID_02", classType.ToString(), info.LastPackText);
	}

	public bool IsVisibleRandomIcon()
	{
		if (IsReplaceDeckSkin)
		{
			return false;
		}
		if (IsSkinRandom)
		{
			return true;
		}
		// Pre-Phase-5b: probed DataMgr.GetClassPrm(...).IsRandomLeaderSkin. Headless has no
		// class prm map so no class is treated as random-skin-eligible.
		return false;
	}
}

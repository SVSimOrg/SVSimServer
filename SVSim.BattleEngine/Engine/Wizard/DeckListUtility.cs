using System.Collections.Generic;
using System.Linq;
using LitJson;

namespace Wizard;

public static class DeckListUtility
{
	private static List<DeckGroup> DeckGroupDataBase => Data.DeckGroupDataBase;

	public static List<DeckGroup> DeckGroupDataBaseClone()
	{
		List<DeckGroup> deckGroups = new List<DeckGroup>();
		DeckGroupDataBase.ForEach(delegate(DeckGroup dg)
		{
			deckGroups.Add(dg.Clone());
		});
		return deckGroups;
	}

	public static List<DeckGroup> ParseDeckInfoResponceData(JsonData jsonData, Format requestFormat)
	{
		if (jsonData.Keys.Contains("user_leader_skin_setting_list"))
		{
			SetLeaderSkinSetting(jsonData["user_leader_skin_setting_list"]);
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (requestFormat == Format.All)
		{
			if (jsonData.Keys.Contains("user_deck_rotation"))
			{
				DeckListUpdate(CreateDeckGroup(jsonData["user_deck_rotation"], Format.Rotation, DeckAttributeType.CustomDeck));
			}
			if (jsonData.Keys.Contains("user_deck_unlimited"))
			{
				DeckListUpdate(CreateDeckGroup(jsonData["user_deck_unlimited"], Format.Unlimited, DeckAttributeType.CustomDeck));
			}
			if (jsonData.Keys.Contains("user_deck_pre_rotation"))
			{
				DeckListUpdate(CreateDeckGroup(jsonData["user_deck_pre_rotation"], Format.PreRotation, DeckAttributeType.CustomDeck));
			}
			if (jsonData.TryGetValue("user_deck_crossover", out var value))
			{
				DeckListUpdate(CreateDeckGroup(value, Format.Crossover, DeckAttributeType.CustomDeck));
			}
			else
			{
				flag = true;
			}
			if (jsonData.TryGetValue("user_deck_my_rotation", out var value2))
			{
				DeckListUpdate(CreateDeckGroup(value2, Format.MyRotation, DeckAttributeType.CustomDeck));
			}
			else
			{
				flag2 = true;
			}
			if (jsonData.TryGetValue("user_deck_avatar", out var value3))
			{
				DeckListUpdate(CreateDeckGroup(value3, Format.Avatar, DeckAttributeType.CustomDeck));
			}
			else
			{
				flag3 = true;
			}
		}
		else if (jsonData.Keys.Contains("user_deck_list"))
		{
			DeckListUpdate(CreateDeckGroup(jsonData["user_deck_list"], requestFormat, DeckAttributeType.CustomDeck));
		}
		List<DeckGroup> list = DeckGroupDataBaseClone();
		if (flag)
		{
			RemoveDeckListGroup(list, Format.Crossover, DeckAttributeType.CustomDeck);
		}
		if (flag2)
		{
			RemoveDeckListGroup(list, Format.MyRotation, DeckAttributeType.CustomDeck);
		}
		if (flag3)
		{
			RemoveDeckListGroup(list, Format.Avatar, DeckAttributeType.CustomDeck);
		}
		if (jsonData.Keys.Contains("trial_deck_list"))
		{
			Format format = ((requestFormat != Format.Rotation) ? Format.Max : Format.Rotation);
			list.Add(CreateDeckGroup(jsonData["trial_deck_list"], format, DeckAttributeType.TrialDeck));
		}
		if (jsonData.TryGetValue("crossover_trial_deck_list", out var value4))
		{
			list.Add(CreateDeckGroup(value4, Format.Crossover, DeckAttributeType.SampleDeck));
		}
		if (jsonData.Keys.Contains("build_deck_list"))
		{
			list.Add(CreateDeckGroup(jsonData["build_deck_list"], Format.Max, DeckAttributeType.BuildDeck));
		}
		if (jsonData.Keys.Contains("default_deck_list"))
		{
			Format format2 = ((requestFormat == Format.All) ? Format.Max : requestFormat);
			list.Add(CreateDeckGroup(jsonData["default_deck_list"], format2, DeckAttributeType.DefaultDeck));
		}
		return list;
	}

	private static void DeckListUpdate(DeckGroup receiveDeckGroup)
	{
		DeckGroup deckGroup = DeckGroupDataBase.FirstOrDefault((DeckGroup d) => d.DeckFormat == receiveDeckGroup.DeckFormat && d.AttributeType == receiveDeckGroup.AttributeType);
		if (receiveDeckGroup.DeckDataList.Count() == 0)
		{
			deckGroup?.MaintenanceCardCheack();
			return;
		}
		if (deckGroup != null)
		{
			DeckGroupDataBase.Remove(deckGroup);
		}
		DeckGroupDataBase.Add(receiveDeckGroup);
	}

	private static void RemoveDeckListGroup(List<DeckGroup> deckGroups, Format format, DeckAttributeType attributeType)
	{
		DeckGroup deckGroup = deckGroups.FirstOrDefault((DeckGroup d) => d.DeckFormat == format && d.AttributeType == attributeType);
		if (deckGroup != null)
		{
			deckGroups.Remove(deckGroup);
		}
	}

	public static DeckGroup CreateDeckGroup(JsonData deckListJson, Format format, DeckAttributeType deckAttributeType)
	{
		return new DeckGroup(ParseDeckListJson(deckListJson, format, deckAttributeType), format, deckAttributeType);
	}

	public static List<DeckData> ParseDeckListJson(JsonData responseData, Format format, DeckAttributeType deckAttributeType)
	{
		List<DeckData> list = new List<DeckData>();
		for (int i = 0; i < responseData.Count; i++)
		{
			JsonData deckData = responseData[i];
			DeckData deckData2 = new DeckData(format, deckAttributeType);
			deckData2.Initialize(deckData);
			list.Add(deckData2);
		}
		return list;
	}

	public static void DeckUpdate(JsonData jsonData, Format format, DeckAttributeType deckAttributeType)
	{
		DeckData deckData = new DeckData(format, deckAttributeType);
		deckData.Initialize(jsonData);
		DeckGroup deckGroup = DeckGroupDataBase.FirstOrDefault((DeckGroup d) => d.DeckFormat == format && d.AttributeType == deckAttributeType);
		DeckData deckData2 = deckGroup?.DeckDataList.FirstOrDefault((DeckData d) => d.GetDeckID() == deckData.GetDeckID());
		if (deckData2 == null)
		{
			Debug.LogError("更新対象デッキがありません");
			return;
		}
		deckGroup.DeckDataList.Insert(deckGroup.DeckDataList.IndexOf(deckData2), deckData);
		deckGroup.DeckDataList.Remove(deckData2);
	}

	public static void DataMgrSaveLastSelectDeckData(DeckData deckData)
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		dataMgr.SetSelectDeckId(deckData.GetDeckID());
		dataMgr.SetPlayerCharaIdBySkinId(deckData.GetSkinId());
		dataMgr.SetCurrentDeckData(deckData.GetCardIdList());
		dataMgr.SetPlayerSubClassID(deckData.GetDeckSubClassID());
		dataMgr.SetPlayerMyRotationInfo(deckData.MyRotationId);
		dataMgr.SetPlayerSleeveId(deckData.GetDeckSleeveID());
		dataMgr.LastSelectDeckAttributeType = deckData.DeckAttributeType;
		dataMgr.SetSelectDeckFormat(deckData.Format);
	}

	public static void SaveLastSelectDeck(int deckId, bool isDefaultDeck, bool isTrialDeck, Format format)
	{
		if (FormatBehaviorManager.GetDefaultBehaviour(format).IsSavableLastSelectDeck)
		{
			switch (format)
			{
			case Format.Rotation:
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_SELECT_IS_DEFDECK_ROTATION, isDefaultDeck);
				PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_ROTATION, deckId);
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_BATTLE_IS_TRIALDECK, isTrialDeck);
				break;
			case Format.Unlimited:
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_SELECT_IS_DEFDECK_UNLIMITED, isDefaultDeck);
				PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_UNLIMITED, deckId);
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_BATTLE_IS_TRIALDECK, isTrialDeck);
				break;
			case Format.PreRotation:
				PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_PRE_ROTATION, deckId);
				break;
			case Format.Crossover:
				PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_CROSSOVER, deckId);
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_BATTLE_IS_TRIALDECK, isTrialDeck);
				break;
			case Format.MyRotation:
				PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_MY_ROTATION, deckId);
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_BATTLE_IS_TRIALDECK, isTrialDeck);
				break;
			case Format.Avatar:
				PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_AVATAR, deckId);
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_BATTLE_IS_TRIALDECK, isTrialDeck);
				break;
			}
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_FORMAT, (int)format);
		}
	}

	public static void ClearLastSelectDeck(Format format)
	{
		switch (format)
		{
		case Format.Rotation:
			PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_SELECT_IS_DEFDECK_ROTATION, flag: false);
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_ROTATION, -1);
			break;
		case Format.Unlimited:
			PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.LAST_SELECT_IS_DEFDECK_UNLIMITED, flag: false);
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_UNLIMITED, -1);
			break;
		case Format.Crossover:
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_CROSSOVER, -1);
			break;
		case Format.MyRotation:
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_MY_ROTATION, -1);
			break;
		case Format.Avatar:
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_SELECT_DECK_ID_AVATAR, -1);
			break;
		}
	}

	private static void SetLeaderSkinSetting(JsonData jsonData)
	{
		for (int i = 0; i < jsonData.Count; i++)
		{
			DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
			int classId = jsonData[i]["class_id"].ToInt();
			ClassCharaPrm classPrm = dataMgr.GetClassPrm(classId);
			classPrm.IsRandomLeaderSkin = jsonData[i]["is_random_leader_skin"].ToBoolean();
			int skinId = jsonData[i]["leader_skin_id"].ToInt();
			ClassCharacterMasterData charaPrmBySkinId = dataMgr.GetCharaPrmBySkinId(skinId);
			classPrm.SetCurrentCharaId(charaPrmBySkinId.chara_id);
		}
	}
}

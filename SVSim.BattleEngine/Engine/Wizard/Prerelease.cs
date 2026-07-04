using System;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class Prerelease
{
	public enum eStatus
	{
		NONE,
		PRE_ROTATION,
		DISPLAY_DECK_ONLY
	}

	public static Prerelease Instance { get; private set; }

	public static eStatus Status { get; set; }

	public DateTime EndTime { get; private set; }

	public DateTime DisplayEndTime { get; private set; }

	public List<int> RotationCardSetList { get; private set; }

	public List<int> ReprintedBaseCardIds { get; private set; }

	public List<int> LatestReprintedBaseCardIds { get; private set; }

	public int NextCardSetId { get; private set; }

	public CardMaster.CardMasterId CardMasterId { get; private set; }

	public bool IsEnableFreeMatch { get; private set; }

	public static void Clear()
	{
		Instance = null;
	}

	private Prerelease(JsonData json)
	{
		ParseJsonData(json);
	}

	private void ParseJsonData(JsonData data)
	{
		EndTime = DateTime.Parse(data["end_time"].ToString());
		DisplayEndTime = DateTime.Parse(data["display_end_time"].ToString());
		RotationCardSetList = new List<int>();
		JsonData jsonData = data["rotation_card_set_id_list"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			RotationCardSetList.Add(jsonData[i].ToInt());
		}
		ReprintedBaseCardIds = new List<int>();
		JsonData jsonData2 = data["reprinted_base_card_ids"];
		for (int j = 0; j < jsonData2.Count; j++)
		{
			ReprintedBaseCardIds.Add(jsonData2[j].ToInt());
		}
		LatestReprintedBaseCardIds = new List<int>();
		JsonData jsonData3 = data["latest_reprinted_base_card_ids"];
		for (int k = 0; k < jsonData3.Count; k++)
		{
			LatestReprintedBaseCardIds.Add(jsonData3[k].ToInt());
		}
		NextCardSetId = data["next_card_set_id"].ToInt();
		CardMasterId = ParseCardMasterId(data["card_master_id"].ToString());
		Status = (eStatus)data["pre_release_status"].ToInt();
		IsEnableFreeMatch = data["is_pre_rotation_free_match_term"].ToInt() == 1;
	}

	private CardMaster.CardMasterId ParseCardMasterId(JsonData cardMasterIdJson)
	{
		Enum.TryParse<CardMaster.CardMasterId>(cardMasterIdJson.ToString(), out var result);
		return result;
	}
}

using System;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class CampaignBattleWin
{
	public enum eBoxGrade
	{
		None	}

	public enum eBoxStatus
	{
	}

	public bool IsInSessionCampaign { get; private set; }

	public bool IsHaveSpecialWinReward { get; private set; }

	public int EndUnixTime { get; private set; }

	public int MaxBoxNum { get; private set; }

	public int GetBoxNum { get; private set; }

	public eBoxGrade BoxGrade { get; private set; }

	public eBoxStatus BoxStatus { get; private set; }

	public List<CampaignRewardInfo> RewardList { get; private set; }

	public SpecialTreasureInfo SpecialTreasureInfo { get; private set; }

	public void Parse(JsonData json)
	{
		IsInSessionCampaign = true;
		GetBoxNum = json["daily_box_got_num"].ToInt();
		MaxBoxNum = json["max_daily_box_got_num"].ToInt();
		BoxGrade = (eBoxGrade)json["grade_id"].ToInt();
		BoxStatus = (eBoxStatus)json["daily_rare_box_state"].ToInt();
		EndUnixTime = (int)ConvertTime.DateTimeToUnixTime(DateTime.Parse(json["end_time"].ToString()));
		RewardList = new List<CampaignRewardInfo>();
		JsonData jsonData = json["limited_reward_info"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			RewardList.Add(new CampaignRewardInfo(jsonData[i]));
		}
		if (json.Keys.Contains("special_treasure_info"))
		{
			IsHaveSpecialWinReward = true;
			SpecialTreasureInfo = new SpecialTreasureInfo(json["special_treasure_info"]);
		}
	}
}

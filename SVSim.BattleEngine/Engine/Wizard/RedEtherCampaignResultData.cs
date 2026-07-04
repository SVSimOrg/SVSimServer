using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class RedEtherCampaignResultData
{
	public class ClearMissionInfo
	{
		public string MissionText { get; private set; }

		public int RedEther { get; private set; }

		public ClearMissionInfo(JsonData json)
		{
			MissionText = json["mission_name"].ToString();
			RedEther = json["reward_number"].ToInt();
		}

		public ClearMissionInfo(string text, int ether)
		{
			MissionText = text;
			RedEther = ether;
		}
	}

	public int BeforeDailyEther { get; private set; }

	public int AfterDailyEther { get; private set; }

	public int MaxDailyEther { get; private set; }

	public int CanGainBattleWin { get; private set; }

	public int BattleRewardEther { get; private set; }

	public List<ClearMissionInfo> ClearMissionList { get; private set; } = new List<ClearMissionInfo>();

	public List<ReceivedReward> RewardList { get; private set; } = new List<ReceivedReward>();

	public RedEtherCampaignResultData(JsonData json)
	{
		BeforeDailyEther = json["before_red_ether"].ToInt();
		AfterDailyEther = json["after_red_ethe"].ToInt();
		MaxDailyEther = json["max_gain_red_ethe"].ToInt();
		CanGainBattleWin = json["can_gain_times_by_battle"].ToInt();
		BattleRewardEther = json["current_gain_red_ether_by_battle"].ToInt();
		JsonData jsonData = json["achieved_mission_info_list"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			ClearMissionInfo clearMissionInfo = new ClearMissionInfo(jsonData[i]);
			ClearMissionList.Add(clearMissionInfo);
			RewardList.Add(new ReceivedReward(1, 0L, clearMissionInfo.RedEther));
		}
		if (BattleRewardEther > 0)
		{
			RewardList.Add(new ReceivedReward(1, 0L, BattleRewardEther));
		}
	}
}

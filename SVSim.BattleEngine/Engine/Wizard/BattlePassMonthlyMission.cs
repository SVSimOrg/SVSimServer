using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class BattlePassMonthlyMission
{
	public class MissionDetail
	{
		public class RewardInfo
		{
			public UserGoods UserGoods { get; private set; }

			public int Number { get; private set; }

			public RewardInfo(UserGoods.Type rewardType, long rewardDetailId, int rewardNumber)
			{
				UserGoods = new UserGoods(rewardType, rewardDetailId);
				Number = rewardNumber;
			}
		}

		public string Name { get; private set; }

		public bool IsCleared { get; private set; }

		public int BattlePassPoint { get; private set; }

		public int RequireNumber { get; private set; }

		public int DoneNumber { get; private set; }

		public RewardInfo Reward { get; private set; }

		public MissionDetail(JsonData jsonMissionData)
		{
			Name = jsonMissionData["name"].ToString();
			IsCleared = jsonMissionData["is_cleared"].ToBoolean();
			BattlePassPoint = jsonMissionData["battle_pass_point"].ToInt();
			RequireNumber = jsonMissionData["require_number"].ToInt();
			DoneNumber = jsonMissionData["done_number"].ToInt();
			if (jsonMissionData.Keys.Contains("reward_info"))
			{
				JsonData jsonData = jsonMissionData["reward_info"];
				int rewardType = jsonData["reward_type"].ToInt();
				long rewardDetailId = jsonData["reward_detail_id"].ToLong();
				int rewardNumber = jsonData["reward_number"].ToInt();
				Reward = new RewardInfo((UserGoods.Type)rewardType, rewardDetailId, rewardNumber);
			}
			else
			{
				Reward = null;
			}
		}
	}

	public DateTime StartDate { get; private set; }

	public string EndDate { get; private set; }

	public List<MissionDetail> MissionList { get; private set; }

	public BattlePassMonthlyMission(JsonData jsonBattlePassMonthlyMission)
	{
		StartDate = DateTime.Parse(jsonBattlePassMonthlyMission["start_date"].ToString());
		EndDate = jsonBattlePassMonthlyMission["end_date"].ToString();
		JsonData jsonData = jsonBattlePassMonthlyMission["mission_list"];
		MissionList = new List<MissionDetail>();
		foreach (JsonData item in (IEnumerable)jsonData)
		{
			MissionList.Add(new MissionDetail(item));
		}
	}
}

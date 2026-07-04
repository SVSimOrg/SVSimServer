using System.Collections.Generic;
using LitJson;
using Wizard;

public class MissionInfoDetail
{
	public enum eMissionReceiveType
	{
		normal,
		solo
	}

	public List<UserMission> user_mission_list;

	public List<UserAchievement> user_achievement_list;

	public List<ReceivedReward> total_reward_list;

	public bool _isChangeMission;

	public long _canChangeMissionTime;

	public BattlePassMonthlyMission BattlePassMonthlyMissionData { get; private set; }

	public eMissionReceiveType _missionReceiveType { get; private set; }

	public bool CanChangeReceiveType { get; private set; }

	public long WaitTimeCanChangeReceiveType { get; private set; }

	public MissionInfoDetail()
	{
		user_mission_list = new List<UserMission>();
		user_achievement_list = new List<UserAchievement>();
		total_reward_list = new List<ReceivedReward>();
	}

	public MissionInfoDetail(JsonData data)
		: this()
	{
		_isChangeMission = data["is_change_mission"].ToBoolean();
		if (!_isChangeMission)
		{
			_canChangeMissionTime = data["can_change_mission_time"].ToLong();
		}
		ReadMissionList(data["user_mission_list"]);
		ReadAchievementList(data["user_achievement_list"]);
		ReadBattlePassMonthlyMission(data);
		switch (data["mission_receive_type"].ToInt())
		{
		case 0:
			_missionReceiveType = eMissionReceiveType.normal;
			break;
		case 1:
			_missionReceiveType = eMissionReceiveType.solo;
			break;
		}
		CanChangeReceiveType = data["is_change_receive_type"].ToBoolean();
		if (!CanChangeReceiveType)
		{
			WaitTimeCanChangeReceiveType = data["can_change_receive_type_time"].ToLong();
		}
		if (data.Keys.Contains("total_receive_count_list"))
		{
			ReadRewardList(data["total_receive_count_list"]);
		}
	}

	private void ReadMissionList(JsonData userMissionList)
	{
		for (int i = 0; i < userMissionList.Count; i++)
		{
			JsonData jsonData = userMissionList[i];
			UserMission userMission = new UserMission();
			userMission.id = (int)jsonData["id"];
			userMission.mission_id = (int)jsonData["mission_id"];
			userMission.mission_status = (int)jsonData["mission_status"];
			userMission.total_count = (int)jsonData["total_count"];
			userMission.mission_name = (string)jsonData["mission_name"];
			userMission.display_order = (int)jsonData["display_order"];
			userMission.require_number = (int)jsonData["require_number"];
			userMission.reward_type = (int)jsonData["reward_type"];
			userMission.RewardUserGoodsId = jsonData["reward_detail_id"].ToLong();
			userMission.reward_number = (int)jsonData["reward_number"];
			userMission.start_time = jsonData["start_time"].ToLong();
			userMission.default_flag = jsonData["default_flag"].ToBoolean();
			userMission.lot_type = jsonData["lot_type"].ToInt();
			user_mission_list.Add(userMission);
			if (jsonData.Keys.Contains("end_time"))
			{
				userMission.end_time = jsonData["end_time"].ToLong();
			}
		}
	}

	private void ReadAchievementList(JsonData userAchievementList)
	{
		for (int i = 0; i < userAchievementList.Count; i++)
		{
			JsonData jsonData = userAchievementList[i];
			UserAchievement userAchievement = new UserAchievement();
			userAchievement.achievement_type = jsonData["achievement_type"].ToInt();
			userAchievement.achievement_status = jsonData["achievement_status"].ToInt();
			userAchievement.level = jsonData["level"].ToInt();
			userAchievement._maxLevel = jsonData["max_level"].ToInt();
			userAchievement.total_count = jsonData["total_count"].ToInt();
			userAchievement.achievement_name = jsonData["achievement_name"].ToString();
			userAchievement.require_number = jsonData["require_number"].ToInt();
			userAchievement.reward_type = jsonData["reward_type"].ToInt();
			userAchievement.RewardUserGoodsId = jsonData["reward_detail_id"].ToLong();
			userAchievement.reward_number = jsonData["reward_number"].ToInt();
			user_achievement_list.Add(userAchievement);
		}
	}

	private void ReadBattlePassMonthlyMission(JsonData jsonData)
	{
		BattlePassMonthlyMissionData = null;
		if (jsonData.Keys.Contains("battle_pass_monthly_mission"))
		{
			BattlePassMonthlyMissionData = new BattlePassMonthlyMission(jsonData["battle_pass_monthly_mission"]);
		}
	}

	private void ReadRewardList(JsonData rewardList)
	{
		if (rewardList != null)
		{
			for (int i = 0; i < rewardList.Count; i++)
			{
				ReceivedReward item = new ReceivedReward(rewardList[i]);
				total_reward_list.Add(item);
			}
		}
	}
}

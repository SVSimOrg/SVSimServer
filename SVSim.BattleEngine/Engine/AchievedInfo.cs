using System.Collections.Generic;
using LitJson;
using Wizard;
using Wizard.Lottery;

public class AchievedInfo
{

	public List<UserMission> _missions;

	public List<UserAchievement> _achievements;

	public List<ReceivedReward> _rewards;

	public List<ReceivedReward> _victoryRewards;

	public LotteryApplyData _lotteryData = LotteryApplyData.EmptyData();

	public AchievedInfo()
	{
		_missions = new List<UserMission>();
		_achievements = new List<UserAchievement>();
		_rewards = new List<ReceivedReward>();
		_victoryRewards = new List<ReceivedReward>();
	}

	public AchievedInfo(JsonData data)
		: this()
	{
		Read(data);
	}

	public void Read(JsonData data)
	{
		if (data.Count == 0)
		{
			return;
		}
		if (data.Keys.Contains("achieved_mission_list"))
		{
			JsonData jsonData = data["achieved_mission_list"];
			if (jsonData != null)
			{
				for (int i = 0; i < jsonData.Count; i++)
				{
					_missions.Add(UserMission.CreateAchievedMission(jsonData[i]));
				}
			}
		}
		if (data.Keys.Contains("achieved_achievement_list"))
		{
			JsonData jsonData2 = data["achieved_achievement_list"];
			if (jsonData2 != null)
			{
				for (int j = 0; j < jsonData2.Count; j++)
				{
					UserAchievement userAchievement = UserAchievement.CreateCompletedAchievement(jsonData2[j]);
					if (!string.IsNullOrEmpty(userAchievement.OsId))
					{
						AchievementImpl.instance.ReleaseAchievement(userAchievement.OsId);
					}
					_achievements.Add(userAchievement);
				}
			}
		}
		if (data.Keys.Contains("grand_master_reward_list"))
		{
			JsonData jsonData3 = data["grand_master_reward_list"];
			if (jsonData3 != null)
			{
				for (int k = 0; k < jsonData3.Count; k++)
				{
					_rewards.Add(ReceivedReward.CreateFromBattleResultGrandMaster(jsonData3[k]));
				}
			}
		}
		if (data.Keys.Contains("achieved_mission_reward_list"))
		{
			JsonData jsonData4 = data["achieved_mission_reward_list"];
			if (jsonData4 != null)
			{
				for (int l = 0; l < jsonData4.Count; l++)
				{
					_rewards.Add(ReceivedReward.CreateFromBattleResult(jsonData4[l]));
				}
			}
		}
		if (data.Keys.Contains("win_reward_list"))
		{
			JsonData jsonData5 = data["win_reward_list"];
			if (jsonData5 != null)
			{
				for (int m = 0; m < jsonData5.Count; m++)
				{
					_victoryRewards.Add(ReceivedReward.CreateVictoryReward(jsonData5[m]));
				}
			}
		}
		if (data.Keys.Contains("achieved_beginner_mission_reward_list"))
		{
			JsonData jsonData6 = data["achieved_beginner_mission_reward_list"];
			if (jsonData6 != null)
			{
				for (int n = 0; n < jsonData6.Count; n++)
				{
					_rewards.Add(ReceivedReward.CreateFromBeginnerMissionReward(jsonData6[n]));
				}
			}
		}
		if (data.Keys.Contains("achieved_beginner_mission_list"))
		{
			JsonData jsonData7 = data["achieved_beginner_mission_list"];
			if (jsonData7 != null)
			{
				for (int num = 0; num < jsonData7.Count; num++)
				{
					_missions.Add(UserMission.CreateAchievedMission(jsonData7[num]));
				}
			}
		}
		if (data.Keys.Contains("battle_pass_reward_list"))
		{
			JsonData jsonData8 = data["battle_pass_reward_list"];
			if (jsonData8 != null)
			{
				for (int num2 = 0; num2 < jsonData8.Count; num2++)
				{
					_rewards.Add(ReceivedReward.CreateFromBattlePassReward(jsonData8[num2]));
				}
			}
		}
		if (data.Keys.Contains("battle_pass_message_list"))
		{
			JsonData jsonData9 = data["battle_pass_message_list"];
			if (jsonData9 != null)
			{
				for (int num3 = 0; num3 < jsonData9.Count; num3++)
				{
					_missions.Add(UserMission.CreateAchievedMission(jsonData9[num3]));
				}
			}
		}
		_lotteryData = LotteryApplyData.Parse(data);
	}
}

namespace Wizard;

public class AchievementReceiveRewardTask : BaseTask
{
	public class AchievementReceiveRewardTaskParam : BaseParam
	{
		public int achievement_type;

		public int level;
	}

	public AchievementReceiveRewardTask()
	{
		base.type = ApiType.Type.AchievementReceiveReward;
	}

	public void SetParameter(int achievement_type, int level)
	{
		AchievementReceiveRewardTaskParam achievementReceiveRewardTaskParam = new AchievementReceiveRewardTaskParam();
		achievementReceiveRewardTaskParam.achievement_type = achievement_type;
		achievementReceiveRewardTaskParam.level = level;
		base.Params = achievementReceiveRewardTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		Data.MissionInfo.data = new MissionInfoDetail(base.ResponseData["data"]);
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}
}

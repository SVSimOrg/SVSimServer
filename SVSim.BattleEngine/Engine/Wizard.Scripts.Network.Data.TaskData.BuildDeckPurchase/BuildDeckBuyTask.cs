using System.Collections.Generic;
using LitJson;
using Wizard.Lottery;

namespace Wizard.Scripts.Network.Data.TaskData.BuildDeckPurchase;

public class BuildDeckBuyTask : BaseTask
{
	public class BuildDeckBuySingleTaskParam : BaseParam
	{
		public int product_id;
	}

	public List<ShopCommonRewardInfo> _seriesRewardList = new List<ShopCommonRewardInfo>();

	public List<ReceivedReward> LotteryRewardList = new List<ReceivedReward>();

	public List<ReceivedReward> MissionRewardList = new List<ReceivedReward>();

	public LotteryApplyData LotteryData { get; private set; }

	public List<NotificatonAnimation.Param> NotificatonAnimationParams { get; set; }

	public BuildDeckBuyTask()
	{
		base.type = ApiType.Type.BuildDeckBuy;
		LotteryData = LotteryApplyData.EmptyData();
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		JsonData jsonData = base.ResponseData["data"]["series_rewards"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			ShopCommonRewardInfo shopCommonRewardInfo = new ShopCommonRewardInfo();
			shopCommonRewardInfo.Type = jsonData[i]["reward_type"].ToInt();
			shopCommonRewardInfo.UserGoodsId = jsonData[i]["reward_detail_id"].ToLong();
			shopCommonRewardInfo.Num = jsonData[i]["reward_number"].ToInt();
			_seriesRewardList.Add(shopCommonRewardInfo);
		}
		LotteryData = LotteryApplyData.Parse(base.ResponseData["data"]);
		if (base.ResponseData["data"].TryGetValue("achieved_info", out var value))
		{
			if (value.TryGetValue("achieved_mission_reward_list", out var value2))
			{
				for (int j = 0; j < value2.Count; j++)
				{
					if (LotteryData.IsEnable)
					{
						LotteryRewardList.Add(ReceivedReward.CreateFromBattleResult(value2[j]));
					}
					else
					{
						MissionRewardList.Add(ReceivedReward.CreateFromBattleResult(value2[j]));
					}
				}
			}
			if (value.TryGetValue("achieved_mission_list", out var value3))
			{
				NotificatonAnimationParams = new List<NotificatonAnimation.Param>();
				for (int k = 0; k < value3.Count; k++)
				{
					NotificatonAnimationParams.Add(new NotificatonAnimation.Param(NotificatonAnimation.Param.Type.TemporaryDeckResult, value3[k]["achieved_message"].ToString()));
				}
			}
		}
		return num;
	}
}

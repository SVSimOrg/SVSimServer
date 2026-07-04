using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class MypageReceiveSpecialTreasureTask : BaseTask
{
	public class MypageReceiveSpecialTreasureTaskParam : BaseParam
	{
	}

	public class MypageTreasureBoxCpOpenTaskData
	{
		public List<ReceivedReward> RewardDataList { get; private set; }

		public MypageTreasureBoxCpOpenTaskData(List<ReceivedReward> rewardDataList)
		{
			RewardDataList = rewardDataList;
		}
	}

	public MypageTreasureBoxCpOpenTaskData Result { get; private set; }

	public MypageReceiveSpecialTreasureTask()
	{
		base.type = ApiType.Type.TreasureOpenSpecialTreasureBox;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"]["treasure_reward_list"];
		List<ReceivedReward> list = new List<ReceivedReward>();
		for (int i = 0; i < jsonData.Count; i++)
		{
			ReceivedReward item = ReceivedReward.CreateFromPackInfoResult(jsonData[i]);
			list.Add(item);
		}
		Result = new MypageTreasureBoxCpOpenTaskData(list);
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}
}

using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class MypageTreasureBoxCpOpenTask : BaseTask
{
	public class MypageTreasureBoxCpOpenTaskParam : BaseParam
	{
	}

	public class MypageTreasureBoxCpOpenTaskData
	{
		public List<ReceivedReward> RewardDataList { get; private set; }

		public int Grade { get; private set; }

		public MypageTreasureBoxCpOpenTaskData(List<ReceivedReward> rewardDataList, int grade)
		{
			RewardDataList = rewardDataList;
			Grade = grade;
		}
	}

	public MypageTreasureBoxCpOpenTaskData Result { get; private set; }

	public MypageTreasureBoxCpOpenTask()
	{
		base.type = ApiType.Type.ReceiveUpgradeTreasureBox;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		JsonData jsonData2 = jsonData["upgrade_treasure_box_reward_list"];
		List<ReceivedReward> list = new List<ReceivedReward>();
		for (int i = 0; i < jsonData2.Count; i++)
		{
			ReceivedReward item = ReceivedReward.CreateFromPackInfoResult(jsonData2[i]);
			list.Add(item);
		}
		int grade = jsonData["grade"].ToInt();
		Result = new MypageTreasureBoxCpOpenTaskData(list, grade);
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}
}

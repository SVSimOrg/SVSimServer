using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class QuestRewardReceiveAllTask : BaseTask
{
	public List<ReceivedReward> ReceiveRewardList { get; private set; }

	public QuestRewardReceiveAllTask()
	{
		base.type = ApiType.Type.QuestRewardReceiveAll;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		ReceiveRewardList = new List<ReceivedReward>();
		JsonData jsonData = base.ResponseData["data"]["total_receive_count_list"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			ReceiveRewardList.Add(new ReceivedReward(jsonData[i]));
		}
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}
}

using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class TreasureBoxCpResultInfo
{
	public int BeforeGrade { get; private set; }

	public int AfterGrade { get; private set; }

	public List<ReceivedReward> RewardDataList { get; private set; }

	public void Parse(JsonData data)
	{
		BeforeGrade = data["before_grade"].ToInt();
		AfterGrade = data["after_grade"].ToInt();
		JsonData jsonData = data["upgrade_treasure_box_reward_list"];
		RewardDataList = new List<ReceivedReward>();
		for (int i = 0; i < jsonData.Count; i++)
		{
			ReceivedReward item = ReceivedReward.CreateFromPackInfoResult(jsonData[i]);
			RewardDataList.Add(item);
		}
	}
}

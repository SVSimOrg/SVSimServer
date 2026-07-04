using System;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class QuestPointInfoTask : BaseTask
{
	public List<QuestRewardInfo> RewardInfoList { get; private set; }

	public DateTime StartTime { get; private set; }

	public DateTime EndTime { get; private set; }

	public int TotalPoint { get; private set; }

	public int MaxPoint { get; private set; }

	public QuestPointInfoTask()
	{
		base.type = ApiType.Type.QuestPoint;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		StartTime = DateTime.Parse(jsonData["start_time"].ToString());
		EndTime = DateTime.Parse(jsonData["end_time"].ToString());
		TotalPoint = jsonData["total_point"].ToInt();
		MaxPoint = jsonData["max_point"].ToInt();
		RewardInfoList = new List<QuestRewardInfo>();
		for (int i = 0; i < jsonData["reward_list"].Count; i++)
		{
			RewardInfoList.Add(new QuestRewardInfo(jsonData["reward_list"][i]));
		}
		return num;
	}
}

using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class QuestMissionData
{
	public List<QuestMissionDetail> MissionDetailList { get; private set; }

	public string MissionTitle { get; private set; }

	public int MissionClassId { get; private set; }

	public QuestMissionData(JsonData data)
	{
		MissionTitle = data["title"]["name"].ToString();
		MissionClassId = data["title"]["class_id"].ToInt();
		MissionDetailList = new List<QuestMissionDetail>(data["mission"].Count);
		for (int i = 0; i < data["mission"].Count; i++)
		{
			MissionDetailList.Add(new QuestMissionDetail(data["mission"][i]));
		}
	}

	public int GetMaxPoint()
	{
		int num = 0;
		for (int i = 0; i < MissionDetailList.Count; i++)
		{
			num += MissionDetailList[i].Point;
		}
		return num;
	}

	public int GetTotalPoint()
	{
		int num = 0;
		for (int i = 0; i < MissionDetailList.Count; i++)
		{
			if (MissionDetailList[i].IsClear)
			{
				num += MissionDetailList[i].Point;
			}
		}
		return num;
	}
}

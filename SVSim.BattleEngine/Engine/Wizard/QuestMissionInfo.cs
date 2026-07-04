using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class QuestMissionInfo
{
	public List<QuestMissionData> MissionDataList { get; private set; }

	public QuestMissionInfo()
	{
		Initialize();
	}

	public QuestMissionInfo(JsonData data)
	{
		Initialize();
		if (data != null)
		{
			for (int i = 0; i < data.Count; i++)
			{
				MissionDataList.Add(new QuestMissionData(data[i]));
			}
		}
	}

	private void Initialize()
	{
		MissionDataList = new List<QuestMissionData>();
	}
}

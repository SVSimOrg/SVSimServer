using System.Collections.Generic;
using LitJson;
using Wizard.Scripts.Network.Data.TableData.Ranking;

namespace Wizard.Scripts.Network.Data.TaskData.Ranking;

public class MyMasterPointHistories : HeaderData
{
	public List<MyMasterRanking> rankingHistories;

	public MyMasterPointHistories()
	{
		Initialize();
	}

	public MyMasterPointHistories(JsonData data, Format inFormat)
	{
		Initialize();
		if (data.Count < 1)
		{
			return;
		}
		JsonData jsonData = ((inFormat == Format.Crossover) ? data["periods"]["crossover"] : data["periods"]["normal"]);
		JsonData jsonData2 = data["histories"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			string text = Wizard.Data.FormatConvertApi(inFormat).ToString();
			if (jsonData2.Keys.Contains(text))
			{
				JsonData jsonData3 = jsonData[i];
				if (jsonData2[text].Keys.Contains(jsonData3["id"].ToString()))
				{
					MyMasterRanking item = new MyMasterRanking(jsonData[i], jsonData2[text]);
					rankingHistories.Add(item);
				}
			}
		}
	}

	public void Initialize()
	{
		rankingHistories = new List<MyMasterRanking>();
	}
}

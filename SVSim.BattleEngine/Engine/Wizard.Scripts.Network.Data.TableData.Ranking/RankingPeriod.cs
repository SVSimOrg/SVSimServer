using System;
using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Ranking;

public class RankingPeriod
{
	public int id;

	public int periodNum;

	public bool hasHistory;

	public DateTime beginTime;

	public string endTime;

	public int _detailType;

	public int IsAfter460;

	public RankingPeriod()
	{
		Initialize();
	}

	public RankingPeriod(JsonData data)
	{
		if (data == null)
		{
			Initialize();
			return;
		}
		id = data["id"].ToInt();
		periodNum = data["period_num"].ToInt();
		hasHistory = true;
		beginTime = DateTime.Parse(data["begin_time"].ToString());
		endTime = data["end_time"].ToString();
		if (data.Keys.Contains("type"))
		{
			_detailType = data["type"].ToInt();
		}
		if (data.TryGetValue("over_460", out var value))
		{
			IsAfter460 = value.ToInt();
		}
	}

	private void Initialize()
	{
		id = 0;
		periodNum = 0;
		hasHistory = false;
		beginTime = new DateTime(0L);
		endTime = "";
	}
}

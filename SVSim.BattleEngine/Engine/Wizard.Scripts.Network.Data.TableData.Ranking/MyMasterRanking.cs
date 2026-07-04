using System;
using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Ranking;

public class MyMasterRanking
{
	public int periodId;

	public int periodNum;

	public DateTime beginTime;

	public string endTime;

	public bool isCalculated;

	public int rank;

	public int score;

	public int _masterRankId;

	public MyMasterRanking(JsonData periods, JsonData histories)
	{
		if (periods == null || histories == null)
		{
			Initialize();
			return;
		}
		periodId = periods["id"].ToInt();
		periodNum = periods["period_num"].ToInt();
		beginTime = DateTime.Parse(periods["begin_time"].ToString());
		endTime = periods["end_time"].ToString();
		isCalculated = periods["is_calculated"].ToBoolean();
		JsonData jsonData = histories[periodId.ToString()];
		rank = jsonData["rank"].ToInt();
		score = jsonData["score"].ToInt();
		if (jsonData.Keys.Contains("rank_id"))
		{
			_masterRankId = jsonData["rank_id"].ToInt();
		}
	}

	private void Initialize()
	{
		periodId = 0;
		periodNum = 0;
		beginTime = default(DateTime);
		endTime = string.Empty;
		isCalculated = false;
		rank = 0;
		score = 0;
		_masterRankId = 0;
	}
}

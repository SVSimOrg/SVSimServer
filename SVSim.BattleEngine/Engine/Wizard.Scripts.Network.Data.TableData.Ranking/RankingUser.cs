using System;
using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Ranking;

public class RankingUser
{
	public int viewerId;

	public int rankingRank;

	public int score;

	public string name;

	public string countryCode;

	public int rank;

	public long emblemId;

	public int degreeId;

	public DateTime lastPlayTime;

	public RankingUser(JsonData data)
	{
		if (data == null)
		{
			Initialize();
			return;
		}
		viewerId = data["viewer_id"].ToInt();
		rankingRank = data["ranking_rank"].ToInt();
		score = data["score"].ToInt();
		name = data["name"].ToString();
		countryCode = (string)data["country_code"];
		rank = data["rank"].ToInt();
		emblemId = data["emblem_id"].ToLong();
		degreeId = data["degree_id"].ToInt();
		string text = data["last_play_time"].ToString();
		lastPlayTime = (string.IsNullOrEmpty(text) ? default(DateTime) : DateTime.Parse(text));
	}

	private void Initialize()
	{
		viewerId = 0;
		rankingRank = 0;
		score = 0;
		rank = 0;
		name = "";
		countryCode = "";
		emblemId = 0L;
		degreeId = 0;
		lastPlayTime = new DateTime(0L);
	}
}

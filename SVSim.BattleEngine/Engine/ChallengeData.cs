using System;
using LitJson;
using Wizard;

public class ChallengeData
{
	public TwoPickFormat TwoPickFormat { get; private set; }

	public string CardPoolName { get; private set; }

	public string CardPoolUrl { get; private set; } = string.Empty;

	public string AnnounceId { get; private set; }

	public string StartTime { get; private set; }

	public string EndTime { get; private set; }

	public int LatestCardPackId { get; private set; }

	public int ChaosNum { get; private set; }

	public ChallengeData(JsonData forMatIndoJson)
	{
		TwoPickFormat = (TwoPickFormat)forMatIndoJson["two_pick_type"].ToInt();
		CardPoolName = forMatIndoJson["card_pool_name"].ToString();
		if (forMatIndoJson.TryGetValue("card_pool_url", out var value))
		{
			CardPoolUrl = value.ToString();
		}
		AnnounceId = forMatIndoJson["announce_id"].ToString();
		StartTime = ConvertTime.ToLocal(DateTime.Parse(forMatIndoJson["start_time"].ToString()));
		EndTime = ConvertTime.ToLocal(DateTime.Parse(forMatIndoJson["end_time"].ToString()));
		if (forMatIndoJson.TryGetValue("last_card_pack_set_id", out var value2))
		{
			LatestCardPackId = value2.ToInt();
		}
		if (forMatIndoJson.TryGetValue("strategy_pick_num", out var value3))
		{
			ChaosNum = value3.ToInt();
		}
	}
}

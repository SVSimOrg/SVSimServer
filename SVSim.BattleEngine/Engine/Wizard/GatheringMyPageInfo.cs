using System;
using LitJson;

namespace Wizard;

public class GatheringMyPageInfo
{
	public bool IsEntry { get; private set; }

	public bool IsDeckEntry { get; private set; }

	public bool IsBattleFinish { get; private set; }

	public bool IsMatchingNotification { get; set; }

	public bool IsOpening { get; private set; }

	public string FinishTime { get; private set; }

	public string DeckEntryFinishTime { get; private set; }

	public GatheringMyPageInfo()
	{
	}

	public GatheringMyPageInfo(JsonData data, double serverTime)
	{
		IsEntry = data["is_entry"].ToInt() == 1;
		if (data.Keys.Contains("battle_end_time"))
		{
			if (data.TryGetValue("battle_end_time", out var value))
			{
				FinishTime = ConvertTime.ToLocal(DateTime.Parse(value.ToString())).ToString();
			}
		}
		else
		{
			FinishTime = string.Empty;
		}
		if (data.Keys.Contains("battle_begin_time"))
		{
			double num = ConvertTime.DateTimeToUnixTime(DateTime.Parse(data["battle_begin_time"].ToString()));
			if (data.TryGetValue("battle_end_time", out var value2))
			{
				double num2 = ConvertTime.DateTimeToUnixTime(DateTime.Parse(value2.ToString()));
				IsBattleFinish = serverTime > num2;
			}
			else
			{
				IsBattleFinish = false;
				IsOpening = true;
			}
			IsDeckEntry = serverTime < num;
			DeckEntryFinishTime = ConvertTime.ToLocal(DateTime.Parse(data["battle_begin_time"].ToString())).ToString();
		}
		IsMatchingNotification = !string.IsNullOrEmpty(data.GetValueOrDefault("matching_established_message", string.Empty));
	}
}

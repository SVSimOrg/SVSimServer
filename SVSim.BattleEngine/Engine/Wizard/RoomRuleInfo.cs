using System;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class RoomRuleInfo
{
	public List<Format> NormalRuleFormatList { get; private set; } = new List<Format>();

	public List<TwoPickFormat> TwoPickRuleFormatList { get; private set; } = new List<TwoPickFormat>();

	public List<TwoPickFormat> SpecialTwoPickRuleFormatList { get; private set; } = new List<TwoPickFormat>();

	public Dictionary<Format, string> NormalRuleFormatEndTime { get; private set; } = new Dictionary<Format, string>();

	public Dictionary<TwoPickFormat, string> TwoPickRuleFormatEndTime { get; private set; } = new Dictionary<TwoPickFormat, string>();

	public TwoPickFormat ChallengePickFormat { get; private set; } = TwoPickFormat.Normal;

	public TwoPickFormat BackDraftFormat { get; private set; } = TwoPickFormat.Backdraft;

	public int RoomChaosNum { get; private set; }

	public RoomRuleInfo()
	{
		NormalRuleFormatList.Add(Format.Rotation);
		NormalRuleFormatList.Add(Format.Unlimited);
	}

	public RoomRuleInfo(JsonData json)
		: this()
	{
		if (json.TryGetValue("room_type_in_session", out var value))
		{
			if (value.TryGetValue("special_deck_format_list", out var value2))
			{
				for (int i = 0; i < value2.Count; i++)
				{
					string value3 = ConvertTime.ToLocal(ConvertTime.UnixTimeToDateTime((int)ConvertTime.DateTimeToUnixTime(DateTime.Parse(value2[i]["end_time"].ToString())))).ToString();
					Format format = Data.ParseApiFormat(value2[i]["deck_format"].ToInt());
					NormalRuleFormatList.Add(format);
					NormalRuleFormatEndTime.Add(format, value3);
				}
			}
			if (value.TryGetValue("special_two_pick_list", out var value4))
			{
				for (int j = 0; j < value4.Count; j++)
				{
					string value5 = ConvertTime.ToLocal(ConvertTime.UnixTimeToDateTime((int)ConvertTime.DateTimeToUnixTime(DateTime.Parse(value4[j]["end_time"].ToString())))).ToString();
					TwoPickFormat twoPickFormat = (TwoPickFormat)value4[j]["two_pick_type"].ToInt();
					if (value4[j]["is_challenge_format"].ToInt() == 1)
					{
						switch (twoPickFormat)
						{
						case TwoPickFormat.Normal:
						case TwoPickFormat.Cube:
						case TwoPickFormat.Chaos:
							ChallengePickFormat = twoPickFormat;
							break;
						case TwoPickFormat.Backdraft:
						case TwoPickFormat.BackdraftCube:
						case TwoPickFormat.BackdraftChaos:
							BackDraftFormat = twoPickFormat;
							break;
						}
					}
					else
					{
						SpecialTwoPickRuleFormatList.Add(twoPickFormat);
						TwoPickRuleFormatEndTime.Add(twoPickFormat, value5);
					}
					if (value4[j].TryGetValue("strategy_pick_num", out var value6))
					{
						RoomChaosNum = value6.ToInt();
						Data.Master.LoadRoomChaosBattleInfo(RoomChaosNum);
						Data.Master.SetRoomClassInfomationOrder(RoomChaosNum);
					}
				}
			}
		}
		TwoPickRuleFormatList.Add(ChallengePickFormat);
		TwoPickRuleFormatList.Add(BackDraftFormat);
		if (SpecialTwoPickRuleFormatList.Count > 0)
		{
			for (int k = 0; k < SpecialTwoPickRuleFormatList.Count; k++)
			{
				TwoPickRuleFormatList.Add(SpecialTwoPickRuleFormatList[k]);
			}
		}
		if (Prerelease.Status == Prerelease.eStatus.PRE_ROTATION)
		{
			NormalRuleFormatList.Add(Format.PreRotation);
		}
	}
}

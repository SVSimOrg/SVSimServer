using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Wizard;
using Wizard.Scripts.Network.Data.TaskData.Arena;

public class ArenaCompetition : ArenaEntryDataBase
{
	public enum EntryStatusType
	{
		NotEntry	}

	public enum FreebieStatusType
	{
}

	public enum EntryCostType
	{
	}

	private bool _isRankMatching;

	public bool IsCompetitionPeriod { get; private set; }

	public bool IsEntry { get; set; }

	public bool IsInFreeBattleRegistDeck { get; set; }

	public bool NeedsFirstTips { get; private set; }

	public int CompetitionId { get; private set; }

	public FreebieStatusType FreebieStatus { get; set; }

	public Format DeckFormat { get; private set; }

	public ArenaColosseum.eRule Rule { get; private set; }

	public bool IsSpecialMode { get; private set; }

	public string NowRoundTimeText { get; private set; }

	public string EntryEndTimeText { get; private set; }

	public string EndTimeText { get; private set; }

	public double EntryRemainingUnixTime { get; set; }

	public double RemainingUnixTime { get; set; }

	public float RemainingSinceTime { get; set; }

	public double RemainingServerUnixTime { get; set; }

	public string EntryTimeText { get; private set; }

	public List<DeckData> DeckList { get; set; }

	public List<Wizard.Scripts.Network.Data.TaskData.Arena.Reward> EntryRewardList { get; set; }

	public bool IsRewardReceived { get; private set; }

	public string AnnounceId { get; private set; }

	public string CompetitionName { get; private set; }

	public int MaxEntryCount { get; private set; }

	public int MaxChallengeCount { get; private set; }

	public int MaxWinCount { get; private set; }

	public int BestWinCount { get; private set; }

	public int MaxLoseCount { get; private set; }

	public int RestChallangeCount { get; private set; }

	public int RestEntryCount { get; private set; }

	public int CurrentWinCount { get; private set; }

	public int FreebieChallengeCount { get; private set; }

	public bool IsChampion { get; private set; }

	public bool IsEntryTimeEnd { get; private set; }

	public int MaxBattleCount { get; private set; }

	public int IsCompletedTwoPickDeck { get; private set; }

	public int MaxFreebieChallengeCount { get; private set; }

	public EntryStatusType EntryStatus { get; private set; }

	public EntryCostType CostType { get; private set; }

	public bool IsRankMatching
	{
		get
		{
			return _isRankMatching;
		}
		set
		{
			if (_isRankMatching != value)
			{
				_isRankMatching = value;
				if (RealTimeNetworkAgent.FinishTaskBase != null)
				{
					RealTimeNetworkAgent.FinishTaskBase = new CompetitionBattleFinishTask();
				}
			}
		}
	}

	public ArenaCompetition()
	{
	}

	public ArenaCompetition(JsonData responseData)
	{
		JsonData jsonData = responseData["data"]["competition_info"];
		IsCompetitionPeriod = jsonData["is_competition_period"].ToBoolean();
		if (IsCompetitionPeriod)
		{
			Rule = (ArenaColosseum.eRule)jsonData["deck_format"].ToInt();
			DeckFormat = ArenaData.ApiDeckFormatParse(Rule);
			IsEntry = jsonData["is_entry"].ToBoolean();
			IsInFreeBattleRegistDeck = jsonData["is_in_battle"].ToBoolean();
			IsSpecialMode = jsonData["is_special_mode"].ToInt() == 1;
			string text = ConvertTime.ToLocal(DateTime.Parse(jsonData["entry_start_time"].ToString()));
			EntryRemainingUnixTime = ConvertTime.DateTimeToUnixTime(DateTime.Parse(jsonData["entry_end_time"].ToString()));
			string text2 = ConvertTime.ToLocal(DateTime.Parse(jsonData["entry_end_time"].ToString()));
			EntryTimeText = Data.SystemText.Get("Colosseum_0033", text, text2);
			EntryEndTimeText = text2;
			string text3 = ConvertTime.ToLocal(DateTime.Parse(jsonData["start_time"].ToString()));
			RemainingUnixTime = ConvertTime.DateTimeToUnixTime(DateTime.Parse(jsonData["end_time"].ToString()));
			string text4 = ConvertTime.ToLocal(DateTime.Parse(jsonData["end_time"].ToString()));
			NowRoundTimeText = Data.SystemText.Get("Colosseum_0033", text3, text4);
			EndTimeText = text4;
			RemainingSinceTime = Time.realtimeSinceStartup;
			RemainingServerUnixTime = responseData["data_headers"]["servertime"].ToDouble();
			NeedsFirstTips = jsonData.GetValueOrDefault("is_display_tips", 0) == 1;
			CompetitionId = jsonData.GetValueOrDefault("competition_id", 0);
			FreebieStatus = (FreebieStatusType)jsonData["freebie_status"].ToInt();
			DeckList = new List<DeckData>();
			EntryRewardList = new List<Wizard.Scripts.Network.Data.TaskData.Arena.Reward>();
			JsonData jsonData2 = jsonData["featured_entry_reward_list"];
			for (int i = 0; i < jsonData2.Count; i++)
			{
				Wizard.Scripts.Network.Data.TaskData.Arena.Reward item = new Wizard.Scripts.Network.Data.TaskData.Arena.Reward(jsonData2[i]);
				EntryRewardList.Add(item);
			}
			IsRewardReceived = jsonData["is_received_featured_entry_reward"].ToBoolean();
			if (jsonData["announce_id"] != null)
			{
				AnnounceId = jsonData["announce_id"].ToString();
			}
			MaxEntryCount = jsonData.GetValueOrDefault("max_entry_count", 0);
			MaxChallengeCount = jsonData.GetValueOrDefault("max_challenge_count", 0);
			MaxWinCount = jsonData.GetValueOrDefault("max_win_count", 0);
			MaxLoseCount = jsonData.GetValueOrDefault("max_lose_count", 0);
			MaxBattleCount = jsonData.GetValueOrDefault("max_battle_count", 0);
			MaxFreebieChallengeCount = jsonData["max_freebie_challenge_count"].ToInt();
			crystalCost = jsonData.GetValueOrDefault("crystal_cost", 0);
			rupyCost = jsonData.GetValueOrDefault("rupy_cost", 0);
			BestWinCount = jsonData["max_win_count_in_entry"].ToInt();
			RestChallangeCount = jsonData["rest_challenge_num"].ToInt();
			RestEntryCount = jsonData["rest_entry_num"].ToInt();
			CurrentWinCount = jsonData["current_win_count"].ToInt();
			FreebieChallengeCount = jsonData["freebie_challenge_count"].ToInt();
			EntryStatus = (EntryStatusType)jsonData["entry_status"].ToInt();
			CostType = (EntryCostType)jsonData["entry_type"].ToInt();
			IsChampion = jsonData.GetValueOrDefault("is_champion", 0) == 1;
			CompetitionName = jsonData.GetValueOrDefault("competition_name", string.Empty).Replace("\\n", "\n");
			double num = RemainingServerUnixTime + (double)Time.realtimeSinceStartup - (double)RemainingSinceTime;
			IsEntryTimeEnd = EntryRemainingUnixTime - num < 0.0;
			bool flag = CompetitionId <= PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.COMPETITION_JOIN_BUTTON_LATEST_ID);
			Data.MyPageNotifications.data.IsCompetitionBadge = !IsRewardReceived && EntryStatus == EntryStatusType.NotEntry && !IsEntryTimeEnd && !flag;
			base.ExpirtyInfo = new ShopExpirtyInfo(jsonData["sales_period_info"]);
			if (DeckFormat == Format.TwoPick)
			{
				IsCompletedTwoPickDeck = jsonData["is_completed_two_pick_deck"].ToInt();
			}
		}
		base.LootBoxType = PlayerStaticData.LootBoxType.COMPETITION;
	}
}

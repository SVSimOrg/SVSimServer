using System;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class BossRushLobbyData
{
	public enum BossRushStatus
	{
		BATTLE,
		FINISH_WIN,
		FINISH_LOSE
	}

	public List<BossRushLobbyBossData> BossDataList { get; }

	public List<BossRushLobbyAbilityData> AbilityList { get; }

	public BossRushLobbyBossData CurrentBattleBossData { get; }

	public List<BossRushLobbyAbilityCandidateData> AbilityCandidateData { get; }

	public List<ShopCommonRewardInfo> RewardList { get; }

	public int CurrentLife { get; }

	public int MaxLife { get; }

	public int WinCount { get; }

	public DeckData Deck { get; }

	public int TreasureBoxGrade { get; }

	public bool IsAlreadyGetReward { get; }

	public bool NeedAbilityDecideFinish { get; }

	public int NextBattleCount { get; }

	public BossRushStatus Status { get; }

	public string AnnounceId { get; }

	public int TotalTurn { get; }

	public BossRushLobbyData(JsonData json)
	{
		if (json["total_turns"] != null)
		{
			TotalTurn = Math.Min(json["total_turns"].ToInt(), 9999);
		}
		NeedAbilityDecideFinish = !json["is_finished_special_ability_select"].ToBoolean();
		int num = json["bossrush_progress"].ToInt();
		NextBattleCount = num + 1;
		BossDataList = new List<BossRushLobbyBossData>();
		JsonData jsonData = json["bossrush_opponent_list"];
		bool flag = json["is_lose"].ToBoolean();
		WinCount = 0;
		for (int i = 0; i < jsonData.Count; i++)
		{
			BossRushLobbyBossData bossRushLobbyBossData = new BossRushLobbyBossData(jsonData[i], i, num, flag);
			BossDataList.Add(bossRushLobbyBossData);
			if (bossRushLobbyBossData.Status == BossRushLobbyBossData.BattleStatus.WIN)
			{
				int winCount = WinCount;
				WinCount = winCount + 1;
			}
		}
		Status = BossRushStatus.BATTLE;
		if (WinCount == BossDataList.Count)
		{
			Status = BossRushStatus.FINISH_WIN;
		}
		if (flag)
		{
			Status = BossRushStatus.FINISH_LOSE;
			NeedAbilityDecideFinish = false;
		}
		CurrentBattleBossData = BossDataList[num];
		JsonData jsonData2 = json["special_ability_list"];
		AbilityList = new List<BossRushLobbyAbilityData>();
		for (int j = 0; j < jsonData2.Count; j++)
		{
			AbilityList.Add(new BossRushLobbyAbilityData(jsonData2[j]));
		}
		JsonData jsonData3 = json["user_bossrush_deck"][0];
		Deck = new DeckData(Data.ParseApiFormat(jsonData3["deck_format"].ToInt()));
		Deck.Initialize(jsonData3);
		CurrentLife = json["current_life"].ToInt();
		MaxLife = json["max_life"].ToInt();
		AbilityCandidateData = new List<BossRushLobbyAbilityCandidateData>();
		JsonData jsonData4 = json["special_ability_candidate_list"];
		for (int k = 0; k < jsonData4.Count; k++)
		{
			BossRushLobbyAbilityCandidateData item = new BossRushLobbyAbilityCandidateData(jsonData4[k]);
			AbilityCandidateData.Add(item);
		}
		RewardList = new List<ShopCommonRewardInfo>();
		JsonData jsonData5 = json["reward_info"];
		for (int l = 0; l < jsonData5.Count; l++)
		{
			JsonData jsonData6 = jsonData5[l];
			ShopCommonRewardInfo item2 = new ShopCommonRewardInfo
			{
				Type = jsonData6["reward_type"].ToInt(),
				UserGoodsId = jsonData6["reward_detail_id"].ToLong(),
				Num = jsonData6["reward_num"].ToInt(),
				IsAlreadyGet = jsonData6["is_received"].ToBoolean()
			};
			RewardList.Add(item2);
		}
		TreasureBoxGrade = json["reward_grade"].ToInt();
		IsAlreadyGetReward = json["is_received_all_rewards"].ToBoolean();
		AnnounceId = json.GetValueOrDefault("announce_id", string.Empty);
	}
}

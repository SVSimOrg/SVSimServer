using System;
using LitJson;

namespace Wizard;

public class BossRushInfo
{
	public bool BossRushInfoExist { get; private set; }

	public bool IsBossRushUnlocked { get; private set; }

	public int BossRushProgress { get; private set; }

	public bool IsReceivedBossrushReward { get; private set; }

	public bool IsFirstChallenge { get; private set; }

	public bool IsAllChallengeFinished { get; private set; }

	public bool IsDeckRegistered { get; private set; }

	public bool IsBossRushInProgress { get; private set; }

	public bool IsTopButton { get; private set; }

	public int? ShortestClearTurn { get; private set; }

	public int? ShortestClearClass { get; private set; }

	public BossRushInfo(JsonData data)
	{
		if (data.TryGetValue("bossrush_info", out var value))
		{
			BossRushInfoExist = true;
			IsBossRushUnlocked = value["is_finished_quest_battle"].ToBoolean();
			BossRushProgress = value["bossrush_progress"].ToInt();
			IsReceivedBossrushReward = value["is_received_bossrush_reward"].ToBoolean();
			IsFirstChallenge = value["is_first_challenge"].ToBoolean();
			IsAllChallengeFinished = value["is_max_challenge"].ToBoolean();
			IsDeckRegistered = value["is_deck_registered"].ToBoolean();
			if (value["shortest_clear_turns"] != null)
			{
				ShortestClearTurn = Math.Min(value["shortest_clear_turns"].ToInt(), 9999);
				ShortestClearClass = value["shortest_clear_class"].ToInt();
			}
			IsBossRushInProgress = IsBossRushUnlocked && !IsAllChallengeFinished;
			IsTopButton = IsBossRushInProgress || !IsBossRushUnlocked;
		}
		else
		{
			BossRushInfoExist = false;
		}
	}
}

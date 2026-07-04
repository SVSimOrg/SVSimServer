using System;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class QuestInfoTask : BaseTask
{
	public List<QuestOpponentData> QuestDataList { get; private set; }

	public DateTime StartTime { get; private set; }

	public DateTime EndTime { get; private set; }

	public bool IsLastDay { get; private set; }

	public bool IsOpenExtra { get; private set; }

	public int UnreceivedRewardCount { get; private set; }

	public bool IsDisplayBadge { get; private set; }

	public string AnnounceId { get; private set; }

	public int QuestId { get; private set; }

	public bool IsDisplayTweetBanner { get; private set; }

	public PuzzleQuestInfo PuzzleQuestInfo { get; private set; }

	public EventStoryQuestInfo EventStoryQuestInfo { get; private set; }

	public BossRushInfo BossRushInfo { get; private set; }

	public SecretBossInfo SecretBossInfo { get; private set; }

	public QuestInfoTask()
	{
		base.type = ApiType.Type.QuestInfo;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		StartTime = DateTime.Parse(jsonData["start_time"].ToString());
		EndTime = DateTime.Parse(jsonData["end_time"].ToString());
		IsLastDay = jsonData["is_last_day"].ToBoolean();
		IsOpenExtra = jsonData["is_open_extra"].ToBoolean();
		QuestDataList = new List<QuestOpponentData>();
		for (int i = 0; i < jsonData["opponent_list"].Count; i++)
		{
			QuestDataList.Add(new QuestOpponentData(jsonData["opponent_list"][i]));
		}
		UnreceivedRewardCount = jsonData["unreceived_reward_count"].ToInt();
		IsDisplayBadge = jsonData["is_display_badge"].ToBoolean();
		IsDisplayTweetBanner = jsonData.GetValueOrDefault("is_display_tweet_reward_banner", defaultValue: false);
		AnnounceId = jsonData.GetValueOrDefault("announce_id", string.Empty);
		QuestId = jsonData.GetValueOrDefault("quest_id", 0);
		PuzzleQuestInfo = new PuzzleQuestInfo(jsonData);
		EventStoryQuestInfo = new EventStoryQuestInfo(jsonData);
		BossRushInfo = new BossRushInfo(jsonData);
		SecretBossInfo = new SecretBossInfo(jsonData);
		return num;
	}
}

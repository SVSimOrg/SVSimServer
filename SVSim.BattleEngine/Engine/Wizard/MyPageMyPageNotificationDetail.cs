using System;
using LitJson;
using UnityEngine;

namespace Wizard;

public class MyPageMyPageNotificationDetail
{
	public RoomRuleInfo RoomRule;

	public GatheringMyPageInfo GatheringMyPageInfo;

	public GuildNotification GuildNotification { get; set; }

	public CampaignBattleWin CampaignBattleWin { get; set; }

	public ShopNotification ShopNotification { get; set; }

	public StoryNotification StoryNotification { get; private set; }

	public QuestOpenInfo QuestOpenInfo { get; set; }

	public bool IsPracticePuzzleBadgeEnable { get; set; }

	public int ReceiveFriendApplyCount { get; set; }

	public bool IsCompetitionBadge { get; set; }

	public bool IsInviteGathering { get; set; }

	public bool IsColosseumFreeEntry { get; set; }

	public MyPageMyPageNotificationDetail()
	{
		GuildNotification = new GuildNotification();
		CampaignBattleWin = new CampaignBattleWin();
		RoomRule = new RoomRuleInfo();
		ShopNotification = new ShopNotification();
		StoryNotification = new StoryNotification();
		QuestOpenInfo = new QuestOpenInfo();
		GatheringMyPageInfo = new GatheringMyPageInfo();
	}

	public void SetIsCompetitionBadge(JsonData json)
	{
		if (json["data"]["competition_info"]["is_competition_period"].ToBoolean())
		{
			bool flag = json["data"]["competition_info"]["is_received_featured_entry_reward"].ToBoolean();
			ArenaCompetition.EntryStatusType entryStatusType = (ArenaCompetition.EntryStatusType)json["data"]["competition_info"]["entry_status"].ToInt();
			double num = ConvertTime.DateTimeToUnixTime(DateTime.Parse(json["data"]["competition_info"]["entry_end_time"].ToString()));
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			double num2 = json["data_headers"]["servertime"].ToDouble() + (double)Time.realtimeSinceStartup - (double)realtimeSinceStartup;
			bool flag2 = num - num2 < 0.0;
			bool flag3 = json["data"]["competition_info"].GetValueOrDefault("competition_id", 0) <= PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.COMPETITION_JOIN_BUTTON_LATEST_ID);
			IsCompetitionBadge = !flag && entryStatusType == ArenaCompetition.EntryStatusType.NotEntry && !flag2 && !flag3;
		}
	}
}

using LitJson;

namespace Wizard;

public class MyPageNotifications : HeaderData
{
	public MyPageMyPageNotificationDetail data = new MyPageMyPageNotificationDetail();

	public void ParseBadgeInfos(JsonData jsonData)
	{
		data.QuestOpenInfo.SetIsDisplayBadge(jsonData["data"]["quest"]);
		data.StoryNotification.SetIsDisplayBadge(jsonData["data"]["story_notification"]);
		data.IsPracticePuzzleBadgeEnable = jsonData["data"]["basic_puzzle"]["is_display_badge"].ToBoolean();
		data.ShopNotification.SetShopBadgeEnable(jsonData);
		data.ReceiveFriendApplyCount = jsonData["data"]["receive_friend_apply_count"].ToInt();
		data.SetIsCompetitionBadge(jsonData);
		data.GatheringMyPageInfo.IsMatchingNotification = !string.IsNullOrEmpty(jsonData["data"]["gathering_info"].GetValueOrDefault("matching_established_message", string.Empty));
		data.IsInviteGathering = jsonData["data"]["gathering_info"]["has_invite"].ToInt() != 0;
		data.IsColosseumFreeEntry = jsonData["data"]["is_available_colosseum_free_entry"].ToBoolean();
	}
}

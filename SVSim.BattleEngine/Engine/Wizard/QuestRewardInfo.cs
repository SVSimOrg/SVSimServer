using LitJson;

namespace Wizard;

public class QuestRewardInfo
{
	public enum QuestRewardStatusList
	{
		NotAcheived,
		NotReceived,
		Received
	}

	public int Id { get; private set; }

	public int Point { get; private set; }

	public int RewardType { get; private set; }

	public int RewardDetailId { get; private set; }

	public int RewardCount { get; private set; }

	public QuestRewardStatusList Status { get; private set; }

	public QuestRewardInfo(JsonData data)
	{
		Id = data["id"].ToInt();
		Point = data["point"].ToInt();
		RewardType = data["reward_type"].ToInt();
		RewardDetailId = data["reward_detail_id"].ToInt();
		RewardCount = data["reward_count"].ToInt();
		Status = (QuestRewardStatusList)data["status"].ToInt();
	}
}

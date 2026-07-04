using LitJson;

namespace Wizard;

public class CampaignRewardInfo
{
	public UserGoods.Type Type { get; private set; }

	public long GoodsId { get; private set; }

	public int GoodsCount { get; private set; }

	public bool IsReceived { get; private set; }

	public CampaignRewardInfo(JsonData json)
	{
		Type = (UserGoods.Type)json["reward_type"].ToInt();
		GoodsId = json["reward_detail_id"].ToLong();
		GoodsCount = json["reward_num"].ToInt();
		IsReceived = json["is_received"].ToBoolean();
	}
}

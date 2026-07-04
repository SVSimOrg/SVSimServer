using System;
using LitJson;

namespace Wizard.Scripts.Network.Data.TableData;

public class ItemAcquireHistory
{
	public int RewardType { get; private set; }

	public long RewardUserGoodsId { get; private set; }

	public int RewardCount { get; private set; }

	public string Message { get; private set; }

	public int AcquireType { get; private set; }

	public DateTime AcquireTime { get; private set; }

	public ItemAcquireHistory()
	{
	}

	public ItemAcquireHistory(JsonData data)
	{
		RewardType = data["reward_type"].ToInt();
		RewardUserGoodsId = data["reward_detail_id"].ToLong();
		RewardCount = data["reward_count"].ToInt();
		Message = data["message"].ToString();
		AcquireType = data["acquire_type"].ToInt();
		AcquireTime = DateTime.Parse(data["acquire_time"].ToString());
	}
}

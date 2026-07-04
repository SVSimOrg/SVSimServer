using System;
using LitJson;
using Wizard;

public class MailData : HeaderData
{
	public int mail_id;

	public string message;

	public int reward_type;

	public long reward_count;

	public int item_type;

	public string create_time;

	public int limit_type;

	public long reward_limit_time;

	public long RewardUserGoodsId { get; set; }

	public MailData(JsonData data)
	{
		mail_id = data["present_id"].ToInt();
		message = data["message"].ToString();
		reward_type = data["reward_type"].ToInt();
		RewardUserGoodsId = data["reward_detail_id"].ToLong();
		reward_count = data["reward_count"].ToLong();
		if (reward_type == 4)
		{
			item_type = data["item_type"].ToInt();
		}
		create_time = ConvertTime.ToLocal(DateTime.Parse(data["create_time"].ToString()));
		limit_type = data["present_limit_type"].ToInt();
		reward_limit_time = data["reward_limit_time"].ToLong();
	}
}

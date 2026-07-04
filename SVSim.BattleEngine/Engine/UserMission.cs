using LitJson;

public class UserMission : HeaderData
{
	public int id;

	public int mission_id;

	public int mission_status;

	public int total_count;

	public string mission_name;

	public int display_order;

	public int require_number;

	public int reward_type;

	public int reward_number;

	public long start_time;

	public long end_time;

	public string achieved_message;

	public string create_time;

	public bool default_flag;

	public int lot_type;

	public long RewardUserGoodsId { get; set; }

	public static UserMission CreateAchievedMission(JsonData data)
	{
		return new UserMission
		{
			achieved_message = data["achieved_message"].ToString()
		};
	}

	public long GetMissionPeriodSec(long nowUnixTime)
	{
		long num = end_time - nowUnixTime;
		if (num < 0)
		{
			num = 0L;
		}
		return num;
	}

	public bool IsGemMission()
	{
		return lot_type == 6;
	}
}

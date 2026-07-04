using LitJson;

public class UserAchievement : HeaderData
{
	public int achievement_type;

	public int achievement_status;

	public int level;

	public int _maxLevel;

	public int total_count;

	public string achievement_name;

	public int require_number;

	public int reward_type;

	public int reward_number;

	public string achieved_message;

	public string create_time;

	public long RewardUserGoodsId { get; set; }

	public string OsId { get; private set; }

	public static UserAchievement CreateCompletedAchievement(JsonData data)
	{
		UserAchievement userAchievement = new UserAchievement();
		userAchievement.achieved_message = data["achieved_message"].ToString();
		if (!string.IsNullOrEmpty(null) && data[null] != null)
		{
			userAchievement.OsId = data[null].ToString();
		}
		return userAchievement;
	}
}

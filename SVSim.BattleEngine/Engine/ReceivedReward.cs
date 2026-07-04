using LitJson;

public class ReceivedReward
{
	public int reward_type;

	public long rewardUserGoodsId;

	public int item_type;

	public string reward_message = "";

	public int reward_count;

	public int lineNum;

	public int? _gradeId;

	public bool IsUsable { get; private set; }

	public ReceivedReward()
	{
	}

	public ReceivedReward(JsonData data)
	{
		reward_type = data["reward_type"].ToInt();
		rewardUserGoodsId = data["reward_detail_id"].ToLong();
		item_type = data["item_type"].ToInt();
		if (data.Keys.Contains("reward_count"))
		{
			reward_count = data["reward_count"].ToInt();
		}
		IsUsable = data["is_usable"].ToBoolean();
	}

	public ReceivedReward(int rewardType, long rewardId, int rewardCount)
	{
		reward_type = rewardType;
		rewardUserGoodsId = rewardId;
		reward_count = rewardCount;
	}

	public static ReceivedReward CreateFromBattleResult(JsonData data)
	{
		return new ReceivedReward
		{
			reward_type = data["mission_reward_type"].ToInt(),
			rewardUserGoodsId = data["mission_reward_detail_id"].ToLong(),
			reward_count = data["mission_reward_number"].ToInt()
		};
	}

	public static ReceivedReward CreateFromBattleResultGrandMaster(JsonData data)
	{
		return new ReceivedReward
		{
			reward_type = data["grand_master_reward_type"].ToInt(),
			rewardUserGoodsId = data["grand_master_reward_detail_id"].ToLong(),
			reward_count = data["grand_master_reward_number"].ToInt()
		};
	}

	public static ReceivedReward CreateFromPackInfoResult(JsonData data)
	{
		return new ReceivedReward
		{
			reward_type = data["reward_type"].ToInt(),
			rewardUserGoodsId = data["reward_id"].ToLong(),
			reward_count = data["reward_num"].ToInt()
		};
	}

	public static ReceivedReward CreateVictoryReward(JsonData data)
	{
		ReceivedReward receivedReward = CreateFromCommonResponce(data);
		receivedReward._gradeId = data["grade_id"].ToInt();
		if (data.Keys.Contains("reward_message"))
		{
			receivedReward.reward_message = data["reward_message"].ToString();
		}
		return receivedReward;
	}

	public static ReceivedReward CreateFromBeginnerMissionReward(JsonData data)
	{
		return CreateFromCommonResponce(data);
	}

	public static ReceivedReward CreateFromBattlePassReward(JsonData data)
	{
		return CreateFromCommonResponce(data);
	}

	public static ReceivedReward CreateFromBingoTreasureBoxReward(JsonData data)
	{
		ReceivedReward receivedReward = CreateFromCommonResponce(data);
		receivedReward._gradeId = data["grade_id"].ToInt();
		return receivedReward;
	}

	public static ReceivedReward CreateFromBingoLineReward(JsonData data)
	{
		ReceivedReward receivedReward = CreateFromCommonResponce(data);
		receivedReward.lineNum = data["line_num"].ToInt();
		return receivedReward;
	}

	private static ReceivedReward CreateFromCommonResponce(JsonData data)
	{
		return new ReceivedReward
		{
			reward_type = data["reward_type"].ToInt(),
			rewardUserGoodsId = data["reward_detail_id"].ToLong(),
			reward_count = data["reward_number"].ToInt()
		};
	}
}

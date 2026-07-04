using System.Collections.Generic;
using LitJson;

namespace Wizard.Bingo;

public class BingoDrawTask : BaseTask
{
	public class BingoDrawTaskParam : BaseParam
	{
	}

	public class BingoDrawTaskData
	{
		public bool IsSheetCompleted { get; private set; }

		public bool IsSwitchNextSheet { get; private set; }

		public List<int> HitSquareIdList { get; private set; }

		public List<ReceivedReward> TreasureBoxReawrdList { get; private set; }

		public List<ReceivedReward> LineRewardData { get; private set; }

		public List<List<int>> CompletedLineConditionList { get; private set; }

		public bool IsBingoLoginBonus { get; private set; }

		public bool IsDisplayTweetDialog { get; private set; }

		public BingoDrawTaskData(bool isSheetCompleted, bool isSwitchNextSheet, List<int> hitSquareIdList, List<ReceivedReward> treasureBoxReawrdList, List<ReceivedReward> lineRewardData, List<List<int>> completedLineConditionList, bool isBingoLoginBonus, bool isDisplayTweetDialog)
		{
			IsSheetCompleted = isSheetCompleted;
			IsSwitchNextSheet = isSwitchNextSheet;
			HitSquareIdList = hitSquareIdList;
			TreasureBoxReawrdList = treasureBoxReawrdList;
			LineRewardData = lineRewardData;
			CompletedLineConditionList = completedLineConditionList;
			IsBingoLoginBonus = isBingoLoginBonus;
			IsDisplayTweetDialog = isDisplayTweetDialog;
		}
	}

	public BingoDrawTaskData Result { get; private set; }

	private static List<int> CompletedLine(JsonData squareJsonData)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < squareJsonData.Count; i++)
		{
			list.Add(squareJsonData[i].ToInt());
		}
		return list;
	}

	public BingoDrawTask()
	{
		base.type = ApiType.Type.BingoDraw;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData response = base.ResponseData["data"];
		Result = ParseBingoDrawResult(response);
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}

	public static BingoDrawTaskData ParseBingoDrawResult(JsonData response, bool isBingoLoginBonus = false)
	{
		bool isSheetCompleted = response["is_sheet_completed"].ToInt() == 1;
		bool isSwitchNextSheet = response["is_switch_next_sheet"].ToInt() == 1;
		JsonData jsonData = response["hit_square_id_list"];
		List<int> list = new List<int>();
		for (int i = 0; i < jsonData.Count; i++)
		{
			int item = jsonData[i].ToInt();
			list.Add(item);
		}
		JsonData jsonData2 = response["treasure_box_reward_list"];
		List<ReceivedReward> list2 = new List<ReceivedReward>();
		for (int j = 1; j < jsonData2.Count; j++)
		{
			ReceivedReward item2 = ReceivedReward.CreateFromBingoTreasureBoxReward(jsonData2[j]);
			list2.Add(item2);
		}
		JsonData jsonData3 = response["line_reward_list"];
		List<ReceivedReward> list3 = new List<ReceivedReward>();
		for (int k = 1; k < jsonData3.Count; k++)
		{
			ReceivedReward item3 = ReceivedReward.CreateFromBingoLineReward(jsonData3[k]);
			list3.Add(item3);
		}
		JsonData jsonData4 = response["completed_line_condition_list"];
		List<List<int>> list4 = new List<List<int>>();
		for (int l = 0; l < jsonData4.Count; l++)
		{
			List<int> item4 = CompletedLine(jsonData4[l]);
			list4.Add(item4);
		}
		bool isDisplayTweetDialog = response["is_display_tweet_dialog"].ToBoolean();
		return new BingoDrawTaskData(isSheetCompleted, isSwitchNextSheet, list, list2, list3, list4, isBingoLoginBonus, isDisplayTweetDialog);
	}
}

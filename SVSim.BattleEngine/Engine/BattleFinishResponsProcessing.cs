using LitJson;
using Wizard;

public class BattleFinishResponsProcessing
{
	public void Processing(JsonData ResponseData, MatchFinishBase matchFinishData)
	{
		matchFinishData.IsProcessed = true;
		if (false /* Pre-Phase-5b: no BattleType headless */ && Data.CurrentFormat != Format.Crossover)
		{
			if (ResponseData["data"].Keys.Contains("target_grand_master_point"))
			{
				UserRank.IsGrandMasterAvailability = true;
			}
			else
			{
				UserRank.IsGrandMasterAvailability = false;
			}
		}
		Data.RedEtherCampaignResultData = null;
		if (false /* Pre-Phase-5b: no BattleType headless */)
		{
			Data.ArenaData.ColosseumData.ResultEffect = ArenaColosseum.eResultEffect.None;
		}
		RankMatchFinishDetail rankMatchFinishDetail = matchFinishData as RankMatchFinishDetail;
		matchFinishData._responseData = ResponseData;
		JsonData jsonData = ResponseData["data"];
		foreach (string key in jsonData.Keys)
		{
			JsonData jsonData2 = jsonData[key.ToString()];
			if (jsonData2 == null)
			{
				if (key.ToString() == "user_promotion_match" && UserRank.IsGrandMasterAvailability && PlayerStaticData.IsMasterRankCurrentFormat())
				{
					Data.Load.data._userRank[(int)Data.CurrentFormat].user_promotion_match.is_promotion = false;
				}
				continue;
			}
			switch (key.ToString())
			{
			case "battle_result":
				switch (jsonData2.ToInt())
				{
				case 0:
					matchFinishData.battleResult = BattleManagerBase.BATTLE_RESULT_TYPE.LOSE;
					break;
				case 1:
					matchFinishData.battleResult = BattleManagerBase.BATTLE_RESULT_TYPE.WIN;
					break;
				case 2:
					matchFinishData.battleResult = BattleManagerBase.BATTLE_RESULT_TYPE.CONSISTENCY;
					break;
				}
				/* Pre-Phase-5b: BattleResultType write dropped */
				break;
			case "get_class_experience":
				matchFinishData.get_class_chara_experience = jsonData2.ToInt();
				break;
			case "class_experience":
				matchFinishData.class_chara_experience = jsonData2.ToInt();
				break;
			case "class_level":
				matchFinishData.class_chara_level = jsonData2.ToInt();
				break;
			case "achieved_info":
				matchFinishData.AchievedInfo.Read(jsonData2);
				break;
			case "is_master_rank":
				Data.Load.data._userRank[(int)Data.CurrentFormat].is_master_rank = jsonData2.ToInt() != 0;
				break;
			case "is_grand_master_rank":
				Data.Load.data._userRank[(int)Data.CurrentFormat].is_grand_master_rank = jsonData2.ToInt() != 0;
				break;
			case "user_promotion_match":
				Data.Load.data._userRank[(int)Data.CurrentFormat].user_promotion_match.match_count = jsonData2["match_count"].ToInt();
				Data.Load.data._userRank[(int)Data.CurrentFormat].user_promotion_match.battle_result = jsonData2["battle_result"].ToInt();
				Data.Load.data._userRank[(int)Data.CurrentFormat].user_promotion_match.win = jsonData2["win"].ToInt();
				Data.Load.data._userRank[(int)Data.CurrentFormat].user_promotion_match.lose = jsonData2["lose"].ToInt();
				Data.Load.data._userRank[(int)Data.CurrentFormat].user_promotion_match.is_promotion = jsonData2["is_promotion"].ToBoolean();
				break;
			case "current_grand_master_point":
				Data.Load.data._userRank[(int)Data.CurrentFormat].grandMasterData.currentMasterPoint = jsonData2.ToInt();
				UserRank.IsGrandMasterAvailability = true;
				break;
			case "target_grand_master_point":
				Data.Load.data._userRank[(int)Data.CurrentFormat].grandMasterData.targetMasterPoint = jsonData2.ToInt();
				UserRank.IsGrandMasterAvailability = true;
				break;
			case "reward_list":
				PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(jsonData2);
				break;
			case "colosseum_special_params":
				if (jsonData2.Keys.Contains("next_round"))
				{
					Data.ArenaData.ColosseumData.ResultEffect = ArenaColosseum.eResultEffect.None;
					if (jsonData2["next_round"].ToInt() == 2)
					{
						Data.ArenaData.ColosseumData.ResultEffect = ArenaColosseum.eResultEffect.GroupA;
					}
					else if (jsonData2["next_round"].ToInt() == 3)
					{
						Data.ArenaData.ColosseumData.ResultEffect = ArenaColosseum.eResultEffect.Final;
					}
				}
				else if (jsonData2.Keys.Contains("is_champion") && jsonData2["is_champion"].ToBoolean())
				{
					Data.ArenaData.ColosseumData.ResultEffect = ArenaColosseum.eResultEffect.Clear;
				}
				break;
			case "red_ether_campagin_info":
				Data.RedEtherCampaignResultData = new RedEtherCampaignResultData(jsonData2);
				break;
			case "battle_dialog_list":
				if (rankMatchFinishDetail != null)
				{
					rankMatchFinishDetail.HomeDialogData = new MyPageHomeDialogData(ResponseData["data"], "battle_dialog_list");
				}
				break;
			case "upgrade_treasure_box_info":
				matchFinishData.TreasureBoxCpResultInfo.Parse(jsonData2);
				break;
			}
		}
	}
}

using LitJson;
using Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

namespace Wizard;

public class SkinPurchaseInfoTask : BaseTask
{
	public class SkinPurchaseInfoTaskParam : BaseParam
	{
	}

	public SkinPurchaseInfoTask()
	{
		base.type = ApiType.Type.SkinInfo;
	}

	public void SetParameter()
	{
		SkinPurchaseInfoTaskParam skinPurchaseInfoTaskParam = new SkinPurchaseInfoTaskParam();
		base.Params = skinPurchaseInfoTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		Data.SkinPurchaseInfo.seriesList.Clear();
		JsonData jsonData = base.ResponseData["data"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			JsonData jsonData2 = jsonData[i];
			int num2 = jsonData2["series_id"].ToInt();
			if (!Data.Master.LeaderSkinSeriesIdDic.ContainsKey(num2))
			{
				continue;
			}
			SkinSeriesPurchaseInfo skinSeriesPurchaseInfo = new SkinSeriesPurchaseInfo();
			skinSeriesPurchaseInfo.series_id = num2;
			skinSeriesPurchaseInfo.is_completed = jsonData2["is_completed"].ToBoolean();
			if (jsonData2.Keys.Contains("ticket_info"))
			{
				JsonData jsonData3 = jsonData2["ticket_info"];
				skinSeriesPurchaseInfo.HaveTicket = jsonData3["has_ticket"].ToBoolean();
			}
			else
			{
				skinSeriesPurchaseInfo.HaveTicket = false;
			}
			skinSeriesPurchaseInfo.IsNew = jsonData2.Keys.Contains("is_new") && jsonData2["is_new"].ToBoolean();
			skinSeriesPurchaseInfo.description = Data.Master.LeaderSkinSeriesIdDic[skinSeriesPurchaseInfo.series_id].Introduction;
			skinSeriesPurchaseInfo.SetSalesStatus = (SkinSeriesPurchaseInfo.eSetSalesStatus)jsonData2["set_sales_status"].ToInt();
			if (skinSeriesPurchaseInfo.SetSalesStatus != SkinSeriesPurchaseInfo.eSetSalesStatus.None)
			{
				skinSeriesPurchaseInfo._rewardStatus = (SkinSeriesPurchaseInfo.RewardStatus)jsonData2["rewards"]["status"].ToInt();
				skinSeriesPurchaseInfo.saleInfo = CreateSetSaleInfo(skinSeriesPurchaseInfo, jsonData2);
				JsonData jsonData4 = jsonData2["rewards"]["items"];
				for (int j = 0; j < jsonData4.Count; j++)
				{
					ShopCommonRewardInfo shopCommonRewardInfo = new ShopCommonRewardInfo();
					if (jsonData4[j].Keys.Contains("reward_type"))
					{
						shopCommonRewardInfo.Type = jsonData4[j]["reward_type"].ToInt();
					}
					if (jsonData4[j].Keys.Contains("reward_detail_id"))
					{
						shopCommonRewardInfo.UserGoodsId = jsonData4[j]["reward_detail_id"].ToLong();
					}
					if (jsonData4[j].Keys.Contains("reward_number"))
					{
						shopCommonRewardInfo.Num = jsonData4[j]["reward_number"].ToInt();
					}
					skinSeriesPurchaseInfo.rewardInfoList.Add(shopCommonRewardInfo);
				}
			}
			else
			{
				skinSeriesPurchaseInfo._rewardStatus = SkinSeriesPurchaseInfo.RewardStatus.none;
				skinSeriesPurchaseInfo.saleInfo = null;
			}
			JsonData jsonData5 = jsonData2["products"];
			for (int k = 0; k < jsonData5.Count; k++)
			{
				SkinProductInfo skinProductInfo = new SkinProductInfo();
				skinProductInfo.product_id = jsonData5[k]["product_id"].ToInt();
				string key = jsonData5[k]["introduction"].ToString();
				skinProductInfo.description = Data.Master.GetLeaderSkinProductText(key);
				string key2 = jsonData5[k]["cv_name"].ToString();
				skinProductInfo.cv_name = Data.Master.GetLeaderSkinProductText(key2);
				skinProductInfo.leader_skin_id = jsonData5[k]["leader_skin_id"].ToInt();
				skinProductInfo.is_purchased = jsonData5[k]["is_purchased"].ToBoolean();
				ShopCommonSaleInfo shopCommonSaleInfo = new ShopCommonSaleInfo();
				string key3 = jsonData5[k]["product_name"].ToString();
				shopCommonSaleInfo.name = Data.Master.GetLeaderSkinProductText(key3);
				shopCommonSaleInfo.path = skinProductInfo.leader_skin_id.ToString();
				if (jsonData5[k]["sale"].Keys.Contains("single_price_crystal"))
				{
					shopCommonSaleInfo.costCrystal = jsonData5[k]["sale"]["single_price_crystal"].ToInt();
				}
				if (jsonData5[k]["sale"].Keys.Contains("single_price_rupy"))
				{
					shopCommonSaleInfo.costRupy = jsonData5[k]["sale"]["single_price_rupy"].ToInt();
				}
				if (jsonData5[k]["sale"].Keys.Contains("single_price_ticket"))
				{
					shopCommonSaleInfo.costTicket = jsonData5[k]["sale"]["single_price_ticket"].ToInt();
				}
				if (jsonData5[k]["sale"].Keys.Contains("ticket_number"))
				{
					shopCommonSaleInfo.haveTicketNum = jsonData5[k]["sale"]["ticket_number"].ToInt();
				}
				if (jsonData5[k]["sale"].Keys.Contains("item_id"))
				{
					shopCommonSaleInfo.costTicketItemId = jsonData5[k]["sale"]["item_id"].ToLong();
				}
				shopCommonSaleInfo.expirtyTimeInfo = new ShopExpirtyInfo(jsonData5[k]["sales_period_info"]);
				shopCommonSaleInfo.isFree = false;
				if (shopCommonSaleInfo.costCrystal.HasValue && shopCommonSaleInfo.costRupy.HasValue && shopCommonSaleInfo.costCrystal <= 0 && shopCommonSaleInfo.costRupy <= 0)
				{
					shopCommonSaleInfo.isFree = true;
				}
				skinProductInfo.saleInfo = shopCommonSaleInfo;
				JsonData jsonData6 = jsonData5[k]["rewards"];
				for (int l = 0; l < jsonData6.Count; l++)
				{
					ShopCommonRewardInfo shopCommonRewardInfo2 = new ShopCommonRewardInfo();
					shopCommonRewardInfo2.Type = jsonData6[l]["reward_type"].ToInt();
					shopCommonRewardInfo2.UserGoodsId = jsonData6[l]["reward_detail_id"].ToLong();
					shopCommonRewardInfo2.Num = jsonData6[l]["reward_number"].ToInt();
					shopCommonRewardInfo2.IsAlreadyGet = jsonData6[l].GetValueOrDefault("is_owned", defaultValue: false);
					skinProductInfo.rewardInfoList.Add(shopCommonRewardInfo2);
				}
				skinSeriesPurchaseInfo.productList.Add(skinProductInfo);
			}
			Data.SkinPurchaseInfo.seriesList.Add(skinSeriesPurchaseInfo);
		}
		return num;
	}

	private ShopCommonSaleInfo CreateSetSaleInfo(SkinSeriesPurchaseInfo seriesInfo, JsonData seriesData)
	{
		ShopCommonSaleInfo shopCommonSaleInfo = new ShopCommonSaleInfo();
		shopCommonSaleInfo.name = Data.Master.LeaderSkinSeriesIdDic[seriesInfo.series_id].SetName;
		shopCommonSaleInfo.path = $"class_skin_set_{seriesInfo.series_id}";
		JsonData jsonData = seriesData["set_prices"];
		if (jsonData.Keys.Contains("set_price_crystal"))
		{
			shopCommonSaleInfo.costCrystal = jsonData["set_price_crystal"].ToInt();
		}
		if (jsonData.Keys.Contains("set_price_rupy"))
		{
			shopCommonSaleInfo.costRupy = jsonData["set_price_rupy"].ToInt();
		}
		if (jsonData.Keys.Contains("set_price_ticket"))
		{
			shopCommonSaleInfo.costTicket = jsonData["set_price_ticket"].ToInt();
			shopCommonSaleInfo.haveTicketNum = seriesData["ticket_info"]["ticket_number"].ToInt();
			shopCommonSaleInfo.costTicketItemId = jsonData["ticket_id"].ToInt();
		}
		shopCommonSaleInfo.expirtyTimeInfo = new ShopExpirtyInfo(seriesData["sales_period_info"]);
		shopCommonSaleInfo.isFree = false;
		if (shopCommonSaleInfo.costCrystal.HasValue && shopCommonSaleInfo.costRupy.HasValue && shopCommonSaleInfo.costCrystal <= 0 && shopCommonSaleInfo.costRupy <= 0)
		{
			shopCommonSaleInfo.isFree = true;
		}
		if (seriesInfo.is_completed && seriesInfo._rewardStatus == SkinSeriesPurchaseInfo.RewardStatus.not_got)
		{
			shopCommonSaleInfo.isFree = true;
		}
		return shopCommonSaleInfo;
	}
}

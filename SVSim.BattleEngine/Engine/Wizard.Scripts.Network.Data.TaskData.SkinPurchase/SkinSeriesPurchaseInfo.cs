using System.Collections.Generic;

namespace Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class SkinSeriesPurchaseInfo
{
	public enum RewardStatus
	{
		none,
		not_got	}

	public enum eSetSalesStatus
	{
		None,
		Disable
	}

	private List<ShopCommonRewardInfo> _rewardInfoList = new List<ShopCommonRewardInfo>();

	private List<SkinProductInfo> _ProductList = new List<SkinProductInfo>();

	public int series_id { get; set; }

	public string description { get; set; }

	public bool is_completed { get; set; }

	public RewardStatus _rewardStatus { get; set; }

	public bool HaveTicket { get; set; }

	public eSetSalesStatus SetSalesStatus { get; set; }

	public ShopCommonSaleInfo saleInfo { get; set; }

	public List<ShopCommonRewardInfo> rewardInfoList => _rewardInfoList;

	public List<SkinProductInfo> productList => _ProductList;

	public bool IsNew { get; set; }

	public int GetProductCount()
	{
		int num = productList.Count;
		if (SetSalesStatus != eSetSalesStatus.None)
		{
			num++;
		}
		return num;
	}
}

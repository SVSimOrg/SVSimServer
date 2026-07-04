using System.Collections.Generic;

namespace Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class SkinProductInfo
{
	private List<ShopCommonRewardInfo> _rewardInfoList = new List<ShopCommonRewardInfo>();

	public int product_id { get; set; }

	public string description { get; set; }

	public int leader_skin_id { get; set; }

	public string cv_name { get; set; }

	public bool is_purchased { get; set; }

	public ShopCommonSaleInfo saleInfo { get; set; }

	public List<ShopCommonRewardInfo> rewardInfoList => _rewardInfoList;

	public bool IsEnableBuyTicket
	{
		get
		{
			if (!saleInfo.costTicket.HasValue || !saleInfo.haveTicketNum.HasValue)
			{
				return false;
			}
			if (saleInfo.costTicket.Value > 0)
			{
				return saleInfo.haveTicketNum.Value > 0;
			}
			return false;
		}
	}
}

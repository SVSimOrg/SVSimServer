namespace Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class SkinBuyMultiTask : BaseTask
{
	public class SkinBuyMultiTaskParam : BaseParam
	{
		public int series_id;

		public int sales_type;

		public long? item_id;
	}

	public SkinBuyMultiTask()
	{
		base.type = ApiType.Type.SkinBuyMulti;
	}

	public void SetParameter(int series_id, ShopCommonUtility.SalesType sales_type, long? ticketItemId)
	{
		SkinBuyMultiTaskParam skinBuyMultiTaskParam = new SkinBuyMultiTaskParam();
		skinBuyMultiTaskParam.series_id = series_id;
		skinBuyMultiTaskParam.sales_type = (int)sales_type;
		skinBuyMultiTaskParam.item_id = ticketItemId;
		base.Params = skinBuyMultiTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}
}

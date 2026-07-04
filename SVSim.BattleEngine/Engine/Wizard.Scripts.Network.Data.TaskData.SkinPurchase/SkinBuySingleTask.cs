namespace Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class SkinBuySingleTask : BaseTask
{
	public class SkinBuySingleTaskParam : BaseParam
	{
		public int product_id;

		public int sales_type;

		public long? item_id;
	}

	public SkinBuySingleTask()
	{
		base.type = ApiType.Type.SkinBuySingle;
	}

	public void SetParameter(int productId, ShopCommonUtility.SalesType sales_type, long? item_id)
	{
		SkinBuySingleTaskParam skinBuySingleTaskParam = new SkinBuySingleTaskParam();
		skinBuySingleTaskParam.product_id = productId;
		skinBuySingleTaskParam.sales_type = (int)sales_type;
		skinBuySingleTaskParam.item_id = item_id;
		base.Params = skinBuySingleTaskParam;
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

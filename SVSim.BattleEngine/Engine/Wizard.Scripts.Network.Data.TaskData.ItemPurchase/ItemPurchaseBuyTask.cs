namespace Wizard.Scripts.Network.Data.TaskData.ItemPurchase;

public class ItemPurchaseBuyTask : BaseTask
{
	public class ItemPurchaseBuyTaskParam : BaseParam
	{
	}

	public ItemPurchaseBuyTask()
	{
		base.type = ApiType.Type.ItemPurchaseBuy;
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

namespace Wizard;

public class SleeveBuyTask : BaseTask
{
	public class SleeveBuyTaskParam : BaseParam
	{
		public int series_id;

		public int product_id;
	}

	public SleeveBuyTask()
	{
		base.type = ApiType.Type.SleeveBuy;
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

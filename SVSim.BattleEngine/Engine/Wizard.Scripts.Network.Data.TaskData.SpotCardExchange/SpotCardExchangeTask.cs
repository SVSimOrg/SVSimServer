namespace Wizard.Scripts.Network.Data.TaskData.SpotCardExchange;

public class SpotCardExchangeTask : BaseTask
{
	public class SpotCardExchangeTaskParam : BaseParam
	{
		public int card_id;
	}

	public SpotCardExchangeTask()
	{
		base.type = ApiType.Type.SpotCardExchange;
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

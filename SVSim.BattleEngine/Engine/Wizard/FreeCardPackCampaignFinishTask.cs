namespace Wizard;

public class FreeCardPackCampaignFinishTask : BaseTask
{
	public class FreeCardPackCampaignFinishTaskParam : BaseParam
	{
	}

	public FreeCardPackCampaignFinishTask()
	{
		base.type = ApiType.Type.FreeCardPackBoxFinish;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		if (base.ResponseData["data"].TryGetValue("reward_list", out var value))
		{
			PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(value);
		}
		return num;
	}
}

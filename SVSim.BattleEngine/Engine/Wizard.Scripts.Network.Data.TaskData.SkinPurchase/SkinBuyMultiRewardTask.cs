namespace Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class SkinBuyMultiRewardTask : BaseTask
{
	public class SkinBuyMultiRewardTaskParam : BaseParam
	{
		public int series_id;
	}

	public SkinBuyMultiRewardTask()
	{
		base.type = ApiType.Type.SkinBuyMultiReward;
	}

	public void SetParameter(int series_id)
	{
		SkinBuyMultiRewardTaskParam skinBuyMultiRewardTaskParam = new SkinBuyMultiRewardTaskParam();
		skinBuyMultiRewardTaskParam.series_id = series_id;
		base.Params = skinBuyMultiRewardTaskParam;
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

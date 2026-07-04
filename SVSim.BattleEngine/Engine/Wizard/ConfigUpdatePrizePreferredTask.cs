namespace Wizard;

public class ConfigUpdatePrizePreferredTask : BaseTask
{
	public class ConfigUpdatePrizePreferredTaskParam : BaseParam
	{
		public int is_prize_preferred;
	}

	public ConfigUpdatePrizePreferredTask()
	{
		base.type = ApiType.Type.ConfigUpdatePrizePreferred;
	}

	public void SetParameter(bool isPrizePreferred)
	{
		ConfigUpdatePrizePreferredTaskParam configUpdatePrizePreferredTaskParam = new ConfigUpdatePrizePreferredTaskParam();
		configUpdatePrizePreferredTaskParam.is_prize_preferred = (isPrizePreferred ? 1 : 0);
		base.Params = configUpdatePrizePreferredTaskParam;
	}
}

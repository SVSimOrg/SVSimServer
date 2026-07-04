namespace Wizard;

public class ConfigUpdateFoilPreferredTask : BaseTask
{
	public class ConfigUpdateFoilPreferredTaskParam : BaseParam
	{
		public int is_foil_preferred;
	}

	public ConfigUpdateFoilPreferredTask()
	{
		base.type = ApiType.Type.ConfigUpdateFoilPreferred;
	}

	public void SetParameter(bool isFoilPreferred)
	{
		ConfigUpdateFoilPreferredTaskParam configUpdateFoilPreferredTaskParam = new ConfigUpdateFoilPreferredTaskParam();
		configUpdateFoilPreferredTaskParam.is_foil_preferred = (isFoilPreferred ? 1 : 0);
		base.Params = configUpdateFoilPreferredTaskParam;
	}
}

namespace Wizard;

public class CrossoverPortalParam
{

	public bool IsSkipFirstTips { get; set; }

	public static CrossoverPortalParam CreateParam(int? status)
	{
		CrossoverPortalParam crossoverPortalParam = new CrossoverPortalParam();
		if (status.HasValue && status.Value == 1)
		{
			crossoverPortalParam.IsSkipFirstTips = true;
		}
		return crossoverPortalParam;
	}
}

namespace Wizard;

public class PracticeInfoTask : BaseTask
{
	public PracticeInfoTask()
	{
		base.type = ApiType.Type.PracticeInfo;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		Data.PracticeDataMgr = new PracticeDataMgr(base.ResponseData["data"]);
		return num;
	}
}

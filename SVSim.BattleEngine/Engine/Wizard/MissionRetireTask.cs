namespace Wizard;

public class MissionRetireTask : BaseTask
{
	public class MissionRetireTaskParam : BaseParam
	{
		public int id;
	}

	public MissionRetireTask()
	{
		base.type = ApiType.Type.MissionRetire;
	}

	public void SetParameter(int id)
	{
		MissionRetireTaskParam missionRetireTaskParam = new MissionRetireTaskParam();
		missionRetireTaskParam.id = id;
		base.Params = missionRetireTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		Data.MissionInfo.data = new MissionInfoDetail(base.ResponseData["data"]);
		return num;
	}
}

using UnityEngine;

namespace Wizard;

public class MissionInfoTask : BaseTask
{
	public class MissionInfoTaskParam : BaseParam
	{
	}

	public long ServerTime { get; private set; }

	public long RequestTime { get; private set; }

	public MissionInfoTask()
	{
		base.type = ApiType.Type.MissionInfo;
	}

	public void SetParameter()
	{
		MissionInfoTaskParam missionInfoTaskParam = new MissionInfoTaskParam();
		base.Params = missionInfoTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		ServerTime = base.ResponseData["data_headers"]["servertime"].ToLong();
		RequestTime = (long)Time.realtimeSinceStartup;
		Data.MissionInfo.data = new MissionInfoDetail(base.ResponseData["data"]);
		return num;
	}
}

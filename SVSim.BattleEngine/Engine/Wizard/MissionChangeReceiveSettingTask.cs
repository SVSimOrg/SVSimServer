namespace Wizard;

public class MissionChangeReceiveSettingTask : BaseTask
{
	public class MissionChangeReceiveSettingTaskParam : BaseParam
	{
		public int mission_receive_type;
	}

	public MissionChangeReceiveSettingTask()
	{
		base.type = ApiType.Type.MissionChangeReceiveSetting;
	}

	public void SetParameter(MissionInfoDetail.eMissionReceiveType missionReceiveType)
	{
		MissionChangeReceiveSettingTaskParam missionChangeReceiveSettingTaskParam = new MissionChangeReceiveSettingTaskParam();
		missionChangeReceiveSettingTaskParam.mission_receive_type = (int)missionReceiveType;
		base.Params = missionChangeReceiveSettingTaskParam;
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

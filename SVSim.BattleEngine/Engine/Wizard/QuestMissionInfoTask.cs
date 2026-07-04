namespace Wizard;

public class QuestMissionInfoTask : BaseTask
{
	public QuestMissionInfoTask()
	{
		base.type = ApiType.Type.QuestMission;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		Data.QuestMissionInfo = new QuestMissionInfo(base.ResponseData["data"]);
		return num;
	}
}

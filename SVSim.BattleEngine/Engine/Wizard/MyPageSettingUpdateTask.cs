using System.Collections.Generic;

namespace Wizard;

public class MyPageSettingUpdateTask : BaseTask
{
	public class TaskParam : BaseParam
	{
		public int select_type;

		public string mypage_id;

		public string[] mypage_id_list;
	}

	public MyPageSettingUpdateTask()
	{
		base.type = ApiType.Type.MyPageSettingUpdate;
	}

	public void SetParameter(MyPageDetail.BGType type, string id, List<string> randomList)
	{
		TaskParam taskParam = new TaskParam();
		taskParam.select_type = (int)type;
		taskParam.mypage_id = id;
		taskParam.mypage_id_list = randomList.ToArray();
		base.Params = taskParam;
	}
}

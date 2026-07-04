using System.Collections.Generic;
using LitJson;
using Wizard.Battle.Recovery;

namespace Wizard;

public class PracticeFinishTask : BaseTask
{
	public class PracticeFinishTaskParam : BaseParam
	{
		public int deck_no;

		public int deck_format;

		public int class_id;

		public Dictionary<string, int> mission;
	}

	public PracticeFinishTask()
	{
		base.type = ApiType.Type.PracticeFinish;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			DeleteRecoveryFileIfBattleAlreadyEnded(num);
			return num;
		}
		Data.PracticeFinish.data = new PracticeFinishDetail();
		Data.PracticeFinish.data._responseData = base.ResponseData;
		Data.PracticeFinish.data.class_chara_experience = 0;
		Data.PracticeFinish.data.class_chara_level = 0;
		Data.PracticeFinish.data.get_class_chara_experience = base.ResponseData["data"]["get_class_experience"].ToInt();
		Data.PracticeFinish.data.class_chara_experience = base.ResponseData["data"]["class_experience"].ToInt();
		Data.PracticeFinish.data.class_chara_level = base.ResponseData["data"]["class_level"].ToInt();
		JsonData jsonData = base.ResponseData["data"]["achieved_info"];
		if (jsonData.GetJsonType() == JsonType.Object)
		{
			Data.PracticeFinish.data.AchievedInfo.Read(jsonData);
		}
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}
}

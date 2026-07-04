using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class CardCreateTask : BaseTask
{
	public class CardCreateTaskParam : BaseParam
	{
	}

	public CardCreateTask()
	{
		base.type = ApiType.Type.CardCreate;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		return num;
	}
}

using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class CardDestructTask : BaseTask
{
	public class CardDestructTaskParam : BaseParam
	{
	}

	public CardDestructTask()
	{
		base.type = ApiType.Type.CardDestruct;
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

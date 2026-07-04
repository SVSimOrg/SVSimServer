using LitJson;

namespace Wizard;

public class SealedSelectPhantomCardTask : BaseTask
{
	public class Param : BaseParam
	{
		public int select_card_id;

		public bool is_retire;

		public Param(int selectCardId, bool isRetire)
		{
			select_card_id = selectCardId;
			is_retire = isRetire;
		}
	}

	public SealedSelectPhantomCardTask(int selectCardId)
	{
		base.type = ApiType.Type.ArenaSealedSelectPhantomCard;
		base.Params = new Param(selectCardId, Data.ArenaData.SealedData.IsRetired.Value);
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		SealedData sealedData = Data.ArenaData.SealedData;
		sealedData.SetRewardInfo(jsonData);
		sealedData.UpdateHaveUserGoodsNum(jsonData);
		return num;
	}
}

using LitJson;

namespace Wizard;

public class SealedRetireTask : BaseTask
{
	public SealedRetireTask()
	{
		base.type = ApiType.Type.ArenaSealedRetire;
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
		sealedData.SetRetired(isRetired: true);
		sealedData.SetRewardCardCandidates(jsonData);
		sealedData.SetRewardInfo(jsonData);
		sealedData.UpdateHaveUserGoodsNum(jsonData);
		return num;
	}
}

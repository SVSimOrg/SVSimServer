using LitJson;

namespace Wizard;

public class SealedFinishTask : BaseTask
{
	public SealedFinishTask()
	{
		base.type = ApiType.Type.ArenaSealedFinish;
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
		sealedData.SetRewardCardCandidates(jsonData);
		sealedData.SetRewardInfo(jsonData);
		sealedData.UpdateHaveUserGoodsNum(jsonData);
		return num;
	}
}

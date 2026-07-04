using LitJson;

namespace Wizard;

public class SealedTopTask : BaseTask
{
	public SealedTopTask()
	{
		base.type = ApiType.Type.ArenaSealedTop;
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
		sealedData.SetEntryInfo(jsonData);
		sealedData.SetClassInfo(jsonData);
		sealedData.SetSealedCardInfo(jsonData, isRegisterSealedCard: true);
		sealedData.SetDeckCompleted(jsonData);
		sealedData.SetBattleResultInfo(jsonData);
		return num;
	}
}

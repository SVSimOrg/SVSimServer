using LitJson;

namespace Wizard;

public class SealedSelectClassTask : BaseTask
{
	public class Param : BaseParam
	{
		public int class_id;

		public Param(int classId)
		{
			class_id = classId;
		}
	}

	public SealedSelectClassTask(int classId)
	{
		base.type = ApiType.Type.ArenaSealedSelectClass;
		base.Params = new Param(classId);
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
		sealedData.UpdateHaveUserGoodsNum(jsonData);
		sealedData.SetSelectedClassId((base.Params as Param).class_id);
		sealedData.SetGachaCardInfo(jsonData);
		sealedData.SetGachaSupplyInfo(jsonData);
		sealedData.SetSealedCardInfo(jsonData, isRegisterSealedCard: true);
		sealedData.SetDeckCompleted(jsonData);
		sealedData.SetIsSpecialEffect(jsonData);
		return num;
	}
}

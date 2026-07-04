using LitJson;

namespace Wizard;

public class SealedGetMaintCardListTask : BaseTask
{
	public SealedGetMaintCardListTask()
	{
		base.type = ApiType.Type.ArenaSealedGetMaintCardList;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"]["maintenance_card_list"];
		int count = jsonData.Count;
		int[] array = ((count > 0) ? new int[count * 3] : null);
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			int num3 = jsonData[i]["card_id"].ToInt();
			array[num2++] = num3;
			array[num2++] = SealedData.ConvertToSealedCardId(num3, isPhantomCard: false);
			array[num2++] = SealedData.ConvertToSealedCardId(num3, isPhantomCard: true);
		}
		/* Pre-Phase-5b: SetMaintenanceCardIds dropped */
		return num;
	}
}

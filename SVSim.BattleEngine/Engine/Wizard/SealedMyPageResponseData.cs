using LitJson;

namespace Wizard;

public class SealedMyPageResponseData
{
	public ArenaEntryDataBase EntryData { get; private set; }

	public int ScheduleId { get; private set; }

	public int DeckCardNumMin { get; private set; }

	public string[] CardPackIdList { get; private set; }

	public ShopExpirtyInfo ExpirtyInfo { get; private set; }

	public SealedMyPageResponseData(JsonData data)
	{
		EntryData = new SealedEntryData(data);
		ScheduleId = data["schedule_id"].ToInt();
		DeckCardNumMin = data["deck_using_num_min"].ToInt();
		SetCardPackInfo(data);
		ExpirtyInfo = new ShopExpirtyInfo(data["sales_period_info"]);
	}

	private void SetCardPackInfo(JsonData rootData)
	{
		JsonData jsonData = rootData["pack_info"];
		int count = jsonData.Count;
		CardPackIdList = new string[count];
		for (int i = 0; i < count; i++)
		{
			CardPackIdList[i] = jsonData[i].ToString();
		}
	}
}

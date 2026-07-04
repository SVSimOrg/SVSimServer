using LitJson;
using Wizard;

public class ArenaTwoPickData : ArenaEntryDataBase
{
	public ChallengeData ChallengeData { get; private set; }

	public ArenaTwoPickData(JsonData data)
	{
		isJoin = data["is_join"].ToBoolean();
		crystalCost = data["cost"].ToInt();
		rupyCost = data["rupy_cost"].ToInt();
		ticketCost = data["ticket_cost"].ToInt();
		base.LootBoxType = PlayerStaticData.LootBoxType.TWOPICK;
		if (data.Keys.Contains("sales_period_info"))
		{
			base.ExpirtyInfo = new ShopExpirtyInfo(data["sales_period_info"]);
		}
		if (data.Keys.Contains("format_info"))
		{
			ChallengeData = new ChallengeData(data["format_info"]);
		}
	}
}

using LitJson;

namespace Wizard;

public class SealedEntryData : ArenaEntryDataBase
{
	public SealedEntryData(JsonData rootData)
	{
		isJoin = rootData["is_join"].ToInt() == 1;
		crystalCost = rootData["crystal_cost"].ToInt();
		rupyCost = rootData["rupy_cost"].ToInt();
		ticketCost = rootData["ticket_cost"].ToInt();
		base.LootBoxType = PlayerStaticData.LootBoxType.SEALED;
	}
}

using Wizard;

public abstract class ArenaEntryDataBase
{
	public bool isJoin;

	public int crystalCost;

	public int rupyCost;

	public int ticketCost;

	public ShopExpirtyInfo ExpirtyInfo { get; set; }

	public PlayerStaticData.LootBoxType LootBoxType { get; set; }
}

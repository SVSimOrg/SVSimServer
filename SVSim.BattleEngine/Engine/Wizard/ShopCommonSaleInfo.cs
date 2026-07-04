namespace Wizard;

public class ShopCommonSaleInfo
{
	public string name { get; set; }

	public string path { get; set; }

	public bool isFree { get; set; }

	public int? costCrystal { get; set; }

	public int? costRupy { get; set; }

	public int? costTicket { get; set; }

	public int? haveTicketNum { get; set; }

	public long? costTicketItemId { get; set; }

	public ShopExpirtyInfo expirtyTimeInfo { get; set; }
}

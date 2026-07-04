public class CardPack : HeaderData
{
	public int card_id;

	public int rarity;

	public string create_time;

	public int SleeveId { get; set; } = 3000011;

	public bool IsSpecialCard { get; set; }

	public bool IsFreePackLeaderSkin { get; set; }
}

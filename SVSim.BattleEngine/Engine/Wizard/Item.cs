namespace Wizard;

public class Item
{
	public enum Type
	{
		TwoPickTicket = 1,
		CardPackTicket,
		Orb,
		ColosseumTicket,
		OrbPiece,
		SpotCardBuildDeckTicket,
		LeaderSkinTicket
	}

	public Type type;

	public int UserGoodsId;

	public string name;

	public string thumbnail;

	public string unit;

	public string unitFormat;

	private int index;

	public Item()
	{
	}

	public Item(string[] columns)
	{
		string text = columns[index++];
		type = (Type)int.Parse(columns[index++]);
		UserGoodsId = int.Parse(text);
		name = Data.Master.GetItemText("IT_" + text);
		thumbnail = columns[index++];
		SystemText systemText = Data.SystemText;
		switch (type)
		{
		case Type.TwoPickTicket:
		case Type.CardPackTicket:
		case Type.ColosseumTicket:
		case Type.SpotCardBuildDeckTicket:
		case Type.LeaderSkinTicket:
			unit = systemText.Get("Common_0117");
			unitFormat = systemText.Get("Mail_0041");
			break;
		case Type.Orb:
		case Type.OrbPiece:
			unit = systemText.Get("Common_0116");
			unitFormat = systemText.Get("Mail_0040");
			break;
		}
	}

	public static Item GetItemData(UserGoods.Type goodsType, int userGoodsId)
	{
		Item result = null;
		if (goodsType == UserGoods.Type.Item)
		{
			result = Data.Master.ItemList.Find((Item data) => data.UserGoodsId == userGoodsId);
		}
		return result;
	}
}

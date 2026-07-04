using LitJson;
using Wizard.AutoTest;

namespace Wizard.Battle.Recovery;

public class CardInfoBase
{
	public string Name { get; private set; }

	public bool IsPlayer { get; private set; }

	public int Index { get; private set; }

	public int? CardId { get; private set; }

	public CardInfoBase(JsonData jsonData)
	{
		Name = jsonData["index"].ToString();
		IsPlayer = Name[0] == 'p';
		Index = int.Parse(Name.Substring(1));
		CardId = jsonData.ToIntOrNull("id");
	}
}

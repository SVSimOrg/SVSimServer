using System.Collections.Generic;

namespace Wizard;

public class DeckGroup
{
	public Format DeckFormat { get; private set; }

	public DeckAttributeType AttributeType { get; private set; }

	public List<DeckData> DeckDataList { get; private set; }

	public DeckGroup(List<DeckData> deckDataList, Format format, DeckAttributeType deckAttributeType)
	{
		DeckDataList = deckDataList;
		DeckFormat = format;
		AttributeType = deckAttributeType;
	}

	public DeckGroup Clone()
	{
		List<DeckData> datas = new List<DeckData>();
		DeckDataList.ForEach(delegate(DeckData d)
		{
			datas.Add(d.Clone());
		});
		return new DeckGroup(datas, DeckFormat, AttributeType);
	}

	public void MaintenanceCardCheack()
	{
		DeckDataList.ForEach(delegate(DeckData deckData)
		{
			deckData.MaintenanceCardCheack();
		});
	}
}

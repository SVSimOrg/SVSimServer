using System.Collections.Generic;

namespace Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class SkinPurchaseInfo
{
	private List<SkinSeriesPurchaseInfo> _SeriesList = new List<SkinSeriesPurchaseInfo>();

	public List<SkinSeriesPurchaseInfo> seriesList => _SeriesList;
}

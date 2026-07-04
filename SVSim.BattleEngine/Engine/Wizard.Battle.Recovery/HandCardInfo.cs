using LitJson;
using Wizard.AutoTest;

namespace Wizard.Battle.Recovery;

public class HandCardInfo : CardInfoBase
{
	public int? Cost { get; private set; }

	public int? ChargeCount { get; private set; }

	public HandCardInfo(JsonData jsonData, bool useDefaultCardValue)
		: base(jsonData)
	{
		Cost = jsonData.ToIntOrNull("cost");
		ChargeCount = jsonData.ToIntOrNull("chage_count");
		if (useDefaultCardValue)
		{
			ChargeCount = ChargeCount.GetValueOrDefault();
		}
	}
}

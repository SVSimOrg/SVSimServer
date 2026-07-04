using LitJson;
using Wizard.AutoTest;

namespace Wizard.Battle.Recovery;

public class InPlayCardInfo : CardInfoBase
{
	public int? Life { get; private set; }

	public int? Offense { get; private set; }

	public bool? Guard { get; private set; }

	public bool? Killer { get; private set; }

	public int? AttackableCount { get; private set; }

	public bool? Evolve { get; private set; }

	public InPlayCardInfo(JsonData jsonData, bool useDefaultCardValue)
		: base(jsonData)
	{
		Life = jsonData.ToIntOrNull("life");
		Offense = jsonData.ToIntOrNull("offense");
		Guard = jsonData.ToBooleanOrNull("guard");
		Killer = jsonData.ToBooleanOrNull("killer");
		AttackableCount = jsonData.ToIntOrNull("attack_count");
		Evolve = jsonData.ToBooleanOrNull("evolve");
		if (useDefaultCardValue)
		{
			if (base.Index == 0)
			{
				Life = Life ?? 20;
				return;
			}
			CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(base.CardId.Value);
			Life = Life ?? cardParameterFromId.Life;
			Evolve = Evolve == true;
		}
	}
}

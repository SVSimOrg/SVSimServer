public class CostSetModifier : ICardCostModifier
{
	public int Cost { get; private set; }

	public bool IsClearBeforeModifier => true;

	public bool IsResidentModifier { get; }

	public CostSetModifier(int cost, bool isResidentModifier = false)
	{
		Cost = cost;
		IsResidentModifier = isResidentModifier;
	}

	public int CalcCost(int cost)
	{
		return Cost;
	}

	public ICardCostModifier Clone()
	{
		return new CostSetModifier(Cost, IsResidentModifier);
	}
}

public class CostAddModifier : ICardCostModifier
{
	public int Cost { get; private set; }

	public bool IsClearBeforeModifier => false;

	public bool IsResidentModifier { get; }

	public CostAddModifier(int cost, bool isResidentModifier = false)
	{
		Cost = cost;
		IsResidentModifier = isResidentModifier;
	}

	public int CalcCost(int cost)
	{
		return cost + Cost;
	}

	public ICardCostModifier Clone()
	{
		return new CostAddModifier(Cost, IsResidentModifier);
	}
}

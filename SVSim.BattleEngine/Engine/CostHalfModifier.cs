public abstract class CostHalfModifier : ICardCostModifier
{
	public int Cost { get; private set; }

	public bool IsClearBeforeModifier => false;

	public bool IsResidentModifier { get; }

	public CostHalfModifier(bool isResidentModifier)
	{
		IsResidentModifier = isResidentModifier;
	}

	public abstract int CalcCost(int cost);

	public abstract ICardCostModifier Clone();
}

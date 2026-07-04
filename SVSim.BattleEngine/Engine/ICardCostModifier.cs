public interface ICardCostModifier
{
	int Cost { get; }

	bool IsClearBeforeModifier { get; }

	bool IsResidentModifier { get; }

	int CalcCost(int baseCost);

	ICardCostModifier Clone();
}

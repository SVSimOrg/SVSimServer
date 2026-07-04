public interface ICardChantCountModifier
{
	bool IsClearBeforeModifier { get; }

	int CalcChantCount(int baseCost);

	ICardChantCountModifier Clone();
}

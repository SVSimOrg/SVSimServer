public interface ICardUnionBurstCountModifier
{
	bool IsClearBeforeModifier { get; }

	int CalcUnionBurstCount(int count);

	ICardUnionBurstCountModifier Clone();
}

public interface ICardLifeModifier
{
	bool IsChangeMaxLife { get; }

	bool IsClearBeforeModifier { get; }

	int CalcLife(int baseLife);

	int CalcMaxLife(int baseMaxLife);
}

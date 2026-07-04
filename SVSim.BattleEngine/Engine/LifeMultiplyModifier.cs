public class LifeMultiplyModifier : ICardLifeModifier
{
	public readonly int Multipli;

	public bool IsClearBeforeModifier => false;

	public bool IsChangeMaxLife => true;

	public LifeMultiplyModifier(int multipli)
	{
		Multipli = multipli;
	}

	public int CalcLife(int baseLife)
	{
		return baseLife * Multipli;
	}

	public int CalcMaxLife(int baseMaxLife)
	{
		return baseMaxLife * Multipli;
	}
}

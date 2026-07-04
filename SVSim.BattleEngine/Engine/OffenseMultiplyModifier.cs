public class OffenseMultiplyModifier : ICardOffenseModifier
{
	public readonly int Multipli;

	public bool IsClearBeforeModifier => false;

	public OffenseMultiplyModifier(int multipli)
	{
		Multipli = multipli;
	}

	public int CalcOffense(int offense)
	{
		return offense * Multipli;
	}
}

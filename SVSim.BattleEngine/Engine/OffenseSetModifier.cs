public class OffenseSetModifier : ICardOffenseModifier
{
	public readonly int Offense;

	public bool IsClearBeforeModifier => true;

	public OffenseSetModifier(int offense)
	{
		Offense = offense;
	}

	public int CalcOffense(int offense)
	{
		return Offense;
	}
}

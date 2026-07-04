public class OffenseAddModifier : ICardOffenseModifier
{
	public readonly int Offense;

	public bool IsClearBeforeModifier => false;

	public OffenseAddModifier(int offense)
	{
		Offense = offense;
	}

	public int CalcOffense(int offense)
	{
		return offense + Offense;
	}
}

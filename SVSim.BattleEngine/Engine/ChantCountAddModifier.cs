public class ChantCountAddModifier : ICardChantCountModifier
{
	public int ChantCount { get; private set; }

	public bool IsClearBeforeModifier => false;

	public ChantCountAddModifier(int chantCount)
	{
		ChantCount = chantCount;
	}

	public int CalcChantCount(int chantCount)
	{
		return chantCount + ChantCount;
	}

	public ICardChantCountModifier Clone()
	{
		return new ChantCountAddModifier(ChantCount);
	}
}

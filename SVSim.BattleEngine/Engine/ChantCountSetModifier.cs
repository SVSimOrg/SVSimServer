public class ChantCountSetModifier : ICardChantCountModifier
{
	public readonly int ChantCount;

	public bool IsClearBeforeModifier => true;

	public ChantCountSetModifier(int chantCount)
	{
		ChantCount = chantCount;
	}

	public int CalcChantCount(int chantCount)
	{
		return ChantCount;
	}

	public ICardChantCountModifier Clone()
	{
		return new ChantCountSetModifier(ChantCount);
	}
}

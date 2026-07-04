public class UnionBurstCountAddModifier : ICardUnionBurstCountModifier
{
	public readonly int UnionBurstCount;

	public bool IsClearBeforeModifier => true;

	public UnionBurstCountAddModifier(int count)
	{
		UnionBurstCount = count;
	}

	public int CalcUnionBurstCount(int unionBurstCount)
	{
		return unionBurstCount += UnionBurstCount;
	}

	public ICardUnionBurstCountModifier Clone()
	{
		return new UnionBurstCountAddModifier(UnionBurstCount);
	}
}

public class SkyboundArtCountAddModifier : ICardSkyboundArtCountModifier
{
	public readonly int SkyboundArtCount;

	public bool IsClearBeforeModifier => true;

	public SkyboundArtCountAddModifier(int count)
	{
		SkyboundArtCount = count;
	}

	public int CalcSkyboundArtCount(int skyboundArtCount)
	{
		return skyboundArtCount += SkyboundArtCount;
	}

	public ICardSkyboundArtCountModifier Clone()
	{
		return new SkyboundArtCountAddModifier(SkyboundArtCount);
	}
}

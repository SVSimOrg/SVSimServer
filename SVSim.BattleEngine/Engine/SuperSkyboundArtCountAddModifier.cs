public class SuperSkyboundArtCountAddModifier : ICardSuperSkyboundArtCountModifier
{
	public readonly int SuperSkyboundArtCount;

	public bool IsClearBeforeModifier => true;

	public SuperSkyboundArtCountAddModifier(int count)
	{
		SuperSkyboundArtCount = count;
	}

	public int CalcSuperSkyboundArtCount(int superSkyboundArtCount)
	{
		return superSkyboundArtCount += SuperSkyboundArtCount;
	}

	public ICardSuperSkyboundArtCountModifier Clone()
	{
		return new SuperSkyboundArtCountAddModifier(SuperSkyboundArtCount);
	}
}

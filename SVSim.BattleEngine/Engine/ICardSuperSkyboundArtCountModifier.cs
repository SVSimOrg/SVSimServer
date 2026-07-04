public interface ICardSuperSkyboundArtCountModifier
{
	bool IsClearBeforeModifier { get; }

	int CalcSuperSkyboundArtCount(int count);

	ICardSuperSkyboundArtCountModifier Clone();
}

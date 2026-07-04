public interface ICardSkyboundArtCountModifier
{
	bool IsClearBeforeModifier { get; }

	int CalcSkyboundArtCount(int count);

	ICardSkyboundArtCountModifier Clone();
}

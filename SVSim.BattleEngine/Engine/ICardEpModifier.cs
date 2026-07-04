public interface ICardEpModifier
{
	bool IsClearBeforeModifier { get; }

	int CalcEp(int baseEp);
}

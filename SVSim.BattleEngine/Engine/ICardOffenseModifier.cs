public interface ICardOffenseModifier
{
	bool IsClearBeforeModifier { get; }

	int CalcOffense(int offense);
}

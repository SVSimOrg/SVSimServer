public class CausedDamageCardParameterModifier : TurnAndIntValue
{
	public int Damage => base.Value;

	public CausedDamageCardParameterModifier(int damage, int turn, bool isSelfTurn)
		: base(damage, turn, isSelfTurn)
	{
	}
}

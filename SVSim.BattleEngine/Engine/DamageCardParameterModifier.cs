public class DamageCardParameterModifier : TurnAndIntValue, ICardLifeModifier
{
	public int Damage => base.Value;

	public bool IsClearBeforeModifier => false;

	public bool IsChangeMaxLife => false;

	public DamageCardParameterModifier(int damage, int turn, bool isSelfTurn)
		: base(damage, turn, isSelfTurn)
	{
	}

	public int CalcLife(int baseLife)
	{
		return baseLife - Damage;
	}

	public int CalcMaxLife(int baseMaxLife)
	{
		return baseMaxLife;
	}
}

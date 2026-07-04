public class HealCardParameterModifier : TurnAndIntValue, ICardLifeModifier
{
	public int Heal => base.Value;

	public bool IsClearBeforeModifier => false;

	public bool IsChangeMaxLife => false;

	public HealCardParameterModifier(int heal, int turn, bool isSelfTurn)
		: base(heal, turn, isSelfTurn)
	{
	}

	public int CalcLife(int baseLife)
	{
		return baseLife + Heal;
	}

	public int CalcMaxLife(int baseMaxLife)
	{
		return baseMaxLife;
	}
}

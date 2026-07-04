public class LifeSetModifier : ICardLifeModifier
{
	public readonly int Life;

	public virtual bool IsClearBeforeModifier => true;

	public bool IsChangeMaxLife => true;

	public LifeSetModifier(int life)
	{
		Life = life;
	}

	public virtual int CalcLife(int baseLife)
	{
		return Life;
	}

	public int CalcMaxLife(int baseMaxLife)
	{
		return Life;
	}
}

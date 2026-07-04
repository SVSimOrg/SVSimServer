public class LifeAddModifier : ICardLifeModifier
{
	public readonly int Life;

	public bool IsClearBeforeModifier => false;

	public bool IsChangeMaxLife => true;

	public LifeAddModifier(int life)
	{
		Life = life;
	}

	public virtual int CalcLife(int baseLife)
	{
		return baseLife + Life;
	}

	public int CalcMaxLife(int baseMaxLife)
	{
		return baseMaxLife + Life;
	}
}

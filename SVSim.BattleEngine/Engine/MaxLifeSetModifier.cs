public class MaxLifeSetModifier : LifeSetModifier
{
	public override bool IsClearBeforeModifier => false;

	public MaxLifeSetModifier(int life)
		: base(life)
	{
	}

	public override int CalcLife(int baseLife)
	{
		return baseLife;
	}
}

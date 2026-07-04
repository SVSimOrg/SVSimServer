public class MaxLifeAddModifier : LifeAddModifier
{
	public MaxLifeAddModifier(int life)
		: base(life)
	{
	}

	public override int CalcLife(int baseLife)
	{
		return baseLife;
	}
}

public abstract class HealModifier
{
	protected BattleCardBase _owner;

	public int OrderCount { get; protected set; }

	public abstract int Calc(int healAmount, BattleCardBase healOwner, BattleCardBase target);
}

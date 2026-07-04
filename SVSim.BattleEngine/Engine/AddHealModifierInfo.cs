public class AddHealModifierInfo : HealModifier
{
	public int AddHealAmount { get; private set; }

	public AddHealModifierInfo(int addHealAmount, int order, BattleCardBase owner)
	{
		AddHealAmount = addHealAmount;
		base.OrderCount = order;
		_owner = owner;
	}

	public override int Calc(int healAmount, BattleCardBase healOwner, BattleCardBase target)
	{
		if (healOwner.IsPlayer != _owner.IsPlayer)
		{
			return healAmount;
		}
		return healAmount + AddHealAmount;
	}
}

public class SetHealModifierInfo : HealModifier
{
	private bool _isTargetSelfClass;

	public int SetHealAmount { get; private set; }

	public SetHealModifierInfo(int setHealAmount, int order, BattleCardBase owner, bool isTargetSelfClass)
	{
		SetHealAmount = setHealAmount;
		base.OrderCount = order;
		_owner = owner;
		_isTargetSelfClass = isTargetSelfClass;
	}

	public override int Calc(int healAmount, BattleCardBase healOwner, BattleCardBase target)
	{
		if (_isTargetSelfClass)
		{
			if (target.IsClass && _owner == target)
			{
				return SetHealAmount;
			}
			return healAmount;
		}
		return SetHealAmount;
	}
}

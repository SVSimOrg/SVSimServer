namespace Wizard;

public class AIBuffExecutingInfo_old
{
	public int AttackValue;

	public int LifeValue;

	public bool IsMultiplyAttack;

	public bool IsMultiplyLife;

	public bool IsEmpty()
	{
		if (AttackValue == 0 && LifeValue == 0 && !IsMultiplyAttack)
		{
			return !IsMultiplyLife;
		}
		return false;
	}

	public bool IsBuff()
	{
		bool flag = ((!IsMultiplyAttack) ? (AttackValue >= 0) : (AttackValue >= 1));
		bool flag2 = ((!IsMultiplyLife) ? (LifeValue >= 0) : (LifeValue >= 1));
		return flag && flag2;
	}

	public int GetExpectedAttackBuffValue(AIVirtualCard target)
	{
		return GetExpectedAttackBuffValue(target.Attack);
	}

	public int GetExpectedAttackBuffValue(int attack)
	{
		if (IsMultiplyAttack)
		{
			return attack * AttackValue - attack;
		}
		return AttackValue;
	}

	public int GetExpectedLifeBuffValue(AIVirtualCard target)
	{
		return GetExpectedLifeBuffValue(target.Life);
	}

	public int GetExpectedLifeBuffValue(int life)
	{
		if (IsMultiplyLife)
		{
			return life * LifeValue - life;
		}
		return LifeValue;
	}
}

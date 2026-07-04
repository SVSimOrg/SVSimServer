using System.Collections.Generic;

public class DamageModifier
{
	public List<string> DamageType { get; protected set; }

	public List<CardBasePrm.ClanType> DamageClan { get; protected set; }

	public bool IsUseClass { get; protected set; }

	public int OrderCount { get; protected set; }

	public virtual int Calc(int damage)
	{
		return 0;
	}

	public bool IsEffective(string damageType, CardBasePrm.ClanType damageClan, bool isUseClass)
	{
		if (isUseClass != IsUseClass)
		{
			return false;
		}
		if (IsUseClass)
		{
			if (DamageType.Contains(damageType) || DamageType.Contains("_OPT_NULL_"))
			{
				if (!DamageClan.Contains(damageClan))
				{
					return DamageClan.Contains(CardBasePrm.ClanType.NONE);
				}
				return true;
			}
			return false;
		}
		return true;
	}
}

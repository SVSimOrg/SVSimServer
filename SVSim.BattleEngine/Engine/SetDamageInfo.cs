using System;
using System.Collections.Generic;

public class SetDamageInfo : DamageModifier
{
	public int SetDamage { get; private set; }

	public SetDamageInfo(int setDamage, string damageType, CardBasePrm.ClanType damageClan, bool isUseClass, int order)
	{
		SetDamage = setDamage;
		base.DamageType = new List<string>();
		base.DamageType.AddRange(damageType.Split(new string[1] { "_and_" }, StringSplitOptions.None));
		base.DamageClan = new List<CardBasePrm.ClanType> { damageClan };
		base.IsUseClass = isUseClass;
		base.OrderCount = order;
	}

	public override int Calc(int damage)
	{
		return SetDamage;
	}
}

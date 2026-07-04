using System;
using System.Collections.Generic;

public class AddDamageInfo : DamageModifier
{
	public int AddDamage { get; protected set; }

	public AddDamageInfo(int addDamage, string damageType, CardBasePrm.ClanType damageClan, bool isUseClass, int order)
	{
		AddDamage = addDamage;
		base.DamageType = new List<string>();
		base.DamageType.AddRange(damageType.Split(new string[1] { "_and_" }, StringSplitOptions.None));
		base.DamageClan = new List<CardBasePrm.ClanType> { damageClan };
		base.IsUseClass = isUseClass;
		base.OrderCount = order;
	}

	public override int Calc(int damage)
	{
		return damage + AddDamage;
	}
}

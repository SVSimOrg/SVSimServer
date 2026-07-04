using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIDamageClippingInfo : AIBarrierInfoBase
{
	public override AIBarrierType BarrierType => AIBarrierType.DamageClipping;

	public int ClippingRange { get; protected set; } = 9999;

	public AIDamageClippingInfo(int amount, int range, AIDamageType damageType, AIBarrierStopTiming stopTiming)
		: base(amount, damageType, stopTiming)
	{
		ClippingRange = range;
		UpdateHash();
	}

	public AIDamageClippingInfo(int amount, int range, AIDamageType damageType, List<AIBarrierStopTiming> stopTimingList)
		: base(amount, damageType, stopTimingList)
	{
		ClippingRange = range;
		UpdateHash();
	}

	public override AIBarrierInfoBase Clone()
	{
		return new AIDamageClippingInfo(base.BarrierAmount, ClippingRange, base.DamageType, base.StopTimingList);
	}

	public override bool IsShield()
	{
		if (ClippingRange == 9999)
		{
			return base.BarrierAmount <= 0;
		}
		return false;
	}

	protected override int CalcDamage(AIVirtualCard owner, int damage)
	{
		if (damage > ClippingRange)
		{
			return damage;
		}
		return Mathf.Min(damage, base.BarrierAmount);
	}

	protected override void UpdateHash()
	{
		base.Hash = AIBarrierSimulationUtility.CalculateDamageClipInfoHash(base.DamageType, BarrierType, base.StopTimingList, base.BarrierAmount, ClippingRange);
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AILifeLowerLimitInfo : AIBarrierInfoBase
{
	public override AIBarrierType BarrierType => AIBarrierType.DamageClippingLifeLowerLimit;

	public AILifeLowerLimitInfo(AIDamageType damageType, AIBarrierStopTiming stopTiming)
		: base(0, damageType, stopTiming)
	{
		UpdateHash();
	}

	public AILifeLowerLimitInfo(AIDamageType damageType, List<AIBarrierStopTiming> stopTimingList)
		: base(0, damageType, stopTimingList)
	{
		UpdateHash();
	}

	public override AIBarrierInfoBase Clone()
	{
		return new AILifeLowerLimitInfo(base.DamageType, base.StopTimingList);
	}

	public override bool IsShield()
	{
		return false;
	}

	protected override int CalcDamage(AIVirtualCard owner, int damage)
	{
		int b = owner.Life - 1;
		return Mathf.Min(damage, b);
	}
}

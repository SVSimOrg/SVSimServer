using System.Collections.Generic;

namespace Wizard;

public class AIShieldInfo : AIBarrierInfoBase
{
	public override AIBarrierType BarrierType => AIBarrierType.Shield;

	public AIShieldInfo(AIDamageType damageType, AIBarrierStopTiming stopTiming)
		: base(0, damageType, stopTiming)
	{
		UpdateHash();
	}

	public AIShieldInfo(AIDamageType damageType, List<AIBarrierStopTiming> stopTimingList)
		: base(0, damageType, stopTimingList)
	{
		UpdateHash();
	}

	public override bool IsShield()
	{
		return true;
	}

	public override AIBarrierInfoBase Clone()
	{
		return new AIShieldInfo(base.DamageType, base.StopTimingList);
	}

	protected override int CalcDamage(AIVirtualCard owner, int damage)
	{
		return 0;
	}
}

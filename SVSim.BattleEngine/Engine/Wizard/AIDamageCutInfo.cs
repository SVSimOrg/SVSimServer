using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIDamageCutInfo : AIBarrierInfoBase
{
	public override AIBarrierType BarrierType => AIBarrierType.DamageCut;

	public AIDamageCutInfo(int amount, AIDamageType damageType, AIBarrierStopTiming stopTiming)
		: base(amount, damageType, stopTiming)
	{
		UpdateHash();
	}

	public AIDamageCutInfo(int amount, AIDamageType damageType, List<AIBarrierStopTiming> stopTimingList)
		: base(amount, damageType, stopTimingList)
	{
		UpdateHash();
	}

	public override AIBarrierInfoBase Clone()
	{
		return new AIDamageCutInfo(base.BarrierAmount, base.DamageType, base.StopTimingList);
	}

	public override bool IsShield()
	{
		return false;
	}

	protected override int CalcDamage(AIVirtualCard owner, int damage)
	{
		return Mathf.Max(0, damage - base.BarrierAmount);
	}

	protected override void UpdateHash()
	{
		base.Hash = AIBarrierSimulationUtility.CalculateDamageCutInfoHash(base.DamageType, BarrierType, base.StopTimingList, base.BarrierAmount);
	}
}

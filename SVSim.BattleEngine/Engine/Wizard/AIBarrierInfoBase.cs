using System.Collections.Generic;

namespace Wizard;

public abstract class AIBarrierInfoBase
{
	public int BarrierAmount { get; protected set; }

	public AIDamageType DamageType { get; protected set; }

	public List<AIBarrierStopTiming> StopTimingList { get; protected set; }

	public abstract AIBarrierType BarrierType { get; }

	public ulong Hash { get; protected set; }

	public AIBarrierInfoBase(int amount, AIDamageType damageType, AIBarrierStopTiming stopTiming = AIBarrierStopTiming.None)
	{
		BarrierAmount = amount;
		DamageType = damageType;
		if (stopTiming != AIBarrierStopTiming.None)
		{
			StopTimingList = new List<AIBarrierStopTiming> { stopTiming };
		}
		else
		{
			StopTimingList = null;
		}
	}

	public AIBarrierInfoBase(int amount, AIDamageType damageType, List<AIBarrierStopTiming> stopTimingList)
	{
		BarrierAmount = amount;
		DamageType = damageType;
		StopTimingList = stopTimingList;
	}

	public int GetDamageAmount(AIVirtualCard owner, int damage, bool isSkillDamage, bool isSpellDamage)
	{
		if (IsDamageType(isSkillDamage, isSpellDamage))
		{
			return CalcDamage(owner, damage);
		}
		return damage;
	}

	public bool IsDamageType(bool isSkillDamage, bool isSpellDamage)
	{
		if (DamageType == AIDamageType.All)
		{
			return true;
		}
		if (DamageType == AIDamageType.Skill && (isSkillDamage || isSpellDamage))
		{
			return true;
		}
		if (DamageType == AIDamageType.Spell && isSpellDamage)
		{
			return true;
		}
		if (DamageType == AIDamageType.Attack && !isSkillDamage && !isSpellDamage)
		{
			return true;
		}
		return false;
	}

	public bool IsMatchingStopTiming(AIBarrierStopTiming timing)
	{
		if (StopTimingList == null || StopTimingList.Count <= 0)
		{
			return timing == AIBarrierStopTiming.None;
		}
		for (int i = 0; i < StopTimingList.Count; i++)
		{
			if (timing == StopTimingList[i])
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void UpdateHash()
	{
		Hash = AIBarrierSimulationUtility.CalculateBarrierInfoBaseHash(DamageType, BarrierType, StopTimingList);
	}

	public abstract AIBarrierInfoBase Clone();

	public abstract bool IsShield();

	protected abstract int CalcDamage(AIVirtualCard owner, int damage);
}

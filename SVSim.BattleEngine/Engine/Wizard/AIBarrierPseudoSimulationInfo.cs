using UnityEngine;

namespace Wizard;

public class AIBarrierPseudoSimulationInfo
{
	private AIBarrierInfoCollection _barrierCollection;

	public AIVirtualCard Owner { get; private set; }

	public AIBarrierPseudoSimulationInfo(AIVirtualCard owner)
	{
		Owner = owner;
		_barrierCollection = ((owner.BarrierInfoCollection != null) ? owner.BarrierInfoCollection.Clone() : new AIBarrierInfoCollection());
	}

	public AIBarrierPseudoSimulationInfo(AIBarrierPseudoSimulationInfo info)
	{
		Owner = info.Owner;
		_barrierCollection = info._barrierCollection.Clone();
	}

	public void AddBarrierInfo(AIBarrierInfoBase barrier)
	{
		_barrierCollection.AddBarrierInfo(barrier);
	}

	public void DepriveBarrier(AIBarrierStopTiming timing)
	{
		_barrierCollection.DepriveAllBarrierOfOneTiming(timing);
	}

	public void DepriveCertainBarrier(AIBarrierStopTiming timing, ulong barrierHash)
	{
		_barrierCollection.DepriveCertainBarrier(barrierHash, timing);
	}

	public int SimulateDamageAmount(int damage, bool isSpellDamage, bool isSkillDamage = true)
	{
		int b = _barrierCollection.CalcDamageAmount(Owner, damage, isSkillDamage, isSpellDamage);
		return Mathf.Max(0, b);
	}
}

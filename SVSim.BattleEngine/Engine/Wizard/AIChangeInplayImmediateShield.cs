using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayImmediateShield : AIChangeInplayImmediateBarrierBase
{
	protected override int _defaultDamageTypeOffset => 2;

	protected override int _stopTimingOffset => 1;

	public AIChangeInplayImmediateShield(string text)
		: base(text)
	{
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIBarrierSimulationUtility.AddShieldToAll(targets, tagOwner, field, _damageType, _stopTiming);
	}

	protected override void DepriveBarrierFromAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIDamageType damageTypeFromArgType = AIBarrierSimulationUtility.GetDamageTypeFromArgType(_damageType);
		AIBarrierStopTiming barrierStopTimingFromArgType = AIBarrierSimulationUtility.GetBarrierStopTimingFromArgType(_stopTiming, tagOwner);
		ulong depriveShieldHash = AIBarrierSimulationUtility.CalculateBarrierInfoBaseHash(damageTypeFromArgType, AIBarrierType.Shield, barrierStopTimingFromArgType);
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].BarrierInfoCollection.DepriveCertainBarrier(depriveShieldHash, barrierStopTimingFromArgType);
		}
	}
}

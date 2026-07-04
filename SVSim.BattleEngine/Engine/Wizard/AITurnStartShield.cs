using System.Collections.Generic;

namespace Wizard;

public class AITurnStartShield : AITurnStartBarrierBase
{
	protected override int _stopTimingOffset => 2;

	protected override int _defaultDamageTypeOffset => 3;

	public AITurnStartShield(string text)
		: base(text)
	{
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIBarrierSimulationUtility.AddShieldToAll(targets, tagOwner, field, _damageType, _stopTiming);
	}
}

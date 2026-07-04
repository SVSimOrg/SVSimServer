using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayShield : AIWhenPlayBarrierBase
{
	protected override int _defaultDamageTypeOffset => 2;

	protected override int _stopTimingOffset => 1;

	public AIWhenPlayShield(string text)
		: base(text)
	{
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIBarrierSimulationUtility.AddShieldToAll(targets, tagOwner, field, _damageType, _stopTiming);
	}
}

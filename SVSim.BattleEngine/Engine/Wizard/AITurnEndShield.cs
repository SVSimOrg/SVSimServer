using System.Collections.Generic;

namespace Wizard;

public class AITurnEndShield : AITurnEndBarrierBase
{
	protected override int _defaultDamageTypeOffset => 3;

	protected override int _stopTimingOffset => 2;

	public AITurnEndShield(string text)
		: base(text)
	{
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIBarrierSimulationUtility.AddShieldToAll(targets, tagOwner, field, _damageType, _stopTiming);
	}
}

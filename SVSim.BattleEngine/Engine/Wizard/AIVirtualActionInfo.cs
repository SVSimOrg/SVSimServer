namespace Wizard;

public abstract class AIVirtualActionInfo : AISituationInfo
{
	public bool IsAlreadyUsed { get; set; }

	public AIVirtualActionInfo PremiseAction { get; private set; }

	public PlaySimulationType ReservedPlayType { get; set; }

	public AIVirtualActionInfo(AIVirtualCard sourceCard, AIOperationType type, AISelectedTargetInfoSet targetSet, AIVirtualActionInfo premise = null)
		: base(sourceCard, type, targetSet)
	{
		IsAlreadyUsed = false;
		PremiseAction = ((premise == null) ? this : premise);
		ReservedPlayType = PlaySimulationType.Undefined;
	}

	public virtual ulong GetHash()
	{
		if (IsAlreadyUsed)
		{
			return 0uL;
		}
		ulong num = 0uL;
		num += base.Actor.GetHash() * 6343;
		if (base.ActionTarget != null)
		{
			num += base.ActionTarget.GetHash() * 211;
		}
		if (PremiseAction != this)
		{
			num += PremiseAction.GetHash() * 735173;
		}
		return num;
	}
}

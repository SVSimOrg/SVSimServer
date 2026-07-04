namespace Wizard;

public class AIBarrierAfterDamageStopInformation : AIAfterDamageStopInformation
{
	private ulong _barrierHash;

	public AIBarrierAfterDamageStopInformation(AIBarrierStopPreprocessOption option)
		: base(option.TargetCard)
	{
		_barrierHash = option.BarrierHash;
		base.Type = AITagPreprocessInfoType.BARRIER_STOP;
	}

	protected override void StopMethod()
	{
		base.TargetCard.BarrierInfoCollection.DepriveCertainBarrier(_barrierHash, AIBarrierStopTiming.AfterDamage);
	}

	public override AITagPreprocessCreationOptionBase CreateOptionInfoForOverride(AIVirtualCard overridedTarget)
	{
		return new AIBarrierStopPreprocessOption(overridedTarget, _barrierHash);
	}
}

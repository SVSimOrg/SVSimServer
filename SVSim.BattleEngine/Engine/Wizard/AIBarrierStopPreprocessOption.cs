namespace Wizard;

public class AIBarrierStopPreprocessOption : AITagPreprocessCreationOptionBase
{
	public ulong BarrierHash { get; private set; }

	public AIBarrierStopPreprocessOption(AIVirtualCard target, ulong hash)
		: base(AITagPreprocessInfoType.BARRIER_STOP, target)
	{
		BarrierHash = hash;
	}
}

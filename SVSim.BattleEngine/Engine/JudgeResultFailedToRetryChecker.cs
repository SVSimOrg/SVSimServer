using System;
using Cute;

public class JudgeResultFailedToRetryChecker : NetworkBattleIntervalCheckerBase
{

	public event Action OnRetry;

	public override void StopChecker()
	{
		base.StopChecker();
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if (NetworkUtility.GetTimeSpanSecond(base.startTick) >= 35)
		{
			this.OnRetry.Call();
			StopChecker();
		}
	}
}

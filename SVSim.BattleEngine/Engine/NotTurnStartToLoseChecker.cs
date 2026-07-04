using System;
using Cute;

public class NotTurnStartToLoseChecker : NetworkBattleIntervalCheckerBase
{

	public event Action OnNotTurnStartToLose;

	public override void StopChecker()
	{
		if (!((float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 75f))
		{
			base.StopChecker();
		}
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if ((float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 75f)
		{
			this.OnNotTurnStartToLose.Call();
			InitTimer();
			StopChecker();
		}
	}
}

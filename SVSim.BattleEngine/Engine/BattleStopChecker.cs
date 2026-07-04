using System;
using Cute;

public class BattleStopChecker : NetworkBattleIntervalCheckerBase
{

	public event Action OnBattleStop;

	public override void StopChecker()
	{
		base.StopChecker();
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if (NetworkUtility.GetTimeSpanSecond(base.startTick) >= 95)
		{
			this.OnBattleStop.Call();
			StartChecker();
		}
	}
}

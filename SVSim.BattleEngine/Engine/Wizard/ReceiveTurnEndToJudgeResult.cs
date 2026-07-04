using System;
using Cute;

namespace Wizard;

public class ReceiveTurnEndToJudgeResult : NetworkBattleIntervalCheckerBase
{

	public event Action OnIntervalTime;

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if ((float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 30f)
		{
			this.OnIntervalTime.Call();
			InitTimer();
			StopChecker();
		}
	}
}

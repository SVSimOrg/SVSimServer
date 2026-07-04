using System;
using Cute;

public class NotMulliganEndToJudgeChecker : NetworkBattleIntervalCheckerBase
{

	public event Action OnNotMulliganEndJudge;

	public bool IsTimeOver()
	{
		if (isEnd)
		{
			return false;
		}
		if ((float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 95f)
		{
			return true;
		}
		return false;
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if ((float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 95f)
		{
			this.OnNotMulliganEndJudge.Call();
			StopChecker();
		}
	}
}

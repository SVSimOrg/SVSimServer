using System;
using Cute;

public class DisconnectToDispChecker : NetworkBattleIntervalCheckerBase
{

	public bool isDisp;

	public event Action OnDisp;

	public event Action OnErase;

	public void EraseDisp()
	{
		if (isDisp)
		{
			this.OnErase.Call();
			isDisp = false;
		}
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if ((float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= 16f)
		{
			DisconnectDisp();
		}
	}

	private void DisconnectDisp()
	{
		this.OnDisp.Call();
		isDisp = true;
		StopChecker();
	}

	public override void FinishChecker()
	{
		base.FinishChecker();
		EraseDisp();
	}
}

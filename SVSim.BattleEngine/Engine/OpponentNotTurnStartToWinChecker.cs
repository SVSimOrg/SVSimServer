using System;
using Cute;

public class OpponentNotTurnStartToWinChecker : NetworkBattleIntervalCheckerBase
{
	private NetworkBattleManagerBase networkBattleManager;

	private float _timeoutDisconnect;

	public event Action OnOpponentNotTurnStartToWin;

	public OpponentNotTurnStartToWinChecker(NetworkBattleManagerBase manager)
	{
		networkBattleManager = manager;
		_timeoutDisconnect = 75f;
	}

	public override void StartChecker(string log = "")
	{
		base.StartChecker(log);
	}

	public override void StopChecker()
	{
		base.StopChecker();
	}

	public void SetTimeoutTime(bool isExtend)
	{
		if (isExtend)
		{
			_timeoutDisconnect = 90f;
		}
		else
		{
			_timeoutDisconnect = 75f;
		}
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		if (!networkBattleManager.disconnectToLoseChecker.IsDisconnect() && (float)NetworkUtility.GetTimeSpanSecond(base.startTick) >= _timeoutDisconnect)
		{
			this.OnOpponentNotTurnStartToWin.Call();
			StopChecker();
		}
	}
}

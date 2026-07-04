using System.Collections;
using Cute;
using UnityEngine;

public class NetworkBattleIntervalCheckerBase
{
	protected IEnumerator coroutine;

	protected bool isEnd;

	protected NetworkBattleManagerBase _networkBattleManagerBase;

	protected long startTick { get; private set; }

	protected bool isStop { get; private set; }

	public NetworkBattleIntervalCheckerBase()
	{
		_networkBattleManagerBase = null; // Pre-Phase-5b: interval checker is UI-only; NRE on invocation acceptable headless
	}

	public virtual void StartChecker(string log = "")
	{
		InitTimer();
		if (!isEnd)
		{
			isStop = false;
			if (coroutine == null)
			{
				coroutine = CheckInterval();
				BattleCoroutine.GetInstance().StartCoroutine(coroutine);
			}
		}
	}

	public virtual void FinishChecker()
	{
		StopChecker();
	}

	public virtual void StopChecker()
	{
		if (coroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(coroutine);
			coroutine = null;
		}
		isStop = true;
	}

	public bool IsStarted()
	{
		return coroutine != null;
	}

	protected void InitTimer()
	{
		startTick = TimeUtil.GetAbsoluteTime().Ticks;
	}

	private IEnumerator CheckInterval()
	{
		WaitForSeconds secondWait = new WaitForSeconds(1f);
		while (!isEnd)
		{
			if (!_networkBattleManagerBase.IsStopIntervalCheck)
			{
				IntervalCheck();
			}
			yield return secondWait;
		}
	}

	protected virtual void IntervalCheck()
	{
	}

	public void EndTimer()
	{
		isEnd = true;
	}
}

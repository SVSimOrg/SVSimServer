using System;
using Cute;

public class OpponentRecoveryToDispChecker
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

	public void CreateDisp()
	{
		isDisp = true;
		this.OnDisp.Call();
	}
}

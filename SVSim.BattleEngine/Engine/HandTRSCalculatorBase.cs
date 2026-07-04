using UnityEngine;

public abstract class HandTRSCalculatorBase
{

	protected readonly Vector3 _handPos = Vector3.zero;

	public HandTRSCalculatorBase(Vector3 handPos)
	{
		_handPos = handPos;
	}
}

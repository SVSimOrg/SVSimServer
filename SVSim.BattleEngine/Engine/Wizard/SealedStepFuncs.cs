using System;
using System.Collections;

namespace Wizard;

public class SealedStepFuncs
{
	public Func<IEnumerator> InitFunc { get; private set; }

	public Action FinalFunc { get; private set; }

	public SealedStepFuncs(Func<IEnumerator> initFunc, Action finalFunc)
	{
		InitFunc = initFunc;
		FinalFunc = finalFunc;
	}
}

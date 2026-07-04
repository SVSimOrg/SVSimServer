using System.Collections.Generic;

namespace Cute;

internal class SkipCuteCheckResultCodes
{
	private List<int> resultCodes = new List<int>();

	private bool skipAll;

	public void setSkipAll(bool pSkipAll)
	{
		skipAll = pSkipAll;
	}

	public bool isSkipAll()
	{
		return skipAll;
	}

	public bool Contains(int resultCode)
	{
		return resultCodes.Contains(resultCode);
	}
}

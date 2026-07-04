using System.Collections.Generic;

namespace Wizard;

public class AIDiscardInfo
{
	public AIVirtualCard Owner;

	public List<AIVirtualCard> TargetList;

	public bool IsSuccess;

	public bool IsNGByAI;

	public bool IsValuable
	{
		get
		{
			if (IsSuccess && !IsNGByAI && TargetList != null)
			{
				return TargetList.Count > 0;
			}
			return false;
		}
	}

	public AIDiscardInfo(AIVirtualCard owner, bool isSuccess, List<AIVirtualCard> targets)
	{
		Owner = owner;
		IsSuccess = isSuccess;
		TargetList = targets;
		IsNGByAI = false;
	}

	public void MarkAsNG()
	{
		IsNGByAI = true;
	}
}

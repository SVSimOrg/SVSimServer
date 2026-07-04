using System.Collections.Generic;

namespace Wizard;

public class AIPolicyCollection
{
	public List<AIPolicyData> PolicyList { get; protected set; }

	public bool HasPolicy
	{
		get
		{
			if (PolicyList != null)
			{
				return 0 < PolicyList.Count;
			}
			return false;
		}
	}

	public AIPolicyCollection()
	{
		PolicyList = null;
	}

	public void AddPolicy(AIPolicyData policy)
	{
		if (PolicyList == null)
		{
			PolicyList = new List<AIPolicyData>();
		}
		PolicyList.Add(policy);
	}

	public void RemovePolicy(AIPolicyData policy)
	{
		if (PolicyList.Contains(policy))
		{
			PolicyList.Remove(policy);
		}
	}
}

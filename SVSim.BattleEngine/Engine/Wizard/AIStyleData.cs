using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIStyleData
{
	private List<AICategory> categoryFilter;

	private Dictionary<AIStyleKey, List<AIPolicyData>> policyDic = new Dictionary<AIStyleKey, List<AIPolicyData>>();

	public AIStyleData()
	{
	}

	public AIStyleData(List<AICategory> filter)
	{
		categoryFilter = filter;
	}

	public List<AIPolicyData> ConvertToPolicyList()
	{
		List<AIPolicyData> list = new List<AIPolicyData>();
		foreach (KeyValuePair<AIStyleKey, List<AIPolicyData>> item in policyDic)
		{
			foreach (AIPolicyData item2 in item.Value)
			{
				list.Add(item2);
			}
		}
		return list;
	}
}

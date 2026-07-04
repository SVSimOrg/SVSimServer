using System.Collections.Generic;

namespace Wizard;

public class SetReferenceIdPolicyCollection : AIPolicyCollection
{
	public Dictionary<int, int> CreateReferenceIdTable()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (!base.HasPolicy)
		{
			return dictionary;
		}
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			int key = aIPolicyData.EvalId();
			int value = aIPolicyData.EvalId(1);
			dictionary.Add(key, value);
		}
		return dictionary;
	}
}

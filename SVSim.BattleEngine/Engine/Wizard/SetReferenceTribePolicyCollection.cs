using System.Collections.Generic;

namespace Wizard;

public class SetReferenceTribePolicyCollection : AIPolicyCollection
{
	public Dictionary<string, List<int>> CreateReferenceTribeTable()
	{
		Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
		if (!base.HasPolicy)
		{
			return dictionary;
		}
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			string text = aIPolicyData.EvalText();
			if (text == null || text == "")
			{
				AIConsoleUtility.LogError("SetReferenceTribePolicyCollection.CreateReferenceTribeTable(): arg0 is not tribe text. ARG:[" + aIPolicyData.ARG + "]");
				continue;
			}
			List<int> list = aIPolicyData.EvalIdList(1);
			if (list == null || list.Count <= 0)
			{
				AIConsoleUtility.LogError("SetReferenceTribePolicyCollection.CreateReferenceTribeTable(): Not contains target Id. ARG:[" + aIPolicyData.ARG + "]");
			}
			else
			{
				dictionary.Add(text, list);
			}
		}
		return dictionary;
	}
}

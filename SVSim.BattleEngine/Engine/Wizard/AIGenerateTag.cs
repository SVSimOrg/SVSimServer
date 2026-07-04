using System.Collections.Generic;

namespace Wizard;

public class AIGenerateTag : AIScriptArgumentExpressions
{
	private List<string> _hashList;

	public AIPlayTag Tag;

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	private int HASH_ARG_END_OFFSET => 1;

	public AIGenerateTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		InitExprList(list[0]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		for (int i = 0; i < _exprList.Count - HASH_ARG_END_OFFSET; i++)
		{
			if (_exprList[i].TokenList[0] is AIScriptTextToken aIScriptTextToken)
			{
				_hashList = AIParamQuery.AddElementToList(aIScriptTextToken.Text, _hashList);
			}
		}
		if (list.Count > 3)
		{
			Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		}
	}

	public bool CheckMatchedHashList(List<string> hashDiffList)
	{
		if (_hashList == null || _hashList.Count <= 0 || hashDiffList == null || hashDiffList.Count <= 0)
		{
			return false;
		}
		if (hashDiffList.Count < _hashList.Count)
		{
			return false;
		}
		for (int i = 0; i < _hashList.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < hashDiffList.Count; j++)
			{
				if (_hashList[i].Equals(hashDiffList[j]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}

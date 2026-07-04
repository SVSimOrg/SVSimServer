using System.Collections.Generic;

namespace Wizard;

public class AddCardToPlayoutPlayPtnTagCollection : TagCollection
{
	public AddCardToPlayoutPlayPtnTagCollection()
		: base(TagCollectionType.AddCardToPlayoutPlayPtn)
	{
	}

	private AddCardToPlayoutPlayPtnTagCollection(AddCardToPlayoutPlayPtnTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new AddCardToPlayoutPlayPtnTagCollection(this);
	}

	public List<int> GetIdList(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return null;
		}
		List<int> list = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!aIPlayTag.CheckCondition(owner, playPtn, field, null))
			{
				continue;
			}
			int num = (int)aIPlayTag.EvalArg(owner, playPtn, field, null, 1);
			if (num > 0)
			{
				int element = aIPlayTag.EvalID();
				for (int j = 0; j < num; j++)
				{
					list = AIParamQuery.AddElementToList(element, list);
				}
			}
		}
		return list;
	}
}

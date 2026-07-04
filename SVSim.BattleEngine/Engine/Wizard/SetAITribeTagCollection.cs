using System.Collections.Generic;

namespace Wizard;

public class SetAITribeTagCollection : TagCollection
{
	public SetAITribeTagCollection()
		: base(TagCollectionType.SetAITribe)
	{
	}

	private SetAITribeTagCollection(SetAITribeTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new SetAITribeTagCollection(this);
	}

	public bool CheckTribe(AIVirtualCard owner, AIVirtualField field, string tribe, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AISetTribe aISetTribe && aISetTribe.CheckTribe(tribe))
			{
				return true;
			}
		}
		return false;
	}
}

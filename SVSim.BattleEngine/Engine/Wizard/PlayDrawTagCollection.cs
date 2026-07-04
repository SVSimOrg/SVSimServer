using System.Collections.Generic;

namespace Wizard;

public class PlayDrawTagCollection : TagCollection
{
	public PlayDrawTagCollection()
		: base(TagCollectionType.PlayDraw)
	{
	}

	private PlayDrawTagCollection(PlayDrawTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayDrawTagCollection(this);
	}

	public int GetPlayDrawCount(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return 0;
		}
		int num = 0;
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				num += (int)aIPlayTag.EvalArg(tagOwner, playPtn, selfField, situation);
			}
		}
		return num;
	}
}

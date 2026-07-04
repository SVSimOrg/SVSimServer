using System.Collections.Generic;

namespace Wizard;

public class PlayBonusTagCollection : TagCollection
{
	public PlayBonusTagCollection()
		: base(TagCollectionType.PlayBonus)
	{
	}

	private PlayBonusTagCollection(PlayBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayBonusTagCollection(this);
	}

	public float GetPlayBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		float num = 0f;
		if (tagOwner == null || !base.HasTag)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				num += aIPlayTag.EvalArg(tagOwner, playPtn, selfField, situation);
			}
		}
		return num;
	}
}

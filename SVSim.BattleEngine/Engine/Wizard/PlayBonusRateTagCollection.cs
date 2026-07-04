using System.Collections.Generic;

namespace Wizard;

public class PlayBonusRateTagCollection : TagCollection
{
	public PlayBonusRateTagCollection()
		: base(TagCollectionType.PlayBonusRate)
	{
	}

	private PlayBonusRateTagCollection(PlayBonusRateTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayBonusRateTagCollection(this);
	}

	public float GetPlayBonusRate(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		float num = 1f;
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				num *= aIPlayTag.EvalArg(tagOwner, playPtn, selfField, situation);
			}
		}
		return num;
	}
}

using System.Collections.Generic;

namespace Wizard;

public class BattleBonusRateTagCollection : TagCollection
{
	public BattleBonusRateTagCollection()
		: base(TagCollectionType.BattleBonusRate)
	{
	}

	private BattleBonusRateTagCollection(BattleBonusRateTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BattleBonusRateTagCollection(this);
	}

	public float GetBattleBonusRate(AIVirtualCard tagOwner, List<int> playPtn)
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
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, null))
			{
				num *= aIPlayTag.EvalArg(tagOwner, playPtn, selfField, null);
			}
		}
		return num;
	}
}

using System.Collections.Generic;

namespace Wizard;

public class FanfareBonusTagCollection : TagCollection
{
	public FanfareBonusTagCollection()
		: base(TagCollectionType.FanfareBonus)
	{
	}

	private FanfareBonusTagCollection(FanfareBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new FanfareBonusTagCollection(this);
	}

	public float GetFanfareBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag || tagOwner.IsAccelerated(selfField, playPtn) || tagOwner.IsCrystalize(selfField, playPtn))
		{
			return num;
		}
		if (selfField.IsEnableIgnoreFanfareBonus(playPtn))
		{
			return num;
		}
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

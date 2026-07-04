using System.Collections.Generic;

namespace Wizard;

public class BuffBonusTagCollection : TagCollection
{
	public BuffBonusTagCollection()
		: base(TagCollectionType.BuffBonus)
	{
	}

	private BuffBonusTagCollection(BuffBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BuffBonusTagCollection(this);
	}

	public float GetBuffBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
		{
			return 0f;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		float num = 0f;
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

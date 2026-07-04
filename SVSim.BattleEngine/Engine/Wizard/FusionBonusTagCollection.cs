using System.Collections.Generic;

namespace Wizard;

public class FusionBonusTagCollection : TagCollection
{
	public FusionBonusTagCollection()
		: base(TagCollectionType.FusionBonus)
	{
	}

	private FusionBonusTagCollection(FusionBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new FusionBonusTagCollection(this);
	}

	public float GetFusionBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
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

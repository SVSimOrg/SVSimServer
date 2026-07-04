using System.Collections.Generic;

namespace Wizard;

public class ModifyHealTagCollection : TagCollection
{
	public ModifyHealTagCollection()
		: base(TagCollectionType.ModifyHeal)
	{
	}

	private ModifyHealTagCollection(ModifyHealTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new ModifyHealTagCollection(this);
	}

	public int GetModifiedHealValue(AIVirtualCard tagOwner, AIVirtualCard healTarget, int healValue, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || healTarget == null || !base.HasTag)
		{
			return healValue;
		}
		int result = healValue;
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AIModifyValue aIModifyValue && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				result = aIModifyValue.GetModifiedValue(tagOwner, healTarget, playPtn, situation, healValue);
				break;
			}
		}
		return result;
	}
}

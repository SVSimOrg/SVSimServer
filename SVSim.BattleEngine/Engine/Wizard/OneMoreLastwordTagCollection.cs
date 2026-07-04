using System.Collections.Generic;

namespace Wizard;

public class OneMoreLastwordTagCollection : TagCollection
{
	public OneMoreLastwordTagCollection()
		: base(TagCollectionType.OneMoreLastword)
	{
	}

	private OneMoreLastwordTagCollection(OneMoreLastwordTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new OneMoreLastwordTagCollection(this);
	}

	public bool CheckCondition(AIVirtualCard tagOwner, AIVirtualField field)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, field.BestPlayPtn, field, null))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckConditionAndRemovePassedTags(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return false;
		}
		bool result = false;
		List<AIPlayTag> list = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, field.BestPlayPtn, field, situation))
			{
				result = true;
				list = AIParamQuery.AddElementToList(base.TagList[i], list);
			}
		}
		if (list == null)
		{
			return result;
		}
		for (int j = 0; j < list.Count; j++)
		{
			AIRemoveTagUtility.RemoveOneTag(tagOwner, field, list[j], situation);
		}
		return result;
	}
}

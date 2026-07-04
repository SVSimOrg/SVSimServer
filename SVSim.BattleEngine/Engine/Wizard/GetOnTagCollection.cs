using System.Collections.Generic;

namespace Wizard;

public class GetOnTagCollection : TagCollection
{
	public GetOnTagCollection()
		: base(TagCollectionType.GetOn)
	{
	}

	private GetOnTagCollection(GetOnTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new GetOnTagCollection(this);
	}

	public bool CanGetOn(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag || tagOwner.IsGetOn || tagOwner.IsDead || targetCard == null)
		{
			return false;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AIGetOn aIGetOn && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation) && aIGetOn.CanGetOn(tagOwner, targetCard, playPtn, situation))
			{
				return true;
			}
		}
		return false;
	}
}

using System.Collections.Generic;

namespace Wizard;

public class ForceBerserkTagCollection : TagCollection
{
	public ForceBerserkTagCollection()
		: base(TagCollectionType.ForceBerserk)
	{
	}

	private ForceBerserkTagCollection(ForceBerserkTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new ForceBerserkTagCollection(this);
	}

	public bool IsForceBerserk(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
		{
			return false;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				return true;
			}
		}
		return false;
	}
}

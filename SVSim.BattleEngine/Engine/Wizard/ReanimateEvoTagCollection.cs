using System.Collections.Generic;

namespace Wizard;

public class ReanimateEvoTagCollection : TagCollection
{
	public ReanimateEvoTagCollection()
		: base(TagCollectionType.ReanimateEvo)
	{
	}

	private ReanimateEvoTagCollection(ReanimateEvoTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new ReanimateEvoTagCollection(this);
	}

	public bool IsReanimateEvo(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return false;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, playPtn, selfField, null))
			{
				return true;
			}
		}
		return false;
	}
}

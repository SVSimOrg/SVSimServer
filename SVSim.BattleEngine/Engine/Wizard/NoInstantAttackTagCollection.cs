using System.Collections.Generic;

namespace Wizard;

public class NoInstantAttackTagCollection : TagCollection
{
	public NoInstantAttackTagCollection()
		: base(TagCollectionType.NoInstantAttack)
	{
	}

	private NoInstantAttackTagCollection(NoInstantAttackTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new NoInstantAttackTagCollection(this);
	}

	public bool IsNoInstantAttackActivate(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, playPtn, field, null))
			{
				return true;
			}
		}
		return false;
	}
}

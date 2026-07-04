using System.Collections.Generic;

namespace Wizard;

public class NoSkipAttackTagCollection : TagCollection
{
	public NoSkipAttackTagCollection()
		: base(TagCollectionType.NoSkipAttack)
	{
	}

	private NoSkipAttackTagCollection(NoSkipAttackTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new NoSkipAttackTagCollection(this);
	}

	public bool IsNoSkipAttack(AIVirtualCard tagOwner, List<int> playPtn, AIParamQuery query)
	{
		if (base.HasTag)
		{
			for (int i = 0; i < base.TagList.Count; i++)
			{
				if (base.TagList[i].CheckCondition(tagOwner, playPtn, tagOwner.SelfField, null))
				{
					return true;
				}
			}
		}
		return false;
	}
}

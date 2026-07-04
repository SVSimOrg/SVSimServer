using System.Collections.Generic;

namespace Wizard;

public class PlagueCityTagCollection : TagCollection
{
	public PlagueCityTagCollection()
		: base(TagCollectionType.PlagueCity)
	{
	}

	private PlagueCityTagCollection(PlagueCityTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new PlagueCityTagCollection(this);
	}

	public bool IsPlagueCity(AIVirtualCard owner, AIVirtualField field, List<int> playPtn)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(owner, playPtn, field, null))
			{
				return true;
			}
		}
		return false;
	}
}

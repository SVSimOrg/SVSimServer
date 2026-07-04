using System.Collections.Generic;

namespace Wizard;

public class FusionDrawTagCollection : TagCollection
{
	public FusionDrawTagCollection()
		: base(TagCollectionType.FusionDraw)
	{
	}

	private FusionDrawTagCollection(FusionDrawTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new FusionDrawTagCollection(this);
	}

	public int GetFusionDrawCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIFusionDraw aIFusionDraw)
			{
				num += aIFusionDraw.GetDrawCount(tagOwner, field, playPtn, situation);
			}
		}
		return num;
	}
}

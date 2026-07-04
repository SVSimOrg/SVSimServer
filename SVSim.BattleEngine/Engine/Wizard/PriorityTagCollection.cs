using System.Collections.Generic;

namespace Wizard;

public class PriorityTagCollection : TagCollection
{
	public PriorityTagCollection()
		: base(TagCollectionType.Priority)
	{
	}

	private PriorityTagCollection(PriorityTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new PriorityTagCollection(this);
	}

	public float GetPriorityBonus(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualTargetSelectAction situation)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		selfField.AI.PlayPtnRecorder.StartCalculatingPriority();
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				num += aIPlayTag.EvalArg(tagOwner, playPtn, selfField, situation);
			}
		}
		selfField.AI.PlayPtnRecorder.StopCalculatingPriority();
		return num;
	}
}

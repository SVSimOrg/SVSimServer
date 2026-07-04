namespace Wizard;

public class FixedCostTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[4]
	{
		AIPlayTagType.Enhance,
		AIPlayTagType.Accelerate,
		AIPlayTagType.Crystalize,
		AIPlayTagType.ChoiceTransform
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public FixedCostTagCollection()
		: base(TagCollectionType.FixedCost)
	{
	}

	public override TagCollection Clone()
	{
		return new FixedCostTagCollection(this);
	}

	private FixedCostTagCollection(FixedCostTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public void CreateFixedUseCostLists(AIVirtualCard owner, AIVirtualField field)
	{
		if (base.HasTag)
		{
			for (int i = 0; i < base.TagList.Count; i++)
			{
				AIPlayTag tag = base.TagList[i];
				RegisterCostData(tag, owner, field);
			}
		}
	}

	private void RegisterCostData(AIPlayTag tag, AIVirtualCard owner, AIVirtualField field)
	{
		switch (tag.Type)
		{
		case AIPlayTagType.Enhance:
			RegisterEnhanceCostData(tag, owner, field);
			break;
		case AIPlayTagType.Accelerate:
			RegisterAccelerateCostData(tag, owner, field);
			break;
		case AIPlayTagType.Crystalize:
			RegisterCrystalizeCostData(tag, owner, field);
			break;
		case AIPlayTagType.ChoiceTransform:
			RegisterChoiceTransformCostData(tag, owner, field);
			break;
		}
	}

	private void RegisterEnhanceCostData(AIPlayTag tag, AIVirtualCard owner, AIVirtualField field)
	{
		(tag.ArgumentExpressions as AIEnhance).RegisterSelfToOwner(owner, field);
	}

	private void RegisterAccelerateCostData(AIPlayTag tag, AIVirtualCard owner, AIVirtualField field)
	{
		(tag.ArgumentExpressions as AIAccelerate).RegisterSelfToOwner(owner, field, tag.ConditionExpressions);
	}

	private void RegisterCrystalizeCostData(AIPlayTag tag, AIVirtualCard owner, AIVirtualField field)
	{
		(tag.ArgumentExpressions as AICrystalize).RegisterSelfToOwner(owner, field);
	}

	private void RegisterChoiceTransformCostData(AIPlayTag tag, AIVirtualCard owner, AIVirtualField field)
	{
		(tag.ArgumentExpressions as AIChoiceTransform).RegisterSelfToOwner(owner, field, tag.ConditionExpressions);
	}
}

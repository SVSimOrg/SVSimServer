using System.Collections.Generic;

namespace Wizard;

public class WhenGetOffTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[2]
	{
		AIPlayTagType.GetOffMetamorphose,
		AIPlayTagType.GetOffEvo
	};

	private List<AIPlayTag> _getOffMetamorphoseList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public WhenGetOffTagCollection()
		: base(TagCollectionType.WhenGetOff)
	{
	}

	private WhenGetOffTagCollection(WhenGetOffTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new WhenGetOffTagCollection(this);
	}

	public override void Clear()
	{
		base.Clear();
		if (_getOffMetamorphoseList != null)
		{
			_getOffMetamorphoseList.Clear();
			_getOffMetamorphoseList = null;
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.GetOffMetamorphose)
		{
			_getOffMetamorphoseList = AIParamQuery.AddElementToList(tag, _getOffMetamorphoseList);
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_getOffMetamorphoseList, tag);
	}

	public void RegisterPaasedConditionTags(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		List<AIPlayTag> passedConditionTags = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags, isBlockDuplicate: true);
			}
		}
		if (passedConditionTags == null)
		{
			return;
		}
		situation.RegisterNewProcessInfo(tagOwner, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
		{
			for (int j = 0; j < passedConditionTags.Count; j++)
			{
				passedConditionTags[j].ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		});
	}
}

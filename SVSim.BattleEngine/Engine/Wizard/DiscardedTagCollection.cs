using System.Collections.Generic;

namespace Wizard;

public class DiscardedTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[1] { AIPlayTagType.DiscardedToken };

	private List<AIPlayTag> _discardedTokenTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public bool HasDiscardedToken
	{
		get
		{
			if (_discardedTokenTagList != null)
			{
				return _discardedTokenTagList.Count > 0;
			}
			return false;
		}
	}

	public DiscardedTagCollection()
		: base(TagCollectionType.WhenDiscarded)
	{
		_discardedTokenTagList = null;
	}

	public DiscardedTagCollection(DiscardedTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection != null && tagCollection.HasDiscardedToken)
		{
			_discardedTokenTagList = new List<AIPlayTag>(tagCollection._discardedTokenTagList);
		}
	}

	public override TagCollection Clone()
	{
		return new DiscardedTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.DiscardedToken)
		{
			if (_discardedTokenTagList == null)
			{
				_discardedTokenTagList = new List<AIPlayTag>();
			}
			_discardedTokenTagList.Add(tag);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_discardedTokenTagList != null)
		{
			_discardedTokenTagList.Clear();
			_discardedTokenTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_discardedTokenTagList, tag);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		AIVirtualField field = tagOwner.SelfField;
		List<AIPlayTag> passedConditionTags = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags, isBlockDuplicate: true);
			}
		}
		if (passedConditionTags == null || passedConditionTags.Count <= 0)
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

	public List<AITokenInformation> GetDiscardedTokenIds(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || !tagOwner.IsInHand || !HasDiscardedToken)
		{
			return null;
		}
		List<AITokenInformation> list = null;
		for (int i = 0; i < _discardedTokenTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = _discardedTokenTagList[i];
			if (aIPlayTag.ArgumentExpressions is AIDiscardedToken aIDiscardedToken && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				List<AITokenInformation> tokenIds = aIDiscardedToken.GetTokenIds(tagOwner, field, playPtn, situation);
				if (tokenIds != null)
				{
					list = AIParamQuery.AddRangeToList(tokenIds, list);
				}
			}
		}
		return list;
	}
}

using System.Collections.Generic;

namespace Wizard;

public class LeaveTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[5]
	{
		AIPlayTagType.LeaveToken,
		AIPlayTagType.LeaveHeal,
		AIPlayTagType.LeaveDamage,
		AIPlayTagType.LeaveAttachTag,
		AIPlayTagType.LeaveBanish
	};

	private List<AIPlayTag> _leaveTokenTags;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public bool HasLeaveToken
	{
		get
		{
			if (_leaveTokenTags != null)
			{
				return _leaveTokenTags.Count > 0;
			}
			return false;
		}
	}

	public LeaveTagCollection()
		: base(TagCollectionType.WhenLeave)
	{
	}

	public LeaveTagCollection(LeaveTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection.HasLeaveToken)
		{
			_leaveTokenTags = new List<AIPlayTag>(tagCollection._leaveTokenTags);
		}
	}

	public override TagCollection Clone()
	{
		return new LeaveTagCollection(this);
	}

	public override void Clear()
	{
		base.Clear();
		if (_leaveTokenTags != null)
		{
			_leaveTokenTags.Clear();
			_leaveTokenTags = null;
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.LeaveToken)
		{
			if (_leaveTokenTags == null)
			{
				_leaveTokenTags = new List<AIPlayTag>();
			}
			_leaveTokenTags.Add(tag);
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_leaveTokenTags, tag);
	}

	public void Execute(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return;
		}
		List<AIPlayTag> passedConditionTags = null;
		AIVirtualField field = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				if (passedConditionTags == null)
				{
					passedConditionTags = new List<AIPlayTag>();
				}
				passedConditionTags.Add(aIPlayTag);
			}
		}
		if (passedConditionTags == null)
		{
			return;
		}
		situation.RegisterNewProcessInfo(tagOwner, AISituationTriggerInformation.TriggerType.Leave).AddExecutingAction(delegate
		{
			for (int j = 0; j < passedConditionTags.Count; j++)
			{
				passedConditionTags[j].ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		});
	}

	public List<AITokenInformation> GetLeaveTokenIds(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!HasLeaveToken)
		{
			return null;
		}
		List<AITokenInformation> list = null;
		for (int i = 0; i < _leaveTokenTags.Count; i++)
		{
			AIPlayTag aIPlayTag = _leaveTokenTags[i];
			if (aIPlayTag.ArgumentExpressions is AILeaveToken aILeaveToken && aIPlayTag.CheckCondition(card, playPtn, field, situation))
			{
				List<AITokenInformation> tokenIds = aILeaveToken.GetTokenIds(card, field, playPtn, situation);
				if (tokenIds != null)
				{
					list = AIParamQuery.AddRangeToList(tokenIds, list);
				}
			}
		}
		return list;
	}
}

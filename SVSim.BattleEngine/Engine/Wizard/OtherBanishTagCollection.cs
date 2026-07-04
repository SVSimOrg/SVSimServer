using System.Collections.Generic;

namespace Wizard;

public class OtherBanishTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTypes = new AIPlayTagType[2]
	{
		AIPlayTagType.OtherBanishToken,
		AIPlayTagType.OtherBanishAddCemetery
	};

	private List<AIPlayTag> _banishTokenTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTypes;

	public bool HasBanishToken
	{
		get
		{
			if (_banishTokenTagList != null)
			{
				return _banishTokenTagList.Count > 0;
			}
			return false;
		}
	}

	public OtherBanishTagCollection()
		: base(TagCollectionType.WhenOtherBanish)
	{
	}

	private OtherBanishTagCollection(OtherBanishTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection.HasBanishToken)
		{
			_banishTokenTagList = new List<AIPlayTag>(tagCollection._banishTokenTagList);
		}
	}

	public override TagCollection Clone()
	{
		return new OtherBanishTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.OtherBanishToken)
		{
			if (_banishTokenTagList == null)
			{
				_banishTokenTagList = new List<AIPlayTag>();
			}
			_banishTokenTagList.Add(tag);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (HasBanishToken)
		{
			_banishTokenTagList.Clear();
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_banishTokenTagList, tag);
	}

	public void RegisterExecutingTagActions(AIVirtualField field, AIVirtualCard tagOwner, AIVirtualCard banishedCard, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !base.HasTag)
		{
			return;
		}
		List<int> conditionPassedIndexList = GetConditionPassedIndexList(tagOwner, field, playPtn, situation);
		if (conditionPassedIndexList != null && conditionPassedIndexList.Count > 0)
		{
			situation.RegisterNewProcessInfo(banishedCard, AISituationTriggerInformation.TriggerType.Banish).AddExecutingAction(delegate
			{
				Execute(tagOwner, banishedCard, conditionPassedIndexList, field, playPtn, situation);
			});
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualCard banishedCard, List<int> conditionPassedIndexList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !base.HasTag || conditionPassedIndexList == null || conditionPassedIndexList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < conditionPassedIndexList.Count; i++)
		{
			if (base.TagList.Count > conditionPassedIndexList[i])
			{
				AIPlayTag aIPlayTag = base.TagList[conditionPassedIndexList[i]];
				if (aIPlayTag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
				{
					aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, banishedCard, situation);
				}
				else
				{
					aIPlayTag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
				}
			}
		}
	}
}

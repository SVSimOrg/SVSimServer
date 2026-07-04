using System.Collections.Generic;

namespace Wizard;

public class DamagedTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[6]
	{
		AIPlayTagType.DamagedBonus,
		AIPlayTagType.DamagedBuff,
		AIPlayTagType.DamagedDamage,
		AIPlayTagType.DamagedToken,
		AIPlayTagType.DamagedHeal,
		AIPlayTagType.DamagedCantUnderAttack
	};

	private List<AIPlayTag> _damagedBuffList;

	private List<AIPlayTag> _damagedTokenList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public bool HasDamagedBuff
	{
		get
		{
			if (_damagedBuffList != null)
			{
				return _damagedBuffList.Count > 0;
			}
			return false;
		}
	}

	public DamagedTagCollection()
		: base(TagCollectionType.WhenDamaged)
	{
	}

	private DamagedTagCollection(DamagedTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.DamagedBuff:
			_damagedBuffList = AIParamQuery.AddElementToList(tag, _damagedBuffList, isBlockDuplicate: true);
			break;
		case AIPlayTagType.DamagedToken:
			_damagedTokenList = AIParamQuery.AddElementToList(tag, _damagedTokenList, isBlockDuplicate: true);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_damagedBuffList != null)
		{
			_damagedBuffList.Clear();
			_damagedBuffList = null;
		}
		if (_damagedTokenList != null)
		{
			_damagedTokenList.Clear();
			_damagedTokenList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		base.RemoveTagFromManagedTagList(tag);
		if (!RemoveTagFromList(_damagedBuffList, tag))
		{
			RemoveTagFromList(_damagedTokenList, tag);
		}
	}

	public override TagCollection Clone()
	{
		return new DamagedTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !base.HasTag)
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
		if (passedConditionTags != null)
		{
			situation.RegisterNewProcessInfo(tagOwner, AISituationTriggerInformation.TriggerType.Damage).AddExecutingAction(delegate
			{
				ExecuteTagAction(passedConditionTags, tagOwner, field, playPtn, situation);
			});
		}
	}

	private void ExecuteTagAction(List<AIPlayTag> passedConditionTags, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.IsDead)
		{
			for (int i = 0; i < passedConditionTags.Count; i++)
			{
				passedConditionTags[i].ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}

	public void GetDamagedBuffValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, out int atkBuff, out int lifeBuff)
	{
		atkBuff = 0;
		lifeBuff = 0;
		if (!HasDamagedBuff || tagOwner.IsDead)
		{
			return;
		}
		for (int i = 0; i < _damagedBuffList.Count; i++)
		{
			if (_damagedBuffList[i].ArgumentExpressions is AIDamagedBuff aIDamagedBuff)
			{
				atkBuff += aIDamagedBuff.GetAttackBuff(tagOwner, field, playPtn, situation);
				lifeBuff += aIDamagedBuff.GetLifeBuff(tagOwner, field, playPtn, situation);
			}
		}
	}
}

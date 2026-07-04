using System.Collections.Generic;

namespace Wizard;

public class SummonTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[11]
	{
		AIPlayTagType.SummonDamage,
		AIPlayTagType.SummonEvo,
		AIPlayTagType.SummonRush,
		AIPlayTagType.SummonQuick,
		AIPlayTagType.SummonBuff,
		AIPlayTagType.SummonBanish,
		AIPlayTagType.SummonAttachTag,
		AIPlayTagType.SummonHeal,
		AIPlayTagType.SummonDestroy,
		AIPlayTagType.Stack,
		AIPlayTagType.SummonBanAttack
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public List<AIPlayTag> SummonAttachTagList { get; private set; }

	public bool HasSummonAttachTag
	{
		get
		{
			if (SummonAttachTagList != null)
			{
				return SummonAttachTagList.Count > 0;
			}
			return false;
		}
	}

	public SummonTagCollection()
		: base(TagCollectionType.WhenSummon)
	{
	}

	private SummonTagCollection(SummonTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection.HasSummonAttachTag)
		{
			SummonAttachTagList = new List<AIPlayTag>(tagCollection.SummonAttachTagList);
		}
	}

	public override TagCollection Clone()
	{
		return new SummonTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.SummonAttachTag)
		{
			SummonAttachTagList = AIParamQuery.AddElementToList(tag, SummonAttachTagList);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (SummonAttachTagList != null)
		{
			SummonAttachTagList.Clear();
			SummonAttachTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(SummonAttachTagList, tag);
	}

	public void RegisterConditionPassedTag(AIVirtualCard tagOwner, AIVirtualCard summonCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		AIVirtualField field = tagOwner.SelfField;
		List<int> list = new List<int>();
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, playPtn, field, situation))
			{
				list.Add(i);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		AISkillProcessInformation aISkillProcessInformation = situation.RegisterNewProcessInfo(summonCard, AISituationTriggerInformation.TriggerType.Summon);
		for (int j = 0; j < list.Count; j++)
		{
			AIPlayTag tag = base.TagList[list[j]];
			aISkillProcessInformation.AddExecutingAction(delegate
			{
				if (!tagOwner.IsDead)
				{
					tag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
				}
			});
		}
	}
}

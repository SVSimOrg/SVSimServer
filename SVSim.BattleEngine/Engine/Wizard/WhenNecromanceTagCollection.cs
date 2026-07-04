using System.Collections.Generic;

namespace Wizard;

public class WhenNecromanceTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[4]
	{
		AIPlayTagType.NecromanceAttachTag,
		AIPlayTagType.NecromanceAddCemetery,
		AIPlayTagType.NecromanceDamage,
		AIPlayTagType.NecromanceHeal
	};

	private List<AIPlayTag> _necromanceAddCemeteryList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public WhenNecromanceTagCollection()
		: base(TagCollectionType.WhenNecromance)
	{
	}

	private WhenNecromanceTagCollection(WhenNecromanceTagCollection tagCollection)
		: base(tagCollection)
	{
		_necromanceAddCemeteryList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._necromanceAddCemeteryList);
	}

	public override TagCollection Clone()
	{
		return new WhenNecromanceTagCollection(this);
	}

	public void Execute(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		AISkillProcessInformation aISkillProcessInformation = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag tag = base.TagList[i];
			if (tag.CheckCondition(owner, playPtn, field, situation))
			{
				if (aISkillProcessInformation == null)
				{
					aISkillProcessInformation = situation.RegisterNewPreprocessProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined);
				}
				aISkillProcessInformation.AddExecutingAction(delegate
				{
					tag.ArgumentExpressions.Execute(owner, field, playPtn, situation);
				});
			}
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.NecromanceAddCemetery)
		{
			_necromanceAddCemeteryList = AIParamQuery.AddElementToList(tag, _necromanceAddCemeteryList);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_necromanceAddCemeteryList != null)
		{
			_necromanceAddCemeteryList.Clear();
			_necromanceAddCemeteryList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_necromanceAddCemeteryList, tag);
	}
}

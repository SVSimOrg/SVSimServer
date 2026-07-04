using System.Collections.Generic;

namespace Wizard;

public class EvolvedResidentTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[3]
	{
		AIPlayTagType.EvolvedAttackableCount,
		AIPlayTagType.EvolvedAttackable,
		AIPlayTagType.EvolvedSkill
	};

	private List<AIPlayTag> _evolvedAttackableCountList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public EvolvedResidentTagCollection()
		: base(TagCollectionType.EvolvedResident)
	{
		_evolvedAttackableCountList = null;
	}

	private EvolvedResidentTagCollection(EvolvedResidentTagCollection param)
		: base(param)
	{
		_evolvedAttackableCountList = param.CreateEvolvedAttackableCountListClone();
	}

	public override TagCollection Clone()
	{
		return new EvolvedResidentTagCollection(this);
	}

	public List<AIPlayTag> CreateEvolvedAttackableCountListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_evolvedAttackableCountList);
	}

	public void Execute(AIVirtualCard tagOwner, AISituationInfo situation)
	{
		if (!tagOwner.IsEvolution || !base.HasTag)
		{
			return;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		List<int> conditionPassedIndexList = GetConditionPassedIndexList(tagOwner, selfField, bestPlayPtn, situation);
		if (conditionPassedIndexList != null && conditionPassedIndexList.Count > 0)
		{
			for (int i = 0; i < conditionPassedIndexList.Count; i++)
			{
				base.TagList[conditionPassedIndexList[i]].ArgumentExpressions.Execute(tagOwner, selfField, bestPlayPtn, situation);
			}
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.EvolvedAttackableCount)
		{
			_evolvedAttackableCountList = AIParamQuery.AddElementToList(tag, _evolvedAttackableCountList);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_evolvedAttackableCountList != null)
		{
			_evolvedAttackableCountList.Clear();
			_evolvedAttackableCountList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_evolvedAttackableCountList, tag);
	}
}

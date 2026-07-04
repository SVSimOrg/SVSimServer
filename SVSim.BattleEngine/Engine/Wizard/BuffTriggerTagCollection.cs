using System.Collections.Generic;

namespace Wizard;

public class BuffTriggerTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[10]
	{
		AIPlayTagType.BuffDamage,
		AIPlayTagType.BuffRecoverPP,
		AIPlayTagType.BuffRush,
		AIPlayTagType.BuffHeal,
		AIPlayTagType.BuffBuff,
		AIPlayTagType.BuffDestroy,
		AIPlayTagType.BuffToken,
		AIPlayTagType.BuffEvo,
		AIPlayTagType.BuffDraw,
		AIPlayTagType.BuffShield
	};

	private List<AIPlayTag> _buffTokenTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public BuffTriggerTagCollection()
		: base(TagCollectionType.WhenBuff)
	{
		_buffTokenTagList = null;
	}

	private BuffTriggerTagCollection(BuffTriggerTagCollection tagCollection)
		: base(tagCollection)
	{
		_buffTokenTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._buffTokenTagList);
	}

	public override TagCollection Clone()
	{
		return new BuffTriggerTagCollection(this);
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualCard buffedCard, List<int> playPtn, AISituationInfo situation)
	{
		if (buffedCard == null || !base.HasTag)
		{
			return;
		}
		AIVirtualField field = tagOwner.SelfField;
		List<int> conditionPassedIndexList = GetConditionPassedIndexList(tagOwner, field, playPtn, situation);
		if (conditionPassedIndexList == null || conditionPassedIndexList.Count <= 0)
		{
			return;
		}
		AISkillProcessInformation aISkillProcessInformation = situation.RegisterNewProcessInfo(buffedCard, AISituationTriggerInformation.TriggerType.Undefined);
		for (int i = 0; i < conditionPassedIndexList.Count; i++)
		{
			AIScriptArgumentExpressions argument = base.TagList[conditionPassedIndexList[i]].ArgumentExpressions;
			aISkillProcessInformation.AddExecutingAction(delegate
			{
				if (argument is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
				{
					aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, buffedCard, situation);
				}
				else
				{
					argument.Execute(tagOwner, field, playPtn, situation);
				}
			});
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.BuffToken)
		{
			_buffTokenTagList = AIParamQuery.AddElementToList(tag, _buffTokenTagList);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_buffTokenTagList != null)
		{
			_buffTokenTagList.Clear();
			_buffTokenTagList = null;
		}
	}
}

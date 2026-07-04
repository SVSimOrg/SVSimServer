using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class BreakTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[9]
	{
		AIPlayTagType.BreakDamage,
		AIPlayTagType.BreakBuff,
		AIPlayTagType.BreakDestroy,
		AIPlayTagType.BreakHeal,
		AIPlayTagType.BreakAttachTag,
		AIPlayTagType.BreakAddStack,
		AIPlayTagType.BreakRecoverAttackableCount,
		AIPlayTagType.BreakSetLeaderMaxLife,
		AIPlayTagType.BreakRecoverPp
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public BreakTagCollection()
		: base(TagCollectionType.WhenBreak)
	{
	}

	private BreakTagCollection(BreakTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new BreakTagCollection(this);
	}

	public void RegisterExecutingTagActions(AIVirtualField field, AIVirtualCard tagOwner, AIVirtualCard dieCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		List<int> conditionPassedIndexList = GetConditionPassedIndexList(tagOwner, field, playPtn, situation);
		if (conditionPassedIndexList != null && conditionPassedIndexList.Count > 0)
		{
			situation.RegisterNewProcessInfo(dieCard, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
			{
				Execute(field, tagOwner, dieCard, playPtn, conditionPassedIndexList, situation);
			});
		}
	}

	public void Execute(AIVirtualField field, AIVirtualCard tagOwner, AIVirtualCard dieCard, List<int> playPtn, List<int> tagIndexList, AISituationInfo situation)
	{
		if (!base.HasTag || tagIndexList == null || tagIndexList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < tagIndexList.Count; i++)
		{
			if (base.TagList.Count > tagIndexList[i])
			{
				AIPlayTag aIPlayTag = base.TagList[tagIndexList[i]];
				if (aIPlayTag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
				{
					aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, field.BestPlayPtn, dieCard, situation);
				}
				else if (aIPlayTag.ArgumentExpressions is AIFiltersArgument aIFiltersArgument)
				{
					aIFiltersArgument.Execute(tagOwner, field, playPtn, dieCard, situation);
				}
			}
		}
	}
}

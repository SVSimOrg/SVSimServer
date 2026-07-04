using System.Collections.Generic;

namespace Wizard;

public class PlaySkipTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[5]
	{
		AIPlayTagType.PlaySkip,
		AIPlayTagType.PlaySkipWithEvo,
		AIPlayTagType.PlaySkipIfEvo,
		AIPlayTagType.PlaySkipWithAction,
		AIPlayTagType.PlaySkipWithActionIfEvo
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public PlaySkipTagCollection()
		: base(TagCollectionType.PlaySkip)
	{
	}

	private PlaySkipTagCollection(PlaySkipTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new PlaySkipTagCollection(this);
	}

	public List<PlaySkipInformation> RegisterPlaySkipInfo(AIVirtualCard tagOwner, List<int> playPtn, List<PlaySkipInformation> lastInfoList, AISituationInfo situation)
	{
		if (!base.HasTag || tagOwner == null)
		{
			return null;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (IsConditionLegal(aIPlayTag, tagOwner) && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				lastInfoList = AIParamQuery.AddElementToList((aIPlayTag.ArgumentExpressions as AIPlaySkipTagArgument).CreatePlaySkipInformation(tagOwner, selfField, playPtn, situation), lastInfoList);
			}
		}
		return lastInfoList;
	}

	private bool IsConditionLegal(AIPlayTag tag, AIVirtualCard tagOwner)
	{
		if (tagOwner.IsAlly)
		{
			if (!tag.Condition.Contains("IS_IN_HAND") && !tag.Condition.Contains("IS_ON_FIELD"))
			{
				AIConsoleUtility.LogError($"IsConditionLegal() Error: Tag has no IS_IN_HAND or IS_ON_FIELD condition! CardId = {tagOwner.BaseId}");
				return false;
			}
		}
		else if (tag.Condition.Contains("IS_IN_HAND"))
		{
			AIConsoleUtility.LogError($"IsConditionLegal() Error: Tag has forbidden IS_IN_HAND condition! CardId = {tagOwner.BaseId}");
			return false;
		}
		return true;
	}
}

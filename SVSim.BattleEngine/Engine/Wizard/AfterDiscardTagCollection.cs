using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AfterDiscardTagCollection : TagCollection
{
	private static AIPlayTagType[] _managedTagTypes = new AIPlayTagType[3]
	{
		AIPlayTagType.DiscardDamage,
		AIPlayTagType.DiscardHeal,
		AIPlayTagType.AllyDiscardBonus
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public AfterDiscardTagCollection()
		: base(TagCollectionType.WhenAfterDiscard)
	{
	}

	public AfterDiscardTagCollection(AfterDiscardTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new AfterDiscardTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !base.HasTag)
		{
			return;
		}
		List<AIPlayTag> passedConditionTags = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, tagOwner.SelfField, situation))
			{
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags);
			}
		}
		if (passedConditionTags != null)
		{
			situation.RegisterNewProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
			{
				ExecuteTagAction(passedConditionTags, tagOwner, tagOwner.SelfField, playPtn, situation);
			});
		}
	}

	private void ExecuteTagAction(List<AIPlayTag> tags, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead)
		{
			return;
		}
		for (int i = 0; i < tags.Count; i++)
		{
			AIPlayTag aIPlayTag = tags[i];
			if (aIPlayTag.Type == AIPlayTagType.AllyDiscardBonus && aIPlayTag.ArgumentExpressions is AIAllyDiscardBonus)
			{
				((AIAllyDiscardBonus)aIPlayTag.ArgumentExpressions).Execute(tagOwner, tagOwner.SelfField, playPtn, useIgnoreInBattle: true, situation);
			}
			else
			{
				aIPlayTag.ArgumentExpressions.Execute(tagOwner, tagOwner.SelfField, playPtn, situation);
			}
		}
	}

	public float GetAllyDiscardBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool useIgnoreInBattle)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		float num = 0f;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.Type == AIPlayTagType.AllyDiscardBonus && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				AIAllyDiscardBonus aIAllyDiscardBonus = aIPlayTag.ArgumentExpressions as AIAllyDiscardBonus;
				num += aIAllyDiscardBonus.GetBonusValue(tagOwner, playPtn, situation, useIgnoreInBattle);
			}
		}
		return num;
	}
}

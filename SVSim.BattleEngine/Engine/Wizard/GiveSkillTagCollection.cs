using System.Collections.Generic;

namespace Wizard;

public class GiveSkillTagCollection : TagCollection
{
	public static bool IsGiveSkillManagedSkillType(AIScriptTokenArgType skillType)
	{
		if (skillType != AIScriptTokenArgType.QUICK && skillType != AIScriptTokenArgType.RUSH && skillType != AIScriptTokenArgType.GUARD && skillType != AIScriptTokenArgType.DRAIN && skillType != AIScriptTokenArgType.KILLER && skillType != AIScriptTokenArgType.SNEAK && skillType != AIScriptTokenArgType.UNTOUCHABLE && skillType != AIScriptTokenArgType.FORCE_TARGETING)
		{
			return skillType == AIScriptTokenArgType.UNBANISHABLE;
		}
		return true;
	}

	public GiveSkillTagCollection()
		: base(TagCollectionType.GiveSkill)
	{
	}

	private GiveSkillTagCollection(GiveSkillTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new GiveSkillTagCollection(this);
	}

	public bool ExecuteSkill(AIVirtualCard tagOwner, List<int> playPtn, AIScriptTokenArgType skill)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return false;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, null) && aIPlayTag.ArgumentExpressions is AIGiveSkill && (aIPlayTag.ArgumentExpressions as AIGiveSkill).GetSkillType() == skill)
			{
				return true;
			}
		}
		return false;
	}

	public void EnqueueGiveSkill(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
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
					aISkillProcessInformation = situation.RegisterNewProcessInfo(situation.Actor, AISituationTriggerInformation.TriggerType.Undefined);
				}
				aISkillProcessInformation.AddExecutingAction(delegate
				{
					AIScriptTokenArgType skillType = (tag.ArgumentExpressions as AIGiveSkill).GetSkillType();
					AISkillSimulationUtility.GiveSkill(owner, field, skillType);
				});
			}
		}
	}

	public override void ExecuteWhenAddTag(AIVirtualCard card, AIVirtualField field, AIPlayTag tag, AISituationInfo situation)
	{
		if (tag.ArgumentExpressions is AIGiveSkill aIGiveSkill && tag.CheckCondition(card, field.BestPlayPtn, field, situation))
		{
			AIScriptTokenArgType skillType = aIGiveSkill.GetSkillType();
			AISkillSimulationUtility.GiveSkill(card, field, skillType);
			if (skillType == AIScriptTokenArgType.SNEAK)
			{
				card.RegisterGiveSneakTag(tag);
			}
		}
	}
}

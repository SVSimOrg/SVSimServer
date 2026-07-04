namespace Wizard;

public class RemoveSkillTagCollection : TagCollection
{
	public RemoveSkillTagCollection()
		: base(TagCollectionType.RemoveSkill)
	{
	}

	private RemoveSkillTagCollection(RemoveSkillTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new RemoveSkillTagCollection(this);
	}

	public void Execute(AIVirtualCard tagOwner, AIScriptTokenArgType timing, AISituationInfo situation)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			AIRemoveSkill aIRemoveSkill = aIPlayTag.ArgumentExpressions as AIRemoveSkill;
			if (aIRemoveSkill.Timing == timing && aIPlayTag.CheckCondition(tagOwner, selfField.BestPlayPtn, selfField, situation))
			{
				if (aIRemoveSkill.RemoveSkillType == AIScriptTokenArgType.ALL)
				{
					tagOwner.RemoveAllSkills(situation);
					break;
				}
				if (aIRemoveSkill.RemoveSkillType == AIScriptTokenArgType.KILLER)
				{
					tagOwner.IsKiller = false;
					break;
				}
			}
		}
	}
}

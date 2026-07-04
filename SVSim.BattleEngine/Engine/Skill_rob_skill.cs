public class Skill_rob_skill : SkillBaseCopy
{
	public Skill_rob_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override bool IsRemain()
	{
		return false;
	}
}

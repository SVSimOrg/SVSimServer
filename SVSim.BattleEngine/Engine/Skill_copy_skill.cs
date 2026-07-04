public class Skill_copy_skill : SkillBaseCopy
{
	public Skill_copy_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override bool IsRemain()
	{
		return true;
	}
}

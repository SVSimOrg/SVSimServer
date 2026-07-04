public class DamageInfo
{

	public SkillBase Skill { get; private set; }

	public int Damage { get; private set; }

	public DamageInfo(SkillBase skill, int damage)
	{
		Skill = skill;
		Damage = damage;
	}
}

public class ReflectionInfo
{
	public enum TargetType
	{
		CLASS,
		DAMAGE_OWNER
	}

	public enum DamageType
	{
		ALL,
		SKILL
	}

	public TargetType Target { get; private set; }

	public DamageType Type { get; private set; }

	public ReflectionInfo(string target, string type)
	{
		switch (target)
		{
		case "class":
			Target = TargetType.CLASS;
			break;
		case "damage_owner":
			Target = TargetType.DAMAGE_OWNER;
			break;
		}
		if (type != null && type == "skill")
		{
			Type = DamageType.SKILL;
		}
		else
		{
			Type = DamageType.ALL;
		}
	}
}

public class ShieldInfo
{
	public enum ShieldType
	{
		ALL,
		SKILL,
		SPELL,
		ATTACK
	}

	public BattleCardBase OwnerCard { get; private set; }

	public ShieldType Type { get; private set; }

	public ShieldInfo(BattleCardBase card, string type)
	{
		OwnerCard = card;
		switch (type)
		{
		case "skill":
			Type = ShieldType.SKILL;
			break;
		case "spell":
			Type = ShieldType.SPELL;
			break;
		case "attack":
			Type = ShieldType.ATTACK;
			break;
		}
	}
}

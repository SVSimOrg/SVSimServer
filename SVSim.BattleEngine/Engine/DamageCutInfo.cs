public class DamageCutInfo
{
	public enum DamageType
	{
		ALL,
		SKILL
	}

	public int CutAmount { get; private set; }

	public DamageType Type { get; private set; }

	public BattleCardBase OwnerCard { get; private set; }

	public string DuplicateBanSkillNum { get; private set; }

	public DamageCutInfo(int amount, DamageType type, BattleCardBase ownerCard, string _duplicateBanSkillNum)
	{
		CutAmount = amount;
		Type = type;
		OwnerCard = ownerCard;
		DuplicateBanSkillNum = _duplicateBanSkillNum;
	}
}

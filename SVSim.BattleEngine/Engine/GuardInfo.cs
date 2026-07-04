public class GuardInfo
{
	public BattleCardBase OwnerCard { get; private set; }

	public string DuplicateBanSkillNum { get; private set; }

	public GuardInfo(BattleCardBase card, string _duplicateBanSkillNum)
	{
		OwnerCard = card;
		DuplicateBanSkillNum = _duplicateBanSkillNum;
	}
}

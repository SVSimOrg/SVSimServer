public class RushInfo
{
	public BattleCardBase OwnerCard { get; private set; }

	public string DuplicateBanSkillNum { get; private set; }

	public RushInfo(BattleCardBase card, string _duplicateBanSkillNum)
	{
		OwnerCard = card;
		DuplicateBanSkillNum = _duplicateBanSkillNum;
	}
}

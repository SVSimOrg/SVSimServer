public class SkillParameterSelectRandomGenericValueFilter : SkillParameterSelectGenericValueFilter
{
	public SkillParameterSelectRandomGenericValueFilter(string index, SkillBase skill)
		: base(index, skill)
	{
	}

	protected override int GetGenericValue(BattleCardBase card)
	{
		int num = int.Parse(_key);
		int num2 = card.SkillApplyInformation.SkillGenericValueArray[num];
		if (num2 <= 0)
		{
			return 0;
		}
		return card.SelfBattlePlayer.BattleMgr.StableRandom(num2 + 1);
	}
}

public class NetworkSkill_force_wrath : Skill_force_wrath
{
	public NetworkSkill_force_wrath(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		ClassBattleCardBase classData = skillPrm.selfBattlePlayer.Class as ClassBattleCardBase;
		classData.OnForceWrathChange += delegate(BattlePlayerBase player, int forceWrathPoint)
		{
			if (skillPrm.selfBattlePlayer.BattleMgr is NetworkStandardBattleMgr)
			{
				NetworkStandardBattleMgr networkStandardBattleMgr = (NetworkStandardBattleMgr)skillPrm.selfBattlePlayer.BattleMgr;
				if (networkStandardBattleMgr != null)
				{
					int num = ((!classData.IsPlayer) ? 1 : 0);
					networkStandardBattleMgr.beforeWrathCount[num] = forceWrathPoint;
				}
			}
		};
	}
}

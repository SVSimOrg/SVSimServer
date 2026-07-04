public class NetworkSkill_force_avarice : Skill_force_avarice
{
	public NetworkSkill_force_avarice(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		ClassBattleCardBase classData = skillPrm.selfBattlePlayer.Class as ClassBattleCardBase;
		classData.OnForceAvariceChange += delegate(BattlePlayerBase player, int forceAvaricePoint)
		{
			int num = 0;
			if (skillPrm.selfBattlePlayer.BattleMgr is NetworkStandardBattleMgr)
			{
				NetworkStandardBattleMgr networkStandardBattleMgr = (NetworkStandardBattleMgr)skillPrm.selfBattlePlayer.BattleMgr;
				if (networkStandardBattleMgr != null)
				{
					num = ((!classData.IsPlayer) ? 1 : 0);
					if (!SkillConditionAvarice.IsAvarice(skillPrm.selfBattlePlayer.TurnDrawCards.Count))
					{
						if (networkStandardBattleMgr.beforeAvariceCount[num] == 0 && forceAvaricePoint >= 1)
						{
							networkStandardBattleMgr.RegisterAvariceTrigger(player, 1);
						}
						else if (networkStandardBattleMgr.beforeAvariceCount[num] >= 1 && forceAvaricePoint <= 0)
						{
							networkStandardBattleMgr.RegisterAvariceTrigger(player, 0);
						}
					}
					networkStandardBattleMgr.beforeAvariceCount[num] = forceAvaricePoint;
				}
			}
		};
	}
}

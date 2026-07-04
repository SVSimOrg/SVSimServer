public class NetworkSkill_force_berserk : Skill_force_berserk
{
	public NetworkSkill_force_berserk(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		ClassBattleCardBase classData = skillPrm.selfBattlePlayer.Class as ClassBattleCardBase;
		classData.OnForceBerserkChange += delegate(BattlePlayerBase player, int forceBerserkPoint)
		{
			int num = 0;
			if (skillPrm.selfBattlePlayer.BattleMgr is NetworkStandardBattleMgr)
			{
				NetworkStandardBattleMgr networkStandardBattleMgr = (NetworkStandardBattleMgr)skillPrm.selfBattlePlayer.BattleMgr;
				if (networkStandardBattleMgr != null)
				{
					num = ((!classData.IsPlayer) ? 1 : 0);
					if (!SkillConditionHalfLife.IsHalfLife(classData.Life))
					{
						if (networkStandardBattleMgr.beforeRevengeCount[num] == 0 && forceBerserkPoint >= 1)
						{
							networkStandardBattleMgr.RegisterRevengeTrigger(player, 1);
						}
						else if (networkStandardBattleMgr.beforeRevengeCount[num] >= 1 && forceBerserkPoint <= 0)
						{
							networkStandardBattleMgr.RegisterRevengeTrigger(player, 0);
						}
					}
					networkStandardBattleMgr.beforeRevengeCount[num] = forceBerserkPoint;
				}
			}
		};
	}
}

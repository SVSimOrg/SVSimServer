using System;
using System.Linq;
using Wizard.Battle;

public class SkillEnvironmentalJatelantDamageAndHealCountFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		int num = playerInfo.SkillInfoNecromanceZoneCards.Where((IReadOnlyBattleCardInfo c) => c.IsDead && (c is FieldBattleCard || c is ChantFieldBattleCard)).Count();
		num += playerInfo.SkillInfoCemeterys.Where((IReadOnlyBattleCardInfo c) => c.IsDead && (c is FieldBattleCard || c is ChantFieldBattleCard)).Count();
		if (num < 2)
		{
			return 0;
		}
		return Math.Min((num - 2) / 3 + 1, 5) * 3;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}

using System;
using System.Linq;
using Wizard.Battle;

public class SkillEnvironmentalJatelantBanishCountFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		int num = playerInfo.SkillInfoNecromanceZoneCards.Where((IReadOnlyBattleCardInfo c) => c.IsDead && (c is FieldBattleCard || c is ChantFieldBattleCard)).Count();
		num += playerInfo.SkillInfoCemeterys.Where((IReadOnlyBattleCardInfo c) => c.IsDead && (c is FieldBattleCard || c is ChantFieldBattleCard)).Count();
		if (num < 1)
		{
			return 0;
		}
		return Math.Min((num - 1) / 3 + 1, 5);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}

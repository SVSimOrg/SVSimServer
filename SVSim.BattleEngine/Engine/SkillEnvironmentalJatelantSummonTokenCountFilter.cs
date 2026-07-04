using System;
using System.Linq;
using Wizard.Battle;

public class SkillEnvironmentalJatelantSummonTokenCountFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		int num = playerInfo.SkillInfoNecromanceZoneCards.Where((IReadOnlyBattleCardInfo c) => c.IsDead && (c is FieldBattleCard || c is ChantFieldBattleCard)).Count();
		num += playerInfo.SkillInfoCemeterys.Where((IReadOnlyBattleCardInfo c) => c.IsDead && (c is FieldBattleCard || c is ChantFieldBattleCard)).Count();
		if (num < 3)
		{
			return 0;
		}
		return Math.Min((num - 3) / 3 + 1, 4);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}

using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class NetworkSkillRandomEachSameBaseCardIdFilter : SkillRandomEachSameBaseCardIdFilter
{
	public NetworkSkillRandomEachSameBaseCardIdFilter(int randomCount, BattlePlayerBase player, SkillBase skill)
		: base(randomCount, player)
	{
		if (!player.IsPlayer)
		{
			skill.OnSkillStart += CaliculateCount;
			skill.OnSkillEnd += ResetCalcCount;
		}
	}

	public void CaliculateCount(SkillBase skill, List<BattleCardBase> targetCards, SkillConditionCheckerOption checkerOption)
	{
		_count = targetCards.Count;
	}

	public VfxBase ResetCalcCount(SkillBase skill, List<BattleCardBase> targetCards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		_count = 0;
		return NullVfx.GetInstance();
	}
}

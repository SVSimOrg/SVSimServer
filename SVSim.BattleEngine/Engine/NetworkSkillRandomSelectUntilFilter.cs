using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkillRandomSelectUntilFilter : SkillRandomSelectUntilFilter
{
	public NetworkSkillRandomSelectUntilFilter(string randomCountText, BattlePlayerBase player, SkillBase skill)
		: base(randomCountText, player)
	{
		if (!player.IsPlayer)
		{
			skill.OnSkillStart += CaliculateCount;
			skill.OnSkillEnd += ResetCalcCount;
		}
	}

	public void CaliculateCount(SkillBase skill, List<BattleCardBase> targetCards, SkillConditionCheckerOption checkerOption)
	{
		BattlePlayerBase battlePlayerBase = ((skill.ApplyBattlePlayerFilter is SelfBattlePlayerFilter) ? skill.SkillPrm.selfBattlePlayer : skill.SkillPrm.opponentBattlePlayer);
		if (targetCards.Count <= 0 && battlePlayerBase.DeckCardList.Count <= 0)
		{
			_count = 1;
			return;
		}
		_count = targetCards.Count;
		if (((NetworkBattleManagerBase)battlePlayerBase.BattleMgr).networkBattleData.GetReceiveData().unapprovedList.Any((CardDataModel u) => u.Index == -99))
		{
			_count++;
		}
	}

	public VfxBase ResetCalcCount(SkillBase skill, List<BattleCardBase> targetCards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		_count = 0;
		return NullVfx.GetInstance();
	}
}

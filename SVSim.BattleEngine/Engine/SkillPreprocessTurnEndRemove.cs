using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnEndRemove : SkillPreprocessBase
{
	private readonly string _target;

	public SkillPreprocessTurnEndRemove(string target)
	{
		_target = target;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		BattlePlayerBase battlePlayer = ((_target == "me") ? playerPair.Self : playerPair.Opponent);
		Func<SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate
		{
			battlePlayer.OnTurnEnd -= callStopOneTime;
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			BattleCardBase ownerCard = skill.SkillPrm.ownerCard;
			parallelVfxPlayer.Register(skill.RemoveAfter());
			ownerCard.Skills.Remove(skill);
			List<BattleCardBase> list = playerPair.Self.InPlayCards.ToList();
			list.AddRange(playerPair.Opponent.InPlayCards);
			SkillIconInitialize(parallelVfxPlayer, list);
			return parallelVfxPlayer;
		};
		battlePlayer.OnTurnEnd += callStopOneTime;
		return NullVfx.GetInstance();
	}
}

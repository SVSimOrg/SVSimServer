using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnEndStopAndRemove : SkillPreprocessBase
{
	private readonly string _target;

	private int _callCount;

	private Func<BattlePlayerReadOnlyInfoPair, int, int, bool> _conditionFunc;

	public SkillPreprocessTurnEndStopAndRemove(string target)
	{
		string[] array = DisassemblySpecialString(target);
		_target = array[0];
		_callCount = 0;
		_conditionFunc = GetConditionJudgeFunc((array.Length > 1) ? array[1] : string.Empty);
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		BattlePlayerBase battlePlayer = ((_target == "me") ? playerPair.Self : playerPair.Opponent);
		Func<SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(SkillProcessor skillProcessorOneTime)
		{
			_callCount++;
			if (_conditionFunc == null || _conditionFunc(playerPair, _callCount, 0))
			{
				battlePlayer.OnTurnEnd -= callStopOneTime;
				ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
				parallelVfxPlayer.Register(StopSkill(skill, skillProcessorOneTime));
				skill.SkillPrm.ownerCard.Skills.Remove(skill);
				List<BattleCardBase> list = playerPair.Self.InPlayCards.ToList();
				list.AddRange(playerPair.Opponent.InPlayCards);
				SkillIconInitialize(parallelVfxPlayer, list);
				return parallelVfxPlayer;
			}
			return NullVfx.GetInstance();
		};
		battlePlayer.OnTurnEnd += callStopOneTime;
		return NullVfx.GetInstance();
	}
}

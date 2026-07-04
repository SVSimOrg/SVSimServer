using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public abstract class SkillPreprocessSkillAfterStopBase : SkillPreprocessBase
{
	protected readonly string _target;

	protected int _callCount;

	protected int _turnEndCallCount;

	protected List<Func<BattlePlayerReadOnlyInfoPair, int, int, bool>> _conditionFuncList = new List<Func<BattlePlayerReadOnlyInfoPair, int, int, bool>>();

	protected List<string> optionList = new List<string>();

	protected SkillPreprocessSkillAfterStopBase(string target)
	{
		optionList = DisassemblySpecialString(target).ToList();
		_target = optionList[0];
		_callCount = 0;
		_turnEndCallCount = 0;
		foreach (string option in optionList)
		{
			_conditionFuncList.Add(GetConditionJudgeFunc((option != "me") ? option : string.Empty));
		}
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		BattlePlayerBase battlePlayer = ((_target == "me") ? playerPair.Self : playerPair.Opponent);
		BattlePlayerBase battleEnemy = ((_target == "me") ? playerPair.Opponent : playerPair.Self);
		Func<SkillProcessor, VfxBase> func = delegate
		{
			if (battlePlayer.IsSelfTurn)
			{
				_turnEndCallCount++;
				return NullVfx.GetInstance();
			}
			return NullVfx.GetInstance();
		};
		Func<SkillProcessor, VfxBase> func2 = delegate
		{
			if (battleEnemy.IsSelfTurn)
			{
				_turnEndCallCount++;
				return NullVfx.GetInstance();
			}
			return NullVfx.GetInstance();
		};
		if (optionList.Any((string s) => s.Contains("me_or_op_turn_end_count")))
		{
			battlePlayer.OnTurnEnd += func;
			battleEnemy.OnTurnEnd += func2;
		}
		else if (optionList.Any((string s) => s.Contains("op_turn_end_count")))
		{
			battleEnemy.OnTurnEnd += func2;
		}
		else if (optionList.Any((string s) => s.Contains("turn_end_count")))
		{
			battlePlayer.OnTurnEnd += func;
		}
		return OnStart(playerPair, skill, battlePlayer, battleEnemy, func, func2);
	}

	protected abstract VfxBase OnStart(BattlePlayerPair playerPair, SkillBase skill, BattlePlayerBase battlePlayer, BattlePlayerBase battleEnemy, Func<SkillProcessor, VfxBase> addTurnEndCallCountOnPlayerTurn, Func<SkillProcessor, VfxBase> addTurnEndCallCountOnEnemyTurn);
}

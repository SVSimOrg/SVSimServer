using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnEndStop : SkillPreprocessBase
{
	private readonly string _target;

	private int _callCount;

	private Func<BattlePlayerReadOnlyInfoPair, int, int, bool> _conditionFunc;

	public SkillPreprocessTurnEndStop(string target)
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
				return StopSkill(skill, skillProcessorOneTime);
			}
			return NullVfx.GetInstance();
		};
		battlePlayer.OnTurnEnd += callStopOneTime;
		return NullVfx.GetInstance();
	}
}

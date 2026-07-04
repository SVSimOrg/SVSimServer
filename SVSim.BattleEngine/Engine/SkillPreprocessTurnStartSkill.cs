using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnStartSkill : SkillPreprocessBase
{
	private string _target;

	private string _timingText;

	private int _turnCount;

	private int _turnStartCallCount;

	private Func<SkillProcessor, VfxBase> _addTurnStartCallCountOnPlayerTurn;

	public SkillPreprocessTurnStartSkill(BattleCardBase ownerCard, string target)
	{
		SkillPreprocessTurnStartSkill skillPreprocessTurnStartSkill = this;
		_turnStartCallCount = 0;
		List<string> list = DisassemblySpecialString(target).ToList();
		_target = list[0];
		BattlePlayerBase battlePlayer = ((_target == "me") ? ownerCard.SelfBattlePlayer : ownerCard.OpponentBattlePlayer);
		string[] array = list[1].Split('=');
		_timingText = array[0];
		_turnCount = int.Parse(array[1]);
		_addTurnStartCallCountOnPlayerTurn = delegate
		{
			if (battlePlayer.IsSelfTurn)
			{
				skillPreprocessTurnStartSkill._turnStartCallCount++;
				return NullVfx.GetInstance();
			}
			return NullVfx.GetInstance();
		};
		string timingText = _timingText;
		if (timingText != null && timingText == "me_turn_start_count")
		{
			battlePlayer.OnTurnStartBeforeDraw += _addTurnStartCallCountOnPlayerTurn;
		}
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return _turnStartCallCount >= _turnCount;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		BattlePlayerBase battlePlayerBase = ((_target == "me") ? playerPair.Self : playerPair.Opponent);
		string timingText = _timingText;
		if (timingText != null && timingText == "me_turn_start_count")
		{
			battlePlayerBase.OnTurnStartBeforeDraw -= _addTurnStartCallCountOnPlayerTurn;
		}
		return NullVfx.GetInstance();
	}
}

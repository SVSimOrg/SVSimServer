using System;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnStartSkillAfterStop : SkillPreprocessSkillAfterStopBase
{
	public SkillPreprocessTurnStartSkillAfterStop(string target)
		: base(target)
	{
	}

	protected override VfxBase OnStart(BattlePlayerPair playerPair, SkillBase skill, BattlePlayerBase battlePlayer, BattlePlayerBase battleEnemy, Func<SkillProcessor, VfxBase> addTurnEndCallCountOnPlayerTurn, Func<SkillProcessor, VfxBase> addTurnEndCallCountOnEnemyTurn)
	{
		Func<SkillProcessor, VfxBase> callStopOnTurnStartSkillAfter = null;
		callStopOnTurnStartSkillAfter = delegate(SkillProcessor _)
		{
			_callCount++;
			if ((_conditionFuncList.Count > 0 && _conditionFuncList.All((Func<BattlePlayerReadOnlyInfoPair, int, int, bool> s) => s(playerPair, _callCount, _turnEndCallCount))) || _conditionFuncList.Count == 0)
			{
				BattlePlayerBase battlePlayerBase2 = battlePlayer;
				battlePlayerBase2.OnTurnStartSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Remove(battlePlayerBase2.OnTurnStartSkillAfter, callStopOnTurnStartSkillAfter);
				battlePlayer.OnTurnEnd -= addTurnEndCallCountOnPlayerTurn;
				battleEnemy.OnTurnEnd -= addTurnEndCallCountOnEnemyTurn;
				return StopSkill(skill, _);
			}
			return NullVfx.GetInstance();
		};
		BattlePlayerBase battlePlayerBase = battlePlayer;
		battlePlayerBase.OnTurnStartSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Combine(battlePlayerBase.OnTurnStartSkillAfter, callStopOnTurnStartSkillAfter);
		return NullVfx.GetInstance();
	}
}

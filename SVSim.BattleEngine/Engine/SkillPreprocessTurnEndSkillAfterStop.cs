using System;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnEndSkillAfterStop : SkillPreprocessSkillAfterStopBase
{
	public SkillPreprocessTurnEndSkillAfterStop(string target)
		: base(target)
	{
	}

	protected override VfxBase OnStart(BattlePlayerPair playerPair, SkillBase skill, BattlePlayerBase battlePlayer, BattlePlayerBase battleEnemy, Func<SkillProcessor, VfxBase> addTurnEndCallCountOnPlayerTurn, Func<SkillProcessor, VfxBase> addTurnEndCallCountOnEnemyTurn)
	{
		Func<SkillProcessor, VfxBase> callStopOnTurnEndSkillAfter = null;
		callStopOnTurnEndSkillAfter = delegate(SkillProcessor _)
		{
			if (!battleEnemy.IsSelfTurn)
			{
				return NullVfx.GetInstance();
			}
			_callCount++;
			if ((_conditionFuncList.Count > 0 && _conditionFuncList.All((Func<BattlePlayerReadOnlyInfoPair, int, int, bool> s) => s(playerPair, _callCount, _turnEndCallCount))) || _conditionFuncList.Count == 0)
			{
				BattlePlayerBase battlePlayerBase2 = battlePlayer;
				battlePlayerBase2.OnTurnEndSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Remove(battlePlayerBase2.OnTurnEndSkillAfter, callStopOnTurnEndSkillAfter);
				battlePlayer.OnTurnEnd -= addTurnEndCallCountOnPlayerTurn;
				battleEnemy.OnTurnEnd -= addTurnEndCallCountOnEnemyTurn;
				return StopSkill(skill, _);
			}
			return NullVfx.GetInstance();
		};
		BattlePlayerBase battlePlayerBase = battleEnemy;
		battlePlayerBase.OnTurnEndSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Combine(battlePlayerBase.OnTurnEndSkillAfter, callStopOnTurnEndSkillAfter);
		return NullVfx.GetInstance();
	}
}

using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessUnitAttackStop : SkillPreprocessBase
{

	private readonly string _target = "";

	public SkillPreprocessUnitAttackStop(string target)
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
		Func<BattleCardBase, BattleCardBase, SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(BattleCardBase attackCard, BattleCardBase target, SkillProcessor skillProcessorOneTime)
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			if (attackCard.SelfBattlePlayer == battlePlayer)
			{
				sequentialVfxPlayer.Register(StopSkill(skill, skillProcessorOneTime));
				battlePlayer.BattleMgr.OperateMgr.OnBeforeAttack -= callStopOneTime;
			}
			return sequentialVfxPlayer;
		};
		battlePlayer.BattleMgr.OperateMgr.OnBeforeAttack += callStopOneTime;
		skill.SkillPrm.ownerCard.OnRemoveFromInPlayAfterOneTime += delegate
		{
			battlePlayer.BattleMgr.OperateMgr.OnBeforeAttack -= callStopOneTime;
			return NullVfx.GetInstance();
		};
		return NullVfx.GetInstance();
	}
}

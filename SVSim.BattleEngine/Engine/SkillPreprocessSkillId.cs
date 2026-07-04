using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessSkillId : SkillPreprocessBase
{
	private long _skillId;

	public SkillPreprocessSkillId(long skillId)
	{
		_skillId = skillId;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		BattleCardBase.SkillActivationInfo item = new BattleCardBase.SkillActivationInfo(_skillId, skill);
		skill.SkillPrm.selfBattlePlayer.Class.SkillActivationList.Add(item);
		return NullVfx.GetInstance();
	}
}

using System;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessDamageAfterStop : SkillPreprocessBase
{
	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		SetUp(skill);
		return NullVfx.GetInstance();
	}

	public override void Clone(SkillPreprocessBase source, SkillBase skill)
	{
		SetUp(skill);
	}

	private void SetUp(SkillBase skill)
	{
		bool isTargetClass = skill.ApplyCardFilterList.Any((ISkillCardFilter f) => f is SkillClassFilter);
		Func<SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(SkillProcessor skillProcessorOneTime)
		{
			if (isTargetClass)
			{
				skill.SkillPrm.ownerCard.SelfBattlePlayer.Class.OnDamageAfter -= callStopOneTime;
			}
			else
			{
				skill.SkillPrm.ownerCard.OnDamageAfter -= callStopOneTime;
			}
			return StopSkill(skill, skillProcessorOneTime);
		};
		if (isTargetClass)
		{
			skill.SkillPrm.ownerCard.SelfBattlePlayer.Class.OnDamageAfter += callStopOneTime;
		}
		else
		{
			skill.SkillPrm.ownerCard.OnDamageAfter += callStopOneTime;
		}
	}
}

using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessEvolutionEndStop : SkillPreprocessBase
{
	private BattleCardBase _ownerCard;

	public SkillPreprocessEvolutionEndStop(BattleCardBase card)
	{
		_ownerCard = card;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return !_ownerCard.IsEvolution;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		Func<SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(SkillProcessor skillProcessorOneTime)
		{
			skill.SkillPrm.ownerCard.OnBeforeEvolve -= callStopOneTime;
			if (skill is Skill_attach_skill skill_attach_skill)
			{
				skill_attach_skill.IsEvolutionEndStop = true;
			}
			return StopSkill(skill, skillProcessorOneTime);
		};
		skill.SkillPrm.ownerCard.OnBeforeEvolve += callStopOneTime;
		return NullVfx.GetInstance();
	}
}

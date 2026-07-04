using Wizard.Battle.View.Vfx;

public class Skill_extra_turn : SkillBase
{
	public Skill_extra_turn(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast)
		{
			return NullVfxWithLoading.GetInstance();
		}
		int addTurn = GetAddTurn();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			VfxBase vfx = targetCard.SkillApplyInformation.GiveExtraTurn(addTurn);
			parallelVfxPlayer.Register(vfx);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public int GetAddTurn()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add, 0);
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		return base.Stop(skillProcessor);
	}
}

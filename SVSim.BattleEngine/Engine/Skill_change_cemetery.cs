using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_change_cemetery : SkillBase
{
	public override bool IsTargetIndicate => false;

	protected int AddCemeteryCount { get; private set; }

	protected int GainCemeteryCount { get; private set; }

	public Skill_change_cemetery(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		AddCemeteryCount = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_count, 0);
		GainCemeteryCount = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_count, 0);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		for (int i = 0; i < AddCemeteryCount; i++)
		{
			VfxBase vfx = base.SkillPrm.selfBattlePlayer.DummyCardToCemetery(CardCreatorBase.GetDummyInstance(), this);
			parallelVfxPlayer.Register(vfx);
		}
		if (GainCemeteryCount > 0)
		{
			parallelVfxPlayer.Register(base.SkillPrm.selfBattlePlayer.GainCemetery(GainCemeteryCount));
		}
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{
		}));
		if (IsBattleLog)
		{
			if (AddCemeteryCount != 0)
			{
				BattleLogManager.GetInstance().AddLogSkillChangeCemetery(AddCemeteryCount, this);
			}
			else if (GainCemeteryCount != 0)
			{
				BattleLogManager.GetInstance().AddLogSkillChangeCemetery(-GainCemeteryCount, this);
			}
		}
		return VfxWithLoading.Create(parallelVfxPlayer);
	}
}

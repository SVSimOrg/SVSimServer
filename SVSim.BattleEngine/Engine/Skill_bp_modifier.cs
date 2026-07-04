using System.Linq;
using Wizard.Battle.View.Vfx;

public class Skill_bp_modifier : SkillBase
{
	public static readonly int BP_NONE = -1;

	public Skill_bp_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_bp, 0);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_bp, BP_NONE);
		_ = IsBattleLog;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			BattlePlayerBase targetClass = parameter.targetCards.ElementAt(i).SelfBattlePlayer;
			if (num != 0)
			{
				parallelVfxPlayer.Register(VfxWithLoadingSequential.Create());
				parallelVfxPlayer.Register(AddBp(targetClass, num));
				if (targetClass is BattlePlayer)
				{
					parallelVfxPlayer.Register(InstantVfx.Create(delegate
					{

					}));
				}
			}
			else if (num2 > BP_NONE)
			{
				parallelVfxPlayer.Register(VfxWithLoadingSequential.Create());
				parallelVfxPlayer.Register(AddBp(targetClass, -num2));
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	protected virtual VfxBase AddBp(BattlePlayerBase target, int value)
	{
		return target.AddBp(value);
	}
}

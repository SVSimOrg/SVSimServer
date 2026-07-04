using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_stack_white_ritual : SkillBase
{

	protected int _addCount;

	public int InitialWhiteRitualStack { get; private set; }

	public Skill_stack_white_ritual(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		InitialWhiteRitualStack = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.white_ritual_stack, 1);
	}

	protected List<BattleCardBase> GetStackTarget()
	{
		return base.SkillPrm.selfBattlePlayer.InPlayCards.Where((BattleCardBase c) => c != base.SkillPrm.ownerCard && c.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL) && (c.IsField || c.IsChantField)).ToList();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		_addCount = 0;
		List<BattleCardBase> stackTarget = GetStackTarget();
		for (int i = 0; i < stackTarget.Count(); i++)
		{
			BattleCardBase target = stackTarget[i];
			parallelVfxPlayer.Register(target.SelfBattlePlayer.CardManagement(target, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.BANISH, base.UsedRandom));
			if (base.SkillPrm.selfBattlePlayer.BanishList.Any((BattleCardBase c) => c == target))
			{
				_addCount += target.SkillApplyInformation.WhiteRitualCount;
				list.Add(target);
			}
		}
		base.SkillPrm.ownerCard.SkillApplyInformation.GiveWhiteRitualCount(_addCount);
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		vfxWithLoadingSequential.RegisterToMainVfx(NullVfx.GetInstance());
		if (base.SkillPrm.ownerCard.HasStackWhiteRitualAndOtherIconSkill())
		{
			vfxWithLoadingSequential.RegisterToMainVfx(base.SkillPrm.ownerCard.BattleCardView.InitializeBattleCardIcon(base.SkillPrm.ownerCard, base.SkillPrm.ownerCard.Skills, _addCount != 0));
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillDeath(list, this);
			BattleLogManager.GetInstance().AddLogGiveWhiteRitualStack(_addCount, base.SkillPrm.ownerCard, this);
		}
		return vfxWithLoadingSequential;
	}
}

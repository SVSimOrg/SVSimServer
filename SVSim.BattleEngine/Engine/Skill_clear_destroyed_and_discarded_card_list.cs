using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_clear_destroyed_and_discarded_card_list : SkillBase
{
	public Skill_clear_destroyed_and_discarded_card_list(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattlePlayerBase selfBattlePlayer = targetCard.SelfBattlePlayer;
			parallelVfxPlayer.Register(selfBattlePlayer.ClearDestroyedAndDiscardedCardList(this));
			if (IsBattleLog)
			{
				BattleLogManager.GetInstance().AddLogSkillClearDestroyedCardList(this, selfBattlePlayer);
				BattleLogManager.GetInstance().ClearDestroyedCardList(selfBattlePlayer.IsPlayer);
			}
		}
		return VfxWithLoading.Create(parallelVfxPlayer);
	}
}

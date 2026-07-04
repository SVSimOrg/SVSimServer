using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_clear_summoned_card_list : SkillBase
{
	public Skill_clear_summoned_card_list(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int id = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.card_id, -1);
		ParallelVfxPlayer mainVfx = ParallelVfxPlayer.Create();
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			BattlePlayerBase selfBattlePlayer = parameter.targetCards.ElementAt(i).SelfBattlePlayer;
			selfBattlePlayer.GameSummonCards.RemoveAll((BattlePlayerBase.TurnAndCard c) => c.Card.BaseParameter.BaseCardId == id);
			if (IsBattleLog)
			{
				BattleLogManager.GetInstance().AddLogSkillClearSummonedCardList(this, selfBattlePlayer);
			}
		}
		return VfxWithLoading.Create(mainVfx);
	}
}

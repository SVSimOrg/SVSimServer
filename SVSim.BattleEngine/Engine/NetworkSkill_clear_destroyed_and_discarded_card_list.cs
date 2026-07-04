using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_clear_destroyed_and_discarded_card_list : Skill_clear_destroyed_and_discarded_card_list
{
	public NetworkSkill_clear_destroyed_and_discarded_card_list(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnClearDestroyedCardList(parameter.targetCards.ElementAt(i).SelfBattlePlayer.IsPlayer);
		}
		return result;
	}
}

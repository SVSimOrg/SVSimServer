using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_stack_white_ritual : Skill_stack_white_ritual
{
	public NetworkSkill_stack_white_ritual(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (base.SkillPrm.selfBattlePlayer.InPlayCards.Any((BattleCardBase c) => c != base.SkillPrm.ownerCard && c.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL)))
		{
			base.SkillPrm.selfBattlePlayer.CallOnSkillDestroyOrBanish(base.SkillPrm.ownerCard);
		}
		VfxWithLoading result = base.Start(parameter);
		(base.SkillPrm.selfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterInplayWhiteRitualStack(base.SkillPrm.selfBattlePlayer);
		base.SkillPrm.selfBattlePlayer.CallOnChangeWhiteRitualStack(base.SkillPrm.ownerCard, _addCount);
		return result;
	}
}

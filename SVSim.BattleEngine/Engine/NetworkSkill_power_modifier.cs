using System.Collections.Generic;
using System.Linq;

public class NetworkSkill_power_modifier : Skill_power_modifier
{
	public NetworkSkill_power_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override void IncrementGameBuffCount(List<BattleCardBase> inplayTargetCards)
	{
		base.IncrementGameBuffCount(inplayTargetCards);
		if (inplayTargetCards.Any())
		{
			(base.SkillPrm.selfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterActionManager.Add(new RegisterPlayerParameter(RegisterActionBase.ActionBaseParameter.buffUnit, 1, base.SkillPrm.selfBattlePlayer.IsPlayer));
		}
	}

	protected override void CallPowerUpEvent(List<BattleCardBase> targetCards)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnPowerUp(base.SkillPrm.ownerCard, targetCards, (_addOffense > 0) ? _addOffense : (-_gainOffense), (_addLife > 0) ? _addLife : (-_gainLife), _multiplyOffense, _multiplyLife, _addMaxLife);
	}
}

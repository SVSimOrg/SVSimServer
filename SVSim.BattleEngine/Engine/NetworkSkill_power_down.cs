using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_power_down : Skill_power_down
{
	public NetworkSkill_power_down(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnPowerDownStart();
		VfxWithLoading result = base.Start(parameter);
		if (_gainOffense > 0 || _gainLife > 0 || _gainMaxLife > 0)
		{
			base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnPowerDown(base.SkillPrm.ownerCard, parameter.targetCards.ToList(), _gainOffense * -1, _gainLife * -1, _gainMaxLife * -1, isSet: false);
			return result;
		}
		if (_setOffense != Skill_power_down.SETPRM_NONE || _setLife != Skill_power_down.SETPRM_NONE || _setMaxLife != Skill_power_down.SETPRM_NONE)
		{
			base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnPowerDown(base.SkillPrm.ownerCard, parameter.targetCards.ToList(), _setOffense, _setLife, _setMaxLife, isSet: true);
			return result;
		}
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnRemoveLatestOperationJsonData(NetworkBattleReceiver.ReplayOperationType.GainPowerDown);
		return result;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnDeprivePowerDown(buffInfoContainer);
		return base.Stop(skillProcessor);
	}
}

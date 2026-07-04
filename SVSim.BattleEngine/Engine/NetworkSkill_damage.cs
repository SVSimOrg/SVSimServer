using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_damage : Skill_damage
{
	private List<BattleCardBase> TargetList;

	private List<BattleCardBase.DamageResult> DamageList;

	public NetworkSkill_damage(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		TargetList = new List<BattleCardBase>();
		DamageList = new List<BattleCardBase.DamageResult>();
		List<BattleCardBase> effectTargets = callParameter.targetCards.Where((BattleCardBase t) => !t.IsDead).ToList();
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnSkillDamageStart(base.SkillPrm.ownerCard);
		VfxWithLoading result = base.Start(callParameter);
		base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "_OPT_NULL_");
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnDamage(TargetList, effectTargets, DamageList);
		return result;
	}

	public override void RegisterDamageTriggerSkill(SkillProcessor skillProcessor, IEnumerable<BattleCardBase> target, int defDamage, BattleCardBase.DamageResult damageResult)
	{
		TargetList.Add(target.FirstOrDefault());
		DamageList.Add(damageResult);
		base.RegisterDamageTriggerSkill(skillProcessor, target, defDamage, damageResult);
	}
}

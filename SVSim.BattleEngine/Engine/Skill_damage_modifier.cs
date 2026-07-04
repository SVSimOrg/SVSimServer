using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_damage_modifier : SkillBase
{
	private DamageModifier _info;

	public Skill_damage_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_damage, 0);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_damage, -1);
		string damageType = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.card_type, "_OPT_NULL_");
		bool isUseClass = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.owner, "all") == "all";
		CardBasePrm.ClanType clanType = CardBasePrm.GetClanType(base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.clan));
		List<BattleCardBase> list = parameter.targetCards.ToList();
		if (num != 0)
		{
			_info = new AddDamageInfo(num, damageType, clanType, isUseClass, base.SkillPrm.selfBattlePlayer.BattleMgr.AllPublishedDamageModifierCount++);
		}
		else if (num2 != -1)
		{
			_info = new SetDamageInfo(num2, damageType, clanType, isUseClass, base.SkillPrm.selfBattlePlayer.BattleMgr.AllPublishedDamageModifierCount++);
		}
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			list[i].SkillApplyInformation.GiveAddDamage(_info);
			BuffInfo buffInfo = ((num != 0 || num2 != -1) ? AddBuffInfoIfNeeded(list[i]) : null);
			buffInfoContainer.Add(new BuffInfoContainer(list[i], buffInfo, -1, "", null, 0L));
			base.IsActivity = true;
			SetOnLoseEvent(list[i], null, null);
		}
		VfxWithLoading result = CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(list, this, SkillGainType.AddDamage, num);
		}
		return result;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			BattleCardBase targetCard = buffInfoContainer[i]._targetCard;
			targetCard.RemoveBuffInfo(buffInfoContainer[i]._buffInfo);
			targetCard.SkillApplyInformation.DepriveAddDamage(_info);
		}
		buffInfoContainer.Clear();
		return base.Stop(skillProcessor);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += (SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card) => card.IsDead ? ((VfxBase)NullVfx.GetInstance()) : ((VfxBase)Stop(skillProcessor));
	}
}

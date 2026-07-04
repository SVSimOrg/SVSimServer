using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_heal_modifier : SkillBase
{
	private HealModifier _healModifier;

	public Skill_heal_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_healing, 0);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_healing, -1);
		bool isTargetSelfClass = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, string.Empty) == "be_healed";
		List<BattleCardBase> list = parameter.targetCards.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (num != 0)
			{
				_healModifier = new AddHealModifierInfo(num, base.SkillPrm.selfBattlePlayer.BattleMgr.AllPublishedHealModifierCount++, list[i]);
			}
			else if (num2 != -1)
			{
				_healModifier = new SetHealModifierInfo(num2, base.SkillPrm.selfBattlePlayer.BattleMgr.AllPublishedHealModifierCount++, list[i], isTargetSelfClass);
			}
			list[i].SkillApplyInformation.GiveHealModifier(_healModifier);
			BuffInfo buffInfo = ((num != 0 || num2 != -1) ? AddBuffInfoIfNeeded(list[i]) : null);
			buffInfoContainer.Add(new BuffInfoContainer(list[i], buffInfo, -1, "", null, 0L));
			base.IsActivity = true;
			SetOnLoseEvent(list[i], null, null);
		}
		VfxWithLoading result = CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(list, this, SkillGainType.HealModifier);
		}
		return result;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			BattleCardBase targetCard = buffInfoContainer[i]._targetCard;
			targetCard.RemoveBuffInfo(buffInfoContainer[i]._buffInfo);
			targetCard.SkillApplyInformation.DepriveHealModifier(_healModifier);
		}
		buffInfoContainer.Clear();
		return base.Stop(skillProcessor);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += (SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card) => card.IsDead ? ((VfxBase)NullVfx.GetInstance()) : ((VfxBase)Stop(skillProcessor));
	}
}

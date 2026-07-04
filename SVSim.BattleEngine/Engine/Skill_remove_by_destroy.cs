using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_remove_by_destroy : SkillBase
{
	public Skill_remove_by_destroy(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		List<BattleCardBase> list = parameter.targetCards.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			BattleCardBase battleCardBase = list[i];
			battleCardBase.SkillApplyInformation.GiveRemoveByDestroy();
			BuffInfo buffInfo = AddBuffInfoIfNeeded(battleCardBase);
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.RemoveByDestroy);
		}
		return CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			BuffInfo buffInfo = buffInfoContainer[i]._buffInfo;
			BattleCardBase targetCard = buffInfoContainer[i]._targetCard;
			targetCard.SkillApplyInformation.DepriveRemoveByDestroy();
			targetCard.RemoveBuffInfo(buffInfo);
		}
		buffInfoContainer.Clear();
		return NullVfxWithLoading.GetInstance();
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.SkillApplyInformation.ForceDepriveRemoveByDestroy();
			buffInfoContainer.Remove(container);
			card.RemoveBuffInfo(buff);
			return NullVfx.GetInstance();
		};
	}
}

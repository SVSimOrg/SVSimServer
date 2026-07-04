using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_not_be_debuffed : SkillBase
{
	public Skill_not_be_debuffed(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			BattleCardBase battleCardBase = parameter.targetCards.ElementAt(i);
			battleCardBase.SkillApplyInformation.GiveNotBeDebuffed();
			BattleCardBase battleCardBase2 = battleCardBase;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(battleCardBase);
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase2, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase2, buffInfo, buffInfoContainer);
		}
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.NotBeDebuffed);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < base.buffInfoContainer.Count; i++)
		{
			BuffInfoContainer buffInfoContainer = base.buffInfoContainer[i];
			buffInfoContainer._targetCard.SkillApplyInformation.DepriveNotBeDebuffed();
			list.Add(buffInfoContainer._targetCard);
			buffInfoContainer._targetCard.RemoveBuffInfo(buffInfoContainer._buffInfo);
		}
		CallOnUpdateSkillEffect(list);
		base.buffInfoContainer.Clear();
		return VfxWithLoading.Create(NullVfx.GetInstance());
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			card.SkillApplyInformation.ForceDepriveNotBeDebuffed();
			return NullVfx.GetInstance();
		};
	}
}

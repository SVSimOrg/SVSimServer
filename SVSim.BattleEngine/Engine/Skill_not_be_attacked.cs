using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_not_be_attacked : SkillBase
{
	private class NotBeAttackedBuffInfoContainer : BuffInfoContainer
	{
		public BattleCardBase Card { get; private set; }

		public NotBeAttackedInfo SkillInfo { get; private set; }

		public NotBeAttackedBuffInfoContainer(BattleCardBase card, BuffInfo info, NotBeAttackedInfo skillInfo)
			: base(card, info, -1, "", null, 0L)
		{
			Card = card;
			SkillInfo = skillInfo;
		}
	}

	public Skill_not_be_attacked(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		string info = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			NotBeAttackedInfo notBeAttackedInfo = new NotBeAttackedInfo(info, targetCard, this);
			VfxBase vfx = targetCard.SkillApplyInformation.GiveNotBeAttacked(notBeAttackedInfo);
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			NotBeAttackedBuffInfoContainer notBeAttackedBuffInfoContainer = new NotBeAttackedBuffInfoContainer(battleCardBase, buffInfo, notBeAttackedInfo);
			buffInfoContainer.Add(notBeAttackedBuffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, notBeAttackedBuffInfoContainer);
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.NotBeAttacked);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (NotBeAttackedBuffInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item.Card.SkillApplyInformation.DepriveNotBeAttacked(item.SkillInfo);
			list.Add(item._targetCard);
			item.Card.RemoveBuffInfo(item._buffInfo);
			parallelVfxPlayer.Register(vfx);
		}
		CallOnUpdateSkillEffect(list);
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.FourceDepriveNotBeAttacked();
		};
	}
}

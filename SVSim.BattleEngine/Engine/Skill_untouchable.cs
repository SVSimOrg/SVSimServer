using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_untouchable : SkillBase
{

	private string _cardType;

	public Skill_untouchable(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		_cardType = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "all");
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			VfxBase vfx = targetCard.SkillApplyInformation.GiveUntouchable(_cardType);
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.Untouchable);
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
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveUntouchable(_cardType);
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
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
			return card.SkillApplyInformation.FourceDepriveUntouchable(_cardType);
		};
	}
}

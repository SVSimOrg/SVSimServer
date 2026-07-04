using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class Skill_trigger : SkillBase
{
	private readonly List<BattleCardBase> _targetList = new List<BattleCardBase>();

	public override bool IsTargetIndicate => false;

	public override bool IsInductionSkill => false;

	public Skill_trigger(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			targetCard.SkillApplyInformation.GiveTriggerCount(parameter.skillProcessor);
			_targetList.Add(targetCard);
			SetOnLoseEvent(targetCard, null, null);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(base.SkillPrm.ownerCard.BattleCardView.InitializeBattleCardIcon(base.SkillPrm.ownerCard, base.SkillPrm.ownerCard.Skills));
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		foreach (BattleCardBase target in _targetList)
		{
			target.SkillApplyInformation.DepriveTriggerCount();
		}
		_targetList.Clear();
		return VfxWithLoading.Create(base.SkillPrm.ownerCard.BattleCardView.InitializeBattleCardIcon(base.SkillPrm.ownerCard, base.SkillPrm.ownerCard.Skills));
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.SkillApplyInformation.ForceDepriveTriggerCount();
			_targetList.Remove(card);
			return (base.SkillPrm.ownerCard.BaseParameter.BaseCardId == 132141010) ? NullVfx.GetInstance() : base.SkillPrm.ownerCard.BattleCardView.InitializeBattleCardIcon(base.SkillPrm.ownerCard, base.SkillPrm.ownerCard.Skills);
		};
	}
}

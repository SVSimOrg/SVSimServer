using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_attack_by_life : SkillBase
{

	public string type => base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "_OPT_NULL_");

	public Skill_attack_by_life(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			VfxBase vfx = targetCard.SkillApplyInformation.GiveAttackByLife(type);
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			parallelVfxPlayer.Register(vfx);
			base.IsActivity = true;
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.AttackByLife);
		}
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "_OPT_NULL_");
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveAttackByLife(text);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			parallelVfxPlayer.Register(vfx);
		}
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "_OPT_NULL_");
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.FourceDepriveAttackByLife(text);
		};
	}
}

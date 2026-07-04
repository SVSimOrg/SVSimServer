using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_quick : SkillBase
{
	public Skill_quick(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			VfxBase vfxToRegister = targetCard.SkillApplyInformation.GiveQuick();
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			List<BattleCardBase> list = new List<BattleCardBase>();
			list.Add(targetCard);
			BattlePlayerPair playerInfoPair = new BattlePlayerPair(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer);
			parameter.skillProcessor.Register(battleCardBase.Skills.CreateWhenAttachAbilityInfo(parameter.skillProcessor, playerInfoPair, this, BattlePlayerBase.ConvertToSkillInfoCollection(list)));
			vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		}
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.Quick);
		}
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			parallelVfxPlayer.Register(item._targetCard.SkillApplyInformation.DepriveQuick());
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
		}
		CallOnUpdateSkillEffect(list, updateAttackEffect: true);
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.ForceDepriveQuick();
		};
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_shortage_deck_win : SkillBase
{

	public Skill_shortage_deck_win(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BattleCardBase target in parameter.targetCards)
		{
			if (target.SkillApplyInformation.IsCantActivateShortageDeckWin)
			{
				vfxWithLoadingSequential.RegisterVfxWithLoading(SkillBase.CreateAreaVfx(base.SkillPrm.resourceMgr, () => target.BattleCardView.GameObject.transform.position, base.SkillPrm.selfBattlePlayer.GetFieldCenterPosition(), base.SkillPrm.ownerCard.IsPlayer, base.SkillPrm.ownerCard.BattleCardView, "btl_nerva_4", EffectMgr.EngineType.SHURIKEN, "se_btl_nerva_4", EffectMgr.MoveType.DIRECT_DECK, EffectMgr.TargetType.AREA_SELF, 0f));
				continue;
			}
			list.Add(target);
			target.SkillApplyInformation.GiveShortageDeckWin();
			BuffInfo buffInfo = AddBuffInfoIfNeeded(target);
			BattleCardBase battleCardBase = target;
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
		}
		base.SkillPrm.selfBattlePlayer.StartSkillWhenShortageDeckWinSkillActivate(parameter.targetCards.ToList(), parameter.skillProcessor);
		if (list.Count <= 0)
		{
			return vfxWithLoadingSequential;
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillShortageDeckWin(list.ToList(), this);
		}
		vfxWithLoadingSequential.RegisterToMainVfx(CreateSingleVfx(SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.BattleResourceMgr, () => Vector3.zero, list));
		vfxWithLoadingSequential.RegisterToMainVfx(NullVfx.GetInstance());
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnEffect(base.SkillPrm.buildInfo, isFollowInHand: false, isTargetPosition: true);
		return vfxWithLoadingSequential;
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.ForceDepriveShortageDeckWin();
		};
	}
}

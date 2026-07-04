using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_banish : SkillBase
{
	public override bool IsTargetIndicate => false;

	public Skill_banish(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		bool flag = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_open, SkillFilterCreator.ContentKeyword._false.ToStringCustom()) == SkillFilterCreator.ContentKeyword._true.ToStringCustom() || (OnWhenDraw != 0 && base.ApplyingTargetFilter is SkillTargetSelfFilter);
		List<BattleCardBase> list = parameter.targetCards.ToList();
		List<BattleCardBase> list2 = ((!base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast) ? list.Where((BattleCardBase c) => c.IsInplay && (!c.SkillApplyInformation.IsBanishByDestroy || !c.SkillApplyInformation.IsIndestructible)).ToList() : null);
		List<BattleCardBase> list3 = ((!base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast) ? list.Where((BattleCardBase c) => c.IsInHand).ToList() : null);
		List<BattleCardBase> list4 = ((!base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast) ? list.Where((BattleCardBase c) => c.IsInDeck).ToList() : null);
		int count = base.SkillPrm.selfBattlePlayer.DeckCardList.Count;
		int count2 = base.SkillPrm.opponentBattlePlayer.DeckCardList.Count;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase item in list)
		{
			item.FlagCardAsDestroyedBySkill();
			parallelVfxPlayer.Register(item.SelfBattlePlayer.CardManagement(item, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.BANISH, base.UsedRandom, null, null, this, null, flag));
		}
		if (IsBattleLog)
		{
			if (list2 != null)
			{
				BattleLogManager.GetInstance().AddLogSkillDeath(list2, this);
			}
			if (list3 != null)
			{
				BattleLogManager.GetInstance().AddLogSkillBanishHand(list3.Where((BattleCardBase c) => c.IsPlayer).ToList(), isPlayer: true, this);
				BattleLogManager.GetInstance().AddLogSkillBanishHand(list3.Where((BattleCardBase c) => !c.IsPlayer).ToList(), isPlayer: false, this);
			}
			if (list4 != null)
			{
				if (flag)
				{
					foreach (BattleCardBase card in list4)
					{
						if (card.BattleCardView != null)
						{
							List<int> costList = card.BattleCardView.GetUseCostList(card.Cost);
							parallelVfxPlayer.Register(InstantVfx.Create(delegate
							{
								card.BattleCardView.UpdateCost(costList, isGenerateInHand: false, playEffect: false, isForceUpdate: true);
								card.BattleCardView.UpdateOffence(card.Atk);
								card.BattleCardView.UpdateLife(card.Life);
							}));
						}
						parallelVfxPlayer.Register(NullVfx.GetInstance());
					}
				}
				BattleLogManager.GetInstance().AddLogSkillBanishDeck(list4.Where((BattleCardBase c) => c.IsPlayer).ToList(), this, flag);
				BattleLogManager.GetInstance().AddLogSkillBanishDeck(list4.Where((BattleCardBase c) => !c.IsPlayer).ToList(), this, flag);
				if (list4.Any((BattleCardBase c) => c.SelfBattlePlayer == base.SkillPrm.selfBattlePlayer))
				{
					base.SkillPrm.selfBattlePlayer.CallOnChangeDeckAfterEvent(count, parameter.skillProcessor, new List<BattleCardBase>());
				}
				if (list4.Any((BattleCardBase c) => c.SelfBattlePlayer == base.SkillPrm.opponentBattlePlayer))
				{
					base.SkillPrm.opponentBattlePlayer.CallOnChangeDeckAfterEvent(count2, parameter.skillProcessor, new List<BattleCardBase>());
				}
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, list, isFollowInHand: false, addToLastOperation: true));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}
}

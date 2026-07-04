using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_return_card : SkillBase
{
	public Skill_return_card(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		List<BattleCardBase> list = parameter.targetCards.Where((BattleCardBase c) => c.IsPlayer == base.SkillPrm.ownerCard.IsPlayer).ToList();
		List<BattleCardBase> list2 = parameter.targetCards.Where((BattleCardBase c) => c.IsPlayer != base.SkillPrm.ownerCard.IsPlayer).ToList();
		base.SkillPrm.ownerCard.SelfBattlePlayer.ReturnList = list;
		base.SkillPrm.ownerCard.OpponentBattlePlayer.ReturnList = list2;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> returnCards = new List<BattleCardBase>(list.Count + list2.Count);
		returnCards.AddRange(list);
		returnCards.AddRange(list2);
		for (int num = 0; num < returnCards.Count(); num++)
		{
			parallelVfxPlayer.Register(returnCards[num].SelfBattlePlayer.CardManagement(returnCards[num], parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.RETURN, base.UsedRandom, null, null, this));
		}
		RegisterReturnOtherTriggerSkill(parameter.skillProcessor, returnCards);
		increaseSkillReturnCardCount();
		List<BattleCardBase> source = (base.SkillPrm.selfBattlePlayer.IsPlayer ? list : list2);
		List<BattleCardBase> source2 = (base.SkillPrm.selfBattlePlayer.IsPlayer ? list2 : list);
		List<BattleCardBase> playerCardsToReturn = source.Where((BattleCardBase c) => !c.IsDestroyedBySkill).ToList();
		List<BattleCardBase> enemyCardsToReturn = source2.Where((BattleCardBase c) => !c.IsDestroyedBySkill).ToList();
		SequentialVfxPlayer returnCardVfx = SequentialVfxPlayer.Create();
		if (IsBattleLog)
		{
			BattleLogManager instance = BattleLogManager.GetInstance();
			List<BattleCardBase> list3 = parameter.targetCards.Where((BattleCardBase s) => s.IsInHand).ToList();
			instance.AddLogSkillReturnCard(list3, this);
			IEnumerable<BattleCardBase> source3 = parameter.targetCards.Where((BattleCardBase s) => !s.IsInHand);
			instance.AddLogSkillDeath(source3.Where((BattleCardBase s) => s.DeathTypeInfo.BanishDestroy || s.IsDead).ToList(), this);
			instance.AddLogSkillDrawToken(source3.Where((BattleCardBase s) => !s.DeathTypeInfo.BanishDestroy && !s.IsDead).ToList(), this, isOpen: true, isOverDraw: true);
			for (int num2 = 0; num2 < list3.Count(); num2++)
			{
				instance.UpdateFusionedCardSkillDrewCard(list3.ElementAt(num2));
			}
		}
		SkillProcessor skillProcessor = parameter.skillProcessor;
		skillProcessor.OnSkillProcedureEnd = (Func<VfxBase>)Delegate.Combine(skillProcessor.OnSkillProcedureEnd, (Func<VfxBase>)delegate
		{
			ClearGetOnCards(returnCards);
			return NullVfx.GetInstance();
		});
		return VfxWithLoading.Create(base.SkillPrm.selfBattlePlayer.BattleMgr.LoadCardResources(returnCards), SequentialVfxPlayer.Create(parallelVfxPlayer, returnCardVfx));
	}

	private void ClearGetOnCards(List<BattleCardBase> cards)
	{
		foreach (BattleCardBase card in cards)
		{
			card.SkillApplyInformation.ClearGetOnCards();
		}
	}

	protected virtual void RegisterReturnOtherTriggerSkill(SkillProcessor skillProcessor, List<BattleCardBase> targets)
	{
		(base.SkillPrm.selfBattlePlayer.IsSelfTurn ? base.SkillPrm.selfBattlePlayer : base.SkillPrm.opponentBattlePlayer).StartSkillWhenReturnSkillActivate(targets, skillProcessor);
	}

	private void increaseSkillReturnCardCount()
	{
		BattlePlayerBase battlePlayer = base.SkillPrm.ownerCard.SelfBattlePlayer;
		TurnAndIntValue turnAndIntValue = battlePlayer.GameSkillReturnCardCountList.FirstOrDefault((TurnAndIntValue t) => t.IsSelfTurn == battlePlayer.IsSelfTurn && t.Turn == battlePlayer.Turn);
		if (turnAndIntValue != null)
		{
			turnAndIntValue.Increment();
		}
		else
		{
			battlePlayer.GameSkillReturnCardCountList.Add(new TurnAndIntValue(1, battlePlayer.Turn, battlePlayer.IsSelfTurn));
		}
	}
}

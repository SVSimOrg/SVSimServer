using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_powerup : Skill_powerup
{
	public NetworkSkill_powerup(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override void IncrementGameBuffCount(List<BattleCardBase> inplayTargetCards)
	{
		base.IncrementGameBuffCount(inplayTargetCards);
		if (inplayTargetCards.Any())
		{
			(base.SkillPrm.selfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterActionManager.Add(new RegisterPlayerParameter(RegisterActionBase.ActionBaseParameter.buffUnit, 1, base.SkillPrm.selfBattlePlayer.IsPlayer));
		}
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnDeprivePowerUp(_targetList);
		return base.Stop(skillProcessor);
	}

	protected override VfxBase GiveCombatModifier(BattleCardBase card, ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier, CallParameter parameter)
	{
		bool flag = !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsReplayBattle && (!base.SkillPrm.ownerCard.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsWatchBattle);
		if (card.IsInDeck && flag && (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase).networkBattleData.GetReceiveData().GetReceiveCardList().Any(delegate(CardDataModel c)
		{
			bool flag2 = c.ToStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Deck);
			return c.Index == card.Index && !flag2;
		}))
		{
			AddBuffInfoIfNeeded(card);
			card.SkillApplyInformation.GiveBuff();
			if (GetAddLife() > 0)
			{
				card.SkillApplyInformation.GiveBuffLife();
			}
			card.OnRemoveFromInPlayAfterOneTime += delegate
			{
				card.SkillApplyInformation.DepriveBuff();
				return NullVfx.GetInstance();
			};
			return NullVfx.GetInstance();
		}
		return base.GiveCombatModifier(card, offenseModifier, lifeModifier, parameter);
	}

	protected override void CallPowerUpEvent(List<BattleCardBase> targetCards)
	{
		List<BattleCardBase> cards = targetCards.Where((BattleCardBase c) => c.MetamorphoseCard == null).ToList();
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnPowerUp(base.SkillPrm.ownerCard, cards, _addOffense, _addLife, _multiplyOffense, _multiplyLife, _addMaxLife);
	}
}

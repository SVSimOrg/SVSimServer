using System.Collections.Generic;

namespace Wizard;

public static class TagCollectionWithTypeCreator
{
	public static TagCollectionWithTypeBase Create(AIPlayTagType type)
	{
		TagCollection tagCollection = null;
		List<AIPlayTagType> list = null;
		switch (type)
		{
		case AIPlayTagType.Priority:
			tagCollection = new PriorityTagCollection();
			break;
		case AIPlayTagType.PlayBonus:
			tagCollection = new PlayBonusTagCollection();
			break;
		case AIPlayTagType.PlayBonusRate:
			tagCollection = new PlayBonusRateTagCollection();
			break;
		case AIPlayTagType.AllyPlayBonus:
			tagCollection = new AllyPlayBonusTagCollection();
			break;
		case AIPlayTagType.EnemyPlayBonus:
			tagCollection = new EnemyPlayBonusTagCollection();
			break;
		case AIPlayTagType.FanfareBonus:
			tagCollection = new FanfareBonusTagCollection();
			break;
		case AIPlayTagType.IgnoreFanfareBonus:
			tagCollection = new IgnoreFanfareBonusTagCollection();
			break;
		case AIPlayTagType.HandPlus:
			tagCollection = new HandPlusTagCollection();
			break;
		case AIPlayTagType.PlayLimit:
			tagCollection = new PlayLimitTagCollection();
			break;
		case AIPlayTagType.PlayptnBonus:
			tagCollection = new PlayPtnBonusTagCollection();
			break;
		case AIPlayTagType.AttackBonus:
			tagCollection = new AttackBonusTagCollection();
			break;
		case AIPlayTagType.BattleBonusRate:
			tagCollection = new BattleBonusRateTagCollection();
			break;
		case AIPlayTagType.BattleBonus:
			tagCollection = new BattleBonusTagCollection();
			break;
		case AIPlayTagType.MemberBattleBonus:
			tagCollection = new MemberBattleBonusTagCollection();
			break;
		case AIPlayTagType.EnemyBattleBonus:
			tagCollection = new EnemyBattleBonusTagCollection();
			break;
		case AIPlayTagType.MemberBattleBonusRate:
			tagCollection = new MemberBattleBonusRateTagCollection();
			break;
		case AIPlayTagType.EnemyBattleBonusRate:
			tagCollection = new EnemyBattleBonusRateTagCollection();
			break;
		case AIPlayTagType.EvoBonus:
			tagCollection = new EvoBonusTagCollection();
			break;
		case AIPlayTagType.MemberEvoBonus:
			tagCollection = new MemberEvoBonusTagCollection();
			break;
		case AIPlayTagType.EnemyEvoBonus:
			tagCollection = new EnemyEvoBonusTagCollection();
			break;
		case AIPlayTagType.PuppetAttack:
			tagCollection = new PuppetAttackTagCollection();
			break;
		case AIPlayTagType.DamagedBonus:
		case AIPlayTagType.DamagedBuff:
		case AIPlayTagType.DamagedDamage:
		case AIPlayTagType.DamagedToken:
		case AIPlayTagType.DamagedHeal:
		case AIPlayTagType.DamagedCantUnderAttack:
			list = new List<AIPlayTagType> { type };
			tagCollection = new DamagedTagCollection();
			break;
		case AIPlayTagType.OtherDamagedDamage:
		case AIPlayTagType.OtherDamagedHeal:
		case AIPlayTagType.OtherDamagedSetLeaderMaxLife:
		case AIPlayTagType.OtherDamagedSubtractCountdown:
		case AIPlayTagType.OtherDamagedBanish:
			list = new List<AIPlayTagType> { type };
			tagCollection = new OtherDamagedTagCollection();
			break;
		case AIPlayTagType.PlayoutNextTurn:
			tagCollection = new PlayoutNextTurnTagCollection();
			break;
		case AIPlayTagType.NoSkipAttack:
			tagCollection = new NoSkipAttackTagCollection();
			break;
		case AIPlayTagType.ClashDestroy:
		case AIPlayTagType.ClashHeal:
		case AIPlayTagType.ClashToken:
		case AIPlayTagType.ClashBanish:
		case AIPlayTagType.ClashSpellboost:
		case AIPlayTagType.AttackBuff:
		case AIPlayTagType.AttackHandBuff:
		case AIPlayTagType.AttackDestroy:
		case AIPlayTagType.AttackBanish:
		case AIPlayTagType.AttackHeal:
		case AIPlayTagType.AttackDiscard:
		case AIPlayTagType.AttackAttackableCount:
		case AIPlayTagType.AttackQuick:
		case AIPlayTagType.AttackRemoveTag:
		case AIPlayTagType.AttackSetStatus:
		case AIPlayTagType.ClashBuff:
		case AIPlayTagType.AttackToken:
		case AIPlayTagType.AttackDamage:
		case AIPlayTagType.AttackEvo:
		case AIPlayTagType.ClashDamage:
		case AIPlayTagType.ClashKiller:
		case AIPlayTagType.AttackKiller:
		case AIPlayTagType.ClashRemoveSkill:
		case AIPlayTagType.ClashRemoveTag:
		case AIPlayTagType.AttackRemoveSkill:
		case AIPlayTagType.AttackAttachTag:
		case AIPlayTagType.AttackSubtractCountdown:
		case AIPlayTagType.AttackShield:
		case AIPlayTagType.ClashShield:
		case AIPlayTagType.AttackDamageClip:
		case AIPlayTagType.ClashDamageClip:
		case AIPlayTagType.AttackAddDeck:
			list = new List<AIPlayTagType> { type };
			tagCollection = new AttackTagCollection();
			break;
		case AIPlayTagType.BounceBonus:
			tagCollection = new BounceBonusTagCollection();
			break;
		case AIPlayTagType.BreakBuff:
		case AIPlayTagType.BreakSetLeaderMaxLife:
		case AIPlayTagType.BreakHeal:
		case AIPlayTagType.BreakDamage:
		case AIPlayTagType.BreakRecoverAttackableCount:
		case AIPlayTagType.BreakDestroy:
		case AIPlayTagType.BreakRecoverPp:
		case AIPlayTagType.BreakAttachTag:
		case AIPlayTagType.BreakAddStack:
			list = new List<AIPlayTagType> { type };
			tagCollection = new BreakTagCollection();
			break;
		case AIPlayTagType.BanishBonus:
			tagCollection = new BanishBonusTagCollection();
			break;
		case AIPlayTagType.AfterAttackEvo:
		case AIPlayTagType.AfterAttackDraw:
		case AIPlayTagType.AfterAttackBanish:
		case AIPlayTagType.AttackBreakDamage:
		case AIPlayTagType.AttackBreakEvo:
		case AIPlayTagType.AttackBreakRecoverPp:
		case AIPlayTagType.AttackBreakAttackTwice:
		case AIPlayTagType.AfterAttackHeal:
			list = new List<AIPlayTagType> { type };
			tagCollection = new AfterAttackTagCollection();
			break;
		case AIPlayTagType.OtherAttackBuff:
		case AIPlayTagType.OtherAttackDamage:
		case AIPlayTagType.OtherAttackHeal:
		case AIPlayTagType.OtherAttackToken:
		case AIPlayTagType.OtherAttackAttachTag:
		case AIPlayTagType.OtherAttackRemoveTag:
			list = new List<AIPlayTagType> { type };
			tagCollection = new OtherAttackTagCollection();
			break;
		case AIPlayTagType.SummonBuff:
		case AIPlayTagType.SummonEvo:
		case AIPlayTagType.SummonRush:
		case AIPlayTagType.SummonQuick:
		case AIPlayTagType.SummonDamage:
		case AIPlayTagType.SummonBanish:
		case AIPlayTagType.SummonHeal:
		case AIPlayTagType.SummonDestroy:
		case AIPlayTagType.SummonBanAttack:
		case AIPlayTagType.SummonAttachTag:
		case AIPlayTagType.Stack:
			list = new List<AIPlayTagType> { type };
			tagCollection = new SummonTagCollection();
			break;
		case AIPlayTagType.OtherSummonDamage:
		case AIPlayTagType.OtherSummonRush:
		case AIPlayTagType.OtherSummonGuard:
		case AIPlayTagType.OtherSummonQuick:
		case AIPlayTagType.OtherSummonKiller:
		case AIPlayTagType.OtherSummonDrain:
		case AIPlayTagType.OtherSummonBanish:
		case AIPlayTagType.OtherSummonHeal:
		case AIPlayTagType.OtherSummonBuff:
		case AIPlayTagType.OtherSummonEvo:
		case AIPlayTagType.OtherSummonDamageCut:
		case AIPlayTagType.OtherSummonDamageClip:
		case AIPlayTagType.OtherSummonSubtractCountdown:
		case AIPlayTagType.OtherSummonAddCemetery:
		case AIPlayTagType.OtherSummonUntouchable:
		case AIPlayTagType.OtherSummonDestory:
		case AIPlayTagType.OtherSummonDraw:
		case AIPlayTagType.OtherSummonAttachTag:
			list = new List<AIPlayTagType> { type };
			tagCollection = new OtherSummonTagCollection();
			break;
		case AIPlayTagType.Necromance:
		case AIPlayTagType.EarthRite:
		case AIPlayTagType.BurialRite:
			list = new List<AIPlayTagType> { type };
			tagCollection = new PreprocessTagCollection();
			break;
		case AIPlayTagType.LastwordToken:
		case AIPlayTagType.LastwordBuff:
		case AIPlayTagType.LastwordDestroy:
		case AIPlayTagType.LastwordDamage:
		case AIPlayTagType.LastwordHeal:
		case AIPlayTagType.LastwordMetamorphose:
		case AIPlayTagType.LastwordBanish:
		case AIPlayTagType.LastwordDraw:
		case AIPlayTagType.LastwordAddDeck:
		case AIPlayTagType.LastwordSetStatus:
		case AIPlayTagType.LastwordReanimate:
		case AIPlayTagType.LastwordAttachTag:
		case AIPlayTagType.LastwordEvo:
		case AIPlayTagType.LastwordRemoveSkill:
		case AIPlayTagType.LastwordAddCemetery:
		case AIPlayTagType.LastwordDamageClip:
		case AIPlayTagType.LastwordShield:
		case AIPlayTagType.LastwordSubtractCountdown:
			list = new List<AIPlayTagType> { type };
			tagCollection = new LastwordTagCollection();
			break;
		case AIPlayTagType.FanfareToken:
		case AIPlayTagType.FanfareHeal:
		case AIPlayTagType.FanfareBanish:
		case AIPlayTagType.FanfareSubtractCountdown:
		case AIPlayTagType.FanfareDamage:
		case AIPlayTagType.FanfareBuff:
		case AIPlayTagType.FanfareDestroy:
		case AIPlayTagType.FanfareBounce:
		case AIPlayTagType.FanfareRecoverAttackableCount:
		case AIPlayTagType.FanfareSneak:
		case AIPlayTagType.FanfareQuick:
		case AIPlayTagType.FanfareRush:
		case AIPlayTagType.FanfareGuard:
		case AIPlayTagType.FanfareDrain:
		case AIPlayTagType.FanfareKiller:
		case AIPlayTagType.FanfareHandBuff:
		case AIPlayTagType.FanfareUntouchable:
		case AIPlayTagType.FanfareForceTargeting:
		case AIPlayTagType.FanfareSelect:
		case AIPlayTagType.FanfareHandSelect:
		case AIPlayTagType.FanfareRemoveSkill:
		case AIPlayTagType.FanfareReanimate:
		case AIPlayTagType.FanfareMetamorphose:
		case AIPlayTagType.FanfareHandMetamorphose:
		case AIPlayTagType.FanfareSetMaxStatus:
		case AIPlayTagType.FanfareBonusInSimulation:
		case AIPlayTagType.FanfareRecoverPp:
		case AIPlayTagType.FanfareAttachTag:
		case AIPlayTagType.FanfareAttachStyle:
		case AIPlayTagType.FanfareCopyTag:
		case AIPlayTagType.FanfareTokenDraw:
		case AIPlayTagType.FanfareSummonHandCard:
		case AIPlayTagType.FanfareDiscard:
		case AIPlayTagType.FanfareSpellboost:
		case AIPlayTagType.FanfareAddCemetery:
		case AIPlayTagType.FanfareBanAttack:
		case AIPlayTagType.FanfareIgnoreGuard:
		case AIPlayTagType.FanfareChangeClass:
		case AIPlayTagType.FanfareChangeTribe:
		case AIPlayTagType.FanfareChangeCost:
		case AIPlayTagType.FanfareNotBeAttacked:
		case AIPlayTagType.FanfareAttackableCount:
		case AIPlayTagType.FanfareModifyConsumeEp:
		case AIPlayTagType.FanfareEvo:
		case AIPlayTagType.FanfareShield:
		case AIPlayTagType.FanfareDamageCut:
		case AIPlayTagType.FanfareDamageClip:
		case AIPlayTagType.FanfareRemoveGuard:
		case AIPlayTagType.FanfareAddDeck:
			list = new List<AIPlayTagType> { type };
			tagCollection = new FanfareTagCollection();
			break;
		case AIPlayTagType.PlayToken:
		case AIPlayTagType.PlayDestroy:
		case AIPlayTagType.PlayDamage:
		case AIPlayTagType.PlayHeal:
		case AIPlayTagType.PlayBanish:
		case AIPlayTagType.PlayBounce:
		case AIPlayTagType.PlaySubtractCountdown:
		case AIPlayTagType.PlayBuff:
		case AIPlayTagType.PlaySneak:
		case AIPlayTagType.PlayQuick:
		case AIPlayTagType.PlayRush:
		case AIPlayTagType.PlayGuard:
		case AIPlayTagType.PlayDrain:
		case AIPlayTagType.PlayKiller:
		case AIPlayTagType.PlayHandBuff:
		case AIPlayTagType.PlayUntouchable:
		case AIPlayTagType.PlaySelect:
		case AIPlayTagType.PlayHandSelect:
		case AIPlayTagType.PlayRemoveSkill:
		case AIPlayTagType.PlayReanimate:
		case AIPlayTagType.PlayMetamorphose:
		case AIPlayTagType.PlayHandMetamorphose:
		case AIPlayTagType.PlaySetMaxStatus:
		case AIPlayTagType.PlaySetLeaderMaxLife:
		case AIPlayTagType.PlayBonusInSimulation:
		case AIPlayTagType.PlayRecoverPP:
		case AIPlayTagType.PlayAttachTag:
		case AIPlayTagType.PlayCopyTag:
		case AIPlayTagType.PlayTokenDraw:
		case AIPlayTagType.PlaySummonHandCard:
		case AIPlayTagType.PlayDiscard:
		case AIPlayTagType.PlaySpellboost:
		case AIPlayTagType.PlayAddCemetery:
		case AIPlayTagType.PlayBanAttack:
		case AIPlayTagType.PlayIgnoreGuard:
		case AIPlayTagType.PlayChangeClass:
		case AIPlayTagType.PlayChangeTribe:
		case AIPlayTagType.PlayChangeCost:
		case AIPlayTagType.PlayNotBeAttacked:
		case AIPlayTagType.PlayAttackableCount:
		case AIPlayTagType.PlayModifyConsumeEp:
		case AIPlayTagType.PlayEvo:
		case AIPlayTagType.PlayShield:
		case AIPlayTagType.PlayDamageCut:
		case AIPlayTagType.PlayDamageClip:
		case AIPlayTagType.PlayAddDeck:
			list = new List<AIPlayTagType> { type };
			tagCollection = new PlayTagCollection();
			break;
		case AIPlayTagType.OtherPlayAttachTag:
		case AIPlayTagType.OtherPlayEvo:
		case AIPlayTagType.OtherEnhanceEvo:
		case AIPlayTagType.OtherPlayDamage:
		case AIPlayTagType.OtherPlayRecoverPp:
		case AIPlayTagType.OtherPlayDestroy:
		case AIPlayTagType.OtherPlayBuff:
		case AIPlayTagType.OtherPlayToken:
		case AIPlayTagType.OtherPlayRemoveTag:
		case AIPlayTagType.OtherPlayBounce:
		case AIPlayTagType.OtherPlayQuick:
			list = new List<AIPlayTagType> { type };
			tagCollection = new OtherPlayTagCollection();
			break;
		case AIPlayTagType.CostBonus:
			tagCollection = new CostBonusTagCollection();
			break;
		case AIPlayTagType.AfterClashHeal:
		case AIPlayTagType.AfterClashDamage:
			list = new List<AIPlayTagType> { type };
			tagCollection = new AfterClashTagCollection();
			break;
		case AIPlayTagType.RemoveSkill:
			tagCollection = new RemoveSkillTagCollection();
			break;
		case AIPlayTagType.PlaySkip:
		case AIPlayTagType.PlaySkipWithEvo:
		case AIPlayTagType.PlaySkipIfEvo:
		case AIPlayTagType.PlaySkipWithAction:
		case AIPlayTagType.PlaySkipWithActionIfEvo:
			list = new List<AIPlayTagType> { type };
			tagCollection = new PlaySkipTagCollection();
			break;
		case AIPlayTagType.HealDamage:
		case AIPlayTagType.HealBuff:
		case AIPlayTagType.HealToken:
		case AIPlayTagType.HealHeal:
		case AIPlayTagType.HealEvo:
		case AIPlayTagType.HealAttachTag:
			list = new List<AIPlayTagType> { type };
			tagCollection = new HealTagCollection();
			break;
		case AIPlayTagType.TurnStartAttachTag:
		case AIPlayTagType.TurnStartSubtractCountdown:
		case AIPlayTagType.TurnStartDamageCut:
		case AIPlayTagType.TurnStartShield:
		case AIPlayTagType.TurnStartDamage:
			list = new List<AIPlayTagType> { type };
			tagCollection = new TurnStartTagCollection();
			break;
		case AIPlayTagType.TurnEndDestroy:
		case AIPlayTagType.TurnEndBanish:
		case AIPlayTagType.TurnEndDamage:
		case AIPlayTagType.TurnEndHeal:
		case AIPlayTagType.TurnEndSetLeaderMaxLife:
		case AIPlayTagType.TurnEndDiscard:
		case AIPlayTagType.TurnEndBuff:
		case AIPlayTagType.TurnEndSubtractCountdown:
		case AIPlayTagType.TurnEndToken:
		case AIPlayTagType.TurnEndBounce:
		case AIPlayTagType.TurnEndAddDeck:
		case AIPlayTagType.TurnEndEvo:
		case AIPlayTagType.TurnEndDraw:
		case AIPlayTagType.TurnEndShield:
		case AIPlayTagType.TurnEndDamageClip:
		case AIPlayTagType.TurnEndDamageCut:
		case AIPlayTagType.TurnEndGuard:
		case AIPlayTagType.TurnEndBanAttack:
		case AIPlayTagType.TurnEndMetamorphose:
		case AIPlayTagType.TurnEndAttachTag:
		case AIPlayTagType.TurnEndRemoveTag:
			list = new List<AIPlayTagType> { type };
			tagCollection = new TurnEndTagCollection();
			break;
		case AIPlayTagType.IgnoreBreak:
			tagCollection = new IgnoreBreakTagCollection();
			break;
		case AIPlayTagType.Break:
			tagCollection = new BreakBonusTagCollection();
			break;
		case AIPlayTagType.LeaveBonus:
			tagCollection = new LeaveBonusTagCollection();
			break;
		case AIPlayTagType.LeaveHeal:
		case AIPlayTagType.LeaveDamage:
		case AIPlayTagType.LeaveBanish:
		case AIPlayTagType.LeaveToken:
		case AIPlayTagType.LeaveAttachTag:
			list = new List<AIPlayTagType> { type };
			tagCollection = new LeaveTagCollection();
			break;
		case AIPlayTagType.OtherLeaveDamage:
		case AIPlayTagType.OtherLeaveToken:
			list = new List<AIPlayTagType> { type };
			tagCollection = new OtherLeaveTagCollection();
			break;
		case AIPlayTagType.EvoEvo:
		case AIPlayTagType.EvoDamage:
		case AIPlayTagType.EvoHandBuff:
		case AIPlayTagType.EvoToken:
		case AIPlayTagType.EvoDestroy:
		case AIPlayTagType.EvoBuff:
		case AIPlayTagType.EvoHeal:
		case AIPlayTagType.EvoBanish:
		case AIPlayTagType.EvoSubtractCountdown:
		case AIPlayTagType.EvoRecoverPp:
		case AIPlayTagType.EvoSetLeaderMaxLife:
		case AIPlayTagType.EvoDiscard:
		case AIPlayTagType.EvoMetamorphose:
		case AIPlayTagType.EvoHandMetamorphose:
		case AIPlayTagType.EvoBounce:
		case AIPlayTagType.EvoRush:
		case AIPlayTagType.EvoQuick:
		case AIPlayTagType.EvoGuard:
		case AIPlayTagType.EvoKiller:
		case AIPlayTagType.EvoDrain:
		case AIPlayTagType.EvoAttackableCount:
		case AIPlayTagType.EvoChangeCost:
		case AIPlayTagType.EvoHandSelect:
		case AIPlayTagType.EvoShield:
		case AIPlayTagType.EvoDamageCut:
		case AIPlayTagType.EvoReanimate:
		case AIPlayTagType.EvoAddDeck:
		case AIPlayTagType.EvoAttachTag:
		case AIPlayTagType.EvoTokenDraw:
		case AIPlayTagType.EvoAddStack:
		case AIPlayTagType.EvoSetStatus:
			list = new List<AIPlayTagType> { type };
			tagCollection = new EvoTagCollection();
			break;
		case AIPlayTagType.OtherEvoBuff:
		case AIPlayTagType.OtherEvoDamage:
		case AIPlayTagType.OtherEvoBanish:
		case AIPlayTagType.OtherEvoSubtractCountdown:
		case AIPlayTagType.OtherEvoEvo:
		case AIPlayTagType.OtherEvoToken:
		case AIPlayTagType.OtherEvoShield:
			list = new List<AIPlayTagType> { type };
			tagCollection = new OtherEvoTagCollection();
			break;
		case AIPlayTagType.SelfAndOtherEvoDamage:
		case AIPlayTagType.SelfAndOtherEvoAttachTag:
		case AIPlayTagType.SelfAndOtherEvoShield:
		case AIPlayTagType.SelfAndOtherEvoDestroy:
		case AIPlayTagType.SelfAndOtherEvoBounce:
		case AIPlayTagType.SelfAndOtherEvoToken:
		case AIPlayTagType.SelfAndOtherEvoDraw:
		case AIPlayTagType.SelfAndOtherEvoAddCemetery:
		case AIPlayTagType.SelfAndOtherEvoHeal:
		case AIPlayTagType.SelfAndOtherEvoTokenDraw:
			list = new List<AIPlayTagType> { type };
			tagCollection = new SelfAndOtherEvoTagCollection();
			break;
		case AIPlayTagType.FirstEvo:
			tagCollection = new FirstEvoTagCollection();
			break;
		case AIPlayTagType.Target:
			tagCollection = new TargetTagCollection();
			break;
		case AIPlayTagType.IgnoreTarget:
			tagCollection = new IgnoreTargetTagCollection();
			break;
		case AIPlayTagType.BreakLast:
			tagCollection = new BreakLastTagCollection();
			break;
		case AIPlayTagType.BreakFirst:
			tagCollection = new BreakFirstTagCollection();
			break;
		case AIPlayTagType.BreakBeforePlay:
			tagCollection = new BreakBeforePlayTagCollection();
			break;
		case AIPlayTagType.AttackByLife:
			tagCollection = new AttackByLifeTagCollection();
			break;
		case AIPlayTagType.EmoteOnPlay:
		case AIPlayTagType.EmoteOnEvo:
		case AIPlayTagType.EmoteOnAtk:
		case AIPlayTagType.EmoteOnDestroy:
		case AIPlayTagType.ForceEmoteOnDestroy:
		case AIPlayTagType.EmoteOnTurnEnd:
			list = new List<AIPlayTagType> { type };
			tagCollection = new EmoteTagCollection();
			break;
		case AIPlayTagType.ReanimateBonus:
			tagCollection = new ReanimateBonusTagCollection();
			break;
		case AIPlayTagType.ReanimateEvo:
			tagCollection = new ReanimateEvoTagCollection();
			break;
		case AIPlayTagType.PlayDraw:
			tagCollection = new PlayDrawTagCollection();
			break;
		case AIPlayTagType.EvolvedAttackable:
		case AIPlayTagType.EvolvedAttackableCount:
		case AIPlayTagType.EvolvedSkill:
			list = new List<AIPlayTagType> { type };
			tagCollection = new EvolvedResidentTagCollection();
			break;
		case AIPlayTagType.PlayPlus:
			tagCollection = new PlayPlusTagCollection();
			break;
		case AIPlayTagType.PlayoutDamageBonus:
		case AIPlayTagType.PlayoutAttackBonus:
			list = new List<AIPlayTagType> { type };
			tagCollection = new PlayoutBonusTagCollection();
			break;
		case AIPlayTagType.AllyPlayoutDamageBonus:
			tagCollection = new OtherPlayoutBonusTagCollection();
			break;
		case AIPlayTagType.AttackableClass:
			tagCollection = new AttackableClassTagCollection();
			break;
		case AIPlayTagType.HandBonus:
			tagCollection = new HandBonusTagCollection();
			break;
		case AIPlayTagType.Fusion:
			tagCollection = new FusionTagCollection();
			break;
		case AIPlayTagType.FusionDraw:
			tagCollection = new FusionDrawTagCollection();
			break;
		case AIPlayTagType.ModifyHeal:
			tagCollection = new ModifyHealTagCollection();
			break;
		case AIPlayTagType.FusionBonus:
			tagCollection = new FusionBonusTagCollection();
			break;
		case AIPlayTagType.PlayptnBaseStatsRate:
			tagCollection = new PlayptnBaseStatsRateTagCollection();
			break;
		case AIPlayTagType.NoInstantAttack:
			tagCollection = new NoInstantAttackTagCollection();
			break;
		case AIPlayTagType.GiveSkill:
			tagCollection = new GiveSkillTagCollection();
			break;
		case AIPlayTagType.Enhance:
		case AIPlayTagType.Accelerate:
		case AIPlayTagType.Crystalize:
		case AIPlayTagType.ChoiceTransform:
			list = new List<AIPlayTagType> { type };
			tagCollection = new FixedCostTagCollection();
			break;
		case AIPlayTagType.DiscardedBonus:
			tagCollection = new DiscardedBonusTagCollection();
			break;
		case AIPlayTagType.DiscardedToken:
			list = new List<AIPlayTagType> { type };
			tagCollection = new DiscardedTagCollection();
			break;
		case AIPlayTagType.DiscardDamage:
		case AIPlayTagType.DiscardHeal:
		case AIPlayTagType.AllyDiscardBonus:
			list = new List<AIPlayTagType> { type };
			tagCollection = new AfterDiscardTagCollection();
			break;
		case AIPlayTagType.OtherBreakBonus:
			tagCollection = new OtherBreakBonusTagCollection();
			break;
		case AIPlayTagType.OtherBanishBonus:
			tagCollection = new OtherBanishBonusTagCollection();
			break;
		case AIPlayTagType.OtherLeaveBonus:
			tagCollection = new OtherLeaveBonusTagCollection();
			break;
		case AIPlayTagType.OneMoreLastword:
			tagCollection = new OneMoreLastwordTagCollection();
			break;
		case AIPlayTagType.BuffDamage:
		case AIPlayTagType.BuffDestroy:
		case AIPlayTagType.BuffShield:
		case AIPlayTagType.BuffToken:
		case AIPlayTagType.BuffRecoverPP:
		case AIPlayTagType.BuffRush:
		case AIPlayTagType.BuffHeal:
		case AIPlayTagType.BuffBuff:
		case AIPlayTagType.BuffEvo:
		case AIPlayTagType.BuffDraw:
			list = new List<AIPlayTagType> { type };
			tagCollection = new BuffTriggerTagCollection();
			break;
		case AIPlayTagType.BanishAttachTag:
			list = new List<AIPlayTagType> { type };
			tagCollection = new BanishTagCollection();
			break;
		case AIPlayTagType.OtherBanishToken:
		case AIPlayTagType.OtherBanishAddCemetery:
			list = new List<AIPlayTagType> { type };
			tagCollection = new OtherBanishTagCollection();
			break;
		case AIPlayTagType.GetOn:
			tagCollection = new GetOnTagCollection();
			break;
		case AIPlayTagType.GetOnBanish:
		case AIPlayTagType.GetOnDamage:
		case AIPlayTagType.GetOnEvo:
			list = new List<AIPlayTagType> { type };
			tagCollection = new GetOnTriggerTagCollection();
			break;
		case AIPlayTagType.GetOffMetamorphose:
		case AIPlayTagType.GetOffEvo:
			list = new List<AIPlayTagType> { type };
			tagCollection = new WhenGetOffTagCollection();
			break;
		case AIPlayTagType.AddCardToPlayoutPlayPtn:
			tagCollection = new AddCardToPlayoutPlayPtnTagCollection();
			break;
		case AIPlayTagType.PlayActivateCount:
		case AIPlayTagType.AttackActivateCount:
		case AIPlayTagType.BreakActivateCount:
		case AIPlayTagType.BanishActivateCount:
		case AIPlayTagType.DamagedActivateCount:
		case AIPlayTagType.BuffActivateCounnt:
		case AIPlayTagType.TurnEndActivateCount:
		case AIPlayTagType.HealActivateCount:
		case AIPlayTagType.NecromanceActivateCount:
		case AIPlayTagType.EvoActivateCount:
		case AIPlayTagType.SummonActivateCount:
			list = new List<AIPlayTagType> { type };
			tagCollection = new ActivateCountTagCollection();
			break;
		case AIPlayTagType.EvoHandPlus:
			tagCollection = new EvoHandPlusTagCollection();
			break;
		case AIPlayTagType.ClashBonus:
			tagCollection = new ClashBonusTagCollection();
			break;
		case AIPlayTagType.NoNormalEvo:
			tagCollection = new NoNormalEvoTagCollection();
			break;
		case AIPlayTagType.ReincarnationSimulation:
			tagCollection = new ReincarnationSimulationTagCollection();
			break;
		case AIPlayTagType.PlagueCity:
			tagCollection = new PlagueCityTagCollection();
			break;
		case AIPlayTagType.CondChoice:
			tagCollection = new CondChoiceTagCollection();
			break;
		case AIPlayTagType.EvoChoice:
		case AIPlayTagType.PlayChoice:
		case AIPlayTagType.FanfareChoice:
		case AIPlayTagType.ChoiceBrave:
			list = new List<AIPlayTagType> { type };
			tagCollection = new ChoiceTagCollection();
			break;
		case AIPlayTagType.CantBeAttacked:
			tagCollection = new CantBeAttackedTagCollection();
			break;
		case AIPlayTagType.RallyCountPlus:
			tagCollection = new RallyCountPlusTagCollection();
			break;
		case AIPlayTagType.BounceDamage:
			list = new List<AIPlayTagType> { type };
			tagCollection = new BounceTagCollection();
			break;
		case AIPlayTagType.NecromanceAttachTag:
		case AIPlayTagType.NecromanceAddCemetery:
		case AIPlayTagType.NecromanceDamage:
		case AIPlayTagType.NecromanceHeal:
			list = new List<AIPlayTagType> { type };
			tagCollection = new WhenNecromanceTagCollection();
			break;
		case AIPlayTagType.ResonanceDamage:
		case AIPlayTagType.ResonanceHeal:
		case AIPlayTagType.ResonanceKiller:
			list = new List<AIPlayTagType> { type };
			tagCollection = new ResonanceTagCollection();
			break;
		case AIPlayTagType.ForceBerserk:
			tagCollection = new ForceBerserkTagCollection();
			break;
		case AIPlayTagType.SetAITribe:
			tagCollection = new SetAITribeTagCollection();
			break;
		case AIPlayTagType.GenerateTag:
			tagCollection = new GenerateTagCollection();
			break;
		case AIPlayTagType.RemoveByDestroy:
			tagCollection = new RemoveByDestroyTagCollection();
			break;
		case AIPlayTagType.ChangeInplayAttachTag:
		case AIPlayTagType.ChangeInplayImmediateRemoveByBanish:
		case AIPlayTagType.ChangeInplayImmediateRemoveByDestroy:
		case AIPlayTagType.ChangeInplayCannotPlay:
		case AIPlayTagType.ChangeInplayCannotAttack:
		case AIPlayTagType.ChangeInplayImmediateShield:
		case AIPlayTagType.ChangeInplayImmediateDamageCut:
		case AIPlayTagType.ChangeInplayImmediateDamageClip:
		case AIPlayTagType.ChangeInplayImmediateLifeLowerLimit:
		case AIPlayTagType.ChangeInplayImmediateDamageModifier:
		case AIPlayTagType.ChangeInplayImmediateUntouchable:
		case AIPlayTagType.ChangeInplayImmediateIndestructible:
		case AIPlayTagType.ChangePpTotalBuff:
			list = new List<AIPlayTagType> { type };
			tagCollection = new ChangeInplayTagCollection();
			break;
		case AIPlayTagType.EvolveToOther:
			tagCollection = new EvolveToOtherTagCollection();
			break;
		case AIPlayTagType.BuffBonus:
			tagCollection = new BuffBonusTagCollection();
			break;
		case AIPlayTagType.ForceImmediateAttack:
			tagCollection = new ForceImmediateAttackTagCollection();
			break;
		case AIPlayTagType.FusionMetamorphose:
			tagCollection = new FusionMetamorphoseTagCollection();
			break;
		default:
			return null;
		}
		if (list != null)
		{
			return new TagCollectionWithMultipleTypes(list, tagCollection);
		}
		return new TagCollectionWithSingleType(type, tagCollection);
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIPlayTag
{
	public static readonly int TAG_WORDS_LENTGH = 3;

	private string _typeText;

	private string _argText;

	private string _conditionText;

	private AIScriptArgumentExpressions argExpr;

	private AIConditionExpressions condExpr;

	public AIPlayTagType Type { get; private set; }

	public string Condition => _conditionText;

	public ulong Hash { get; private set; }

	public AIScriptArgumentExpressions ArgumentExpressions => argExpr;

	public AIConditionExpressions ConditionExpressions => condExpr;

	public bool InitFromTextAsset(AIPlayTagAsset asset)
	{
		Type = AIScriptParser.ConvertWordToTagType(asset.Type);
		if (Type == AIPlayTagType.None)
		{
			return false;
		}
		if (CheckInvalidTagPattern(asset, out var _))
		{
			return false;
		}
		_typeText = asset.Type;
		_argText = asset.Arg;
		_conditionText = asset.Condition;
		Hash = GetHash();
		argExpr = CreateArgument(_argText);
		condExpr = new AIConditionExpressions(_conditionText);
		return true;
	}

	private bool CheckInvalidTagPattern(AIPlayTagAsset asset, out string errorLog)
	{
		errorLog = "";
		bool flag = false;
		switch (Type)
		{
		case AIPlayTagType.FanfareToken:
		case AIPlayTagType.PlayToken:
			flag = asset.Arg.IndexOf("PLAY_TOKEN_COUNT") >= 0 || asset.Condition.IndexOf("PLAY_TOKEN_COUNT") >= 0;
			if (flag)
			{
				errorLog = "タグ " + asset.Type + " のArgに PLAY_TOKEN_COUNT が使用されています。\n";
			}
			break;
		case AIPlayTagType.TurnEndActivateCount:
			flag = asset.Condition.IndexOf("IS_IN_HAND") < 0 && asset.Condition.IndexOf("IS_ON_FIELD") < 0;
			if (flag)
			{
				errorLog = "タグ " + asset.Type + " のCondに IS_IN_HAND,IS_ON_FIELD のどちらも指定されていません。\n";
			}
			break;
		}
		AIPlayTagType type = Type;
		if ((uint)(type - 23) > 2u && (uint)(type - 27) > 1u && asset.Condition.IndexOf("NEXT_PLAY_PRIORITY ") > 0)
		{
			flag = true;
			errorLog = "PlaySkip系以外のタグ " + asset.Type + " のCondには NEXT_PLAY_PRIORITY を使用することを許可されていません。\n";
		}
		if (flag)
		{
			errorLog = errorLog + "tag:" + asset.Type + ", arg:" + asset.Arg + ", cond:" + asset.Condition;
		}
		return flag;
	}

	private AIScriptArgumentExpressions CreateArgument(string arg)
	{
		switch (Type)
		{
		case AIPlayTagType.AllyPlayBonus:
		case AIPlayTagType.EnemyPlayBonus:
			return new AIOtherPlayBonus(arg);
		case AIPlayTagType.MemberBattleBonusRate:
		case AIPlayTagType.EnemyBattleBonusRate:
			return new AIOtherBattleBonusRate(arg);
		case AIPlayTagType.MemberEvoBonus:
		case AIPlayTagType.EnemyEvoBonus:
			return new AIOtherEvoBonus(arg);
		case AIPlayTagType.PlayAttachTag:
		case AIPlayTagType.FanfareAttachTag:
			return new AIWhenPlayAttachTag(arg);
		case AIPlayTagType.PlayCopyTag:
		case AIPlayTagType.FanfareCopyTag:
			return new AIWhenPlayCopyTag(arg);
		case AIPlayTagType.AttackAttachTag:
			return new AIAttackAttachTag(arg);
		case AIPlayTagType.LastwordAttachTag:
			return new AILastwordAttachTag(arg);
		case AIPlayTagType.EvoAttachTag:
			return new AIEvoAttachTag(arg);
		case AIPlayTagType.SummonAttachTag:
			return new AISummonAttachTag(arg);
		case AIPlayTagType.OtherSummonAttachTag:
			return new AIOtherSummonAttachTag(arg);
		case AIPlayTagType.FanfareAttachStyle:
			return new AIWhenPlayAttachStyle(arg);
		case AIPlayTagType.FanfareToken:
		case AIPlayTagType.PlayToken:
			return new AIWhenPlaySummonToken(arg);
		case AIPlayTagType.BreakAttachTag:
			return new AIBreakAttachTag(arg);
		case AIPlayTagType.NecromanceAttachTag:
			return new AINecromanceAttachTag(arg);
		case AIPlayTagType.NecromanceAddCemetery:
			return new AINecromanceAddCemetery(arg);
		case AIPlayTagType.TurnStartAttachTag:
			return new AITurnStartAttachTag(arg);
		case AIPlayTagType.TurnStartSubtractCountdown:
			return new AITurnStartSubtractCountdown(arg);
		case AIPlayTagType.TurnStartDamageCut:
			return new AITurnStartDamageCut(arg);
		case AIPlayTagType.TurnStartShield:
			return new AITurnStartShield(arg);
		case AIPlayTagType.TurnStartDamage:
			return new AITurnStartDamage(arg);
		case AIPlayTagType.TurnEndAttachTag:
			return new AITurnEndAttachTag(arg);
		case AIPlayTagType.HealAttachTag:
			return new AIHealAttachTag(arg);
		case AIPlayTagType.DiscardedToken:
			return new AIDiscardedToken(arg);
		case AIPlayTagType.LeaveToken:
			return new AILeaveToken(arg);
		case AIPlayTagType.DamagedToken:
			return new AIDamagedToken(arg);
		case AIPlayTagType.PlayDamage:
		case AIPlayTagType.FanfareDamage:
			return new AIWhenPlayDamage(arg);
		case AIPlayTagType.PlayBuff:
		case AIPlayTagType.FanfareBuff:
			return new AIWhenPlayBuff(arg);
		case AIPlayTagType.PlaySetMaxStatus:
		case AIPlayTagType.FanfareSetMaxStatus:
			return new AIWhenPlaySetMaxStatus(arg);
		case AIPlayTagType.PlaySetLeaderMaxLife:
			return new AIWhenPlaySetLeaderMaxLife(arg);
		case AIPlayTagType.PlayDestroy:
		case AIPlayTagType.FanfareDestroy:
			return new AIWhenPlayDestroy(arg);
		case AIPlayTagType.PlayBanish:
		case AIPlayTagType.FanfareBanish:
			return new AIWhenPlayBanish(arg);
		case AIPlayTagType.PlayBounce:
		case AIPlayTagType.FanfareBounce:
			return new AIWhenPlayBounce(arg);
		case AIPlayTagType.PlayMetamorphose:
		case AIPlayTagType.FanfareMetamorphose:
			return new AIWhenPlayMetamorphose(arg);
		case AIPlayTagType.PlayHandMetamorphose:
		case AIPlayTagType.FanfareHandMetamorphose:
			return new AIWhenPlayHandMetamorphose(arg);
		case AIPlayTagType.FanfareSpellboost:
		case AIPlayTagType.PlaySpellboost:
			return new AIWhenPlaySpellboost(arg);
		case AIPlayTagType.FanfareAddCemetery:
		case AIPlayTagType.PlayAddCemetery:
			return new AIWhenPlayAddCemetery(arg);
		case AIPlayTagType.PlayReanimate:
		case AIPlayTagType.FanfareReanimate:
			return new AIWhenPlayReanimate(arg);
		case AIPlayTagType.PlayBanAttack:
		case AIPlayTagType.FanfareBanAttack:
			return new AIWhenPlayBanAttack(arg);
		case AIPlayTagType.FanfareHandBuff:
		case AIPlayTagType.PlayHandBuff:
			return new AIWhenPlayHandBuff(arg);
		case AIPlayTagType.PlayChangeCost:
		case AIPlayTagType.FanfareChangeCost:
			return new AIWhenPlayChangeCost(arg);
		case AIPlayTagType.FanfareBonusInSimulation:
		case AIPlayTagType.PlayBonusInSimulation:
			return new AIWhenPlayBonusInSimulation(arg);
		case AIPlayTagType.Target:
		case AIPlayTagType.IgnoreTarget:
		case AIPlayTagType.AttackByLife:
		case AIPlayTagType.FirstEvo:
			return new AIFiltersArgument(arg);
		case AIPlayTagType.PlaySkip:
			return new AIPlaySkipTagArgument(arg);
		case AIPlayTagType.PlaySkipWithEvo:
			return new AIPlaySkipWithEvo(arg);
		case AIPlayTagType.PlaySkipIfEvo:
			return new AIPlaySkipIfEvo(arg);
		case AIPlayTagType.PlaySkipWithAction:
			return new AIPlaySkipWithAction(arg);
		case AIPlayTagType.PlaySkipWithActionIfEvo:
			return new AIPlaySkipWithActionIfEvo(arg);
		case AIPlayTagType.AttackDamage:
			return new AIAttackDamage(arg);
		case AIPlayTagType.AttackDestroy:
			return new AIAttackDestroy(arg);
		case AIPlayTagType.ClashBanish:
		case AIPlayTagType.AttackBanish:
			return new AIAttackOrClashBanish(arg);
		case AIPlayTagType.ClashHeal:
		case AIPlayTagType.AttackHeal:
			return new AIAttackOrClashHeal(arg);
		case AIPlayTagType.AttackEvo:
			return new AIAttackEvo(arg);
		case AIPlayTagType.AttackBreakDamage:
			return new AIAttackBreakDamage(arg);
		case AIPlayTagType.AttackBreakEvo:
			return new AIAttackBreakEvo(arg);
		case AIPlayTagType.AttackBreakRecoverPp:
			return new AIAttackBreakRecoverPp(arg);
		case AIPlayTagType.AttackBreakAttackTwice:
			return new AIAttackBreakAttackTwice(arg);
		case AIPlayTagType.ClashToken:
		case AIPlayTagType.AttackToken:
			return new AIAttackOrClashToken(arg);
		case AIPlayTagType.AttackAttackableCount:
			return new AIAttackAttackableCount(arg);
		case AIPlayTagType.AttackQuick:
			return new AIAttackOrClashKeywordSkill(arg, AIScriptTokenArgType.QUICK);
		case AIPlayTagType.PlayHeal:
		case AIPlayTagType.FanfareHeal:
			return new AIWhenPlayHeal(arg);
		case AIPlayTagType.AttackBuff:
			return new AIAttackBuff(arg);
		case AIPlayTagType.AttackHandBuff:
			return new AIAttackHandBuff(arg);
		case AIPlayTagType.SummonBuff:
			return new AISummonBuff(arg);
		case AIPlayTagType.SummonDestroy:
			return new AISummonDestroy(arg);
		case AIPlayTagType.SummonBanAttack:
			return new AISummonBanAttack(arg);
		case AIPlayTagType.BreakDamage:
			return new AIBreakDamage(arg);
		case AIPlayTagType.BreakRecoverAttackableCount:
			return new AIBreakRecoverAttackableCount(arg);
		case AIPlayTagType.LastwordDestroy:
			return new AILastwordDestroy(arg);
		case AIPlayTagType.DamagedBonus:
			return new AIDamagedBonus(arg);
		case AIPlayTagType.DamagedBuff:
			return new AIDamagedBuff(arg);
		case AIPlayTagType.DamagedDamage:
			return new AIDamagedDamage(arg);
		case AIPlayTagType.DamagedHeal:
			return new AIDamagedHeal(arg);
		case AIPlayTagType.DamagedCantUnderAttack:
			return new AIDamagedCantUnderAttack(arg);
		case AIPlayTagType.OtherDamagedDamage:
			return new AIOtherDamagedDamage(arg);
		case AIPlayTagType.OtherDamagedHeal:
			return new AIOtherDamagedHeal(arg);
		case AIPlayTagType.OtherDamagedSetLeaderMaxLife:
			return new AIOtherDamagedSetLeaderMaxLife(arg);
		case AIPlayTagType.OtherDamagedSubtractCountdown:
			return new AIOtherDamagedSubtractCountdown(arg);
		case AIPlayTagType.OtherDamagedBanish:
			return new AIOtherDamagedBanish(arg);
		case AIPlayTagType.SummonEvo:
			return new AISummonEvo(arg);
		case AIPlayTagType.EvoToken:
			return new AIEvoToken(arg);
		case AIPlayTagType.LastwordToken:
			return new AILastwordToken(arg);
		case AIPlayTagType.SummonRush:
			return new AISummonKeywordSkill(arg, AIScriptTokenArgType.RUSH);
		case AIPlayTagType.SummonQuick:
			return new AISummonKeywordSkill(arg, AIScriptTokenArgType.QUICK);
		case AIPlayTagType.OtherSummonRush:
			return new AIOtherSummonKeywordSkill(arg, AIScriptTokenArgType.RUSH);
		case AIPlayTagType.SummonDamage:
			return new AISummonDamage(arg);
		case AIPlayTagType.SummonBanish:
			return new AISummonBanish(arg);
		case AIPlayTagType.SummonHeal:
			return new AISummonHeal(arg);
		case AIPlayTagType.OtherSummonDamage:
			return new AIOtherSummonDamage(arg);
		case AIPlayTagType.OtherSummonGuard:
			return new AIOtherSummonKeywordSkill(arg, AIScriptTokenArgType.GUARD);
		case AIPlayTagType.OtherSummonQuick:
			return new AIOtherSummonKeywordSkill(arg, AIScriptTokenArgType.QUICK);
		case AIPlayTagType.OtherSummonKiller:
			return new AIOtherSummonKeywordSkill(arg, AIScriptTokenArgType.KILLER);
		case AIPlayTagType.OtherSummonDrain:
			return new AIOtherSummonKeywordSkill(arg, AIScriptTokenArgType.DRAIN);
		case AIPlayTagType.OtherSummonBanish:
			return new AIOtherSummonBanish(arg);
		case AIPlayTagType.OtherSummonHeal:
			return new AIOtherSummonHeal(arg);
		case AIPlayTagType.OtherSummonBuff:
			return new AIOtherSummonBuff(arg);
		case AIPlayTagType.OtherSummonEvo:
			return new AIOtherSummonEvo(arg);
		case AIPlayTagType.OtherSummonDamageCut:
			return new AIOtherSummonDamageCut(arg);
		case AIPlayTagType.OtherSummonDamageClip:
			return new AIOtherSummonDamageClip(arg);
		case AIPlayTagType.OtherSummonSubtractCountdown:
			return new AIOtherSummonSubtractCountdown(arg);
		case AIPlayTagType.OtherSummonAddCemetery:
			return new AIOtherSummonAddCemetery(arg);
		case AIPlayTagType.OtherSummonUntouchable:
			return new AIOtherSummonKeywordSkill(arg, AIScriptTokenArgType.UNTOUCHABLE);
		case AIPlayTagType.OtherSummonDestory:
			return new AIOtherSummonDestroy(arg);
		case AIPlayTagType.OtherSummonDraw:
			return new AIOtherSummonDraw(arg);
		case AIPlayTagType.Necromance:
			return new AINecromance(arg);
		case AIPlayTagType.EarthRite:
			return new AIEarthRite(arg);
		case AIPlayTagType.BurialRite:
			return new AIBurialRite(arg);
		case AIPlayTagType.LastwordDamage:
			return new AILastwordDamage(arg);
		case AIPlayTagType.LastwordHeal:
			return new AILastwordHeal(arg);
		case AIPlayTagType.LastwordMetamorphose:
			return new AILastwordMetamorphose(arg);
		case AIPlayTagType.LastwordBanish:
			return new AILastwordBanish(arg);
		case AIPlayTagType.LastwordDraw:
			return new AILastwordDraw(arg);
		case AIPlayTagType.LastwordAddDeck:
			return new AILastwordAddDeck(arg);
		case AIPlayTagType.LastwordSetStatus:
			return new AILastwordSetStatus(arg);
		case AIPlayTagType.TurnEndDamage:
			return new AITurnEndDamage(arg);
		case AIPlayTagType.ClashDamage:
			return new AIClashDamage(arg);
		case AIPlayTagType.AfterClashDamage:
			return new AIAfterClashDamage(arg);
		case AIPlayTagType.ClashDestroy:
			return new AIClashDestroy(arg);
		case AIPlayTagType.ClashBuff:
			return new AIClashBuff(arg);
		case AIPlayTagType.ClashKiller:
		case AIPlayTagType.AttackKiller:
			return new AIAttackOrClashKeywordSkill(arg, AIScriptTokenArgType.KILLER);
		case AIPlayTagType.AttackRemoveTag:
		case AIPlayTagType.ClashRemoveTag:
			return new AIAttackOrClashRemoveTag(arg);
		case AIPlayTagType.ClashRemoveSkill:
		case AIPlayTagType.AttackRemoveSkill:
			return new AIAttackOrClashRemoveSkill(arg);
		case AIPlayTagType.ClashSpellboost:
			return new AIAttackOrClashSpellboost(arg);
		case AIPlayTagType.AfterClashHeal:
			return new AIAfterClashHeal(arg);
		case AIPlayTagType.RemoveSkill:
			return new AIRemoveSkill(arg);
		case AIPlayTagType.HealDamage:
			return new AIHealDamage(arg);
		case AIPlayTagType.HealBuff:
			return new AIHealBuff(arg);
		case AIPlayTagType.HealToken:
			return new AIHealToken(arg);
		case AIPlayTagType.HealHeal:
			return new AIHealHeal(arg);
		case AIPlayTagType.HealEvo:
			return new AIHealEvo(arg);
		case AIPlayTagType.TurnEndMetamorphose:
			return new AITurnEndMetamorphose(arg);
		case AIPlayTagType.BreakBuff:
			return new AIBreakBuff(arg);
		case AIPlayTagType.BreakSetLeaderMaxLife:
			return new AIBreakSetLeaderMaxLife(arg);
		case AIPlayTagType.LastwordBuff:
			return new AILastwordBuff(arg);
		case AIPlayTagType.EvoBuff:
			return new AIEvoBuff(arg);
		case AIPlayTagType.EvoHeal:
			return new AIEvoHeal(arg);
		case AIPlayTagType.EvoBanish:
			return new AIEvoBanish(arg);
		case AIPlayTagType.EvoSubtractCountdown:
			return new AIEvoSubtractCountdown(arg);
		case AIPlayTagType.EvoEvo:
			return new AIEvoEvo(arg);
		case AIPlayTagType.EvoDamage:
			return new AIEvoDamage(arg);
		case AIPlayTagType.EvoDestroy:
			return new AIEvoDestroy(arg);
		case AIPlayTagType.EvoMetamorphose:
			return new AIEvoMetamorphose(arg);
		case AIPlayTagType.EvoHandMetamorphose:
			return new AIEvoHandMetamorphose(arg);
		case AIPlayTagType.EvoBounce:
			return new AIEvoBounce(arg);
		case AIPlayTagType.EvoRush:
			return new AIEvoGiveBasicSkill(arg, AIScriptTokenArgType.RUSH);
		case AIPlayTagType.EvoQuick:
			return new AIEvoGiveBasicSkill(arg, AIScriptTokenArgType.QUICK);
		case AIPlayTagType.EvoGuard:
			return new AIEvoGiveBasicSkill(arg, AIScriptTokenArgType.GUARD);
		case AIPlayTagType.EvoKiller:
			return new AIEvoGiveBasicSkill(arg, AIScriptTokenArgType.KILLER);
		case AIPlayTagType.EvoDrain:
			return new AIEvoGiveBasicSkill(arg, AIScriptTokenArgType.DRAIN);
		case AIPlayTagType.EvoAttackableCount:
			return new AIEvoAttackableCount(arg);
		case AIPlayTagType.EvoChangeCost:
			return new AIEvoChangeCost(arg);
		case AIPlayTagType.EvoHandSelect:
			return new AIEvoHandSelect(arg);
		case AIPlayTagType.EvoTokenDraw:
			return new AIEvoTokenDraw(arg);
		case AIPlayTagType.EvoShield:
			return new AIEvoShield(arg);
		case AIPlayTagType.EvoDamageCut:
			return new AIEvoDamageCut(arg);
		case AIPlayTagType.EvoReanimate:
			return new AIEvoReanimate(arg);
		case AIPlayTagType.EvoAddDeck:
			return new AIEvoAddDeck(arg);
		case AIPlayTagType.TurnEndDestroy:
			return new AITurnEndDestroy(arg);
		case AIPlayTagType.TurnEndHeal:
			return new AITurnEndHeal(arg);
		case AIPlayTagType.TurnEndBanish:
			return new AITurnEndBanish(arg);
		case AIPlayTagType.TurnEndSetLeaderMaxLife:
			return new AITurnEndSetLeaderMaxLife(arg);
		case AIPlayTagType.TurnEndDiscard:
			return new AITurnEndDiscard(arg);
		case AIPlayTagType.TurnEndBuff:
			return new AITurnEndBuff(arg);
		case AIPlayTagType.TurnEndSubtractCountdown:
			return new AITurnEndSubtractCountdown(arg);
		case AIPlayTagType.TurnEndToken:
			return new AITurnEndToken(arg);
		case AIPlayTagType.TurnEndBounce:
			return new AITurnEndBounce(arg);
		case AIPlayTagType.TurnEndAddDeck:
			return new AITurnEndAddDeck(arg);
		case AIPlayTagType.TurnEndEvo:
			return new AITurnEndEvo(arg);
		case AIPlayTagType.TurnEndDraw:
			return new AITurnEndDraw(arg);
		case AIPlayTagType.TurnEndRemoveTag:
			return new AITurnEndRemoveTag(arg);
		case AIPlayTagType.TurnEndShield:
			return new AITurnEndShield(arg);
		case AIPlayTagType.TurnEndDamageClip:
			return new AITurnEndDamageClip(arg);
		case AIPlayTagType.TurnEndDamageCut:
			return new AITurnEndDamageCut(arg);
		case AIPlayTagType.TurnEndGuard:
			return new AITurnEndKeywordSkill(arg, AIScriptTokenArgType.GUARD);
		case AIPlayTagType.TurnEndBanAttack:
			return new AITurnEndBanAttack(arg);
		case AIPlayTagType.EvoSetLeaderMaxLife:
			return new AIEvoSetLeaderMaxLife(arg);
		case AIPlayTagType.EvolvedAttackableCount:
			return new AIEvolvedAttackableCount(arg);
		case AIPlayTagType.EvolvedAttackable:
			return new AIEvolvedAttackable(arg);
		case AIPlayTagType.EvolvedSkill:
			return new AIEvolvedSkill(arg);
		case AIPlayTagType.AfterAttackEvo:
			return new AIAfterAttackEvo(arg);
		case AIPlayTagType.AfterAttackDraw:
			return new AIAfterAttackDraw(arg);
		case AIPlayTagType.AfterAttackBanish:
			return new AIAfterAttackBanish(arg);
		case AIPlayTagType.BreakDestroy:
			return new AIBreakDestroy(arg);
		case AIPlayTagType.PlayRecoverPP:
		case AIPlayTagType.FanfareRecoverPp:
			return new AIWhenPlayRecoverPp(arg);
		case AIPlayTagType.BuffRecoverPP:
			return new AIBuffRecoverPp(arg);
		case AIPlayTagType.BreakRecoverPp:
			return new AIBreakRecoverPp(arg);
		case AIPlayTagType.Fusion:
			return new AIFusion(arg);
		case AIPlayTagType.FusionDraw:
			return new AIFusionDraw(arg);
		case AIPlayTagType.HandBonus:
			return new AIBonusArgumentWithIgnoreInBattle(arg);
		case AIPlayTagType.EvoRecoverPp:
			return new AIEvoRecoverPp(arg);
		case AIPlayTagType.PlayptnBaseStatsRate:
			return new AIPlayptnBaseStatsRate(arg);
		case AIPlayTagType.GiveSkill:
			return new AIGiveSkill(arg);
		case AIPlayTagType.PlayDiscard:
		case AIPlayTagType.FanfareDiscard:
			return new AIWhenPlayDiscard(arg);
		case AIPlayTagType.AttackDiscard:
			return new AIAttackDiscard(arg);
		case AIPlayTagType.DiscardDamage:
			return new AIDiscardDamage(arg);
		case AIPlayTagType.BreakHeal:
			return new AIBreakHeal(arg);
		case AIPlayTagType.BuffDamage:
			return new AIBuffDamage(arg);
		case AIPlayTagType.BuffDestroy:
			return new AIBuffDestroy(arg);
		case AIPlayTagType.BuffToken:
			return new AIBuffToken(arg);
		case AIPlayTagType.BuffEvo:
			return new AIBuffEvo(arg);
		case AIPlayTagType.BuffDraw:
			return new AIBuffDraw(arg);
		case AIPlayTagType.BuffShield:
			return new AIBuffShield(arg);
		case AIPlayTagType.EvoDiscard:
			return new AIEvoDiscard(arg);
		case AIPlayTagType.BounceDamage:
			return new AIBounceDamage(arg);
		case AIPlayTagType.ResonanceDamage:
			return new AIResonanceDamage(arg);
		case AIPlayTagType.ResonanceHeal:
			return new AIResonanceHeal(arg);
		case AIPlayTagType.ResonanceKiller:
			return new AIResonanceGiveBasicSkill(arg, AIScriptTokenArgType.KILLER);
		case AIPlayTagType.DiscardHeal:
			return new AIDiscardHeal(arg);
		case AIPlayTagType.AllyDiscardBonus:
			return new AIAllyDiscardBonus(arg);
		case AIPlayTagType.BanishAttachTag:
			return new AIBanishAttachTag(arg);
		case AIPlayTagType.OtherBanishToken:
			return new AIOtherBanishToken(arg);
		case AIPlayTagType.OtherBanishAddCemetery:
			return new AIOtherBanishAddCemetery(arg);
		case AIPlayTagType.EvoChoice:
		case AIPlayTagType.PlayChoice:
		case AIPlayTagType.FanfareChoice:
			return new AIChoiceTagArgument(arg);
		case AIPlayTagType.ChoiceBrave:
			return new AIChoiceBrave();
		case AIPlayTagType.Enhance:
			return new AIEnhance(arg);
		case AIPlayTagType.Accelerate:
			return new AIAccelerate(arg);
		case AIPlayTagType.Crystalize:
			return new AICrystalize(arg);
		case AIPlayTagType.ChoiceTransform:
			return new AIChoiceTransform(arg);
		case AIPlayTagType.PlaySubtractCountdown:
		case AIPlayTagType.FanfareSubtractCountdown:
			return new AIWhenPlaySubtractCountdown(arg);
		case AIPlayTagType.FanfareRecoverAttackableCount:
			return new AIWhenPlayRecoverAttackableCount(arg);
		case AIPlayTagType.FanfareSneak:
		case AIPlayTagType.PlaySneak:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.SNEAK);
		case AIPlayTagType.FanfareQuick:
		case AIPlayTagType.PlayQuick:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.QUICK);
		case AIPlayTagType.FanfareRush:
		case AIPlayTagType.PlayRush:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.RUSH);
		case AIPlayTagType.FanfareGuard:
		case AIPlayTagType.PlayGuard:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.GUARD);
		case AIPlayTagType.FanfareKiller:
		case AIPlayTagType.PlayKiller:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.KILLER);
		case AIPlayTagType.FanfareDrain:
		case AIPlayTagType.PlayDrain:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.DRAIN);
		case AIPlayTagType.FanfareUntouchable:
		case AIPlayTagType.PlayUntouchable:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.UNTOUCHABLE);
		case AIPlayTagType.FanfareForceTargeting:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.FORCE_TARGETING);
		case AIPlayTagType.PlayIgnoreGuard:
		case AIPlayTagType.FanfareIgnoreGuard:
			return new AIWhenPlayKeywordSkill(arg, AIScriptTokenArgType.IGNORE_GUARD);
		case AIPlayTagType.PlaySummonHandCard:
		case AIPlayTagType.FanfareSummonHandCard:
			return new AIWhenPlaySummonHandCard(arg);
		case AIPlayTagType.PlaySelect:
		case AIPlayTagType.FanfareSelect:
			return new AIWhenPlaySelect(arg);
		case AIPlayTagType.PlayHandSelect:
		case AIPlayTagType.FanfareHandSelect:
			return new AIWhenPlayHandSelect(arg);
		case AIPlayTagType.EmoteOnTurnEnd:
			return new AIEmoteOnTurnTransition(arg);
		case AIPlayTagType.LastwordReanimate:
			return new AILastwordReanimate(arg);
		case AIPlayTagType.ModifyHeal:
			return new AIModifyValue(arg);
		case AIPlayTagType.LeaveBonus:
		case AIPlayTagType.DiscardedBonus:
			return new AIBonusArgumentWithIgnoreInBattle(arg);
		case AIPlayTagType.LeaveHeal:
			return new AILeaveHeal(arg);
		case AIPlayTagType.LeaveDamage:
			return new AILeaveDamage(arg);
		case AIPlayTagType.LeaveAttachTag:
			return new AILeaveAttachTag(arg);
		case AIPlayTagType.LeaveBanish:
			return new AILeaveBanish(arg);
		case AIPlayTagType.OtherLeaveDamage:
			return new AIOtherLeaveDamage(arg);
		case AIPlayTagType.OtherLeaveToken:
			return new AIOtherLeaveToken(arg);
		case AIPlayTagType.AllyPlayoutDamageBonus:
			return new AIOtherPlayoutDamageBonus(arg);
		case AIPlayTagType.OtherBreakBonus:
		case AIPlayTagType.OtherBanishBonus:
		case AIPlayTagType.OtherLeaveBonus:
			return new AIOtherBreakBonus(arg);
		case AIPlayTagType.BuffRush:
			return new AIBuffRush(arg);
		case AIPlayTagType.BuffHeal:
			return new AIBuffHeal(arg);
		case AIPlayTagType.BuffBuff:
			return new AIBuffBuff(arg);
		case AIPlayTagType.GetOn:
			return new AIGetOn(arg);
		case AIPlayTagType.GetOnBanish:
			return new AIGetOnBanish(arg);
		case AIPlayTagType.GetOnDamage:
			return new AIGetOnDamage(arg);
		case AIPlayTagType.GetOnEvo:
			return new AIGetOnEvo(arg);
		case AIPlayTagType.GetOffMetamorphose:
			return new AIGetOffMetamorphose(arg);
		case AIPlayTagType.GetOffEvo:
			return new AIGetOffEvo(arg);
		case AIPlayTagType.AfterAttackHeal:
			return new AIAfterAttackHeal(arg);
		case AIPlayTagType.OtherAttackBuff:
			return new AIOtherAttackBuff(arg);
		case AIPlayTagType.OtherAttackDamage:
			return new AIOtherAttackDamage(arg);
		case AIPlayTagType.OtherAttackHeal:
			return new AIOtherAttackHeal(arg);
		case AIPlayTagType.OtherAttackToken:
			return new AIOtherAttackToken(arg);
		case AIPlayTagType.OtherAttackAttachTag:
			return new AIOtherAttackAttachTag(arg);
		case AIPlayTagType.OtherAttackRemoveTag:
			return new AIOtherAttackRemoveTag(arg);
		case AIPlayTagType.OtherEvoBuff:
			return new AIOtherEvoBuff(arg);
		case AIPlayTagType.OtherEvoDamage:
		case AIPlayTagType.SelfAndOtherEvoDamage:
			return new AIOtherEvoDamage(arg);
		case AIPlayTagType.OtherEvoBanish:
			return new AIOtherEvoBanish(arg);
		case AIPlayTagType.OtherEvoSubtractCountdown:
			return new AIOtherEvoSubtractCountdown(arg);
		case AIPlayTagType.OtherEvoEvo:
			return new AIOtherEvoEvo(arg);
		case AIPlayTagType.OtherEvoToken:
		case AIPlayTagType.SelfAndOtherEvoToken:
			return new AIOtherEvoToken(arg);
		case AIPlayTagType.OtherEvoShield:
		case AIPlayTagType.SelfAndOtherEvoShield:
			return new AIOtherEvoShield(arg);
		case AIPlayTagType.SelfAndOtherEvoAttachTag:
			return new AIOtherEvoAttachTag(arg);
		case AIPlayTagType.SelfAndOtherEvoDestroy:
			return new AIOtherEvoDestroy(arg);
		case AIPlayTagType.SelfAndOtherEvoBounce:
			return new AIOtherEvoBounce(arg);
		case AIPlayTagType.SelfAndOtherEvoDraw:
			return new AIOtherEvoDraw(arg);
		case AIPlayTagType.SelfAndOtherEvoAddCemetery:
			return new AIOtherEvoAddCemetery(arg);
		case AIPlayTagType.SelfAndOtherEvoHeal:
			return new AIOtherEvoHeal(arg);
		case AIPlayTagType.SelfAndOtherEvoTokenDraw:
			return new AIOtherEvoTokenDraw(arg);
		case AIPlayTagType.RallyCountPlus:
			return new AIFiltersArgument(arg);
		case AIPlayTagType.TurnEndActivateCount:
			return new AIActivateCountTagArgument(arg);
		case AIPlayTagType.PlayActivateCount:
		case AIPlayTagType.AttackActivateCount:
		case AIPlayTagType.BreakActivateCount:
		case AIPlayTagType.BanishActivateCount:
		case AIPlayTagType.DamagedActivateCount:
		case AIPlayTagType.BuffActivateCounnt:
		case AIPlayTagType.HealActivateCount:
		case AIPlayTagType.NecromanceActivateCount:
		case AIPlayTagType.EvoActivateCount:
		case AIPlayTagType.SummonActivateCount:
			return new AIFilteringActivateCountArgument(arg);
		case AIPlayTagType.SetAITribe:
			return new AISetTribe(arg);
		case AIPlayTagType.GenerateTag:
			return new AIGenerateTag(arg);
		case AIPlayTagType.PlayTokenDraw:
		case AIPlayTagType.FanfareTokenDraw:
			return new AIWhenPlayTokenDraw(arg);
		case AIPlayTagType.PlayChangeClass:
		case AIPlayTagType.FanfareChangeClass:
			return new AIWhenPlayChangeClass(arg);
		case AIPlayTagType.PlayChangeTribe:
		case AIPlayTagType.FanfareChangeTribe:
			return new AIWhenPlayChangeTribe(arg);
		case AIPlayTagType.RemoveByDestroy:
			return new AIRemoveByDestroy(arg);
		case AIPlayTagType.EvoHandBuff:
			return new AIEvoHandBuff(arg);
		case AIPlayTagType.PlayNotBeAttacked:
		case AIPlayTagType.FanfareNotBeAttacked:
			return new AIWhenPlayNotBeAttacked(arg);
		case AIPlayTagType.PlayAttackableCount:
		case AIPlayTagType.FanfareAttackableCount:
			return new AIWhenPlayAttackableCount(arg);
		case AIPlayTagType.PlayRemoveSkill:
		case AIPlayTagType.FanfareRemoveSkill:
			return new AIWhenPlayRemoveSkill(arg);
		case AIPlayTagType.BreakAddStack:
			return new AIBreakAddStack(arg);
		case AIPlayTagType.ChangeInplayImmediateRemoveByBanish:
			return new AIChangeInplayFixRemoveType(arg, FixedRemoveType.RemoveByBanish, isImmediate: true);
		case AIPlayTagType.ChangeInplayImmediateRemoveByDestroy:
			return new AIChangeInplayFixRemoveType(arg, FixedRemoveType.RemoveByDestroy, isImmediate: true);
		case AIPlayTagType.ChangeInplayCannotPlay:
			return new AIChangeInplayCannotPlay(arg, isImmediate: false);
		case AIPlayTagType.ChangeInplayCannotAttack:
			return new AIChangeInplayCannotAttack(arg, isImmediate: false);
		case AIPlayTagType.ChangeInplayAttachTag:
			return new AIChangeInplayAttachTag(arg, isImmediate: false);
		case AIPlayTagType.ChangeInplayImmediateShield:
			return new AIChangeInplayImmediateShield(arg);
		case AIPlayTagType.ChangeInplayImmediateDamageCut:
			return new AIChangeInplayImmediateDamageCut(arg);
		case AIPlayTagType.ChangeInplayImmediateDamageClip:
			return new AIChangeInplayImmediateDamageClip(arg);
		case AIPlayTagType.ChangeInplayImmediateLifeLowerLimit:
			return new AIChangeInplayImmediateLifeLowerLimit(arg);
		case AIPlayTagType.ChangeInplayImmediateDamageModifier:
			return new AIChangeInplayImmediateDamageModifier(arg);
		case AIPlayTagType.ChangeInplayImmediateUntouchable:
			return new AIChangeInplayImmediateKeywordSkill(arg, AIScriptTokenArgType.UNTOUCHABLE);
		case AIPlayTagType.ChangeInplayImmediateIndestructible:
			return new AIChangeInplayImmediateIndestructible(arg);
		case AIPlayTagType.ChangePpTotalBuff:
			return new AIChangePpTotalBuff(arg, isImmediate: false);
		case AIPlayTagType.EvoSetStatus:
			return new AIEvoSetStatus(arg);
		case AIPlayTagType.PlayModifyConsumeEp:
		case AIPlayTagType.FanfareModifyConsumeEp:
			return new AIWhenPlayModifyConsumeEp(arg);
		case AIPlayTagType.PlayEvo:
		case AIPlayTagType.FanfareEvo:
			return new AIWhenPlayEvo(arg);
		case AIPlayTagType.OtherPlayEvo:
			return new AIOtherWhenPlayEvo(arg, PlaySimulationType.Undefined);
		case AIPlayTagType.OtherEnhanceEvo:
			return new AIOtherWhenPlayEvo(arg, PlaySimulationType.Enhance);
		case AIPlayTagType.OtherPlayDamage:
			return new AIOtherWhenPlayDamage(arg);
		case AIPlayTagType.OtherPlayRecoverPp:
			return new AIOtherWhenPlayRecoverPp(arg);
		case AIPlayTagType.OtherPlayDestroy:
			return new AIOtherWhenPlayDestroy(arg);
		case AIPlayTagType.OtherPlayBounce:
			return new AIOtherWhenPlayBounce(arg);
		case AIPlayTagType.OtherPlayBuff:
			return new AIOtherWhenPlayBuff(arg);
		case AIPlayTagType.OtherPlayToken:
			return new AIOtherWhenPlayToken(arg);
		case AIPlayTagType.OtherPlayRemoveTag:
			return new AIOtherWhenPlayRemoveTag(arg);
		case AIPlayTagType.OtherPlayQuick:
			return new AIOtherWhenPlayKeywordSkill(arg, AIScriptTokenArgType.QUICK);
		case AIPlayTagType.AttackSubtractCountdown:
			return new AIAttackSubtractCountdown(arg);
		case AIPlayTagType.EvolveToOther:
			return new AIEvolveToOtherTagArgument(arg);
		case AIPlayTagType.LastwordEvo:
			return new AILastwordEvo(arg);
		case AIPlayTagType.LastwordRemoveSkill:
			return new AILastwordRemoveSkill(arg);
		case AIPlayTagType.LastwordAddCemetery:
			return new AILastwordAddCemetery(arg);
		case AIPlayTagType.LastwordDamageClip:
			return new AILastwordDamageClip(arg);
		case AIPlayTagType.LastwordShield:
			return new AILastwordShield(arg);
		case AIPlayTagType.PlayShield:
		case AIPlayTagType.FanfareShield:
			return new AIWhenPlayShield(arg);
		case AIPlayTagType.PlayDamageCut:
		case AIPlayTagType.FanfareDamageCut:
			return new AIWhenPlayDamageCut(arg);
		case AIPlayTagType.PlayDamageClip:
		case AIPlayTagType.FanfareDamageClip:
			return new AIWhenPlayDamageClip(arg);
		case AIPlayTagType.OtherPlayAttachTag:
			return new AIOtherWhenPlayAttachTag(arg);
		case AIPlayTagType.AttackShield:
		case AIPlayTagType.ClashShield:
			return new AIAttackOrClashShield(arg);
		case AIPlayTagType.AttackDamageClip:
		case AIPlayTagType.ClashDamageClip:
			return new AIAttackOrClashDamageClip(arg);
		case AIPlayTagType.NecromanceDamage:
			return new AINecromanceDamage(arg);
		case AIPlayTagType.NecromanceHeal:
			return new AINecromanceHeal(arg);
		case AIPlayTagType.FanfareRemoveGuard:
			return new AIWhenPlayRemoveKeywordSkill(arg, AIScriptTokenArgType.GUARD);
		case AIPlayTagType.Stack:
			return new AIStack(arg);
		case AIPlayTagType.EvoAddStack:
			return new AIEvoAddStack(arg);
		case AIPlayTagType.FusionMetamorphose:
			return new AIFusionMetamorphose(arg);
		case AIPlayTagType.LastwordSubtractCountdown:
			return new AILastwordSubtractCountdown(arg);
		case AIPlayTagType.AttackSetStatus:
			return new AIAttackSetStatus(arg);
		case AIPlayTagType.PlayAddDeck:
		case AIPlayTagType.FanfareAddDeck:
			return new AIWhenPlayAddDeck(arg);
		case AIPlayTagType.AttackAddDeck:
			return new AIAttackAddDeck(arg);
		default:
			return new AIScriptArgumentExpressions(arg);
		}
	}

	public bool CheckCondition(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		return condExpr.CheckCondition(tagOwner, playPtn, field, situation);
	}

	public float EvalArg(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation, int index = 0)
	{
		return argExpr.EvalArg(index, tagOwner, playPtn, field, situation);
	}

	public int EvalID(int index = 0)
	{
		return argExpr.EvalID(index);
	}

	public bool IsHoldingEVAL()
	{
		if (!ArgumentExpressions.IsHoldingEVAL())
		{
			return condExpr.IsHoldingEVAL();
		}
		return true;
	}

	public bool IsClashTagType()
	{
		if (Type != AIPlayTagType.ClashDestroy && Type != AIPlayTagType.ClashBanish && Type != AIPlayTagType.ClashDamage && Type != AIPlayTagType.ClashHeal && Type != AIPlayTagType.ClashKiller && Type != AIPlayTagType.ClashBuff && Type != AIPlayTagType.ClashRemoveSkill && Type != AIPlayTagType.ClashToken && Type != AIPlayTagType.ClashShield && Type != AIPlayTagType.ClashDamageClip && Type != AIPlayTagType.ClashRemoveTag)
		{
			return Type == AIPlayTagType.ClashSpellboost;
		}
		return true;
	}

	public bool IsAttackTagType()
	{
		if (Type != AIPlayTagType.AttackBuff && Type != AIPlayTagType.AttackDamage && Type != AIPlayTagType.AttackHeal && Type != AIPlayTagType.AttackDestroy && Type != AIPlayTagType.AttackBanish && Type != AIPlayTagType.AttackToken && Type != AIPlayTagType.AttackDiscard && Type != AIPlayTagType.AttackAttackableCount && Type != AIPlayTagType.AttackAttachTag && Type != AIPlayTagType.AttackEvo && Type != AIPlayTagType.AttackSubtractCountdown && Type != AIPlayTagType.AttackKiller && Type != AIPlayTagType.AttackShield && Type != AIPlayTagType.AttackDamageClip && Type != AIPlayTagType.AttackRemoveTag && Type != AIPlayTagType.AttackRemoveSkill && Type != AIPlayTagType.AttackSetStatus && Type != AIPlayTagType.AttackAddDeck)
		{
			return Type == AIPlayTagType.AttackHandBuff;
		}
		return true;
	}

	public bool IsLastwordTag()
	{
		switch (Type)
		{
		case AIPlayTagType.IgnoreBreak:
		case AIPlayTagType.LastwordToken:
		case AIPlayTagType.LastwordBuff:
		case AIPlayTagType.LastwordDestroy:
		case AIPlayTagType.LastwordDamage:
		case AIPlayTagType.LastwordHeal:
		case AIPlayTagType.LastwordMetamorphose:
		case AIPlayTagType.LastwordBanish:
		case AIPlayTagType.LastwordDraw:
		case AIPlayTagType.LastwordSetStatus:
		case AIPlayTagType.Break:
		case AIPlayTagType.LastwordReanimate:
		case AIPlayTagType.LastwordAttachTag:
		case AIPlayTagType.LastwordEvo:
		case AIPlayTagType.LastwordRemoveSkill:
		case AIPlayTagType.LastwordAddCemetery:
		case AIPlayTagType.LastwordDamageClip:
		case AIPlayTagType.LastwordShield:
		case AIPlayTagType.LastwordSubtractCountdown:
			return true;
		default:
			return false;
		}
	}

	public ulong GetHash()
	{
		return (ulong)(_argText.GetHashCode() * 79 + _conditionText.GetHashCode() * 167 + Type.GetHashCode() * 2851);
	}
}

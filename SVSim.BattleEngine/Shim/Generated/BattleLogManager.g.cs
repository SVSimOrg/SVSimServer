// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.UI\BattleLogManager.cs
// TODO(engine-cleanup-pass2): 145 of 159 methods unrun in baseline
//   Type: Wizard.Battle.UI.BattleLogManager
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle.View.Vfx;
namespace Wizard.Battle.UI
{
public partial class BattleLogManager
{
        public partial class WarPair { }
        public partial class CostCardLogInfo { }
        public partial class CardLogInfo { }
        public delegate void FuncSetup(BattleLogItem logItem);
        // PlayerFusionCard / EnemyFusionCard hoisted to BattleManagerBase as instance fields
        // (2026-07-02) so concurrent battles don't alias a process-wide list — see
        // BattleManagerBase.EnemyFusionCard.
        private static BattleLogManager _instance;
        public static BattleLogManager GetInstance() => _instance ??= new BattleLogManager(); // HEADLESS-FIX (M9): non-null singleton so the draw's unguarded BattleLog tail (UpdateFusionedCardSkillDrewCard, and the IsBattleLog AddLogSkillDrawCard calls) no-ops instead of NRE on a null instance
        private BattleLogManager() { }
        public void SetUp(Transform parent, BattleManagerBase battleMgr, OperateMgr operateMgr, BattlePlayer battlePlayer) { }
        public void ClearDestroyedCardList(bool isPlayer) { }
        public void SetActiveShowButton(bool isActive) { }
        public void HideLog() { }
        public void BeginLogBlockEvolution(BattleCardBase card) { }
        public VfxBase EndLogBlockEvolution() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase EndLogBlockFusion() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public void BeginLogBlockPlay(BattleCardBase card) { }
        public void BeginLogAccelerate(BattleCardBase card) { }
        public void BeginLogCrystallize(BattleCardBase card) { }
        public VfxBase EndLogBlockPlay() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase BeginLogBlockTurnChangeReactive() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase EndLogBlockTurnChangeReactive() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase SetupWarActionLog() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase BeginLogBlockWar(BattleCardBase attackCard, BattleCardBase attackedCard) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase EndLogBlockWar(BattleCardBase attackCard, BattleCardBase attackedCard, bool needAttack) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public void AddNecromanceIcon(SkillBase skill, BattleCardBase card, bool isSpell) { }
        public void AddLogDestFollower(BattleLogWindow.BattleLogType type, BattleCardBase card, List<NewReplayBattleMgr.BattleLogTextureInfo> battleLogTextureInfo = null) { }
        public VfxBase AddLogWar(BattleCardBase attackCard, BattleCardBase attackedCard) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public void AddLogFusion(BattleCardBase card, List<BattleCardBase> ingrediens) { }
        public void AddLogSkillGetOn(BattleCardBase card, List<BattleCardBase> ingrediens) { }
        public void AddLogSkillGetOff(SkillBase skill, List<BattleCardBase> ingrediens) { }
        public BattleLogItem AddLogTurn(bool isSelfTurn, int turn = -1) => default!;
        public void AddLogMulliganChanged(BattlePlayerBase player, int changedNum) { }
        public void AddLogSkillDrawCard(List<BattleCardBase> drawCards, SkillBase skill, bool isOpen, bool isPlayerDraw, bool isOverDraw) { }
        public void AddLogSkillDrawToken(List<BattleCardBase> drawCards, SkillBase skill, bool isOpen, bool isOverDraw = false) { }
        public void AddLogOverDrawCards(List<BattleCardBase> overDrawCards) { }
        public void AddLogSkillReturnCard(List<BattleCardBase> returnCards, SkillBase skill) { }
        public void AddLogSkillDiscard(List<BattleCardBase> discardCards, bool isPlayer, SkillBase skill) { }
        public void AddLogSkillBanishHand(List<BattleCardBase> banishCards, bool isPlayer, SkillBase skill) { }
        public void AddLogSkillBanishDeck(List<BattleCardBase> banishCards, SkillBase skill, bool isOpen) { }
        public void AddLogSkillChangeCemetery(int num, SkillBase skill) { }
        public void AddLogSkillClearDestroyedCardList(SkillBase skill, BattlePlayerBase player) { }
        public void AddLogSkillClearSummonedCardList(SkillBase skill, BattlePlayerBase player) { }
        public void AddLogSkillChangeChantCount(int num, BattleCardBase targetCard, SkillBase skill) { }
        public void AddLogSkillChangePP(BattleCardBase targetCard, int changePP, int ppTotalPrev, bool isTotal, SkillBase skill) { }
        public void AddLogSkillSetEP(int ep, BattleCardBase targetCard, SkillBase skill) { }
        public void AddLogSkillGain(List<BattleCardBase> targetCards, SkillBase skill, SkillGainType gainType, int val1 = 0) { }
        public void AddLogSkillAttachSkill(BattleCardBase targetCard, SkillBase attachedSkill, SkillBase skill, bool isTargetInOpponentHand = false) { }
        public void AddLogSkillCantAttack(List<BattleCardBase> targetCards, SkillBase skill, CantAttackType type) { }
        public void AddLogSkillAttackCountRecovery(List<BattleCardBase> targetCards, SkillBase skill) { }
        public void AddLogSkillChangeDeck(BattleCardBase classCard, SkillBase skill) { }
        public void AddLogSkillAddDeck(List<BattleCardBase> addCards, SkillBase skill) { }
        public void AddLogSkillSummon(List<BattleCardBase> summonCards, SkillBase skill) { }
        public void AddLogSkillSummon(List<BattleCardBase> summonCards) { }
        public void AddLogSkillRandomArray(List<BattleCardBase> targetCards, int[] randomArray, SkillBase skill) { }
        public void AddLogSkillBuffSet(List<BattleCardBase> buffCards, int setAttack, int setLife, Skill_power_down skill, bool isTargetInOpponentHand, List<int> beforeAttackList, List<int> beforeLifeList) { }
        public void AddLogSkillBuffAdd(List<BattleCardBase> buffCards, int addAttack, int addLife, SkillBase skill, bool isMinusZeroAttack, bool isMinusZeroLife) { }
        public void AddLogSkillBuffAdd(List<BattleCardBase> buffCards, int addAttack, int addLife, int gainAttack, int gainLife, SkillBase skill) { }
        public void AddLogSkillBuffMultiply(List<BattleCardBase> buffCards, int multiplyAttack, int multiplyLife, SkillBase skill) { }
        public void AddLogSkillBuffAddClass(List<BattleCardBase> buffCards, SkillBase skill) { }
        public void AddLogSkillBuffSetLife(BattleCardBase ownercard, LogType logType, List<BattleCardBase> buffCards, int setLife, bool isTargetInOpponentHand) { }
        public void AddLogSkillBuffSetMaxLife(List<BattleCardBase> buffCards, int setMaxLife, SkillBase skill, List<int> beforeLifeList) { }
        public void AddLogSkillBuffAddMaxLife(List<BattleCardBase> buffCards, int addMaxLife, SkillBase skill) { }
        public void AddLogSkillBuffInHandAdd(List<BattleCardBase> buffCards, int addAttack, int addLife, SkillBase skill, bool isTargetInOpponentHand, bool isTargetSelfOpenCardSkill) { }
        public void AddLogSkillBuffInDeckAdd(BattleCardBase target, int addAttack, int addLife, SkillBase skill) { }
        public void AddLogSkillHeal(List<BattleCardBase> beforeHealCards, List<BattleCardBase.HealResult> healResults, SkillBase skill) { }
        public void AddLogSkillHeal(List<BattleCardBase> beforeHealCards, List<BattleCardBase.HealResult> healResults) { }
        public void AddLogHeal(BattleCardBase beforeHealCard, int healAmount) { }
        public void AddLogSkillDamage(BattleCardBase beforeDamage, BattleCardBase afterDamage, BattleCardBase beforeRefrection, BattleCardBase afterRefrection, SkillBase skill) { }
        public void AddLogSkillDeath(List<BattleCardBase> deathCards, SkillBase skill) { }
        public void AddLogSkillDeath(List<BattleCardBase> deathCards) { }
        public void AddLogDeath(BattleCardBase deathCard) { }
        public void AddLogSkillEvolution(List<BattleCardBase> evolveCards, SkillBase skill) { }
        public void AddLogSkillMetamorphose(List<Skill_metamorphose.MetamorphoseCardPair> pairList, SkillBase skill, bool isTargetInOpponentHand = false, bool isFusion = false) { }
        public void AddLogSkillUnite(Skill_unite.UniteCardPair pair, SkillBase skill) { }
        public void AddLogSkillForceBerserk(BattleCardBase classCard, SkillBase skill) { }
        public void AddLogLose(List<BattleCardBase> cards, SkillBase skill) { }
        public void AddLogPlayAsChoiceTransform(BattleCardBase card) { }
        public void AddLogCopiedSkill(BattleCardBase card, SkillBase skill, bool isRemain) { }
        public void AddLogSkillChangeClan(List<BattleCardBase> cards, CardBasePrm.ClanType newClan, SkillBase skill, bool isTargetInOpponentHand = false) { }
        public void AddLogSkillChangeTribe(List<BattleCardBase> cards, List<CardBasePrm.TribeType> newTribe, SkillBase skill, bool isTargetInOpponentHand = false) { }
        public void AddTokenDrawModifier(List<BattleCardBase> targetCards, SkillBase skill) { }
        public void AddLogSkillChangePlayCount(BattleCardBase card, int count, SkillBase skill) { }
        public void AddLogSkillShortageDeckWin(List<BattleCardBase> cards, SkillBase skill) { }
        public void AddLogCostChange(List<BattleCardBase> cards, SkillBase skill, int cost, bool isSetCost, bool isTargetInOpponentHand, List<int> setCostDifferenceList) { }
        public void AddLogOpenCard(BattleCardBase card) { }
        public void InsertExclusionTargetListLog(SkillBase skill) { }
        public void AddLogSkillUsePp(SkillBase skill, BattleCardBase card, int usePp) { }
        public void AddLogGiveWhiteRitualStack(int num, BattleCardBase targetCard, SkillBase skill) { }
        public void AddLogDepriveWhiteRitualStack(int num, BattleCardBase targetCard, SkillBase skill) { }
        public void UpdateSkillTiming(BattleCardBase card, LogType oldType, LogType newType) { }
        public static int ConvertPremiumIdToNormalId(int cardId) => default!;
        public void AddFusionIngredients(BattleCardBase fusionCard, bool isCreateClone) { }
        public void UpdateFusionedCardSkillDrewCard(BattleCardBase fusionCard) { }
}
}

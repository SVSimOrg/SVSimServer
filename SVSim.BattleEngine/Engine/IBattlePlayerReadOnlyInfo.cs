using System.Collections.Generic;
using Wizard.Battle;

public interface IBattlePlayerReadOnlyInfo
{
	bool IsPlayer { get; }

	bool IsSelfTurn { get; }

	int Turn { get; }

	bool IsGameFirst { get; }

	int PpTotal { get; }

	int Pp { get; }

	int EpTotal { get; }

	int CurrentEpCount { get; }

	int Bp { get; }

	int EvolveWaitTurnCount { get; }

	int GameUsedEpCount { get; }

	int TurnUsedEpCount { get; }

	bool IsShortageDeckLose { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDeckCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoBattleStartDeckCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoHandCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoClassAndInPlayCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoCemeterys { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoBanishCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoFusionIngredientList { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnFusionCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoNecromanceZoneCards { get; }

	IEnumerable<IEnumerable<IReadOnlyBattleCardInfo>> SkillInfoLastTargets { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDiscards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDiscardedCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoFusionIngredientAndDiscardedCards { get; }

	IEnumerable<BattlePlayerBase.TurnAndCard> SkillInfoReturnedCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoHealingCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoSkillSummonedCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoEvolvedCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDestroyedWhenDestroyCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnPlayCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnDrawCards { get; }

	IEnumerable<BattlePlayerBase.CardAndId> SkillInfoTurnDrawTokenCardsWithId { get; }

	IEnumerable<BattlePlayerBase.TurnAndCard> SkillInfoGameSummonCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGamePlayCards { get; }

	IEnumerable<BattlePlayerBase.TurnAndCard> SkillInfoGameTurnPlayCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameCrystallizedPlayCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameSkillActivated { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoInplayMetamorphosedCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameBurialRiteCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnBurialRiteCards { get; }

	IEnumerable<BattlePlayerBase.TurnAndCard> SkillInfoGameReanimatedCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameDrawCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameDrawTokenCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameAddUpdateDeckCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameLeftCards { get; }

	IEnumerable<BattlePlayerBase.TurnAndCard> SkillInfoGameTurnLeftCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameSuperSkyboundArtCards { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameQuickAttackCards { get; }

	List<TurnAndIntValue> TurnPlayCardCountInfo { get; }

	List<TurnAndIntValue> TurnFusionCountInfo { get; }

	int TurnNecromanceCount { get; }

	int GameNecromanceCount { get; }

	int GameUsedPpCount { get; }

	int RallyCount { get; }

	int DeckBanishCount { get; }

	IEnumerable<IReadOnlyBattleCardInfo> SkillInfoInPlayCards { get; }

	IReadOnlyBattleCardInfo SkillInfoClass { get; }

	List<TurnAndIntValue> TurnStartLifeList { get; }

	int GameResonanceStartCount { get; }

	int TurnResonanceStartCount { get; }

	int GameUsedWhiteRitualCount { get; }

	int LastInplayWhiteRitualStack { get; }

	List<TurnAndIntValue> GameSkillReturnCardCountList { get; }

	List<TurnAndIntValue> GameSkillDiscardCountList { get; }

	List<TurnAndIntValue> GameSkillBuffCountList { get; }

	List<TurnAndIntValue> GameSkillMetamorphoseCountList { get; }

	int GetCurrentTurnEvolveCount();

	int GetSpecificTurnEvolveCount(TurnPlayerInfo turnPlayerInfo);

	IEnumerable<IReadOnlyBattleCardInfo> GetSpecificTurnDestroyCards(TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnWhenHealingCount(TurnPlayerInfo turnPlayerInfo, bool isTextKeyword);

	int GetSpecificTurnSkillReturnCardCount(TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnSkillDiscardCount(TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnEnhanceCardCount(TurnPlayerInfo turnPlayerInfo);

	int GetAttachTurnBySkillId(string id);

	int GetCurrentTurnPlayCount();

	int GetSpecificTurnPlayCount(TurnPlayerInfo turnPlayerInfo);
}

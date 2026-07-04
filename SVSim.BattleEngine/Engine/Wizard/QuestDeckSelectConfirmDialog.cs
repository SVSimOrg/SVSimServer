using Cute;

namespace Wizard;

// Post-Phase-5b (2026-07-03) UI stub. QuestDeckSelectConfirmDialog was the quest
// mode deck-select confirmation dialog — DataMgr writes for enemy AI setup +
// BattleControl.BattleEnd + scene change via QuestStartTask. Every path is
// UI-driven and unreachable headless. Class body stubbed to preserve the two
// external entry points (DecideDeck static, referenced by MyPage flows).
public static class QuestDeckSelectConfirmDialog
{
	public static void DecideDeck(DeckData deck, bool isBattleAgain)
	{
		// Pre-Phase-5b: full quest-battle setup via DataMgr + BattleControl. Preserved just
		// the format update since Data.CurrentFormat may still be observed.
		Data.CurrentFormat = deck.Format;
		CardMaster.SetBattleCardMasterId(FormatBehaviorManager.GetDefaultBehaviour(deck.Format).CardMasterId);
	}
}

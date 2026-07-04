using System.Collections;
using Wizard.Battle.Phase;
using Wizard.DeckCardEdit;

namespace Wizard;

public static class FreeAndRankMatchDeckSelectConfirmDialog
{

	public static void DecideDeck(DeckData deck, bool isBattleEnd, bool notBlack = false, bool notCollider = false)
	{
		DeckListUtility.DataMgrSaveLastSelectDeckData(deck);
		ToolboxGame.UIManager.createInSceneLoadingMatching(notBlack, notCollider);

		UIManager.GetInstance().StartCoroutine(ChangeMatchingScene(isBattleEnd));
	}

	private static IEnumerator ChangeMatchingScene(bool isBattleEnd)
	{
		yield return UIManager.GetInstance().StartCoroutine(MasterResetMonthTask.MasterReset());
		// Pre-Phase-5b: gated on mgr.GetCurrentPhase() to trace + BattleControl.BattleEnd
		// on the way out. BattleControl is a stub (chunk 7); collapse to the else branch's
		// UIManager scene change, which is the only observable output.
		UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.RankMatch);
	}
}

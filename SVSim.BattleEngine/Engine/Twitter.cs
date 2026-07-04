using UnityEngine;
using Wizard;

// Post-Phase-5b (2026-07-03) UI stub. Twitter was the deck-share-to-Twitter
// coroutine driver — dialog display, deck-image download, browser-open of an
// intent URL. Nothing headless runs. Kept as a MonoBehaviour type with the one
// externally-called method (TweetDataFromPortal, from UICardList) preserved as
// a no-op so the stub compiles cleanly with the rest of the culled UI cluster.
public class Twitter : MonoBehaviour
{
	public void TweetDataFromPortal(int[] cardIds, ClassSet classSet, GenerateDeckCodeTask.SubmitDeckType submitType, int[] phantomCardIdList, string rotationId)
	{
	}
}

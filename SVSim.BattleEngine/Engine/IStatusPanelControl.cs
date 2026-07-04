using UnityEngine;
using Wizard.Battle.View.Vfx;

public interface IStatusPanelControl
{
	Vector3 EpPanelOffScreenPosition { get; }

	void SetUp(BattleManagerBase battleMgr);

	void ShowStatus(bool isNewReplayMoveTurn);

	void ShowPpEp(bool isNotEPMax3, bool fixDirection = false, bool isNewReplay = false, bool isBanmenkun = false);

	void HideUI();

	void SetDeck(int num);

	void SetGrave(int num);

	void SetHandCount(int num);

	void ShowStatusPanelAlways();

	void HideStatusPanelAlways();

	void ShowStatusPanelOnBattle();

	void SetPp(int num, int max, bool isNewReplayMoveTurn = false);

	void PlayIncreasePpAnimation(int oldPp, int newPp);

	void SetEp(int evo, int cnt);

	VfxBase PlayIncreaseMaxEpAnimation(int oldMaxEp, int newEp);

	VfxBase PlayIncreaseUsableEpAnimation(int oldUsableEpAmount, int amountOfUsableEpGained, int maxEp);

	VfxBase PlayDecreaseUsableEpAnimation(int oldUsableEpAmount, int usedEp);

	GameObject GetPPPanel();

	GameObject GetEPIcon();

	Transform GetClassInfoAnchor();
}

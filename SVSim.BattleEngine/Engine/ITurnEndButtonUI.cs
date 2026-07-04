using UnityEngine;

public interface ITurnEndButtonUI
{
	GameObject GameObject { get; }

	bool _isButtonForcedOff { get; set; }

	bool _isChangeViewLock { get; set; }

	bool GetEnableLabel { get; }

	Vector3 GetBtnPosition();

	void ChangeButtonView(bool isMyTurn);

	void EnableButton();

	void StartTurnEndCountdown();

	void HideBtn();

	GameObject GetTurnEndButton();

	void DisableButton();

	void SettingTimer(float second, bool isRed);

	void HideAnimation();

	void Recovery();

	void ShowBtn(bool canPlayerEndTurnImmediately = false);

	void EnableEndTurnPulsateEffect();

	void DisableEndTurnPulsateEffect();
}

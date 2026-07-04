using UnityEngine;

// Post-Phase-5b (2026-07-03) UI stub. The turn-end button is a UIBase with
// SetActive/sprite/iTween logic driven by BattlePlayer/NetworkBattleManagerBase
// UI paths (ShowBtn/HideBtn/ChangeButtonView/HideAnimation/EnableButton/etc.).
// Nothing headless observes the button state; every method body has been reduced
// to a no-op while the surface stays intact so the ~15 external call sites
// (BattlePlayer, BattlePlayerBase, NetworkBattleManagerBase, PuzzleBattleManager,
// NetworkBattleReceiver, RecoveryManagerBase) still resolve.
public class TurnEndButtonUI : UIBase, ITurnEndButtonUI
{
	public enum ViewType
	{
		Normal,
		Watch	}

	[SerializeField]
	private GameObject BtnMain;

	[SerializeField]
	private UILabel TitleLabel;

	[SerializeField]
	private UIButton TurnEndButtonButton;

	public bool _isButtonForcedOff { get; set; }

	public bool GetEnableLabel => TitleLabel != null && TitleLabel.gameObject.activeSelf;

	public bool _isChangeViewLock { get; set; }

	public GameObject GameObject => base.gameObject;

	public void StartTurnEndCountdown()
	{
	}

	public void ShowBtn(bool canPlayerEndTurnImmediately = false)
	{
	}

	public void HideBtn()
	{
	}

	public void HideAnimation()
	{
	}

	public void ChangeButtonView(bool isMyTurn)
	{
	}

	public void EnableButton()
	{
	}

	public void DisableButton()
	{
	}

	public void EnableEndTurnPulsateEffect()
	{
	}

	public void DisableEndTurnPulsateEffect()
	{
	}

	public Vector3 GetBtnPosition()
	{
		return BtnMain != null ? BtnMain.transform.position : Vector3.zero;
	}

	public void SettingTimer(float second, bool isRed)
	{
	}

	public void Recovery()
	{
	}

	public GameObject GetTurnEndButton()
	{
		return TurnEndButtonButton != null ? TurnEndButtonButton.gameObject : null;
	}
}

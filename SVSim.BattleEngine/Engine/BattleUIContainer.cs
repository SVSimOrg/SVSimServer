using System;
using UnityEngine;
using Wizard;

public class BattleUIContainer : MonoBehaviour
{

	[SerializeField]
	private WizardUIButton BattleMenuBtn;

	[SerializeField]
	private UIButton TurnEndBtn;

	[SerializeField]
	public Transform EnemyChoiceBraveBtn;

	[SerializeField]
	private GameObject _battery;

	public Action<bool> ShowPrediction;

	private bool _enableMenuRequest;

	public bool PlayerCardPlaying;

	private bool _forceDisableMenu;

	public GameObject Battery => _battery;

	public void EnableMenu()
	{
		if (!PlayerCardPlaying && !_forceDisableMenu)
		{
			BattleMenuBtn.isEnabled = true;
			SetEnableReset(isEnable: true);
			SetEnableHint(isEnable: true);
		}
	}

	public void RequestEnableMenuWhenTouchable()
	{
		_enableMenuRequest = true;
	}

	public void ForceEnableMenu()
	{
		_forceDisableMenu = false;
		EnableMenu();
	}

	public void DisableMenu(bool isForceDisable = false)
	{
		// Pre-Phase-5b: gate was `isForceDisable || (!IsRecovery && !IsReplayBattle && !IsWatchBattle)`.
		// IsReplayBattle/IsWatchBattle are const-false in headless (Phase 4); the mgr's IsRecovery
		// state is entirely a UI-hint concern here. Collapsing the branch to `isForceDisable` matches
		// the semantic in the only path that matters headless (button visibility never observed).
		if (isForceDisable)
		{
			BattleMenuBtn.isEnabled = false;
			SetEnableReset(isEnable: false);
			SetEnableHint(isEnable: false);
		}
	}

	public void ForceDisableMenu()
	{
		_forceDisableMenu = true;
		_enableMenuRequest = false;
		DisableMenu();
	}

	public void SetEnableReset(bool isEnable)
	{
		// Puzzle-mode only. Headless never runs PuzzleBattleManager; the branch was dead
		// past the type check, so dropping the ambient reach preserves runtime behavior.
	}

	public void SetEnableHint(bool isEnable)
	{
		// Puzzle-mode only. See SetEnableReset — dropping the ambient reach is a no-op.
	}

	public bool IsEnableMenu()
	{
		return BattleMenuBtn.isEnabled;
	}
}

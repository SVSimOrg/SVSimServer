using System;
using UnityEngine;
using Wizard.Battle.Resource;
using Wizard.Battle.Touch;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 9 of 12 methods unrun in baseline
//   Type: Wizard.Battle.Phase.MainPhase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Phase;

public class MainPhase : IPhase
{
	protected readonly BattleManagerBase _battleManager;

	protected readonly BattlePlayer _battlePlayer;

	protected readonly BattleEnemy _battleEnemy;

	private readonly TouchControl _touchControl;

	protected readonly GameObject _menuButton;

	private readonly IBattleResourceMgr _battleResourceMgr;

	private readonly BattleLogManager _battleLogManager;

	private readonly GameObject _battery;

	protected bool _enableTouch;

	private VfxBase _canNotTouchCardVfx;

	private readonly Func<OperateMgr> _getOperateMgr;

	private OperateMgr OperateMgr => _getOperateMgr();

	public MainPhase(BattleManagerBase battleManager, BattleLogManager logManager)
	{
		_battleManager = battleManager;
		_battlePlayer = battleManager.BattlePlayer;
		_battleEnemy = battleManager.BattleEnemy;
		_touchControl = battleManager.TouchControl;
		_menuButton = battleManager.MenuButtonObject;
		_getOperateMgr = () => battleManager.OperateMgr;
		_battleResourceMgr = battleManager.BattleResourceMgr;
		_battleLogManager = logManager;
		_battery = battleManager.BattleUIContainer.Battery;
	}

	public virtual VfxBase Setup()
	{
		ParallelVfxPlayer parallelVfxPlayer = CreateUpdateBattlePlayersVfx();
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{
			if (!_battleManager.IsBattleEnd)
			{
				if (_menuButton != null)
				{
					_menuButton.SetActive(value: true);
				}
				_battleLogManager.SetActiveShowButton(isActive: true);
			}
		}));
		return SequentialVfxPlayer.Create(parallelVfxPlayer, InstantVfx.Create(delegate
		{
			_enableTouch = true;
		}));
	}

	public VfxWith<IPhase> Update(float dt)
	{
		if (_enableTouch)
		{
			return new VfxWith<IPhase>(_touchControl.Update(dt), null);
		}
		return new VfxWith<IPhase>(NullVfx.GetInstance(), null);
	}

	public virtual VfxBase Teardown()
	{
		_battleManager.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
		_menuButton.SetActive(value: false);
		_battlePlayer.PlayerBattleView.HideTurnEndButton();
		_battleLogManager.HideLog();
		_battleLogManager.SetActiveShowButton(isActive: false);
		OperateMgr.AllClearBattleView();
		_enableTouch = false;
		_battlePlayer.ClassInformationUIController.HideInfomation();
		_battleEnemy.ClassInformationUIController.HideInfomation();
		_battery.SetActive(value: false);
		_battlePlayer.StatusPanelControl.HideStatusPanelAlways();
		_battleEnemy.StatusPanelControl.HideStatusPanelAlways();
		if (_battleManager is NewReplayBattleMgr)
		{
			(_battleManager as NewReplayBattleMgr).SetActiveMoveTurnButton(isActive: false);
		}
		_battleManager.FinishBattle();
		return ParallelVfxPlayer.Create(NullVfx.GetInstance(), NullVfx.GetInstance(), (_touchControl._touchProcessor == null) ? NullVfx.GetInstance() : _touchControl._touchProcessor.End().Vfx, InstantVfx.Create(delegate
		{
			if (UIManager.GetInstance().NowOpenDialog != null)
			{
				UIManager.GetInstance().NowOpenDialog.Close();
			}
		}), InstantVfx.Create(_battlePlayer.PlayerEmotion.CancelShowButtons), _battlePlayer.PlayerEmotion.HideButtons());
	}

	public void Pause()
	{
		bool isSelecting = _battlePlayer.PlayerBattleView.IsSelecting;
		if (isSelecting)
		{
			_battlePlayer.BattleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
			{
				SetTouchable(enable: false);
			}));
		}
		_battlePlayer.PlayerBattleView.DragArrowStop(_battleManager);
		_battlePlayer.PlayerBattleView.ReleaseLockOnTarget();
		BattleCardBase hitCard = _touchControl._hitCard;
		if (hitCard != null && hitCard.IsOnMove)
		{
			_touchControl.StopMovingHandCard(hitCard);
		}
		ResetInput();
		if (isSelecting)
		{
			_battlePlayer.BattleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
			{
				SetTouchable(enable: true);
			}));
		}
	}

	protected virtual void ResetInput()
	{
		_touchControl.Exit();
		_battlePlayer.BattleMgr.VfxMgr.RegisterSequentialVfx(_touchControl.ForceEndTouchProcessor());
		if (_battlePlayer.IsSelfTurn && !_battlePlayer.PlayerBattleView.IsEvolutionVfx)
		{
			_battlePlayer.PlayerBattleView.TurnEndButtonUI.ShowBtn(_battlePlayer.PlayerBattleView.CanPlayerEndTurnImmediately);
		}
	}

	protected ParallelVfxPlayer CreateUpdateBattlePlayersVfx()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(ParallelVfxPlayer.Create(_battlePlayer.CreateUpdateDeckCountLabelVfx(), _battleEnemy.CreateUpdateDeckCountLabelVfx()));
		parallelVfxPlayer.Register(_battlePlayer.StartBattleMainView());
		parallelVfxPlayer.Register(_battleEnemy.StartBattleMainView());
		return parallelVfxPlayer;
	}

	protected virtual void SetTouchable(bool enable)
	{
		if (enable && _canNotTouchCardVfx == null)
		{
			bool isUpdateHandCardsPlayability = true;
			if (_battleManager.GameMgr.IsWatchBattle)
			{
				isUpdateHandCardsPlayability = !(_battleManager as NetworkBattleManagerBase).IsSkillSelectTiming;
			}
			_canNotTouchCardVfx = NullVfx.GetInstance();
		}
		else if (_canNotTouchCardVfx != null)
		{
			_canNotTouchCardVfx = null;
		}
	}
}

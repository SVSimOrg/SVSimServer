using System;
using Cute;
using Wizard;

public class BattleFinishToOpponentDisConnectChecker : NetworkBattleIntervalCheckerBase
{
	private NetworkBattleManagerBase networkBattleManager;

	private DialogBase BattleFinishWaitDialog;

	private SystemText _systemText;

	private int _dispScene;

	private bool _isDisconnect;

	public bool IsStart { get; private set; }

	public event Action OnDisConnectWin;

	public BattleFinishToOpponentDisConnectChecker(NetworkBattleManagerBase manager)
	{
		networkBattleManager = manager;
		_systemText = Data.SystemText;
	}

	public override void StartChecker(string log = "")
	{
		if (BattleFinishWaitDialog == null)
		{
			LocalLog.AccumulateTraceLog("#6911825CreateBattleFinishWaitDialog" + log);
		}
		base.StartChecker();
		IsStart = true;
		CreateBattleFinishWaitDialog();
		BattleFinishWaitDialog.SetActive(inActive: false);
		UIManager.GetInstance().closeInSceneNotNetwork();
	}

	public override void StopChecker()
	{
		base.StopChecker();
		if (BattleFinishWaitDialog != null)
		{
			BattleFinishWaitDialog.Close();
			LocalLog.AccumulateTraceLog("#691182DialogCloseStopChecker");
		}
		UIManager.GetInstance().closeInSceneNotNetwork();
	}

	protected override void IntervalCheck()
	{
		base.IntervalCheck();
		switch (_dispScene)
		{
		case 0:
			if (!networkBattleManager.VfxMgr.IsEnd)
			{
				InitTimer();
			}
			else if (NetworkUtility.GetTimeSpanSecond(base.startTick) >= 8)
			{
				if (networkBattleManager.disconnectToLoseChecker.IsDisconnect())
				{
					ShowDisconnectAlert();
				}
				_dispScene++;
			}
			break;
		case 1:
			if (networkBattleManager.disconnectToLoseChecker.IsDisconnect())
			{
				ShowDisconnectAlert();
				break;
			}
			ShowBattleFinishWaitDialog();
			if (NetworkUtility.GetTimeSpanSecond(base.startTick) <= 98)
			{
				int num = 98 - NetworkUtility.GetTimeSpanSecond(base.startTick);
				BattleFinishWaitDialog.SetText(_systemText.Get("Battle_0425", num.ToString() ?? ""));
			}
			else
			{
				BattleFinishWaitDialog.SetText(_systemText.Get("Battle_0426"));
				_dispScene++;
			}
			break;
		case 2:
			if (NetworkUtility.GetTimeSpanSecond(base.startTick) >= 128)
			{
				BattleFinishWaitDialog.Close();
				LocalLog.AccumulateTraceLog("#691182DialogClose");
				this.OnDisConnectWin.Call();
				StopChecker();
			}
			break;
		}
	}

	private void ShowDisconnectAlert()
	{
		BattleFinishWaitDialog.SetActive(inActive: false);
		if (!_isDisconnect)
		{
			_isDisconnect = true;
			UIManager.GetInstance().closeInSceneNotNetwork();
			networkBattleManager.BattlePlayer.PlayerBattleView.ShowAlert(PanelMgr.BattleAlertType.DisconnectInfomation, isClass: false);
		}
	}

	private void ShowBattleFinishWaitDialog()
	{
		if (!BattleFinishWaitDialog.gameObject.activeSelf)
		{
			LocalLog.AccumulateTraceLog("#691182ShowBattleFinishWaitDialog");
		}
		BattleFinishWaitDialog.SetActive(inActive: true);
		if (_isDisconnect)
		{
			_isDisconnect = false;
			UIManager.GetInstance().createInSceneNotNetwork();
			networkBattleManager.BattlePlayer.PlayerBattleView.OffNotHideAndNotCreate();
			networkBattleManager.BattlePlayer.PlayerBattleView.HideAlertDialogue();
		}
	}

	private void CreateBattleFinishWaitDialog()
	{
		UIManager.GetInstance().closeInSceneNotNetwork();
		UIManager.GetInstance().createInSceneNotNetwork();
		BattleFinishWaitDialog = UIManager.GetInstance().CreateDialogClose();
		BattleFinishWaitDialog.SetTitleLabel(Data.SystemText.Get("Battle_0423"));
		BattleFinishWaitDialog.SetButtonLayout(DialogBase.ButtonLayout.NONE);
		BattleFinishWaitDialog.SetFadeButtonEnabled(flag: false);
		BattleFinishWaitDialog.SetPanelDepth(5000);
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using Cute;
using LitJson;
using Wizard.AutoTest;
using Wizard.ErrorDialog;

namespace Wizard.Battle.Recovery;

public abstract class RecoveryRecordManagerBase : IRecoveryRecordManager
{
	protected OperationRecorderBase _recorder;

	protected GameMgr _gameMgr;

	protected DataMgr.BattleType _battleType;

	protected readonly string _recoveryFilePath;

	protected abstract string DefaultRecoveryFileName { get; }

	public RecoveryRecordManagerBase()
	{
		_recoveryFilePath = OperationRecorderBase.RecordDirectoryPath + DefaultRecoveryFileName;
	}

	public RecoveryRecordManagerBase(string recoveryFilePath)
	{
		_recoveryFilePath = OperationRecorderBase.RecordDirectoryPath + recoveryFilePath;
	}

	public void RecordSkillTarget(IEnumerable<BattleCardBase> targetCards)
	{
		_recorder.RecordSkillTargets(targetCards);
	}

	public virtual void SetupRecording(BattleManagerBase battleMgr, DataMgr.BattleType battleType, int randomSeed, int backGroundId, string bgmId = "NONE")
	{
		Directory.CreateDirectory(OperationRecorderBase.RecordDirectoryPath);
		_gameMgr = null; // Pre-Phase-5b: GameMgr not reachable in ctor headless
		_battleType = battleType;
		_recorder = CreateOperationRecorder();
		SetupRecorderEvents(_recorder, battleMgr);
	}

	public virtual void SetupMulliganStartTimeRecorderEvent(BattleManagerBase battleMgr)
	{
	}

	protected abstract OperationRecorderBase CreateOperationRecorder();

	protected virtual void SetupRecorderEvents(OperationRecorderBase operationRecorder, BattleManagerBase battleMgr)
	{
		battleMgr.OnStartOpening += operationRecorder.RecordStartTurnIsPlayer;
		battleMgr.BattlePlayer.OnMulliganEnd += operationRecorder.RecordPlayerMulliganReplaceCards;
		battleMgr.BattleEnemy.OnMulliganEnd += operationRecorder.RecordEnemyMulliganReplaceCards;
		battleMgr.OperateMgr.OnSetCardSuccess += operationRecorder.RecordPlay;
		battleMgr.OperateMgr.OnBeforeAttack += operationRecorder.RecordAttack;
		battleMgr.OperateMgr.OnEvolveSuccess += operationRecorder.RecordEvolve;
		battleMgr.OperateMgr.OnStartSelect += operationRecorder.RecordStartSelect;
		battleMgr.OperateMgr.OnSelect += operationRecorder.RecordSelect;
		battleMgr.OperateMgr.OnCompleteSelect += operationRecorder.RecordCompleteSelect;
		battleMgr.OperateMgr.OnStartChoice += operationRecorder.RecordStartChoice;
		battleMgr.OperateMgr.OnCompleteChoice += operationRecorder.RecordCompleteChoice;
		battleMgr.OperateMgr.OnCancelSelect += operationRecorder.RecordCancelSelect;
		battleMgr.OperateMgr.OnCancelChoice += operationRecorder.RecordCancelChoice;
		battleMgr.OperateMgr.OnStartFusion += operationRecorder.RecordStartFusion;
		battleMgr.OperateMgr.OnSelectFusionForRecovery += operationRecorder.RecordSelectFusion;
		battleMgr.OperateMgr.OnCancelFusion += operationRecorder.RecordCancelFusion;
		battleMgr.OperateMgr.OnBeforeFusion += operationRecorder.RecordCompleteFusionSelect;
		if (battleMgr is SingleBattleMgr singleBattleMgr && _gameMgr.GetDataMgr().BossRushBattleData != null)
		{
			singleBattleMgr.OnBattleRetire += operationRecorder.RecordRetire;
		}
	}

	public static bool IsExistsSingleRecoveryFile()
	{
		return File.Exists(OperationRecorderBase.RecordDirectoryPath + "recovery_single.json");
	}

	public static void DeleteRecoveryFile()
	{
		if (File.Exists(OperationRecorderBase.RecordDirectoryPath + "recovery_network.json"))
		{
			File.Delete(OperationRecorderBase.RecordDirectoryPath + "recovery_network.json");
		}
		if (File.Exists(OperationRecorderBase.RecordDirectoryPath + "recovery_ai_network.json"))
		{
			File.Delete(OperationRecorderBase.RecordDirectoryPath + "recovery_ai_network.json");
		}
		if (IsExistsSingleRecoveryFile())
		{
			File.Delete(OperationRecorderBase.RecordDirectoryPath + "recovery_single.json");
		}
	}
}

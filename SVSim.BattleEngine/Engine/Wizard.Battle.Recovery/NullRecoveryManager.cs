using System;
using System.Collections;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 18 of 20 methods unrun in baseline
//   Type: Wizard.Battle.Recovery.NullRecoveryManager
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Recovery;

public class NullRecoveryManager : IRecoveryManager
{
	public DataMgr.BattleType BattleType => DataMgr.BattleType.None;

	public bool? DidPlayerGoFirst => null;

	public int RandomSeed => 0;

	public bool HasMulliganInfo => false;

	public int BackGroundId => -1;

	public string BgmId => "NONE";

	public long RecordTime => 0L;

	public int IdxChangeSeed => -1;

	public event Action OnStartRecovery;

	public event Action OnEndDataRecovery;

	public event Action OnEndRecovery;

	public void Setup()
	{
		this.OnStartRecovery.Call();
		this.OnEndDataRecovery.Call();
		this.OnEndRecovery.Call();
	}

	public VfxBase Recovery(BattlePlayer battlePlayer, BattleEnemy battleEnemy, Func<IEnumerator, Coroutine> startCoroutine)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase UpdateRecovery()
	{
		return NullVfx.GetInstance();
	}

	public void RecoveryBeforeMulligan()
	{
	}

	public VfxBase RecoveryMulligan(BattlePlayer battlePlayer, BattleEnemy battleEnemy)
	{
		return NullVfx.GetInstance();
	}

	public string RecoveryPopSkillTargetCardName()
	{
		return string.Empty;
	}
}

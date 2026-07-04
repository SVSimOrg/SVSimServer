using System;
using System.Collections;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Recovery;

public interface IRecoveryManager
{
	DataMgr.BattleType BattleType { get; }

	bool? DidPlayerGoFirst { get; }

	int RandomSeed { get; }

	bool HasMulliganInfo { get; }

	int BackGroundId { get; }

	string BgmId { get; }

	long RecordTime { get; }

	int IdxChangeSeed { get; }

	event Action OnStartRecovery;

	event Action OnEndDataRecovery;

	event Action OnEndRecovery;

	void Setup();

	VfxBase Recovery(BattlePlayer battlePlayer, BattleEnemy battleEnemy, Func<IEnumerator, Coroutine> startCoroutine);

	VfxBase UpdateRecovery();

	void RecoveryBeforeMulligan();

	VfxBase RecoveryMulligan(BattlePlayer battlePlayer, BattleEnemy battleEnemy);

	string RecoveryPopSkillTargetCardName();
}

using Wizard.Battle.Phase;
using Wizard.Battle.Recovery;
using Wizard.Battle.Replay;
using Wizard.Battle.Resource;
using Wizard.Battle.View.Vfx;

namespace Wizard.BattleMgr;

public interface IBattleMgrContentsCreator
{
	int RandomSeed { get; }

	IRecoveryManager RecoveryManager { get; }

	IRecoveryRecordManager RecoveryRecordManager { get; }

	IReplayRecordManager ReplayRecordManager { get; }

	IBattleResourceMgr CreateResourceMgr();

	VfxMgr CreateVfxMgr();

	IPhaseCreator CreatePhaseCreator(BattleManagerBase battleMgr);
}

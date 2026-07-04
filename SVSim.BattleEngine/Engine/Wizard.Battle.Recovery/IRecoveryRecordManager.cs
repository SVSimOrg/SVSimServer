using System.Collections.Generic;

namespace Wizard.Battle.Recovery;

public interface IRecoveryRecordManager
{
	void SetupRecording(BattleManagerBase battleMgr, DataMgr.BattleType battleType, int randomSeed, int backGroundId, string bgmId = "NONE");

	void RecordSkillTarget(IEnumerable<BattleCardBase> targetCards);

	void SetupMulliganStartTimeRecorderEvent(BattleManagerBase battleMgr);
}

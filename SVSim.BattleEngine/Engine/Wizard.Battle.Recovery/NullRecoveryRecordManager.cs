using System.Collections.Generic;

namespace Wizard.Battle.Recovery;

public class NullRecoveryRecordManager : IRecoveryRecordManager
{
	public void SetupRecording(BattleManagerBase battleMgr, DataMgr.BattleType battleType, int randomSeed, int backGroundId, string bgmId = "NONE")
	{
	}

	public void RecordSkillTarget(IEnumerable<BattleCardBase> targetCards)
	{
	}

	public void SetupMulliganStartTimeRecorderEvent(BattleManagerBase battleMgr)
	{
	}
}

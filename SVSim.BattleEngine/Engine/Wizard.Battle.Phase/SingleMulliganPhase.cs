using Wizard.Battle.Mulligan;

namespace Wizard.Battle.Phase;

public class SingleMulliganPhase : MulliganPhaseBase
{
	public SingleMulliganPhase(BattleManagerBase battleMgr)
		: base(battleMgr)
	{
		Initialize(new SingleMulliganMgr());
	}
}

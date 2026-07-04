using Wizard.Battle.UI;
// TODO(engine-cleanup-pass2): 4 of 6 methods unrun in baseline
//   Type: Wizard.Battle.Phase.PhaseCreatorBase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Phase;

public abstract class PhaseCreatorBase : IPhaseCreator
{
	protected readonly BattleManagerBase _battleMgr;

	protected PhaseCreatorBase(BattleManagerBase battleMgr)
	{
		_battleMgr = battleMgr;
	}

	public virtual IPhase CreateFirstPhase()
	{
		return new LoadingPhase(_battleMgr);
	}

	public virtual IPhase CreateOpeningPhase()
	{
		CreateBattleLogManager();
		return new OpeningPhase(_battleMgr);
	}

	public virtual IPhase CreateMulliganPhase()
	{
		return new SingleMulliganPhase(_battleMgr);
	}

	public virtual IPhase CreateMainPhase()
	{
		return new MainPhase(_battleMgr, BattleLogManager.GetInstance());
	}

	public virtual IResultPhase CreateResultPhase(bool winnerIsPlayer)
	{
		return new ResultPhase(_battleMgr, winnerIsPlayer);
	}

	protected BattleLogManager CreateBattleLogManager()
	{
		BattleLogManager instance = BattleLogManager.GetInstance();
		instance.SetUp(_battleMgr.BtlUIContainer.transform, _battleMgr, _battleMgr.OperateMgr, _battleMgr.BattlePlayer);
		return instance;
	}
}

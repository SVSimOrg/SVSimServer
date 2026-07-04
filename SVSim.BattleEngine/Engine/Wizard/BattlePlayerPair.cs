// TODO(engine-cleanup-pass2): 1 of 5 methods unrun in baseline
//   Type: Wizard.BattlePlayerPair
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

namespace Wizard;

public class BattlePlayerPair : BattlePlayerReadOnlyInfoPair
{
	public BattlePlayerBase Self { get; protected set; }

	public BattlePlayerBase Opponent { get; protected set; }

	public BattlePlayerPair(BattlePlayerBase self, BattlePlayerBase opponent)
		: base(self, opponent)
	{
		Self = self;
		Opponent = opponent;
	}

	public BattlePlayerPair VirtualClone(CloneActualFlags cloneFlags)
	{
		BattlePlayerBase battlePlayerBase = Self.CreateVirtualPlayer();
		BattlePlayerBase battlePlayerBase2 = Opponent.CreateVirtualPlayer();
		battlePlayerBase.SetupClone(Self, battlePlayerBase2, cloneFlags);
		battlePlayerBase2.SetupClone(Opponent, battlePlayerBase, cloneFlags);
		battlePlayerBase.Setup(battlePlayerBase2);
		battlePlayerBase2.Setup(battlePlayerBase);
		return new BattlePlayerPair(battlePlayerBase, battlePlayerBase2);
	}
}

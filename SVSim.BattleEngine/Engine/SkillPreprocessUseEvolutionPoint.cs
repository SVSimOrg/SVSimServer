using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessUseEvolutionPoint : SkillPreprocessBase
{
	private readonly int _consumeValue;

	private readonly BattleCardBase _ownerCard;

	public SkillPreprocessUseEvolutionPoint(int count, BattleCardBase ownerCard)
	{
		_consumeValue = count;
		_ownerCard = ownerCard;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (playerInfoPair.ReadOnlySelf.EvolveWaitTurnCount <= 0)
		{
			return playerInfoPair.ReadOnlySelf.CurrentEpCount >= _consumeValue;
		}
		return false;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		int currentEpCount = playerPair.ReadOnlySelf.CurrentEpCount;
		int num = currentEpCount - _consumeValue;
		int epTotal = playerPair.Self.EpTotal;
		playerPair.Self.UseEpCount(_consumeValue);
		playerPair.Self.StartSkillWhenUseEpSelfAndOther(skillProcessor);
		playerPair.Self.CallOnEpModifier(_ownerCard, _consumeValue * -1, isAdd: true);
		if (playerPair.Self.BattleMgr is NetworkStandardBattleMgr networkStandardBattleMgr)
		{
			networkStandardBattleMgr.RegisterUseEpTrigger(playerPair.Self);
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		if (!playerPair.Self.BattleMgr.IsVirtualBattle)
		{
			BattleLogManager.GetInstance().AddLogSkillSetEP(num, playerPair.Self.Class, skill);
		}
		return parallelVfxPlayer;
	}
}

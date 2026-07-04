using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessUsePp : SkillPreprocessBase
{
	private readonly int _consumeValue;

	public SkillPreprocessUsePp(int count)
	{
		_consumeValue = count;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return playerInfoPair.ReadOnlySelf.Pp >= _consumeValue;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		playerPair.Self.Pp -= _consumeValue;
		playerPair.Self.AddGameUsedPp(_consumeValue);
		if (!skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
		{
			BattleLogManager.GetInstance().AddLogSkillUsePp(skill, skill.SkillPrm.ownerCard.SelfBattlePlayer.Class, -_consumeValue);
		}
		playerPair.Self.CallOnAddPp(_consumeValue * -1, skill.SkillPrm.ownerCard);
		return NullVfx.GetInstance();
	}
}

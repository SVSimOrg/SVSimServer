using System.Linq;
using Wizard;
using Wizard.Battle;

public class SkillConditionBurialRite : ISkillConditionChecker
{
	private BattleCardBase _ownerCard;

	private bool _isInvoked;

	public bool judgeFlg { get; private set; }

	public SkillConditionBurialRite(BattleCardBase card, string flg)
	{
		judgeFlg = flg == "true";
		_ownerCard = card;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (_isInvoked)
		{
			return judgeFlg;
		}
		return IsBurialRite(playerInfoPair) == judgeFlg;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (_isInvoked)
		{
			return true;
		}
		return IsRight(playerInfoPair, option);
	}

	private bool IsBurialRite(BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		if (playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Count() <= ((_ownerCard.IsInplay || _ownerCard.IsSpell) ? 4 : 3))
		{
			return playerInfoPair.ReadOnlySelf.SkillInfoHandCards.Any((IReadOnlyBattleCardInfo s) => s != _ownerCard && s != _ownerCard.TransformInfo.OriginalCard && s.IsUnit);
		}
		return false;
	}

	public void SetInvoked()
	{
		_isInvoked = true;
	}
}

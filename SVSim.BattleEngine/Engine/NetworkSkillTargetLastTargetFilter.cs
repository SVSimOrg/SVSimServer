using System.Collections.Generic;
using Wizard.Battle;

public class NetworkSkillTargetLastTargetFilter : SkillTargetLastTargetFilter
{
	private SkillBase _skill;

	private NetworkBattleSetupCardEvent _networkBattleSetupCardEvent;

	public NetworkSkillTargetLastTargetFilter(string option, SkillBase skill)
		: base(option)
	{
		_skill = skill;
		_networkBattleSetupCardEvent = (skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase)._networkBattleSetupCardEventBase;
		if (_skill != null)
		{
			_skill.OnSkillStart -= _networkBattleSetupCardEvent.EventDiscardOrBanishSkillConditionCheck;
			_skill.OnSkillStart += _networkBattleSetupCardEvent.EventDiscardOrBanishSkillConditionCheck;
		}
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (_networkBattleSetupCardEvent.IsSettingUnapprovedCard(_skill) && IsLastTargetDiscardOrBanishSkill(option) && RegisterSkillConditionCheck.CheckLastTargetFilter(_skill))
		{
			(_skill._executionInfoCreator as NetworkExecutionInfoCreator).SetReceiveSkillConditionCheck();
			_skill.OnSkillStart -= _networkBattleSetupCardEvent.ReplaceLastTargetDiscardOrBanishSkillOption;
			_skill.OnSkillStart += _networkBattleSetupCardEvent.ReplaceLastTargetDiscardOrBanishSkillOption;
		}
		return base.Filtering(battlePlayerInfos, option);
	}

	private bool IsLastTargetDiscardOrBanishSkill(SkillConditionCheckerOption option)
	{
		if (option == null)
		{
			return false;
		}
		if (option.SelectedCards.Count < 1)
		{
			return false;
		}
		if (option.SelectedCards[0].SelectSkill is Skill_discard || option.SelectedCards[0].SelectSkill is Skill_banish)
		{
			return true;
		}
		return false;
	}
}

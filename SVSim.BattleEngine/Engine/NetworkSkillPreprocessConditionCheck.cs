using System.Collections.Generic;
using System.Linq;
using Wizard;

public class NetworkSkillPreprocessConditionCheck : SkillPreprocessConditionCheck
{
	private NetworkBattleSetupCardEvent _networkBattleSetupCardEvent;

	public NetworkSkillPreprocessConditionCheck(SkillBase skill, string condition)
		: base(skill, condition)
	{
		_networkBattleSetupCardEvent = (_skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase)._networkBattleSetupCardEventBase;
		if (_skill != null)
		{
			_skill.OnSkillStart -= _networkBattleSetupCardEvent.EventPreprocessConditionCheck;
			_skill.OnSkillStart += _networkBattleSetupCardEvent.EventPreprocessConditionCheck;
		}
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool preexecutionCheck = false)
	{
		if (_networkBattleSetupCardEvent.IsSettingUnapprovedCard(_skill) && RegisterSkillConditionCheck.IsPreprocessConditionCheck(_filter, _skill))
		{
			(_skill._executionInfoCreator as NetworkExecutionInfoCreator).SetReceiveSkillConditionCheck();
			_skill.OnSkillStart -= _networkBattleSetupCardEvent.ReplacePreprocessConditionCheck;
			_skill.OnSkillStart += _networkBattleSetupCardEvent.ReplacePreprocessConditionCheck;
			return true;
		}
		BattleCardBase ownerCard = _skill.SkillPrm.ownerCard;
		GameMgr ins = _skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr;
		if (!ownerCard.IsPlayer && !ins.IsAdminWatch && RegisterFilter.IsFilterPreprocessCondition(_skill) && !_skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsReplayBattle)
		{
			if (preexecutionCheck && _filter.VariableCompareFilter.Any((SkillVariableComareFilter c) => c.Text.Contains(SkillFilterCreator.ContentKeyword.last_target.ToString())))
			{
				List<BattleCardBase> lastTargetCardsList = ownerCard.SelfBattlePlayer.GetLastTargetCardsList(0);
				if (lastTargetCardsList.Count == 0)
				{
					return false;
				}
				if (lastTargetCardsList[0].LastDrawOpenCard == null || lastTargetCardsList[0].LastDrawOpenCard != lastTargetCardsList[0])
				{
					return true;
				}
				bool isSkipPrivateCardCheck = option.IsSkipPrivateCardCheck;
				option.IsSkipPrivateCardCheck = true;
				bool result = base.IsRight(playerInfoPair, option, preexecutionCheck);
				option.IsSkipPrivateCardCheck = isSkipPrivateCardCheck;
				return result;
			}
			return true;
		}
		return base.IsRight(playerInfoPair, option, preexecutionCheck);
	}

	public ConditionSkillFilterCollection GetConditionSkillFilterCollection()
	{
		return _filter;
	}
}

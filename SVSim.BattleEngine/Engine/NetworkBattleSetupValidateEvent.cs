using System.Collections.Generic;

public class NetworkBattleSetupValidateEvent
{
	private NetworkBattleManagerBase _battleMgr;

	private NetworkBattleSetupCardEvent _networkBattleSetupCardEvent;

	public NetworkBattleSetupValidateEvent(NetworkBattleManagerBase battleMgr, NetworkBattleSetupCardEvent cardEvent)
	{
		_battleMgr = battleMgr;
		_networkBattleSetupCardEvent = cardEvent;
	}

	public void SettingPlayerValidateEvent(SkillBase skillData)
	{
		if (_networkBattleSetupCardEvent.IsCheckValidateSkill(skillData))
		{
			skillData.OnSkillStart -= Event_SendValidateCard_MemorySkillIndex;
			skillData.OnSkillStart += Event_SendValidateCard_MemorySkillIndex;
		}
	}

	private void Event_SendValidateCard_MemorySkillIndex(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		NetworkBattleManagerBase battleMgr = _battleMgr;
		foreach (BattleCardBase card in cards)
		{
			battleMgr.AddValidateSkillIndexList(card.Index, card.IsPlayer, NetworkBattleGenericTool.GetSkillIndex(skillBase));
		}
		if (skillBase.SkillPrm.ownerCard.IsHaveBurialRiteJudgeBothFlag && battleMgr.IsValidateSkillIndexListEmpty)
		{
			battleMgr.AddValidateSkillIndexList(-1, isPlayer: true, NetworkBattleGenericTool.GetSkillIndex(skillBase));
		}
	}

	public void OpponentPlayerIncludedValidateSkillToNotPlay(SkillBase skillData)
	{
		if (_networkBattleSetupCardEvent.IsCheckValidateSkill(skillData))
		{
			return;
		}
		bool flag = false;
		int num = 0;
		foreach (SkillBase skill in skillData.SkillPrm.ownerCard.Skills)
		{
			if (_networkBattleSetupCardEvent.IsCheckValidateSkill(skill) && !_battleMgr.GetValidateTargetSkillIndexList().Contains(num))
			{
				flag = true;
				break;
			}
			num++;
		}
		if (!flag)
		{
			return;
		}
		foreach (SkillVariableComareFilter item in skillData.ConditionFilterCollection.VariableCompareFilter)
		{
			foreach (string item2 in (IEnumerable<string>)item.Text.Split('.'))
			{
				if (item2 == SkillFilterCreator.ContentKeyword.hand_other_self.ToString() || item2 == SkillFilterCreator.ContentKeyword.hand_other_oldest.ToString())
				{
					(skillData._executionInfoCreator as NetworkExecutionInfoCreator).SetNotPlaySkill();
					break;
				}
			}
		}
	}
}

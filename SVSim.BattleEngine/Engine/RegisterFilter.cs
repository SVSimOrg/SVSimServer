using System.Collections.Generic;
using System.Linq;

public class RegisterFilter : RegisterTargetBase
{
	private List<BattleCardBase> _targetCardList;

	public List<SkillBase> ExecOrderSkills { get; private set; }

	public bool IsSpellBoost { get; private set; }

	public RegisterFilter(RegisterActionManager registerActionManager, BattleManagerBase mgr, bool isplayer, SkillBase skill, List<BattleCardBase> cardList, bool isStop, SkillConditionCheckerOption option)
		: base(skill, registerActionManager, isplayer, mgr)
	{
		_targetCardList = cardList;
		base.IndexList = new List<int>();
		ExecOrderSkills = new List<SkillBase>();
		if (isStop && (skill.ApplyingTargetFilter is SkillTargetHandFilter || skill.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter))
		{
			foreach (BattleCardBase handCard in mgr.GetBattlePlayer(isplayer).HandCardList)
			{
				base.IndexList.Add(handCard.Index);
			}
		}
		if ((!isStop && (skill.ApplyingTargetFilter is SkillTargetHandFilter || skill.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter)) || skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetHandFilter) || skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetHandOtherSelfFilter))
		{
			FromPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Hand;
		}
		else if (!isStop && (skill.ApplyingTargetFilter is SkillTargetDeckFilter || skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetDeckFilter)))
		{
			FromPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Deck;
		}
		else if (_targetCardList != null)
		{
			foreach (BattleCardBase targetCard in _targetCardList)
			{
				NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(targetCard.SelfBattlePlayer, targetCard.Index);
				if (cardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Hand || cardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Deck)
				{
					base.IndexList.Add(targetCard.Index);
				}
			}
		}
		SettingTargetStatusToSearchSkill(mgr, skill, 0, null, option);
		if (_conditionsList.Count > 1 && skill.ApplyAndFilter.Count > 0)
		{
			for (int num = 0; num < _conditionsList.Count - 1; num++)
			{
				AddGroupIndex();
			}
		}
	}

	public List<RegisterActionBase> AddSettingExecOrder(SkillBase skill, bool isStop)
	{
		ExecOrderSkills.Add(skill);
		RegisterActionBase registerActionBase = null;
		if (skill is Skill_metamorphose)
		{
			Skill_metamorphose skill_metamorphose = skill as Skill_metamorphose;
			bool isFirstOnly = IsFirstOnlyMetamorphoseSkill(skill_metamorphose.ApplyFilterCollection);
			registerActionBase = new RegisterMetamorphoseData(skill_metamorphose.GetMetamorphoseCardId(), -1, IsSelf, skill, isChoice: false, isFirstOnly);
		}
		else if (skill is Skill_cost_change)
		{
			if (!isStop)
			{
			}
		}
		else
		{
			if (skill is Skill_spell_charge)
			{
				Skill_spell_charge skill_spell_charge = skill as Skill_spell_charge;
				RegisterSpellboost registerSpellboost = new RegisterSpellboost(_targetCardList, skill_spell_charge, skill_spell_charge.GetAddBoostCount(), skill_spell_charge.GetDiffBoostCount());
				registerSpellboost.SettingSpellBoostGrope(GroupMsgList[0]);
				IsSpellBoost = true;
				registerActionBase = registerSpellboost;
				return new List<RegisterActionBase> { registerActionBase };
			}
			if (skill is Skill_banish)
			{
				if (_targetCardList.Count() >= 1)
				{
					BattleCardBase battleCardBase = _targetCardList.First();
					NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(battleCardBase.SelfBattlePlayer, battleCardBase.Index);
					List<RegisterActionBase> list = new List<RegisterActionBase>();
					for (int i = 0; i < GroupMsgList.Count; i++)
					{
						RegisterStateChangeCard registerStateChangeCard = new RegisterStateChangeCard(battleCardBase, cardPlaceState, NetworkBattleDefine.NetworkCardPlaceState.Banish, skill);
						SettingGroupIndexMsg(registerStateChangeCard, i);
						list.Add(registerStateChangeCard);
					}
					return list;
				}
			}
			else if (skill is Skill_discard)
			{
				if (_targetCardList.Count() >= 1)
				{
					registerActionBase = new RegisterStateChangeCard(_targetCardList.First(), NetworkBattleDefine.NetworkCardPlaceState.Hand, NetworkBattleDefine.NetworkCardPlaceState.Cemetery, skill);
				}
			}
			else if (skill is Skill_powerup || skill is Skill_change_affiliation || skill is Skill_attach_skill || skill is Skill_attack_by_life)
			{
				List<BattleCardBase> list2 = _targetCardList.ToList().FindAll((BattleCardBase x) => NetworkBattleGenericTool.GetCardPlaceState(x.SelfBattlePlayer, x.Index) == NetworkBattleDefine.NetworkCardPlaceState.Hand || NetworkBattleGenericTool.GetCardPlaceState(x.SelfBattlePlayer, x.Index) == NetworkBattleDefine.NetworkCardPlaceState.Deck);
				if (list2.Count() >= 1)
				{
					registerActionBase = new RegisterAttach(list2, skill);
				}
			}
		}
		if (registerActionBase != null)
		{
			SettingGroupIndexMsg(registerActionBase);
			return new List<RegisterActionBase> { registerActionBase };
		}
		return null;
	}

	public static bool IsNeedFilter(BattleManagerBase mgr, bool isplayer, SkillBase skill, IEnumerable<BattleCardBase> cardList, bool isStop)
	{
		if (!isStop && (skill.ApplyingTargetFilter is SkillTargetHandFilter || skill.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter))
		{
			if (mgr.GetBattlePlayer(isplayer).HandCardList.Count <= 0)
			{
				return false;
			}
			return true;
		}
		if (cardList == null)
		{
			return false;
		}
		foreach (BattleCardBase card in cardList)
		{
			NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(card.SelfBattlePlayer, card.Index);
			if (cardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Hand || cardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Deck)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsDetailCheckSkillDrewCard(SkillBase skill)
	{
		if (skill.ApplyCardFilterList.Any((ISkillCardFilter s) => s is SkillParameterBaseCostFilter))
		{
			return true;
		}
		if (IsFilterPreprocessCondition(skill))
		{
			return true;
		}
		return false;
	}

	private static bool IsPrivateFilter(ISkillTargetFilter filter)
	{
		if (!(filter is SkillTargetHandFilter) && !(filter is SkillTargetHandOtherSelfFilter) && !(filter is SkillTargetInHandCardFilter) && !(filter is SkillTargetDeckFilter) && !(filter is SkillTargetSkillUpdateDeckCardFilter) && !(filter is SkillTargetDeckBanishedCardListFilter))
		{
			return filter is SkillTargetHandBanishedCardListFilter;
		}
		return true;
	}

	public static bool IsFilterCard(SkillBase skill)
	{
		if (skill.ApplyingTargetFilter is SkillTargetSkillDrewCardFilter || (skill.ApplyingTargetFilter is SkillTargetLoadTargetFilter && skill.OnBeforeAttackStart != 0))
		{
			return IsDetailCheckSkillDrewCard(skill);
		}
		bool flag = skill is NetworkSkill_attach_skill && skill.ApplyingTargetFilter is NetworkSkillTargetLastTargetFilter;
		if (skill is NetworkSkill_attach_skill && skill.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyAndFilter.Count; i++)
			{
				flag |= skill.ApplyAndFilter[i].TargetFilter is NetworkSkillTargetLastTargetFilter;
			}
		}
		if (flag)
		{
			SkillBase lastTargetSkillReferenceSkill = NetworkBattleGenericTool.GetLastTargetSkillReferenceSkill(skill);
			if (lastTargetSkillReferenceSkill != null)
			{
				return IsFilterCard(lastTargetSkillReferenceSkill);
			}
		}
		if (skill.ApplyAndFilter.Count > 0)
		{
			for (int j = 0; j < skill.ApplyAndFilter.Count; j++)
			{
				if (!IsPrivateFilter(skill.ApplyAndFilter[j].TargetFilter))
				{
					return false;
				}
				if (skill is Skill_token_draw && skill.ApplyAndFilter[j].TargetFilter is SkillTargetHandOtherSelfFilter)
				{
					return false;
				}
			}
		}
		else if (!IsPrivateFilter(skill.ApplyingTargetFilter) && !(skill.ApplySelectFilter is SkillRandomSelectFilter))
		{
			return false;
		}
		if (RegisterTool.IsSkillRandom(skill))
		{
			return false;
		}
		if (skill.ApplySelectFilter is SkillUserSelectFilter || skill is Skill_select)
		{
			return false;
		}
		if (skill.ApplySelectFilter is SkillSelectIndexFilter)
		{
			return false;
		}
		if (!RegisterTool.IsSkillFilterEffect(skill))
		{
			return false;
		}
		return true;
	}

	public static bool IsFilterPreprocessCondition(SkillBase skill)
	{
		if (skill.PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessConditionCheck) is SkillPreprocessConditionCheck skillPreprocessConditionCheck)
		{
			bool num = skillPreprocessConditionCheck.Contains(SkillFilterCreator.ContentKeyword.last_target.ToString()) || skillPreprocessConditionCheck.Contains(SkillFilterCreator.ContentKeyword.load_target.ToString());
			SkillCollectionBase skills = skill.SkillPrm.ownerCard.Skills;
			int num2 = skills.IndexOf(skill);
			bool flag = num2 > 0 && NetworkBattleGenericTool.IsUnapprovedTarget(skills.ElementAt(num2 - 1));
			return num && flag;
		}
		return false;
	}

	public static bool IsBothTurnFilterSkill(SkillBase skill)
	{
		if (skill.OnWhenDestroyStart != 0)
		{
			return IsFilterCard(skill);
		}
		return false;
	}

	public static bool IsHandAllSelect(SkillBase skill)
	{
		if (!(skill is Skill_attach_skill) && !(skill is Skill_attack_by_life))
		{
			return false;
		}
		if (skill.IsHaveLastTarget)
		{
			return false;
		}
		if (NetworkBattleGenericTool.IsUnapprovedTarget(skill))
		{
			return true;
		}
		return false;
	}

	public static bool IsDeckAllSelect(SkillBase skill)
	{
		if (!(skill is Skill_powerup))
		{
			return false;
		}
		bool flag = skill.ApplyingTargetFilter is SkillTargetDeckFilter;
		if (skill.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyAndFilter.Count; i++)
			{
				flag |= skill.ApplyAndFilter[i].TargetFilter is SkillTargetDeckFilter;
			}
		}
		return flag;
	}

	public static bool IsFilterCardUnapproved(SkillBase skill)
	{
		if (skill.ApplyingTargetFilter is SkillTargetSkillDrewCardFilter)
		{
			return false;
		}
		bool isWatchBattle = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsWatchBattle;
		bool flag = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdmin || (isWatchBattle && skill.SkillPrm.ownerCard.IsPlayer);
		if (skill is Skill_spell_charge || skill is Skill_discard || skill is Skill_destroy || skill is Skill_banish || skill is Skill_powerup || skill is Skill_attach_skill || skill is Skill_update_deck || (skill is Skill_metamorphose && (!flag || !(skill.ApplyingTargetFilter is SkillTargetInHandCardFilter))) || (skill is Skill_change_affiliation && (!isWatchBattle || !skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetHandOtherSelfFilter || f.TargetFilter is SkillTargetDeckFilter))))
		{
			return true;
		}
		return false;
	}

	private bool IsFirstOnlyMetamorphoseSkill(ApplySkillTargetFilterCollection targetFilterCollection)
	{
		List<ApplySkillTargetFilterCollection> applyAndFilter = targetFilterCollection.ApplyAndFilter;
		if (applyAndFilter.Count <= 0)
		{
			ISkillTargetFilter targetFilter = targetFilterCollection.TargetFilter;
			List<ISkillCardFilter> cardFilterList = targetFilterCollection.CardFilterList;
			if (!(targetFilter is SkillTargetInHandCardFilter) && !(targetFilter is SkillTargetSummonedCardFilter) && !(targetFilter is SkillTargetPlayedCardFilter))
			{
				if (targetFilter is SkillTargetSelfFilter)
				{
					return cardFilterList.Any((ISkillCardFilter f) => f is SkillGetOffCardListFilter);
				}
				return false;
			}
			return true;
		}
		for (int num = 0; num < applyAndFilter.Count; num++)
		{
			if (IsFirstOnlyMetamorphoseSkill(applyAndFilter[num]))
			{
				return true;
			}
		}
		return false;
	}
}

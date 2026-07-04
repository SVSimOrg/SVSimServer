using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class ExecutionInfoCreatorBase
{
	protected SkillBase _skill;

	public ExecutionInfoCreatorBase(SkillBase skill)
	{
		_skill = skill;
	}

	public virtual bool IsSkipTargetAiSelect()
	{
		return false;
	}

	public virtual bool CheckCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay, bool isSkipTargetAiSelect = false)
	{
		return _skill.ConditionFilterCollection.Filtering(playerInfoPair, _skill.SkillPrm.ownerCard, option, _skill.OptionValue, isPrePlay, _skill, isSkipTargetAiSelect);
	}

	public virtual bool CheckScanCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		return false;
	}

	public bool VisualCheckCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		return _skill.ConditionFilterCollection.Filtering(playerInfoPair, _skill.SkillPrm.ownerCard, option, _skill.OptionValue, isPrePlay, _skill);
	}

	public virtual IEnumerable<BattleCardBase> CalcApplyTargets(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, ref int targetCount, bool isCheckInHand = false)
	{
		if (_skill.ApplySelectFilter is SkillRandomSelectFilter skillRandomSelectFilter)
		{
			targetCount = skillRandomSelectFilter.CalcCount(_skill.OptionValue);
		}
		if (option.SelectedCards.Count < 0 || !option.SelectedCards.Any((SkillConditionCheckerOption.SkillAndSelectTarget s) => s.SelectSkill == _skill && s.SelectCard != null))
		{
			IEnumerable<BattleCardBase> selectableCards = _skill.GetSelectableCards(playerInfoPair, option);
			return _skill.ApplySelectFilter.Filtering(selectableCards, _skill.OptionValue, option);
		}
		return IfNeededSelectCardCheck(playerInfoPair, option);
	}

	protected List<BattleCardBase> IfNeededSelectCardCheck(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> target = _skill.FilteringByTargetFilter(playerInfoPair, option).ToList();
		List<BattleCardBase> list = new List<BattleCardBase>();
		int i;
		for (i = 0; i < target.Count; i++)
		{
			if (option.SelectedCards.Count > 0 && option.SelectedCards.Any((SkillConditionCheckerOption.SkillAndSelectTarget s) => s.SelectSkill == _skill && s.SelectCard == target[i]))
			{
				list.Add((BattleCardBase)target[i]);
			}
		}
		if (option.SelectedCards.Count == 0)
		{
			return new List<BattleCardBase>();
		}
		return list;
	}

	public virtual VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>> FixedSkillApplyTarget(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, ref int targetCount)
	{
		IEnumerable<BattleCardBase> source = CalcApplyTargets(playerInfoPair, option, ref targetCount);
		VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>> vfxWith = NotIndependentCardFiltering(source.ToList());
		return new VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>>(vfxWith.Vfx, SkillAllowTargetFiltering(vfxWith.Value_1), vfxWith.Value_2);
	}

	public virtual List<BattleCardBase> GetSelectableCards(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isSkipForceSelect = false, List<BattleCardBase> selectedCards = null)
	{
		if (_skill.IsWhenPlaySkill && ((_skill.SkillPrm.ownerCard.IsUnit && _skill.SkillPrm.selfBattlePlayer.Class.SkillApplyInformation.IsCantActivateFanfareUnit) || (_skill.SkillPrm.ownerCard.IsField && _skill.SkillPrm.selfBattlePlayer.Class.SkillApplyInformation.IsCantActivateFanfareField)))
		{
			return new List<BattleCardBase>();
		}
		if (_skill.IsChoiceType)
		{
			IEnumerable<int> enumerable = SkillOptionValue.ParseOptionTokenID(_skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.card_id, "_OPT_NULL_"));
			List<BattleCardBase> list = new List<BattleCardBase>();
			int num = (_skill.SkillPrm.ownerCard.BaseParameter.IsFoil ? 1 : 0);
			{
				foreach (int item2 in enumerable)
				{
					BattleCardBase item = _skill.SkillPrm.selfBattlePlayer.CreateVirtualCard(item2 + num, item2 + num);
					list.Add(item);
				}
				return list;
			}
		}
		List<IReadOnlyBattleCardInfo> list2 = _skill.ApplyFilterCollection.Filtering(playerInfoPair, option, _skill.OptionValue);
		if (selectedCards != null && selectedCards.Count > 0)
		{
			list2 = list2.Where((IReadOnlyBattleCardInfo c) => !selectedCards.Contains(c)).ToList();
		}
		List<IReadOnlyBattleCardInfo> list3 = _skill.FilteringSneakTarget(list2).ToList();
		if (!isSkipForceSelect)
		{
			list3 = _skill.FilteringForceSelectTargets(list3).ToList();
		}
		return list3.Cast<BattleCardBase>().ToList();
	}

	protected virtual VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>> NotIndependentCardFiltering(List<BattleCardBase> cards)
	{
		bool num = _skill is Skill_powerup || _skill is Skill_power_down || _skill is Skill_damage || _skill is Skill_summon_card || _skill is Skill_summon_token || _skill is Skill_token_draw || _skill is Skill_update_deck || _skill is Skill_invoke_skill;
		bool flag = _skill.IsAllResidentTiming && cards.Count == 1 && cards.Contains(_skill.SkillPrm.ownerCard);
		if (num || flag)
		{
			return new VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>>(NullVfx.GetInstance(), cards, new Dictionary<int, BattleCardBase>());
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		Dictionary<int, BattleCardBase> dictionary = new Dictionary<int, BattleCardBase>();
		for (int i = 0; i < cards.Count; i++)
		{
			if (cards[i].SkillApplyInformation.IsIndependent)
			{
				if (!(_skill is Skill_select) && !(_skill is Skill_copy_skill))
				{
					dictionary.Add(i, cards[i]);
					sequentialVfxPlayer.Register(NullVfx.GetInstance());
				}
				else
				{
					dictionary.Add(i, cards[i]);
				}
			}
			else
			{
				list.Add(cards[i]);
			}
		}
		return new VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>>(sequentialVfxPlayer, list, dictionary);
	}

	protected virtual List<BattleCardBase> SkillAllowTargetFiltering(List<BattleCardBase> cards)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		if (!_skill.IsAllowDestroyTarget)
		{
			for (int i = 0; i < cards.Count; i++)
			{
				if (!cards[i].IsDead)
				{
					list.Add(cards[i]);
				}
			}
		}
		else
		{
			list = cards;
		}
		return list;
	}

	protected bool IsSkipTargetSkill(SkillBase skill)
	{
		if (skill.SkillPrm.ownerCard.IsPlayer)
		{
			return false;
		}
		if (!(skill is Skill_powerup) && !(skill is Skill_cost_change))
		{
			return false;
		}
		if (!skill.IsUserSelectType)
		{
			return false;
		}
		if (!(skill.ApplyingTargetFilter is SkillTargetHandFilter) && !(skill.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter) && !skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetHandFilter || f.TargetFilter is SkillTargetHandOtherSelfFilter))
		{
			return false;
		}
		return true;
	}
}

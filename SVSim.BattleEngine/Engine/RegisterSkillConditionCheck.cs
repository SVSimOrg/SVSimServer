using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wizard.Battle;

public class RegisterSkillConditionCheck : RegisterActionBase
{
	public enum SkillConditionType
	{
		NONE,
		count,
		count_check,
		count_compare,
		check_highlander,
		callCount,
		param,
		moved_to_hand_count,
		add_deck_count_check
	}

	public enum SkillConditionParameter
	{
		target,
		state,
		skillTarget,
		condition
	}

	private class ConditionTargetDataListPair
	{
		public ConditionTargetDataList LhsConditionTargetDataList = new ConditionTargetDataList();

		public ConditionTargetDataList RhsConditionTargetDataList = new ConditionTargetDataList();
	}

	private class ConditionTargetDataList
	{
		public List<List<object>> IndexDataList = new List<List<object>>();

		public List<List<object>> ClanDataList = new List<List<object>>();

		public List<List<object>> TribeDataList = new List<List<object>>();

		public List<List<object>> CharaTypeList = new List<List<object>>();

		public List<List<object>> BaseCardIdList = new List<List<object>>();

		public List<List<object>> ExcludeCardIdList = new List<List<object>>();

		public List<List<object>> LibraryTypeList = new List<List<object>>();

		public List<List<object>> DuplicationInfoList = new List<List<object>>();

		public List<List<object>> BaseCostList = new List<List<object>>();

		public bool IsEmpty()
		{
			if (ClanDataList.Count == 0 && TribeDataList.Count == 0 && CharaTypeList.Count == 0 && BaseCardIdList.Count == 0 && ExcludeCardIdList.Count == 0 && LibraryTypeList.Count == 0 && DuplicationInfoList.Count == 0 && IndexDataList.Count == 0)
			{
				return BaseCostList.Count == 0;
			}
			return false;
		}
	}

	private int _skillIndex;

	private string _conditionVal = "";

	private string _conditionCompare = "";

	private int _placeState;

	private bool _containsField;

	private Dictionary<string, string> _conditionList = new Dictionary<string, string>();

	private Dictionary<string, object> _andFilter = new Dictionary<string, object>();

	private ConditionTargetDataListPair _conditionTargetDataListPair = new ConditionTargetDataListPair();

	private bool _isPreprocess;

	private bool _isIncludeSelf;

	private bool _isExcludePlayIdx;

	private static List<SkillFilterCreator.ContentKeyword> DeckKeywordList = new List<SkillFilterCreator.ContentKeyword> { SkillFilterCreator.ContentKeyword.deck };

	private static List<SkillFilterCreator.ContentKeyword> HandKeywordList = new List<SkillFilterCreator.ContentKeyword>
	{
		SkillFilterCreator.ContentKeyword.hand,
		SkillFilterCreator.ContentKeyword.hand_other_self,
		SkillFilterCreator.ContentKeyword.hand_other_oldest
	};

	private static List<SkillFilterCreator.ContentKeyword> PrivateKeywordList = new List<SkillFilterCreator.ContentKeyword>
	{
		SkillFilterCreator.ContentKeyword.unit,
		SkillFilterCreator.ContentKeyword.spell,
		SkillFilterCreator.ContentKeyword.field,
		SkillFilterCreator.ContentKeyword.tribe,
		SkillFilterCreator.ContentKeyword.clan,
		SkillFilterCreator.ContentKeyword.base_card_id,
		SkillFilterCreator.ContentKeyword.ability,
		SkillFilterCreator.ContentKeyword.play_card_type,
		SkillFilterCreator.ContentKeyword.base_cost
	};

	private static List<SkillFilterCreator.ContentKeyword> LastTargetKeywordList = new List<SkillFilterCreator.ContentKeyword> { SkillFilterCreator.ContentKeyword.last_target };

	private static List<SkillFilterCreator.ContentKeyword> PrivateKeywordListInLastTarget = new List<SkillFilterCreator.ContentKeyword>
	{
		SkillFilterCreator.ContentKeyword.tribe,
		SkillFilterCreator.ContentKeyword.base_card_id,
		SkillFilterCreator.ContentKeyword.clan
	};

	private static List<SkillFilterCreator.ContentKeyword> PreprocessConditionKeywordList = new List<SkillFilterCreator.ContentKeyword>
	{
		SkillFilterCreator.ContentKeyword.discarded_card,
		SkillFilterCreator.ContentKeyword.hand_other_self,
		SkillFilterCreator.ContentKeyword.hand_other_oldest,
		SkillFilterCreator.ContentKeyword.deck
	};

	private static List<SkillFilterCreator.ContentKeyword> PrivateKeywordListInPreprocessCondition = new List<SkillFilterCreator.ContentKeyword>
	{
		SkillFilterCreator.ContentKeyword.unit,
		SkillFilterCreator.ContentKeyword.spell,
		SkillFilterCreator.ContentKeyword.field,
		SkillFilterCreator.ContentKeyword.tribe,
		SkillFilterCreator.ContentKeyword.clan,
		SkillFilterCreator.ContentKeyword.base_cost
	};

	public SkillBase Skill { get; private set; }

	public int SkillPlayCardIndex { get; private set; }

	public int SkillPublishedCount { get; private set; }

	public bool IsInvoked { get; private set; }

	public SkillConditionType ConditionType { get; private set; }

	public int SkillTargetId { get; private set; }

	public List<int> TargetCardIndexs { get; private set; }

	private RegisterSkillConditionCheck(int idx, int skillPublishedCount, SkillBase skill, bool isLastTargetDiscard = false, int skillIndex = -1, bool isLastTargetBanish = false)
	{
		IsSelf = skill.SkillPrm.ownerCard.IsPlayer;
		Skill = skill;
		SkillPlayCardIndex = idx;
		SkillPublishedCount = skillPublishedCount;
		IsInvoked = skill.IsInvoked;
		if (IsDeckNumInvestigationSkill(skill))
		{
			_placeState = 0;
		}
		if (IsHandNumInvestigationSkill(skill))
		{
			_placeState = 10;
		}
		if (IsCemeteryInvestigationSkill(skill, isLastTargetDiscard))
		{
			_placeState = 30;
		}
		if (isLastTargetBanish)
		{
			_placeState = 40;
		}
		if (IsGameDrawCardsNumInvestigationSkill(skill) || IsGameAddDeckCardsNumInvestigationSkill(skill) || IsSelectedCardSkillConditionCheck(skill))
		{
			_placeState = 50;
		}
		_containsField = skill.IsRefVariable(SkillFilterCreator.ContentKeyword.inplay_and_hand.ToString());
		_skillIndex = skillIndex;
		_isPreprocess = IsPreprocessConditionCheck(skill) || (DoesSkillUsePrivateCount(skill) && skill.OnSelfTurnEndStart == 0) || IsCemeteryInvestigationSkill(skill, isLastTargetDiscard) || isLastTargetBanish || IsInvokedCheckDeckSkill(skill);
		_isIncludeSelf = skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => f.Text.Contains("hand_self"));
		_isExcludePlayIdx = skill.OnWhenPlayOtherStart != 0 && skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => f.Text.Contains(SkillFilterCreator.ContentKeyword.hand.ToString()) && f.Text.Contains(SkillFilterCreator.ContentKeyword.base_card_id.ToString()));
	}

	private void SetHighlander()
	{
		ConditionType = SkillConditionType.check_highlander;
		int excludeCardId = GetExcludeCardId(Skill);
		if (excludeCardId != -1)
		{
			List<object> list = new List<object>();
			list.Add(excludeCardId);
			_conditionTargetDataListPair.LhsConditionTargetDataList.ExcludeCardIdList.Add(list);
		}
		List<string> excludeTribe = GetExcludeTribe(Skill);
		if (excludeTribe != null)
		{
			List<object> list2 = new List<object>();
			list2.AddRange(excludeTribe);
			_conditionTargetDataListPair.LhsConditionTargetDataList.TribeDataList.Add(list2);
		}
	}

	private void SetPrivateCountCommon(SkillBase skill)
	{
		bool flag = DoesSkillCallCountUseSkillDrewCard(skill);
		if (IsOptionDrew_cardTarget(skill) || flag || IsContainPreprocessLoadOrLastTarget(skill))
		{
			_placeState = 50;
			MakeSkillTargetId(skill);
			TargetCardIndexs = new List<int>();
			IEnumerable<IReadOnlyBattleCardInfo> enumerable;
			if (!IsContainPreprocessLoadTarget(skill))
			{
				enumerable = skill.SkillDrewCards;
			}
			else
			{
				IEnumerable<IReadOnlyBattleCardInfo> enumerable2 = skill.SkillPrm.ownerCard.SkillApplyInformation.LoadTargetList();
				enumerable = enumerable2;
			}
			IEnumerable<IReadOnlyBattleCardInfo> enumerable3 = enumerable;
			if (enumerable3 != null)
			{
				foreach (IReadOnlyBattleCardInfo item in enumerable3)
				{
					TargetCardIndexs.Add(item.Index);
				}
			}
		}
		ConditionType = SkillConditionType.count;
		if (flag)
		{
			ConditionType = SkillConditionType.callCount;
		}
	}

	private void SetOther(SkillBase skill, IEnumerable<BattleCardBase> targetCards, List<SkillConditionCheckerOption.SkillAndSelectTarget> selectCards, bool isLastTargetDiscard = false, bool isLastTargetBanish = false)
	{
		ConditionSkillFilterCollection conditionSkillFilterCollection = skill.ConditionFilterCollection;
		if (conditionSkillFilterCollection.ConditionCheckerFilterList.Count > 0 && conditionSkillFilterCollection.ConditionCheckerFilterList.ElementAt(0) is NetworkSkillPreprocessConditionCheck)
		{
			conditionSkillFilterCollection = (conditionSkillFilterCollection.ConditionCheckerFilterList.ElementAt(0) as NetworkSkillPreprocessConditionCheck).GetConditionSkillFilterCollection();
		}
		MakeConditionData(conditionSkillFilterCollection.VariableCompareFilter, skill);
		for (int i = 0; i < conditionSkillFilterCollection.AnyConditionFilter.Count; i++)
		{
			SkillAnyConditionFilter skillAnyConditionFilter = conditionSkillFilterCollection.AnyConditionFilter[i];
			for (int j = 0; j < skillAnyConditionFilter.Filters.Count; j++)
			{
				MakeConditionData(skillAnyConditionFilter.Filters[j].VariableCompareFilter, skill);
			}
		}
		bool flag = skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => WithdrawnIncludedContentKeyword(x.Text, SkillFilterCreator.ContentKeyword.hand_other_oldest) != "");
		BattleCardBase battleCardBase = skill.SkillPrm.ownerCard.SelfBattlePlayer.HandCardList.FirstOrDefault((BattleCardBase c) => c != skill.SkillPrm.ownerCard);
		if ((IsCemeteryInvestigationSkill(skill, isLastTargetDiscard) || isLastTargetBanish) && targetCards != null)
		{
			List<object> list = new List<object>();
			for (int num = 0; num < targetCards.Count(); num++)
			{
				BattleCardBase battleCardBase2 = targetCards.ElementAt(num);
				list.Add(battleCardBase2.Index);
			}
			_conditionTargetDataListPair.LhsConditionTargetDataList.IndexDataList.Add(list);
		}
		else if (flag && battleCardBase != null)
		{
			List<object> list2 = new List<object>();
			list2.Add(battleCardBase.Index);
			_conditionTargetDataListPair.LhsConditionTargetDataList.IndexDataList.Add(list2);
		}
		else if (IsSelectedCardSkillConditionCheck(skill) && selectCards != null)
		{
			List<object> list3 = new List<object>();
			for (int num2 = 0; num2 < selectCards.Count(); num2++)
			{
				SkillConditionCheckerOption.SkillAndSelectTarget skillAndSelectTarget = selectCards[num2];
				list3.Add(skillAndSelectTarget.SelectCard.Index);
			}
			_conditionTargetDataListPair.LhsConditionTargetDataList.IndexDataList.Add(list3);
		}
	}

	private void MakeConditionData(List<SkillVariableComareFilter> variableCompareFilter, SkillBase skill)
	{
		List<SkillVariableComareFilter> targetFilter = variableCompareFilter.Where((SkillVariableComareFilter f) => IsContainPrivateKeyword(f.Text) || IsContainPrivateKeywordInLastTarget(f.Text) || (IsSelectedCardSkillConditionCheck(skill) && IsContainSelectedTargetKeyword(f.Text))).ToList();
		if (skill.ConditionFilterCollection.ConditionCheckerFilterList.FirstOrDefault() is NetworkSkillPreprocessConditionCheck networkSkillPreprocessConditionCheck)
		{
			targetFilter.AddRange(networkSkillPreprocessConditionCheck.GetConditionSkillFilterCollection().VariableCompareFilter.Where((SkillVariableComareFilter f) => !targetFilter.Contains(f) && IsContainPrivatePreprocessConditionText(f.Text, skill)));
		}
		for (int num = 0; num < targetFilter.Count; num++)
		{
			SkillVariableComareFilter skillVariableComareFilter = targetFilter[num];
			int result = 0;
			bool flag = false;
			if (MakeConditionDataToReturnIsAdd(skillVariableComareFilter.Lhs, isLeft: true, skill))
			{
				flag = true;
			}
			if (int.TryParse(skillVariableComareFilter.Rhs, out result))
			{
				_conditionVal = result.ToString();
				ConditionType = SkillConditionType.count_check;
				if (HasGameDrawCardsText(skillVariableComareFilter.Text))
				{
					ConditionType = SkillConditionType.moved_to_hand_count;
				}
				if (HasGameAddDeckCardsText(skillVariableComareFilter.Text))
				{
					ConditionType = SkillConditionType.add_deck_count_check;
				}
			}
			else
			{
				ConditionType = SkillConditionType.count_compare;
				if (MakeConditionDataToReturnIsAdd(skillVariableComareFilter.Rhs, isLeft: false, skill))
				{
					flag = true;
				}
			}
			if (flag)
			{
				_conditionCompare = skillVariableComareFilter.Compare;
			}
		}
	}

	private static List<RegisterSkillConditionCheck> CreatePrivateCountList(int idx, int skillPublishedCount, SkillBase skill)
	{
		if (skill is Skill_damage)
		{
			RegisterSkillConditionCheck registerSkillConditionCheck = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
			registerSkillConditionCheck.SetPrivateCountCommon(skill);
			string option = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.damage);
			registerSkillConditionCheck.RegisterFromOptionValue(option);
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck };
		}
		if (skill is Skill_powerup)
		{
			string option2 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_offense, "NONE");
			string option3 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_life, "NONE");
			List<RegisterSkillConditionCheck> list = new List<RegisterSkillConditionCheck>();
			if (option2 != "NONE")
			{
				RegisterSkillConditionCheck registerSkillConditionCheck2 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
				registerSkillConditionCheck2.SetPrivateCountCommon(skill);
				registerSkillConditionCheck2.RegisterFromOptionValue(option2);
				list.Add(registerSkillConditionCheck2);
			}
			if (option3 != "NONE")
			{
				RegisterSkillConditionCheck registerSkillConditionCheck3 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
				registerSkillConditionCheck3.SetPrivateCountCommon(skill);
				registerSkillConditionCheck3.RegisterFromOptionValue(option3);
				list.Add(registerSkillConditionCheck3);
			}
			return list;
		}
		if (skill is Skill_summon_token)
		{
			RegisterSkillConditionCheck registerSkillConditionCheck4 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
			registerSkillConditionCheck4.SetPrivateCountCommon(skill);
			string option4 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.repeat_count);
			registerSkillConditionCheck4.RegisterFromOptionValue(option4);
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck4 };
		}
		if (skill is Skill_heal)
		{
			RegisterSkillConditionCheck registerSkillConditionCheck5 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
			registerSkillConditionCheck5.SetPrivateCountCommon(skill);
			string option5 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.healing);
			registerSkillConditionCheck5.RegisterFromOptionValue(option5);
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck5 };
		}
		if (skill is Skill_pp_modifier)
		{
			RegisterSkillConditionCheck registerSkillConditionCheck6 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
			registerSkillConditionCheck6.SetPrivateCountCommon(skill);
			string option6 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_pp);
			registerSkillConditionCheck6.RegisterFromOptionValue(option6);
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck6 };
		}
		if (skill is Skill_chant_count_change)
		{
			RegisterSkillConditionCheck registerSkillConditionCheck7 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
			registerSkillConditionCheck7.SetPrivateCountCommon(skill);
			string option7 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.gain_chant);
			registerSkillConditionCheck7.RegisterFromOptionValue(option7);
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck7 };
		}
		return null;
	}

	private static List<RegisterSkillConditionCheck> CreatePreprocessPrivateCountList(int idx, int skillPublishedCount, SkillBase skill)
	{
		RegisterSkillConditionCheck registerSkillConditionCheck = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
		registerSkillConditionCheck.SetPrivateCountCommon(skill);
		for (int i = 0; i < skill.PreprocessList.Count; i++)
		{
			registerSkillConditionCheck.RegisterFromOptionValue((skill.PreprocessList[i] as SkillPreprocessConditionCheck).ConditionText);
		}
		return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck };
	}

	private static List<RegisterSkillConditionCheck> CreatePrivateParameterList(int idx, int skillPublishedCount, SkillBase skill, List<SkillConditionCheckerOption.SkillAndSelectTarget> selectCards)
	{
		if (skill is Skill_damage)
		{
			RegisterSkillConditionCheck registerSkillConditionCheck = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
			registerSkillConditionCheck.ConditionType = SkillConditionType.param;
			string option = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.damage);
			registerSkillConditionCheck.RegisterConditionFromOptionValue(option);
			registerSkillConditionCheck.RegisterFromOptionValue(option);
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck };
		}
		if (skill is Skill_powerup)
		{
			List<RegisterSkillConditionCheck> list = new List<RegisterSkillConditionCheck>();
			string option2 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_offense, "NONE");
			string option3 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_life, "NONE");
			if (option2 != "NONE")
			{
				RegisterSkillConditionCheck registerSkillConditionCheck2 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
				registerSkillConditionCheck2.ConditionType = SkillConditionType.param;
				string option4 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_offense);
				registerSkillConditionCheck2.RegisterConditionFromOptionValue(option4);
				registerSkillConditionCheck2.RegisterFromOptionValue(option4);
				if (!registerSkillConditionCheck2.RegisterDiscardInfo(selectCards, option4) || selectCards.Count > 0)
				{
					list.Add(registerSkillConditionCheck2);
				}
			}
			if (option3 != "NONE")
			{
				RegisterSkillConditionCheck registerSkillConditionCheck3 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
				registerSkillConditionCheck3.ConditionType = SkillConditionType.param;
				string option5 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_life);
				registerSkillConditionCheck3.RegisterConditionFromOptionValue(option5);
				registerSkillConditionCheck3.RegisterFromOptionValue(option5);
				if (!registerSkillConditionCheck3.RegisterDiscardInfo(selectCards, option5) || selectCards.Count > 0)
				{
					list.Add(registerSkillConditionCheck3);
				}
			}
			return list;
		}
		return null;
	}

	public static List<RegisterSkillConditionCheck> CreateList(int idx, int skillPublishedCount, SkillBase skill, List<SkillConditionCheckerOption.SkillAndSelectTarget> selectCards, List<SkillBase> processSkillList, List<RegisterActionBase> registerDataList, IEnumerable<BattleCardBase> targetCards = null, bool isLastTargetDiscard = false, bool isLastTargetBanish = false)
	{
		if (IsHighlander(skill.ConditionFilterCollection) || IsHighlanderPreprocessConditionCheck(skill))
		{
			if ((from d in registerDataList
				where d is RegisterSkillConditionCheck
				select d as RegisterSkillConditionCheck).Any((RegisterSkillConditionCheck c) => c.SkillPlayCardIndex == idx && c.SkillPublishedCount == skillPublishedCount && c.Skill == skill && c.ConditionType == SkillConditionType.check_highlander))
			{
				return new List<RegisterSkillConditionCheck>();
			}
			RegisterSkillConditionCheck registerSkillConditionCheck = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
			registerSkillConditionCheck.SetHighlander();
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck };
		}
		if (DoesSkillUsePrivateCount(skill) || IsOptionDrew_cardTarget(skill))
		{
			bool num = DoesSkillCallCountUseSkillDrewCard(skill);
			bool flag = IsNotCheckCount(skill);
			if (num)
			{
				RegisterSkillConditionCheck registerSkillConditionCheck2 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill);
				registerSkillConditionCheck2.SetPrivateCountCommon(skill);
				registerSkillConditionCheck2.RegisterFromOptionValue(skill.CallCountText);
				return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck2 };
			}
			if (flag)
			{
				return CreatePrivateParameterList(idx, skillPublishedCount, skill, selectCards);
			}
			return CreatePrivateCountList(idx, skillPublishedCount, skill);
		}
		if (IsContainPreprocessLoadOrLastTarget(skill))
		{
			return CreatePreprocessPrivateCountList(idx, skillPublishedCount, skill);
		}
		if ((RegisterValidate.IsSendOpenMyCardsSkill(skill) && skill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction)) || skill.SkillPrm.ownerCard.BaseParameter.BaseCardId == 120341020)
		{
			int num2 = skill.SkillPrm.ownerCard.Skills.IndexOf(skill);
			num2 += processSkillList.Count((SkillBase s) => RegisterValidate.IsSendOpenMyCardsSkill(s) && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction) && s.SkillPrm.ownerCard == skill.SkillPrm.ownerCard);
			RegisterSkillConditionCheck registerSkillConditionCheck3 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill, isLastTargetDiscard, num2, isLastTargetBanish);
			registerSkillConditionCheck3.SetOther(skill, targetCards, selectCards, isLastTargetDiscard, isLastTargetBanish);
			return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck3 };
		}
		RegisterSkillConditionCheck registerSkillConditionCheck4 = new RegisterSkillConditionCheck(idx, skillPublishedCount, skill, isLastTargetDiscard, -1, isLastTargetBanish);
		registerSkillConditionCheck4.SetOther(skill, targetCards, selectCards, isLastTargetDiscard, isLastTargetBanish);
		return new List<RegisterSkillConditionCheck> { registerSkillConditionCheck4 };
	}

	private void RegisterFromOptionValue(string optionValueText)
	{
		string[] array = optionValueText.Split('.');
		_andFilter = new Dictionary<string, object>();
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			string text2 = "";
			bool flag = text.Contains("!=");
			if (flag)
			{
				text2 = "ne";
			}
			if (text.Contains(SkillFilterCreator.ContentKeyword.tribe.ToString()))
			{
				List<object> list = new List<object>();
				if (_andFilter.ContainsKey(SkillFilterCreator.ContentKeyword.tribe.ToString()))
				{
					List<object> list2 = _andFilter[SkillFilterCreator.ContentKeyword.tribe.ToString()] as List<object>;
					for (int j = 0; j < list2.Count; j++)
					{
						list.Add(list2[j]);
					}
					_andFilter.Remove(SkillFilterCreator.ContentKeyword.tribe.ToString());
				}
				List<CardBasePrm.TribeType> list3 = CardBasePrm.CreateTribeTypeList(text, isTribeCheck: true, flag);
				for (int k = 0; k < list3.Count; k++)
				{
					int num = (int)list3[k];
					if (flag)
					{
						list.Add(text2 + num);
					}
					else
					{
						list.Add(num.ToString());
					}
				}
				_andFilter.Add(ActionBaseParameter.tribe.ToString(), list);
			}
			if (text.Contains(SkillFilterCreator.ContentKeyword.clan.ToString()))
			{
				text = text.Replace("self", "");
				int clanType = (int)CardBasePrm.GetClanType(text);
				object value = ((!flag) ? ((object)clanType) : (text2 + clanType));
				_andFilter.Add(ActionBaseParameter.clan.ToString(), value);
			}
			if (text.Contains(SkillFilterCreator.ContentKeyword.spell_and_field.ToString()))
			{
				object value2 = ((!flag) ? ("ne" + 1) : ((object)1));
				_andFilter.Add(ActionBaseParameter.charType.ToString(), value2);
			}
			else if (text.Contains(SkillFilterCreator.ContentKeyword.unit.ToString()))
			{
				object obj = null;
				obj = ((!flag) ? ((object)1) : (text2 + 1));
				_andFilter.Add(ActionBaseParameter.charType.ToString(), obj);
			}
			else if (text.Contains(SkillFilterCreator.ContentKeyword.field.ToString()))
			{
				List<object> list4 = new List<object>();
				if (flag)
				{
					list4.Add(text2 + 2);
					list4.Add(text2 + 3);
				}
				else
				{
					list4.Add(2);
					list4.Add(3);
				}
				_andFilter.Add(ActionBaseParameter.charType.ToString(), list4);
			}
			else if (text.Contains(SkillFilterCreator.ContentKeyword.spell.ToString()))
			{
				object obj2 = null;
				obj2 = ((!flag) ? ((object)4) : (text2 + 4));
				_andFilter.Add(ActionBaseParameter.charType.ToString(), obj2);
			}
			if (text.Contains(SkillFilterCreator.ContentKeyword.base_card_id.ToString()))
			{
				int num2 = int.Parse(Regex.Match(text, "\\d+").Value);
				object value3 = ((!flag) ? ((object)num2) : (text2 + num2));
				_andFilter.Add(ActionBaseParameter.baseCardId.ToString(), value3);
			}
			if (text.Contains(SkillFilterCreator.ContentKeyword.cost.ToString()) && !text.Contains(SkillFilterCreator.ContentKeyword.base_cost.ToString()))
			{
				int num3 = int.Parse(Regex.Match(text, "\\d+").Value);
				if (text.Contains(">="))
				{
					text2 = "ge";
				}
				object value4 = text2 + num3;
				_andFilter.Add(ActionBaseParameter.cost.ToString(), value4);
			}
			if (text.Contains(SkillFilterCreator.ContentKeyword.base_cost.ToString()) && (text.Contains(">=") || text.Contains("<=")))
			{
				int num4 = int.Parse(Regex.Match(text, "\\d+").Value);
				bool num5 = text.Contains(">=");
				bool flag2 = text.Contains("<=");
				if (num5)
				{
					text2 = "ge";
				}
				else if (flag2)
				{
					text2 = "le";
				}
				object value5 = text2 + num4;
				_andFilter.Add(ActionBaseParameter.baseCost.ToString(), value5);
			}
		}
	}

	public bool RegisterDiscardInfo(List<SkillConditionCheckerOption.SkillAndSelectTarget> selectCards, string optionValueText)
	{
		string[] array = optionValueText.Split('.');
		if (_andFilter == null)
		{
			_andFilter = new Dictionary<string, object>();
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].Contains(SkillFilterCreator.ContentKeyword.discarded_card.ToString()))
			{
				continue;
			}
			for (int j = 0; j < selectCards.Count; j++)
			{
				if (selectCards[j].SelectSkill is Skill_discard)
				{
					_andFilter.Add(ActionBaseParameter.idx.ToString(), selectCards[j].SelectCard.Index);
					break;
				}
			}
			return true;
		}
		return false;
	}

	private void RegisterConditionFromOptionValue(string optionValueText)
	{
		if (optionValueText.Contains(SkillFilterCreator.ContentKeyword.offense.ToString()))
		{
			string key = ActionBaseParameter.atk.ToString();
			if (optionValueText.Contains(SkillFilterCreator.ContentKeyword.max.ToString()))
			{
				_conditionList.Add(key, SkillFilterCreator.ContentKeyword.max.ToString());
			}
		}
		else if (optionValueText.Contains(SkillFilterCreator.ContentKeyword.base_cost.ToString()))
		{
			string key2 = ActionBaseParameter.baseCost.ToString();
			_conditionList.Add(key2, SkillFilterCreator.ContentKeyword.max.ToString());
		}
	}

	private bool MakeConditionDataToReturnIsAdd(string data, bool isLeft, SkillBase skill)
	{
		ConditionTargetDataList conditionTargetDataList = (isLeft ? _conditionTargetDataListPair.LhsConditionTargetDataList : _conditionTargetDataListPair.RhsConditionTargetDataList);
		bool flag = false;
		string text = WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.clan, isContains: true);
		if (text != "")
		{
			List<object> list = new List<object>();
			list.Add((int)CardBasePrm.GetClanType(text));
			conditionTargetDataList.ClanDataList.Add(list);
			flag = true;
		}
		if (WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.tribe, isContains: true) != "")
		{
			List<object> list2 = new List<object>();
			bool flag2 = data.Contains(SkillFilterCreator.ContentKeyword.tribe.ToString() + "!=");
			List<CardBasePrm.TribeType> list3 = CardBasePrm.CreateTribeTypeList(data, isTribeCheck: true, flag2);
			for (int i = 0; i < list3.Count; i++)
			{
				int num = (int)list3[i];
				if (flag2)
				{
					list2.Add("ne" + num);
				}
				else
				{
					list2.Add(num);
				}
			}
			conditionTargetDataList.TribeDataList.Add(list2);
			flag = true;
		}
		List<object> list4 = new List<object>();
		if (!list4.Contains(2) && (WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_self, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_oldest, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.discarded_card, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.selected_cards, isContains: true) != "") && WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.field, isContains: true) != "")
		{
			list4.Add(2);
			list4.Add(3);
			conditionTargetDataList.CharaTypeList.Add(list4);
			flag = true;
		}
		if (!list4.Contains(1) && (WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_self, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_oldest, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.deck, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.discarded_card, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.selected_cards, isContains: true) != "") && WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.unit, isContains: true) != "")
		{
			list4.Add(1);
			conditionTargetDataList.CharaTypeList.Add(list4);
			flag = true;
		}
		if (!list4.Contains(4))
		{
			string text2 = data.Replace("spell_charge", "");
			if ((WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_self, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_oldest, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.discarded_card, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.selected_cards, isContains: true) != "") && WithdrawnIncludedContentKeyword(text2, SkillFilterCreator.ContentKeyword.spell, isContains: true) != "")
			{
				list4.Add(4);
				conditionTargetDataList.CharaTypeList.Add(list4);
				flag = true;
			}
		}
		string text3 = WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.play_card_type, isContains: true);
		if ((WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_self, isContains: true) != "" || WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.hand_other_oldest, isContains: true) != "") && text3 != "")
		{
			string[] array = text3.Split('=');
			string option = ((array.Length > 1) ? array[1] : "");
			IEnumerable<IReadOnlyBattleCardInfo> playCardList = new SkillPlayCardTypeFilter(skill.SkillPrm.ownerCard, option).GetPlayCardList();
			bool flag3 = playCardList.Any((IReadOnlyBattleCardInfo c) => c.IsUnit);
			bool flag4 = playCardList.Any((IReadOnlyBattleCardInfo c) => c.IsSpell);
			bool flag5 = playCardList.Any((IReadOnlyBattleCardInfo c) => c.IsField || c.IsChantField);
			if (!list4.Contains(1) && flag3)
			{
				list4.Add(1);
				conditionTargetDataList.CharaTypeList.Add(list4);
				flag = true;
			}
			if (!list4.Contains(4) && flag4)
			{
				list4.Add(4);
				conditionTargetDataList.CharaTypeList.Add(list4);
				flag = true;
			}
			if (!list4.Contains(2) && flag5)
			{
				list4.Add(2);
				list4.Add(3);
				conditionTargetDataList.CharaTypeList.Add(list4);
				flag = true;
			}
		}
		string text4 = WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.spell_charge, isContains: true);
		if (text4 != "")
		{
			List<object> list5 = new List<object>();
			list5.Add(text4.Contains("!=") ? 2004 : 1003);
			conditionTargetDataList.LibraryTypeList.Add(list5);
			flag = true;
		}
		if (WithdrawnIncludedContentKeyword(data, SkillFilterCreator.ContentKeyword.unique_base_card_id_card, isContains: true) != "")
		{
			List<object> list6 = new List<object>();
			list6.Add("cardId");
			conditionTargetDataList.DuplicationInfoList.Add(list6);
			flag = true;
		}
		string text5 = WithdrawnIncludedContentKeyword(data, new List<SkillFilterCreator.ContentKeyword> { SkillFilterCreator.ContentKeyword.base_card_id }, isContains: true);
		if (text5 != "")
		{
			string text6 = text5;
			if (text6.Contains("!="))
			{
				flag = true;
				text6 = text6.Replace(SkillFilterCreator.ContentKeyword.base_card_id.ToString() + "!=", "");
				if (flag)
				{
					List<object> list7 = new List<object>();
					list7.Add(int.Parse(text6));
					conditionTargetDataList.ExcludeCardIdList.Add(list7);
				}
			}
			else if (text6.Contains("="))
			{
				flag = true;
				text6 = text6.Replace(SkillFilterCreator.ContentKeyword.base_card_id.ToString() + "=", "");
				if (flag)
				{
					List<object> list8 = new List<object>();
					list8.Add(int.Parse(text6));
					conditionTargetDataList.BaseCardIdList.Add(list8);
				}
			}
		}
		string text7 = WithdrawnIncludedContentKeyword(data, new List<SkillFilterCreator.ContentKeyword> { SkillFilterCreator.ContentKeyword.base_cost }, isContains: true);
		if (text7 != string.Empty)
		{
			if (text7.Contains(">="))
			{
				flag = true;
				text7 = RegisterTool.MakeParameterOptionText(">=") + text7.Replace(SkillFilterCreator.ContentKeyword.base_cost.ToString() + ">=", "");
				conditionTargetDataList.BaseCostList.Add(new List<object> { text7 });
			}
			else if (text7.Contains("<="))
			{
				flag = true;
				text7 = RegisterTool.MakeParameterOptionText("<=") + text7.Replace(SkillFilterCreator.ContentKeyword.base_cost.ToString() + "<=", "");
				conditionTargetDataList.BaseCostList.Add(new List<object> { text7 });
			}
			else if (text7.Contains("="))
			{
				flag = true;
				text7 = text7.Replace(SkillFilterCreator.ContentKeyword.base_cost.ToString() + "=", "");
				if (conditionTargetDataList.BaseCostList.Count == 0)
				{
					conditionTargetDataList.BaseCostList.Add(new List<object> { int.Parse(text7) });
				}
				else
				{
					conditionTargetDataList.BaseCostList.LastOrDefault().Add(int.Parse(text7));
				}
			}
		}
		return flag;
	}

	private void MakeSkillTargetId(SkillBase skill)
	{
		BattleCardBase ownerCard = skill.SkillPrm.ownerCard;
		int num = (ownerCard.IsPlayer ? 1 : 0);
		int index = ownerCard.Index;
		int num2 = skill.SkillPrm.ownerCard.Skills.ToList().IndexOf(skill);
		NetworkExecutionInfoCreator networkExecutionInfoCreator = skill._executionInfoCreator as NetworkExecutionInfoCreator;
		int num3 = 0;
		if (networkExecutionInfoCreator != null)
		{
			num3 = networkExecutionInfoCreator.GetSkillMovementNum();
		}
		SkillTargetId = num * 10000 + index * 100 + num2 * 10 + num3;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(ActionBaseParameter.idx.ToString(), SkillPlayCardIndex);
		int num = Skill.SkillPrm.ownerCard.Skills.IndexOf(Skill);
		if (num == -1)
		{
			num = Skill.SkillPrm.ownerCard.NormalSkills.IndexOf(Skill);
		}
		if (_skillIndex != -1)
		{
			num = _skillIndex;
		}
		dictionary.Add(ActionBaseParameter.skillIdx.ToString(), num);
		dictionary.Add(ActionBaseParameter.skillCount.ToString(), SkillPublishedCount);
		dictionary.Add(ActionBaseParameter.type.ToString(), ConditionType.ToString());
		int expectCount = ((NetworkStandardBattleMgr)Skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr).GetExpectCount(SkillPublishedCount);
		if (!IsSelf && expectCount != -1)
		{
			dictionary.Add(ActionBaseParameter.expect.ToString(), expectCount);
		}
		List<Dictionary<string, object>> targetDataList = new List<Dictionary<string, object>>();
		if (_andFilter.Count == 0)
		{
			if (!_conditionTargetDataListPair.LhsConditionTargetDataList.IsEmpty())
			{
				targetDataList.Add(MakeTargetData(_conditionTargetDataListPair.LhsConditionTargetDataList));
			}
			if (!_conditionTargetDataListPair.RhsConditionTargetDataList.IsEmpty())
			{
				targetDataList.Add(MakeTargetData(_conditionTargetDataListPair.RhsConditionTargetDataList));
			}
			if (targetDataList.Count == 0 && ConditionType == SkillConditionType.check_highlander)
			{
				SettingTargetStateOnCheckHighlander(ref targetDataList);
			}
		}
		else
		{
			SettingAndTargetDataToList(ref targetDataList, _andFilter);
		}
		if (targetDataList.Count >= 1)
		{
			dictionary.Add(SkillConditionParameter.target.ToString(), targetDataList);
		}
		if (_conditionCompare != "" || _conditionVal != "")
		{
			dictionary.Add(SkillConditionParameter.condition.ToString(), RegisterTool.MakeParameterOptionText(_conditionCompare) + _conditionVal);
		}
		if (_conditionList.Count > 0)
		{
			dictionary.Add(SkillConditionParameter.condition.ToString(), _conditionList);
		}
		if (IsInvoked)
		{
			dictionary.Add(ActionBaseParameter.isInvoke.ToString(), 1);
		}
		if (_isPreprocess)
		{
			dictionary.Add(ActionBaseParameter.isPreprocess.ToString(), 1);
		}
		if (_isIncludeSelf)
		{
			dictionary.Add(ActionBaseParameter.isIncludeSelf.ToString(), 1);
		}
		if (_isExcludePlayIdx)
		{
			dictionary.Add(ActionBaseParameter.isExcludePlayIdx.ToString(), 1);
		}
		return dictionary;
	}

	private Dictionary<string, object> MakeTargetData(ConditionTargetDataList conditionTargetDataList)
	{
		List<Dictionary<string, object>> targetDataList = new List<Dictionary<string, object>>();
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.ClanDataList, ActionBaseParameter.clan);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.TribeDataList, ActionBaseParameter.tribe);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.CharaTypeList, ActionBaseParameter.charType);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.BaseCardIdList, ActionBaseParameter.baseCardId);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.ExcludeCardIdList, ActionBaseParameter.excludeList);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.LibraryTypeList, ActionBaseParameter.libraryType);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.DuplicationInfoList, ActionBaseParameter.duplication);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.IndexDataList, ActionBaseParameter.idx);
		SettingTargetDataToList(ref targetDataList, conditionTargetDataList.BaseCostList, ActionBaseParameter.baseCost);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		for (int i = 0; i < targetDataList.Count; i++)
		{
			dictionary = (from c in dictionary.Concat(targetDataList[i])
				group c by c.Key).ToDictionary((IGrouping<string, KeyValuePair<string, object>> c) => c.Key, (IGrouping<string, KeyValuePair<string, object>> c) => c.FirstOrDefault().Value);
		}
		return dictionary;
	}

	private void SettingTargetDataToList(ref List<Dictionary<string, object>> targetDataList, List<List<object>> dataList, ActionBaseParameter actionBaseParameter)
	{
		if (dataList.Count == 0)
		{
			return;
		}
		foreach (List<object> data in dataList)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add(actionBaseParameter.ToString(), data);
			if (_placeState != 50)
			{
				dictionary.Add(SkillConditionParameter.state.ToString(), _placeState);
			}
			if (SkillTargetId != 0)
			{
				dictionary.Add(SkillConditionParameter.skillTarget.ToString(), SkillTargetId.ToString());
			}
			targetDataList.Add(dictionary);
		}
	}

	private void SettingAndTargetDataToList(ref List<Dictionary<string, object>> targetDataList, Dictionary<string, object> dataList)
	{
		if (dataList.Count == 0)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (string key in dataList.Keys)
		{
			if (typeof(List<object>).IsAssignableFrom(dataList[key].GetType()))
			{
				dictionary.Add(key, dataList[key]);
				continue;
			}
			dictionary.Add(key, new List<object> { dataList[key] });
		}
		if (_placeState != 50)
		{
			if (_containsField)
			{
				List<int> list = new List<int> { _placeState };
				list.Add(20);
				dictionary.Add(SkillConditionParameter.state.ToString(), list);
			}
			else
			{
				dictionary.Add(SkillConditionParameter.state.ToString(), _placeState);
			}
		}
		if (SkillTargetId != 0)
		{
			dictionary.Add(SkillConditionParameter.skillTarget.ToString(), SkillTargetId.ToString());
		}
		targetDataList.Add(dictionary);
	}

	private void SettingTargetStateOnCheckHighlander(ref List<Dictionary<string, object>> targetDataList)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(SkillConditionParameter.state.ToString(), _placeState);
		targetDataList.Add(dictionary);
	}

	public static bool IsSkillConditionCheck(SkillBase skill, bool isNotHandCheck = false, bool isNotDeckCheck = false)
	{
		if (skill.IsDeckSelfSkill || RegisterValidate.IsValidateCard(skill))
		{
			return false;
		}
		if (skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => IsContainPrivateKeyword(f.Text, isNotHandCheck, isNotDeckCheck)))
		{
			return true;
		}
		if (skill.ConditionFilterCollection.AnyConditionFilter.Any((SkillAnyConditionFilter f) => IsContainPrivateKeyword(f.Text, isNotHandCheck, isNotDeckCheck)))
		{
			return true;
		}
		if (!DoesSkillUsePrivateCount(skill, isNotHandCheck, isNotDeckCheck))
		{
			return false;
		}
		return true;
	}

	public static bool IsSelectedCardSkillConditionCheck(SkillBase skill)
	{
		if ((skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => IsContainSelectedTargetKeyword(f.Text)) || skill.ConditionFilterCollection.AnyConditionFilter.Any((SkillAnyConditionFilter f) => IsContainSelectedTargetKeyword(f.Text))) && (skill.SkillPrm.ownerCard.BaseParameter.BaseCardId == 930344070 || skill.SkillPrm.ownerCard.BaseParameter.BaseCardId == 930444050))
		{
			return true;
		}
		return false;
	}

	private static bool IsContainPrivateKeyword(string text, bool isIgnoreHand = false, bool isIgnoreDeck = false)
	{
		if (!isIgnoreHand && WithdrawnIncludedContentKeyword(text, HandKeywordList) != string.Empty && WithdrawnIncludedContentKeyword(text, PrivateKeywordList, isContains: true) != string.Empty)
		{
			return true;
		}
		if (!isIgnoreDeck && WithdrawnIncludedContentKeyword(text, DeckKeywordList) != string.Empty && WithdrawnIncludedContentKeyword(text, PrivateKeywordList, isContains: true) != string.Empty)
		{
			return true;
		}
		if (HasGameDrawCardsText(text))
		{
			return true;
		}
		if (HasGameAddDeckCardsText(text))
		{
			return true;
		}
		return false;
	}

	private static bool IsContainSelectedTargetKeyword(string text)
	{
		if (WithdrawnIncludedContentKeyword(text, SkillFilterCreator.ContentKeyword.selected_cards) != string.Empty && WithdrawnIncludedContentKeyword(text, PrivateKeywordList, isContains: true) != string.Empty)
		{
			return true;
		}
		return false;
	}

	public static bool CheckLastTargetFilter(SkillBase skill)
	{
		return skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => IsContainPrivateKeywordInLastTarget(f.Text));
	}

	public static bool IsContainPrivateKeywordInLastTarget(string text)
	{
		if (WithdrawnIncludedContentKeyword(text, LastTargetKeywordList) != string.Empty)
		{
			return WithdrawnIncludedContentKeyword(text, PrivateKeywordListInLastTarget, isContains: true) != string.Empty;
		}
		return false;
	}

	public static bool IsPreprocessConditionCheck(SkillBase skill)
	{
		ConditionSkillFilterCollection conditionFilterCollection = skill.ConditionFilterCollection;
		if (conditionFilterCollection.ConditionCheckerFilterList.Count < 1 || !(conditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) is NetworkSkillPreprocessConditionCheck))
		{
			return false;
		}
		if (RegisterValidate.IsValidateCard(skill))
		{
			return false;
		}
		if (IsPreprocessConditionCheck((conditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) as NetworkSkillPreprocessConditionCheck).GetConditionSkillFilterCollection(), skill))
		{
			return true;
		}
		return false;
	}

	public static bool IsPreprocessConditionCheck(ConditionSkillFilterCollection filter, SkillBase skill)
	{
		return filter.VariableCompareFilter.Any((SkillVariableComareFilter f) => IsContainPrivatePreprocessConditionText(f.Text, skill));
	}

	public static bool IsContainPrivatePreprocessConditionText(string text, SkillBase skill)
	{
		if (WithdrawnIncludedContentKeyword(text, PreprocessConditionKeywordList) != string.Empty && WithdrawnIncludedContentKeyword(text, PrivateKeywordListInPreprocessCondition, isContains: true) != string.Empty)
		{
			return true;
		}
		if (WithdrawnIncludedContentKeyword(text, SkillFilterCreator.ContentKeyword.load_target) != string.Empty)
		{
			if (skill.OnBeforeAttackStart != 0)
			{
				return false;
			}
			if (skill.SkillPrm.ownerCard.Skills.Where((SkillBase x) => x.OptionValue.HasInfoByName(SkillFilterCreator.ContentKeyword.save_target)).Any((SkillBase x) => NetworkBattleGenericTool.IsUnapprovedTarget(x) || x is Skill_return_card))
			{
				return true;
			}
		}
		if (WithdrawnIncludedContentKeyword(text, SkillFilterCreator.ContentKeyword.skill_drew_card, isContains: true) != string.Empty && WithdrawnIncludedContentKeyword(text, SkillFilterCreator.ContentKeyword.base_cost, isContains: true) != string.Empty)
		{
			return true;
		}
		if (HasGameDrawCardsText(text))
		{
			return true;
		}
		return false;
	}

	private static string WithdrawnIncludedContentKeyword(string text, SkillFilterCreator.ContentKeyword keywords, bool isContains = false)
	{
		return WithdrawnIncludedContentKeyword(text, new List<SkillFilterCreator.ContentKeyword> { keywords }, isContains);
	}

	private static string WithdrawnIncludedContentKeyword(string text, List<SkillFilterCreator.ContentKeyword> keywords, bool isContains = false)
	{
		foreach (string item in (IEnumerable<string>)text.Split('.'))
		{
			foreach (SkillFilterCreator.ContentKeyword keyword in keywords)
			{
				if (item == keyword.ToString())
				{
					return item;
				}
				if (isContains && item.Contains(keyword.ToString()))
				{
					return item;
				}
			}
		}
		return "";
	}

	private static bool IsDeckNumInvestigationSkill(SkillBase skill)
	{
		if (skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => WithdrawnIncludedContentKeyword(x.Text, SkillFilterCreator.ContentKeyword.deck) != "") || skill.IsRefVariable(SkillFilterCreator.ContentKeyword.deck.ToString()))
		{
			return true;
		}
		return false;
	}

	private static bool IsHandNumInvestigationSkill(SkillBase skill)
	{
		if (skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => WithdrawnIncludedContentKeyword(x.Text, SkillFilterCreator.ContentKeyword.hand) != "") || skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => WithdrawnIncludedContentKeyword(x.Text, SkillFilterCreator.ContentKeyword.hand_other_self) != "") || skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => WithdrawnIncludedContentKeyword(x.Text, SkillFilterCreator.ContentKeyword.hand_other_oldest) != "") || skill.IsRefVariable(SkillFilterCreator.ContentKeyword.hand.ToString()))
		{
			return true;
		}
		if (skill.ConditionFilterCollection.ConditionCheckerFilterList.Count == 0)
		{
			return false;
		}
		if (!(skill.ConditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) is NetworkSkillPreprocessConditionCheck networkSkillPreprocessConditionCheck))
		{
			return false;
		}
		ConditionSkillFilterCollection conditionSkillFilterCollection = networkSkillPreprocessConditionCheck.GetConditionSkillFilterCollection();
		List<SkillFilterCreator.ContentKeyword> keywords = new List<SkillFilterCreator.ContentKeyword>
		{
			SkillFilterCreator.ContentKeyword.hand_other_self,
			SkillFilterCreator.ContentKeyword.hand_other_oldest
		};
		for (int num = 0; num < conditionSkillFilterCollection.VariableCompareFilter.Count(); num++)
		{
			if (WithdrawnIncludedContentKeyword(conditionSkillFilterCollection.VariableCompareFilter.ElementAt(num).Text, keywords) != string.Empty)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsCemeteryInvestigationSkill(SkillBase skill, bool isLastTargetDiscard)
	{
		if (isLastTargetDiscard)
		{
			if (skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => WithdrawnIncludedContentKeyword(x.Text, SkillFilterCreator.ContentKeyword.last_target) != "") || skill.IsRefVariable(SkillFilterCreator.ContentKeyword.last_target.ToString()))
			{
				return true;
			}
			return false;
		}
		if (skill.IsRefVariable(SkillFilterCreator.ContentKeyword.discarded_card.ToString()))
		{
			return true;
		}
		if (skill.ConditionFilterCollection.ConditionCheckerFilterList.Count == 0)
		{
			return false;
		}
		if (!(skill.ConditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) is NetworkSkillPreprocessConditionCheck networkSkillPreprocessConditionCheck))
		{
			return false;
		}
		ConditionSkillFilterCollection conditionSkillFilterCollection = networkSkillPreprocessConditionCheck.GetConditionSkillFilterCollection();
		List<SkillFilterCreator.ContentKeyword> keywords = new List<SkillFilterCreator.ContentKeyword> { SkillFilterCreator.ContentKeyword.discarded_card };
		for (int num = 0; num < conditionSkillFilterCollection.VariableCompareFilter.Count(); num++)
		{
			if (WithdrawnIncludedContentKeyword(conditionSkillFilterCollection.VariableCompareFilter.ElementAt(num).Text, keywords) != string.Empty && WithdrawnIncludedContentKeyword(conditionSkillFilterCollection.VariableCompareFilter.ElementAt(num).Text, PrivateKeywordListInPreprocessCondition, isContains: true) != string.Empty)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsGameDrawCardsNumInvestigationSkill(SkillBase skill)
	{
		if (skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => HasGameDrawCardsText(x.Text)))
		{
			return true;
		}
		if (skill.ConditionFilterCollection.ConditionCheckerFilterList.Count == 0)
		{
			return false;
		}
		if (!(skill.ConditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) is NetworkSkillPreprocessConditionCheck networkSkillPreprocessConditionCheck))
		{
			return false;
		}
		ConditionSkillFilterCollection conditionSkillFilterCollection = networkSkillPreprocessConditionCheck.GetConditionSkillFilterCollection();
		for (int num = 0; num < conditionSkillFilterCollection.VariableCompareFilter.Count(); num++)
		{
			if (HasGameDrawCardsText(conditionSkillFilterCollection.VariableCompareFilter.ElementAt(num).Text))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasGameDrawCardsText(string text)
	{
		if (WithdrawnIncludedContentKeyword(text, SkillFilterCreator.ContentKeyword.game_draw_cards) != string.Empty && text.Contains(SkillFilterCreator.ContentKeyword.base_card_id.ToString()))
		{
			return true;
		}
		return false;
	}

	public static bool IsGameAddDeckCardsNumInvestigationSkill(SkillBase skill)
	{
		return skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => HasGameAddDeckCardsText(x.Text));
	}

	private static bool HasGameAddDeckCardsText(string text)
	{
		if (WithdrawnIncludedContentKeyword(text, SkillFilterCreator.ContentKeyword.game_add_update_deck_cards) != string.Empty)
		{
			if (!text.Contains(SkillFilterCreator.ContentKeyword.base_card_id.ToString()))
			{
				return text.Contains(SkillFilterCreator.ContentKeyword.tribe.ToString());
			}
			return true;
		}
		return false;
	}

	public static bool DoesSkillUsePrivateCount(SkillBase skill, bool notHandCheck = false, bool notDeckCheck = false)
	{
		if (skill is Skill_attach_skill)
		{
			return false;
		}
		if (DoesSkillCallCountUseSkillDrewCard(skill) && Regex.IsMatch(skill.CallCountText, SkillFilterCreator.ContentKeyword.spell_and_field.ToString()))
		{
			return true;
		}
		string[] array = skill.Option.Split('+');
		for (int i = 0; i < array.Count(); i++)
		{
			bool flag = Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.skill_drew_card.ToString());
			bool flag2 = Regex.IsMatch(array[i], "discarded");
			bool flag3 = false;
			if (notDeckCheck && !notHandCheck)
			{
				flag3 = Regex.IsMatch(array[i], "hand");
			}
			else if (notHandCheck && !notDeckCheck)
			{
				flag3 = Regex.IsMatch(array[i], "deck");
			}
			else if (!notDeckCheck && !notHandCheck)
			{
				flag3 = Regex.IsMatch(array[i], "hand") || Regex.IsMatch(array[i], "deck");
			}
			if (flag3 || flag || flag2)
			{
				bool num = Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.clan.ToString()) || Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.tribe.ToString()) || Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.base_card_id.ToString()) || Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.unit.ToString()) || Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.spell.ToString()) || Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.field.ToString()) || Regex.IsMatch(array[i], SkillFilterCreator.ContentKeyword.base_cost.ToString());
				bool flag4 = Regex.IsMatch(array[i], "damage") || Regex.IsMatch(array[i], "healing") || Regex.IsMatch(array[i], "add_offense") || Regex.IsMatch(array[i], "add_life") || Regex.IsMatch(array[i], "add_pp") || Regex.IsMatch(array[i], "repeat_count") || Regex.IsMatch(array[i], "gain_chant");
				if (num && flag4)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsHighlander(ConditionSkillFilterCollection conditionSkillFilterCollection)
	{
		if (conditionSkillFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => x.Text.Contains("unique_base_card_id_card.count") && x.Rhs.Contains("{")))
		{
			return true;
		}
		return false;
	}

	public static bool IsHighlanderPreprocessConditionCheck(SkillBase skill)
	{
		ConditionSkillFilterCollection conditionFilterCollection = skill.ConditionFilterCollection;
		if (conditionFilterCollection.ConditionCheckerFilterList.Count < 1 || !(conditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) is NetworkSkillPreprocessConditionCheck))
		{
			return false;
		}
		return IsHighlander((conditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) as NetworkSkillPreprocessConditionCheck).GetConditionSkillFilterCollection());
	}

	private int GetExcludeCardId(SkillBase skill)
	{
		for (int i = 0; i < skill.ConditionFilterCollection.VariableCompareFilter.Count; i++)
		{
			if (!skill.ConditionFilterCollection.VariableCompareFilter[i].Text.Contains("unique_base_card_id_card.count"))
			{
				continue;
			}
			int num = skill.ConditionFilterCollection.VariableCompareFilter[i].Text.IndexOf("unique_base_card_id_card");
			if (num != -1 && num >= 10)
			{
				int result = -1;
				if (int.TryParse(skill.ConditionFilterCollection.VariableCompareFilter[i].Text.Substring(num - 10, 9), out result))
				{
					return result;
				}
			}
		}
		return -1;
	}

	private List<string> GetExcludeTribe(SkillBase skill)
	{
		for (int i = 0; i < skill.ConditionFilterCollection.VariableCompareFilter.Count; i++)
		{
			string text = skill.ConditionFilterCollection.VariableCompareFilter[i].Text;
			if (!text.Contains("unique_base_card_id_card.count"))
			{
				continue;
			}
			bool num = text.Contains("unique_base_card_id_card");
			bool flag = text.Contains("tribe!=");
			if (num && flag)
			{
				List<string> list = new List<string>();
				List<CardBasePrm.TribeType> list2 = CardBasePrm.CreateTribeTypeList(text, isTribeCheck: true, notEqual: true);
				for (int j = 0; j < list2.Count; j++)
				{
					list.Add("ne" + (int)list2[j]);
				}
				return list;
			}
		}
		return null;
	}

	private static bool IsOptionDrew_cardTarget(SkillBase skill)
	{
		if (skill is Skill_attach_skill)
		{
			return false;
		}
		if (skill.IsRefVariable("skill_drew_card"))
		{
			return true;
		}
		return false;
	}

	private static bool IsNotCheckCount(SkillBase skill)
	{
		return !skill.IsRefVariable(SkillFilterCreator.ContentKeyword.count.ToString());
	}

	public static bool DoesSkillCallCountUseSkillDrewCard(SkillBase skill)
	{
		return Regex.IsMatch(skill.CallCountText, "skill_drew_card");
	}

	public static bool IsContainPreprocessLoadTarget(SkillBase skill)
	{
		return skill.PreprocessList.Where((SkillPreprocessBase x) => x is SkillPreprocessConditionCheck).Any((SkillPreprocessBase x) => (x as SkillPreprocessConditionCheck).Contains(SkillFilterCreator.ContentKeyword.load_target.ToString()));
	}

	public static bool IsContainPreprocessLoadOrLastTarget(SkillBase skill)
	{
		return skill.PreprocessList.Where((SkillPreprocessBase x) => x is SkillPreprocessConditionCheck).Any((SkillPreprocessBase x) => (x as SkillPreprocessConditionCheck).Contains(SkillFilterCreator.ContentKeyword.load_target.ToString()) || (x as SkillPreprocessConditionCheck).Contains(SkillFilterCreator.ContentKeyword.last_target.ToString()));
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.skillConditionCheck.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}

	public static int GetMovementCount(SkillBase skill)
	{
		if (DoesSkillUsePrivateCount(skill) && skill is Skill_powerup)
		{
			int num = 0;
			string option = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_offense, "NONE");
			string option2 = skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_life, "NONE");
			if (option != "NONE")
			{
				num++;
			}
			if (option2 != "NONE")
			{
				num++;
			}
			return num;
		}
		return 1;
	}

	private bool IsInvokedCheckDeckSkill(SkillBase skill)
	{
		if (!skill.IsInvoked)
		{
			return false;
		}
		List<SkillFilterCreator.ContentKeyword> tribeKeywordList = new List<SkillFilterCreator.ContentKeyword> { SkillFilterCreator.ContentKeyword.tribe };
		List<SkillFilterCreator.ContentKeyword> costKeywordList = new List<SkillFilterCreator.ContentKeyword> { SkillFilterCreator.ContentKeyword.cost };
		return skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => WithdrawnIncludedContentKeyword(f.Text, DeckKeywordList) != string.Empty && WithdrawnIncludedContentKeyword(f.Text, tribeKeywordList, isContains: true) != string.Empty && WithdrawnIncludedContentKeyword(f.Text, costKeywordList, isContains: true) != string.Empty);
	}
}

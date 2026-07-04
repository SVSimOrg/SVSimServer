using System.Collections.Generic;
using System.Linq;

public class RegisterValidate : RegisterActionBase
{
	public class ValidateData
	{
		public string NowCost;

		public string BaseCost;

		public string NowAtk;

		public string BaseAtk;

		public string NowLife;

		public string BaseLife;

		public List<int> CharaTypes;

		public string Clan;

		public string Tribe;

		public string ChantCount;

		public List<int> LibraryType;

		public List<int> ExcludeList;

		public List<int> IncludeList;

		public List<int> State;

		public string SpellBoost;

		public List<int> ExcludeIdxList;

		public ValidateData()
		{
			CharaTypes = new List<int>();
			LibraryType = new List<int>();
			ExcludeList = new List<int>();
			IncludeList = new List<int>();
			State = new List<int>();
			ExcludeIdxList = new List<int>();
		}
	}

	public enum ValidateParamater
	{
		idx,
		conditions,
		nowCost,
		baseCost,
		nowAtk,
		baseAtk,
		nowLife,
		baseLife,
		clan,
		tribe,
		chant_count,
		excludeList,
		includeList,
		spellboost,
		state,
		checkDuplicate,
		checkParam
	}

	protected List<ValidateData> ValidateDataList = new List<ValidateData>();

	protected List<ValidateData> OrValidateDataList = new List<ValidateData>();

	private bool _isCheckDuplicate;

	private bool _isCheckDeckParam;

	public void AddValidateData(SkillBase skill)
	{
		if (NetworkBattleGenericTool.IsBurialRite(skill))
		{
			ValidateData validateData = new ValidateData();
			validateData.CharaTypes.Add(1);
			ValidateDataList.Add(validateData);
		}
		else if (skill.ApplyFilterCollection.ApplyAndFilter.Count() > 0)
		{
			for (int i = 0; i < skill.ApplyFilterCollection.ApplyAndFilter.Count; i++)
			{
				OrValidateDataList.Add(CreateValidateData(skill.ApplyFilterCollection.ApplyAndFilter[i].CardFilterList, skill));
			}
		}
		else if (skill.IsRandomUntilDrawSkill)
		{
			ValidateData validateData2 = new ValidateData();
			validateData2.LibraryType.Add(1003);
			ValidateDataList.Add(validateData2);
		}
		else if (NetworkBattleGenericTool.IsTargetDeckSelf(skill) && IsDeckParamVariable(skill))
		{
			_isCheckDeckParam = true;
			ValidateDataList.Add(CreateValidateDataForReferenceSelfStatus(skill));
		}
		else
		{
			ValidateDataList.Add(CreateValidateData(skill.ApplyCardFilterList, skill));
		}
	}

	public void SetCardIdValidateData(BattleCardBase card)
	{
		ValidateData validateData = new ValidateData();
		base.IndexList.Add(card.Index);
		validateData.IncludeList.Add(card.BaseParameter.BaseCardId);
		ValidateDataList.Add(validateData);
	}

	public static bool IsDeckParamVariable(SkillBase skill)
	{
		return skill.PreprocessList.Where((SkillPreprocessBase x) => x is SkillPreprocessConditionCheck).Any((SkillPreprocessBase x) => (x as SkillPreprocessConditionCheck).Contains(SkillFilterCreator.ContentKeyword.deck.ToString()) && (x as SkillPreprocessConditionCheck).Contains(SkillFilterCreator.ContentKeyword.life.ToString()));
	}

	protected ValidateData CreateValidateData(List<ISkillCardFilter> cardFilterList, SkillBase skill)
	{
		ValidateData validateData = new ValidateData();
		List<CARD_TYPE> list = new List<CARD_TYPE>();
		foreach (ISkillCardFilter cardFilter in cardFilterList)
		{
			if (cardFilter is SkillParameterCostFilter skillParameterCostFilter)
			{
				string parameterText = skillParameterCostFilter.GetParameterText();
				if (parameterText.Contains(".max_pp") || (!parameterText.Contains(".max") && !parameterText.Contains(".min")))
				{
					validateData.NowCost = RegisterTool.MakeParameterOptionTextList(skillParameterCostFilter.GetParameterOptionText())[0] + skill.OptionValue.ParseInt(parameterText);
				}
			}
			else if (cardFilter is SkillParameterBaseCostFilter skillParameterBaseCostFilter)
			{
				string parameterText2 = skillParameterBaseCostFilter.GetParameterText();
				if (parameterText2.Contains(SkillFilterCreator.ContentKeyword.fixed_generic_value.ToString()))
				{
					validateData.BaseCost = RegisterTool.MakeParameterOptionTextList(skillParameterBaseCostFilter.GetParameterOptionText())[0] + "v1";
				}
				else
				{
					validateData.BaseCost = RegisterTool.MakeParameterOptionTextList(skillParameterBaseCostFilter.GetParameterOptionText())[0] + skill.OptionValue.ParseInt(parameterText2);
				}
			}
			if (cardFilter is SkillParameterOffenseFilter skillParameterOffenseFilter)
			{
				validateData.NowAtk = RegisterTool.MakeParameterOptionTextList(skillParameterOffenseFilter.GetParameterOptionText())[0] + skill.OptionValue.ParseInt(skillParameterOffenseFilter.GetParameterText());
			}
			else if (cardFilter is SkillParameterBaseOffenseFilter skillParameterBaseOffenseFilter)
			{
				validateData.BaseAtk = RegisterTool.MakeParameterOptionTextList(skillParameterBaseOffenseFilter.GetParameterOptionText())[0] + skillParameterBaseOffenseFilter.GetParameterText();
			}
			if (cardFilter is SkillParameterLifeFilter skillParameterLifeFilter)
			{
				validateData.NowLife = RegisterTool.MakeParameterOptionTextList(skillParameterLifeFilter.GetParameterOptionText())[0] + skillParameterLifeFilter.GetParameterText();
			}
			else if (cardFilter is SkillParameterBaseLifeFilter skillParameterBaseLifeFilter)
			{
				validateData.BaseLife = RegisterTool.MakeParameterOptionTextList(skillParameterBaseLifeFilter.GetParameterOptionText())[0] + skillParameterBaseLifeFilter.GetParameterText();
			}
			list.AddRange(RegisterTool.GetCardTypeList(cardFilter));
			if (cardFilter is SkillClanFilter)
			{
				SkillClanFilter skillClanFilter = cardFilter as SkillClanFilter;
				validateData.Clan = RegisterTool.MakeParameterOptionTextList(skillClanFilter.OptionText)[0] + (int)skillClanFilter._clan;
			}
			if (cardFilter is SkillTribeFilter skillTribeFilter)
			{
				validateData.Tribe = (skillTribeFilter.IsEqual ? ((int)skillTribeFilter._type).ToString() : ("ne" + (int)skillTribeFilter._type));
			}
			if (cardFilter is SkillParameterChantCountFilter)
			{
				SkillParameterChantCountFilter skillParameterChantCountFilter = cardFilter as SkillParameterChantCountFilter;
				validateData.ChantCount = RegisterTool.MakeParameterOptionTextList(skillParameterChantCountFilter.GetParameterOptionText())[0] + skillParameterChantCountFilter.GetParameterText();
			}
			SPECIAL_LIBRARY sPECIAL_LIBRARY = RegisterTool.CalculationCardSkillType(cardFilter, (list.Count >= 1) ? list[list.Count - 1] : CARD_TYPE.NON);
			if (sPECIAL_LIBRARY != SPECIAL_LIBRARY.NONE)
			{
				validateData.LibraryType.Add((int)sPECIAL_LIBRARY);
			}
			if (cardFilter is SkillParameterChargeCountFilter)
			{
				validateData.LibraryType.Add(1003);
				SkillParameterChargeCountFilter skillParameterChargeCountFilter = cardFilter as SkillParameterChargeCountFilter;
				validateData.SpellBoost = RegisterTool.MakeParameterOptionTextList(skillParameterChargeCountFilter.Option)[0] + skillParameterChargeCountFilter.Parameter;
			}
			if (cardFilter is SkillParameterIdFilter)
			{
				SkillParameterIdFilter skillParameterIdFilter = cardFilter as SkillParameterIdFilter;
				if (skillParameterIdFilter.GetOptionText() == "!=" || skillParameterIdFilter.GetOptionText() == "=")
				{
					string[] filterId = skillParameterIdFilter.GetFilterId();
					for (int i = 0; i < filterId.Length; i++)
					{
						int item = int.Parse(filterId[i]);
						if (skillParameterIdFilter.GetOptionText() == "=")
						{
							validateData.IncludeList.Add(item);
						}
						else
						{
							validateData.ExcludeList.Add(item);
						}
					}
				}
			}
			if (cardFilter is SkillTargetNotUniqueBaseCardIdFilter)
			{
				_isCheckDuplicate = true;
			}
		}
		validateData.CharaTypes = new List<int>();
		if (list.Count >= 1)
		{
			foreach (CARD_TYPE item2 in list)
			{
				validateData.CharaTypes.Add((int)item2);
			}
		}
		return validateData;
	}

	protected ValidateData CreateValidateDataForReferenceSelfStatus(SkillBase skill)
	{
		ValidateData validateData = new ValidateData();
		validateData.IncludeList.Add(skill.SkillPrm.ownerCard.BaseParameter.BaseCardId);
		validateData.State.Add(0);
		IEnumerable<SkillPreprocessBase> source = skill.PreprocessList.Where((SkillPreprocessBase x) => x is SkillPreprocessConditionCheck);
		for (int num = 0; num < source.Count(); num++)
		{
			if ((source.ElementAt(num) as SkillPreprocessConditionCheck).Contains("life.max"))
			{
				validateData.NowLife = "max";
			}
		}
		return validateData;
	}

	public void AddIncludeList(int baseCardId, int index)
	{
		ValidateData validateData = new ValidateData();
		validateData.IncludeList.Add(baseCardId);
		ValidateDataList.Add(validateData);
		base.IndexList.Add(index);
	}

	public static bool IsValidateCard(SkillBase skill)
	{
		if (NetworkBattleGenericTool.IsTargetDeckSelf(skill))
		{
			if (IsDeckParamVariable(skill))
			{
				return true;
			}
			return false;
		}
		if (skill.IsRandomUntilDrawSkill)
		{
			return true;
		}
		if (IsDeckRandomEachSkill(skill))
		{
			return true;
		}
		if (IsOpenMyHandSkill(skill))
		{
			return true;
		}
		if (NetworkBattleGenericTool.IsBurialRite(skill))
		{
			return true;
		}
		if (!(skill.ApplyingTargetFilter is SkillTargetHandFilter) && !(skill.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter) && !skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetHandFilter || f.TargetFilter is SkillTargetHandOtherSelfFilter))
		{
			return false;
		}
		if (RegisterTool.IsSkillRandom(skill))
		{
			return false;
		}
		if (!(skill.ApplySelectFilter is SkillUserSelectFilter) && !(skill is Skill_select))
		{
			return false;
		}
		if (!RegisterTool.IsSkillFilterEffect(skill))
		{
			return false;
		}
		return true;
	}

	public static bool IsDeckRandomEachSkill(SkillBase skill)
	{
		if (skill.ApplyingTargetFilter is SkillTargetDeckFilter)
		{
			return skill.ApplySelectFilter is SkillRandomEachSameBaseCardIdFilter;
		}
		return false;
	}

	public static bool IsOpenMyHandSkill(SkillBase skill)
	{
		if (skill is Skill_token_draw && skill.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter && skill.ApplyingTargetFilter is SkillTargetHandFilter && skill.ApplySelectFilter is SkillSelectAllFilter)
		{
			return true;
		}
		return false;
	}

	public static bool IsSendOpenMyCardsSkill(SkillBase skill)
	{
		if ((skill.OnSelfTurnEndStart == 0 || !skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessOpenCard)) && (skill is Skill_summon_card || skill is Skill_draw || !(skill.ConditionTargetFilter is SkillTargetDeckSelfFilter)))
		{
			if (skill.OnWhenBanish != 0)
			{
				return skill.ConditionTargetFilter is SkillTargetHandSelfFilter;
			}
			return false;
		}
		return true;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (PrivateGroupIndexMsg != string.Empty)
		{
			dictionary.Add(ValidateParamater.idx.ToString(), PrivateGroupIndexMsg);
		}
		else
		{
			dictionary.Add(ValidateParamater.idx.ToString(), base.IndexList);
		}
		List<Dictionary<string, object>> list = CreateConditionData((OrValidateDataList.Count > 0) ? OrValidateDataList : ValidateDataList);
		if (_isCheckDeckParam)
		{
			dictionary.Add(ValidateParamater.checkParam.ToString(), CreateDeckParamConditionData(ValidateDataList));
		}
		else if (OrValidateDataList.Count > 0)
		{
			List<List<Dictionary<string, object>>> list2 = new List<List<Dictionary<string, object>>>();
			list2.Add(list);
			dictionary.Add(ValidateParamater.conditions.ToString(), list2);
		}
		else if (list.Where((Dictionary<string, object> c) => c.Keys.Count > 0).Count() > 0)
		{
			dictionary.Add(ValidateParamater.conditions.ToString(), list);
		}
		if (_isCheckDuplicate)
		{
			dictionary.Add(ValidateParamater.checkDuplicate.ToString(), 1);
		}
		return dictionary;
	}

	protected List<Dictionary<string, object>> CreateConditionData(List<ValidateData> validateDataList)
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (ValidateData validateData in validateDataList)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (validateData.NowCost != null)
			{
				dictionary.Add(ValidateParamater.nowCost.ToString(), validateData.NowCost);
			}
			if (validateData.BaseCost != null)
			{
				dictionary.Add(ValidateParamater.baseCost.ToString(), validateData.BaseCost);
			}
			if (validateData.NowAtk != null)
			{
				dictionary.Add(ValidateParamater.nowAtk.ToString(), validateData.NowAtk);
			}
			if (validateData.BaseAtk != null)
			{
				dictionary.Add(ValidateParamater.baseAtk.ToString(), validateData.BaseAtk);
			}
			if (validateData.NowLife != null)
			{
				dictionary.Add(ValidateParamater.nowLife.ToString(), validateData.NowLife);
			}
			if (validateData.BaseLife != null)
			{
				dictionary.Add(ValidateParamater.baseLife.ToString(), validateData.BaseLife);
			}
			if (validateData.CharaTypes.Count >= 1)
			{
				dictionary.Add(ActionBaseParameter.charType.ToString(), validateData.CharaTypes);
			}
			if (validateData.Clan != null)
			{
				dictionary.Add(ValidateParamater.clan.ToString(), validateData.Clan);
			}
			if (validateData.Tribe != null)
			{
				dictionary.Add(ValidateParamater.tribe.ToString(), validateData.Tribe);
			}
			if (validateData.ChantCount != null)
			{
				dictionary.Add(ValidateParamater.chant_count.ToString(), validateData.ChantCount);
			}
			if (validateData.SpellBoost != null)
			{
				dictionary.Add(ValidateParamater.spellboost.ToString(), validateData.SpellBoost);
			}
			if (validateData.LibraryType.Count() >= 1)
			{
				dictionary.Add(ActionBaseParameter.libraryType.ToString(), validateData.LibraryType);
			}
			if (validateData.ExcludeList.Count >= 1)
			{
				List<int> list2 = new List<int>();
				foreach (int exclude in validateData.ExcludeList)
				{
					list2.Add(exclude);
				}
				dictionary.Add(ValidateParamater.excludeList.ToString(), list2);
			}
			if (validateData.IncludeList.Count >= 1)
			{
				List<int> list3 = new List<int>();
				foreach (int include in validateData.IncludeList)
				{
					list3.Add(include);
				}
				dictionary.Add(ValidateParamater.includeList.ToString(), list3);
			}
			if (validateData.ExcludeIdxList.Count >= 1)
			{
				dictionary.Add(RegisterScan.ScanParameter.excludeIdxList.ToString(), validateData.ExcludeIdxList);
			}
			list.Add(dictionary);
		}
		return list;
	}

	protected Dictionary<string, object> CreateDeckParamConditionData(List<ValidateData> validateDataList)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (ValidateData validateData in validateDataList)
		{
			if (validateData.NowCost != null)
			{
				dictionary.Add(ValidateParamater.nowCost.ToString(), validateData.NowCost);
			}
			if (validateData.BaseCost != null)
			{
				dictionary.Add(ValidateParamater.baseCost.ToString(), validateData.BaseCost);
			}
			if (validateData.NowAtk != null)
			{
				dictionary.Add(ValidateParamater.nowAtk.ToString(), validateData.NowAtk);
			}
			if (validateData.BaseAtk != null)
			{
				dictionary.Add(ValidateParamater.baseAtk.ToString(), validateData.BaseAtk);
			}
			if (validateData.NowLife != null)
			{
				dictionary.Add(ValidateParamater.nowLife.ToString(), validateData.NowLife);
			}
			if (validateData.BaseLife != null)
			{
				dictionary.Add(ValidateParamater.baseLife.ToString(), validateData.BaseLife);
			}
			if (validateData.IncludeList.Count >= 1)
			{
				List<int> list = new List<int>();
				foreach (int include in validateData.IncludeList)
				{
					list.Add(include);
				}
				dictionary.Add(ValidateParamater.includeList.ToString(), list);
			}
			if (validateData.State.Count < 1)
			{
				continue;
			}
			List<int> list2 = new List<int>();
			foreach (int item in validateData.State)
			{
				list2.Add(item);
			}
			dictionary.Add(ValidateParamater.state.ToString(), list2);
		}
		return dictionary;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.validate.ToString();
	}

	public List<ValidateData> GetValidateDataList()
	{
		return ValidateDataList;
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}

	public static bool IsSetValidateConditionCheckSkill(SkillBase skill)
	{
		if (NetworkBattleGenericTool.IsTargetDeckSelf(skill))
		{
			return false;
		}
		if (skill.IsRandomUntilDrawSkill)
		{
			return false;
		}
		if (IsOpenMyHandSkill(skill))
		{
			return false;
		}
		if (skill.ApplyingTargetFilter is SkillTargetDeckFilter && skill.ApplySelectFilter is SkillRandomEachSameBaseCardIdFilter)
		{
			return false;
		}
		if (skill is Skill_fusion)
		{
			return false;
		}
		return true;
	}
}

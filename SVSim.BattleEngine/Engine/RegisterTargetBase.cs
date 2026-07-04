using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RegisterTargetBase : RegisterActionBase
{
	public class Conditions
	{
		public enum DuplicationStatusKind
		{
			NONE,
			cost,
			cardId
		}

		public SPECIAL_LIBRARY SpecialLibrary { get; set; }

		public List<CARD_TYPE> CharaTypeList { get; set; }

		public NetworkBattleDefine.NetworkCardPlaceState State { get; set; }

		public string Tribe { get; set; }

		public string Clan { get; set; }

		public List<int> ExcludeList { get; set; }

		public List<int> IncludeIdList { get; set; }

		public List<int> BaseCardIdxList { get; set; }

		public List<int> HandIdxList { get; set; }

		public DuplicationStatusKind Duplication { get; set; }

		public List<string> BaseCost { get; set; }

		public List<string> Cost { get; set; }

		public List<string> InHandCost { get; set; } = new List<string>();

		public string Atk { get; set; }

		public string BaseAtk { get; set; }

		public string Life { get; set; }

		public string BaseLife { get; set; }

		public string HasBuffLife { get; set; }

		public bool IsDiscarded { get; set; }

		public Conditions()
		{
			SpecialLibrary = SPECIAL_LIBRARY.NONE;
			CharaTypeList = new List<CARD_TYPE>();
			State = NetworkBattleDefine.NetworkCardPlaceState.None;
			Tribe = "NONE";
			Clan = "NONE";
			ExcludeList = new List<int>();
			IncludeIdList = new List<int>();
			BaseCardIdxList = new List<int>();
			HandIdxList = new List<int>();
			Duplication = DuplicationStatusKind.NONE;
			BaseCost = new List<string>();
			Cost = new List<string>();
			Atk = null;
			BaseAtk = null;
			Life = null;
			BaseLife = null;
		}
	}

	protected enum ExtractType
	{
		None,
		BaseCost,
		LastTribe
	}

	public List<string> GroupMsgList = new List<string>();

	protected List<Conditions> _conditionsList = new List<Conditions>();

	protected ExtractType _extractType;

	protected string _extractOperator = string.Empty;

	protected string _extractString = string.Empty;

	private List<List<double>> _fixedRandomList = new List<List<double>>();

	protected NetworkBattleDefine.NetworkCardPlaceState FromPlaceState = NetworkBattleDefine.NetworkCardPlaceState.None;

	protected SkillBase LotSkillBase;

	protected int SkillMovementNum;

	protected BattleManagerBase BattleMgr;

	protected RegisterActionManager RegisterActionManagerData;

	public SkillBase Skill { get; protected set; }

	public List<string> GetGroupMsgList()
	{
		return GroupMsgList;
	}

	public RegisterTargetBase(SkillBase skill, RegisterActionManager registerActionManager, bool isplayer, BattleManagerBase mgr)
	{
		Skill = skill;
		BattleMgr = mgr;
		RegisterActionManagerData = registerActionManager;
		AddGroupIndex();
		IsSelf = isplayer;
		FromPlaceState = NetworkBattleDefine.NetworkCardPlaceState.None;
		if (IsSelf || !RegisterExtract.IsExtract(Skill))
		{
			return;
		}
		if (skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.CardFilterList.Any((ISkillCardFilter cf) => cf is SkillLastTargetTribeFilter)))
		{
			_extractOperator = "=";
			_extractType = ExtractType.LastTribe;
		}
		else
		{
			_extractOperator = (Skill.ApplyCardFilterList.SingleOrDefault((ISkillCardFilter s) => s is SkillParameterBaseCostFilter) as SkillParameterBaseCostFilter).GetParameterOptionText();
			_extractType = ExtractType.BaseCost;
		}
		if (RegisterActionManagerData != null && RegisterActionManagerData.Any((RegisterActionBase r) => r is RegisterExtract))
		{
			List<string> list = RegisterTool.MakeParameterOptionTextList(_extractOperator);
			for (int num = 0; num < list.Count; num++)
			{
				_extractString = list[num] + "v1";
			}
		}
	}

	protected void AddGroupIndex()
	{
		if (RegisterActionManagerData != null)
		{
			RegisterActionManagerData.AddTargetGroupeNum();
			GroupMsgList.Add("g" + RegisterActionManagerData.TargetGroupNum);
		}
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		dictionary.Add(ActionBaseParameter.group.ToString(), GroupMsgList);
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (Conditions conditions in _conditionsList)
		{
			list.Add(CreateConditionsDict(conditions));
		}
		dictionary.Add(ActionBaseParameter.conditions.ToString(), list);
		for (int i = 0; i < _fixedRandomList.Count(); i++)
		{
			List<object> list2 = new List<object>();
			for (int j = 0; j < _fixedRandomList.Count; j++)
			{
				list2.Add(_fixedRandomList.ElementAt(j));
			}
			dictionary.Add(ActionBaseParameter.rand.ToString(), list2);
		}
		return dictionary;
	}

	private Dictionary<string, object> CreateConditionsDict(Conditions conditions)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (conditions.State != NetworkBattleDefine.NetworkCardPlaceState.None)
		{
			dictionary.Add(ActionBaseParameter.state.ToString(), (int)conditions.State);
		}
		else if (FromPlaceState != NetworkBattleDefine.NetworkCardPlaceState.None)
		{
			dictionary.Add(ActionBaseParameter.state.ToString(), (int)FromPlaceState);
		}
		if (conditions.SpecialLibrary != SPECIAL_LIBRARY.NONE)
		{
			dictionary.Add(ActionBaseParameter.libraryType.ToString(), (int)conditions.SpecialLibrary);
		}
		if (conditions.CharaTypeList.Count >= 1)
		{
			List<object> list = new List<object>();
			foreach (CARD_TYPE charaType in conditions.CharaTypeList)
			{
				list.Add(charaType);
			}
			dictionary.Add(ActionBaseParameter.charType.ToString(), list);
		}
		if (conditions.Clan != "NONE")
		{
			dictionary.Add(ActionBaseParameter.clan.ToString(), conditions.Clan);
		}
		if (conditions.Tribe != "NONE")
		{
			dictionary.Add(ActionBaseParameter.tribe.ToString(), conditions.Tribe);
		}
		if (conditions.ExcludeList.Count >= 1)
		{
			List<object> list2 = new List<object>();
			foreach (int exclude in conditions.ExcludeList)
			{
				list2.Add(exclude);
			}
			dictionary.Add(ActionBaseParameter.excludeList.ToString(), list2);
		}
		if (conditions.IncludeIdList.Count >= 1)
		{
			List<object> list3 = new List<object>();
			foreach (int includeId in conditions.IncludeIdList)
			{
				list3.Add(includeId);
			}
			dictionary.Add(ActionBaseParameter.includeList.ToString(), list3);
		}
		if (conditions.BaseCardIdxList.Count >= 1)
		{
			List<object> list4 = new List<object>();
			for (int i = 0; i < conditions.BaseCardIdxList.Count; i++)
			{
				list4.Add(conditions.BaseCardIdxList[i]);
			}
			dictionary.Add(ActionBaseParameter.baseCardIdx.ToString(), list4);
		}
		if (conditions.HandIdxList.Count >= 1)
		{
			List<object> list5 = new List<object>();
			for (int j = 0; j < conditions.HandIdxList.Count; j++)
			{
				list5.Add(conditions.HandIdxList[j]);
			}
			dictionary.Add(ActionBaseParameter.handIdxList.ToString(), list5);
		}
		if (conditions.Duplication != Conditions.DuplicationStatusKind.NONE)
		{
			dictionary.Add(ActionBaseParameter.duplication.ToString(), conditions.Duplication.ToString());
		}
		if (conditions.IsDiscarded)
		{
			dictionary.Add(ActionBaseParameter.discarded.ToString(), 1);
		}
		if (conditions.BaseCost.Count >= 1)
		{
			dictionary.Add(ActionBaseParameter.baseCost.ToString(), conditions.BaseCost);
		}
		if (conditions.Cost.Count >= 1)
		{
			dictionary.Add(ActionBaseParameter.nowCost.ToString(), conditions.Cost);
		}
		if (conditions.InHandCost.Count >= 1)
		{
			dictionary.Add(ActionBaseParameter.inHandCost.ToString(), conditions.InHandCost);
		}
		if (conditions.Atk != null)
		{
			dictionary.Add(ActionBaseParameter.atk.ToString(), conditions.Atk);
		}
		if (conditions.Life != null)
		{
			dictionary.Add(ActionBaseParameter.life.ToString(), conditions.Life);
		}
		if (conditions.BaseAtk != null)
		{
			dictionary.Add(ActionBaseParameter.baseAtk.ToString(), conditions.BaseAtk);
		}
		if (conditions.BaseLife != null)
		{
			dictionary.Add(ActionBaseParameter.life.ToString(), conditions.BaseLife);
		}
		if (conditions.HasBuffLife != null)
		{
			dictionary.Add(ActionBaseParameter.hasBuffLife.ToString(), conditions.HasBuffLife);
		}
		return dictionary;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.target.ToString();
	}

	public void SettingTargetStatusToSearchSkill(BattleManagerBase mgr, SkillBase skill, int destroyLotCount = 0, CardDataModel unapprovedCard = null, SkillConditionCheckerOption checkerOption = null)
	{
		if (skill is NetworkSkill_attach_skill && skill.ApplyingTargetFilter is NetworkSkillTargetLastTargetFilter)
		{
			SkillBase lastTargetSkillReferenceSkill = NetworkBattleGenericTool.GetLastTargetSkillReferenceSkill(skill);
			if (lastTargetSkillReferenceSkill != null)
			{
				SettingTargetStatusToSearchSkill(mgr, lastTargetSkillReferenceSkill, destroyLotCount, unapprovedCard, checkerOption);
				SetSkillMovementNum(skill);
				return;
			}
		}
		if (skill.ApplyAndFilter.Count > 0)
		{
			bool flag = skill.ApplySelectFilter is SkillRandomSelectFilter && skill.ApplyAndFilter.All((ApplySkillTargetFilterCollection f) => f.CardFilterList.Exists((ISkillCardFilter c) => c is SkillTribeFilter));
			List<ApplySkillTargetFilterCollection> list = skill.ApplyAndFilter;
			if (!(skill.ApplySelectFilter is SkillRandomSelectFilter) && !(skill.ApplySelectFilter is SkillIdNoDuplicationRandomSelectFilter) && unapprovedCard != null && !flag)
			{
				list = SelectActiveApplyAndFilter(list, unapprovedCard.RandomTargetIndex);
			}
			for (int num = 0; num < list.Count; num++)
			{
				NetworkBattleDefine.NetworkCardPlaceState state = NetworkBattleDefine.NetworkCardPlaceState.None;
				bool isDiscarded = list[num].TargetFilter is SkillTargetDiscardCardListFilter;
				if (list.Select((ApplySkillTargetFilterCollection f) => f.TargetFilter).Distinct().Count() > 1)
				{
					if (list[num].TargetFilter is SkillTargetDiscardCardListFilter)
					{
						state = NetworkBattleDefine.NetworkCardPlaceState.Cemetery;
					}
					else if (list[num].TargetFilter is SkillTargetGameFusionIngredientedCards)
					{
						state = NetworkBattleDefine.NetworkCardPlaceState.FusionIngredient;
					}
				}
				AddConditionsList(mgr, skill, list[num].CardFilterList, destroyLotCount, unapprovedCard, checkerOption, state, isDiscarded);
			}
			if (flag)
			{
				MergeTribeIntoHeadCondition();
			}
		}
		else if (skill.PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessConditionCheck) is SkillPreprocessConditionCheck skillPreprocessConditionCheck && RegisterFilter.IsFilterPreprocessCondition(skill))
		{
			string text = skillPreprocessConditionCheck.GetFilter().VariableCompareFilter.FirstOrDefault().Text;
			AddConditionsFromPreprocessCondition(skill, text);
		}
		else
		{
			AddConditionsList(mgr, skill, skill.ApplyCardFilterList, destroyLotCount, unapprovedCard, checkerOption);
		}
		SetSkillMovementNum(skill);
	}

	private void SetSkillMovementNum(SkillBase skill)
	{
		LotSkillBase = skill;
		if (skill._executionInfoCreator is NetworkExecutionInfoCreator networkExecutionInfoCreator)
		{
			SkillMovementNum = networkExecutionInfoCreator.GetSkillMovementNum();
		}
	}

	private void AddConditionsList(BattleManagerBase mgr, SkillBase skill, List<ISkillCardFilter> cardFilterList, int destroyLotCount, CardDataModel unapprovedCard, SkillConditionCheckerOption checkerOption, NetworkBattleDefine.NetworkCardPlaceState state = NetworkBattleDefine.NetworkCardPlaceState.None, bool isDiscarded = false)
	{
		Conditions item = SettingTargetStatus(mgr, skill, cardFilterList, checkerOption, isDiscarded, destroyLotCount, unapprovedCard, state);
		_conditionsList.Add(item);
	}

	private void AddConditionsFromPreprocessCondition(SkillBase skill, string text)
	{
		Conditions conditions = new Conditions();
		if (!text.Contains(SkillFilterCreator.ContentKeyword.base_cost.ToString()) && text.Contains(SkillFilterCreator.ContentKeyword.cost.ToString()))
		{
			string text2 = "";
			bool flag = text.Contains(">=");
			string text3 = text.Split('=')[1];
			if (flag)
			{
				text2 = "ge";
			}
			conditions.Cost.Add(text2 + text3);
		}
		if (text.Contains(SkillFilterCreator.ContentKeyword.last_target.ToString()) && text.Contains(SkillFilterCreator.ContentKeyword.base_cost.ToString()) && skill.OnWhenPlayStart != 0)
		{
			string[] array = text.Split('.');
			for (int i = 0; i < array.Count(); i++)
			{
				string text4 = array[i];
				if (text4.Contains(SkillFilterCreator.ContentKeyword.base_cost.ToString()))
				{
					string text5 = "";
					if (text4.Contains(">="))
					{
						text5 = "ge";
					}
					conditions.BaseCost.Add(text5 + text4.Split('=')[1]);
				}
				else if (text4.Contains(SkillFilterCreator.ContentKeyword.clan.ToString()))
				{
					string text6 = "";
					if (text4.Contains("="))
					{
						text6 = "eq";
					}
					conditions.Clan = text6 + (int)CardBasePrm.GetClanType(text4.Split('=')[1]);
				}
				else if (text4.Contains(SkillFilterCreator.ContentKeyword.unit.ToString()))
				{
					conditions.CharaTypeList.Add(CARD_TYPE.FOLLOWER);
				}
			}
		}
		_conditionsList.Add(conditions);
	}

	private void MergeTribeIntoHeadCondition()
	{
		if (_conditionsList.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder(_conditionsList[0].Tribe, 50);
			for (int i = 1; i < _conditionsList.Count; i++)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(_conditionsList[i].Tribe.Replace("eq", string.Empty));
			}
			_conditionsList[0].Tribe = stringBuilder.ToString();
			_conditionsList = _conditionsList.Take(1).ToList();
		}
	}

	public static List<ApplySkillTargetFilterCollection> SelectActiveApplyAndFilter(List<ApplySkillTargetFilterCollection> applyAndFilter, int activeApplyAndFilterIndex)
	{
		List<ApplySkillTargetFilterCollection> list = new List<ApplySkillTargetFilterCollection>();
		if (activeApplyAndFilterIndex == -1)
		{
			return list;
		}
		for (int i = 0; i < applyAndFilter.Count; i++)
		{
			if (activeApplyAndFilterIndex == i)
			{
				list.Add(applyAndFilter[i]);
			}
		}
		return list;
	}

	public Conditions SettingTargetStatus(BattleManagerBase mgr, SkillBase skill, List<ISkillCardFilter> applyCardFilterList, SkillConditionCheckerOption checkerOption, bool isDiscarded, int destroyLotCount = 0, CardDataModel unapprovedCard = null, NetworkBattleDefine.NetworkCardPlaceState state = NetworkBattleDefine.NetworkCardPlaceState.None)
	{
		Conditions conditions = new Conditions();
		for (int i = 0; i < applyCardFilterList.Count; i++)
		{
			ISkillCardFilter skillCardFilter = applyCardFilterList.ElementAt(i);
			if (state != NetworkBattleDefine.NetworkCardPlaceState.None)
			{
				conditions.State = state;
			}
			if (RegisterTool.HasTargetOverCostFromFilter(skill))
			{
				conditions.Cost.Add(RegisterTool.MakeCostFromSkillDestroyed(mgr, IsSelf, destroyLotCount, unapprovedCard));
			}
			else if (skillCardFilter is SkillParameterCostFilter || skillCardFilter is SkillParameterBaseCostFilter)
			{
				if (skill.ApplyingTargetFilter is SkillTargetInHandCardFilter)
				{
					if (skillCardFilter is SkillParameterBaseCostFilter)
					{
						conditions.BaseCost.AddRange(checkerOption.PopSkillTargetCondition());
					}
					else
					{
						conditions.InHandCost.AddRange(checkerOption.PopSkillTargetCondition());
					}
				}
				else if (skillCardFilter is SkillParameterCostFilter)
				{
					conditions.Cost.AddRange(checkerOption.PopSkillTargetCondition());
				}
				else if (_extractType == ExtractType.BaseCost)
				{
					conditions.BaseCost.AddRange(new List<string> { _extractString });
				}
				else
				{
					conditions.BaseCost.AddRange(checkerOption.PopSkillTargetCondition());
				}
			}
			if (skillCardFilter is SkillParameterOffenseFilter)
			{
				conditions.Atk = checkerOption.PopSkillTargetCondition()[0];
			}
			if (skillCardFilter is SkillParameterLifeFilter)
			{
				conditions.Life = checkerOption.PopSkillTargetCondition()[0];
			}
			if (skillCardFilter is SkillParameterBaseOffenseFilter)
			{
				conditions.BaseAtk = checkerOption.PopSkillTargetCondition()[0];
			}
			if (skillCardFilter is SkillParameterBaseLifeFilter)
			{
				conditions.BaseLife = checkerOption.PopSkillTargetCondition()[0];
			}
			if (skillCardFilter is SkillClanFilter)
			{
				SkillClanFilter skillClanFilter = skillCardFilter as SkillClanFilter;
				conditions.Clan = RegisterTool.MakeParameterOptionText(skillClanFilter.OptionText) + (int)skillClanFilter._clan;
			}
			if (skillCardFilter is SkillParameterBuffLifeCountFilter)
			{
				conditions.HasBuffLife = "1";
			}
			if (skillCardFilter is SkillParameterIdFilter)
			{
				NetworkBattleManagerBase networkBattleManagerBase = (NetworkBattleManagerBase)mgr;
				SkillParameterIdFilter skillParameterIdFilter = skillCardFilter as SkillParameterIdFilter;
				string optionText = skillParameterIdFilter.GetOptionText();
				if (optionText == "!=" || optionText == "=")
				{
					string[] filterId = skillParameterIdFilter.GetFilterId();
					for (int j = 0; j < filterId.Length; j++)
					{
						if (optionText == "!=")
						{
							conditions.ExcludeList.Add(int.Parse(filterId[j]));
						}
						else
						{
							if (!(optionText == "="))
							{
								continue;
							}
							if (filterId[j].Contains("last_target"))
							{
								conditions.BaseCardIdxList.AddRange(networkBattleManagerBase.networkBattleData.GetReceiveData().OpponentTargetDataList.Select((NetworkBattleReceiver.TargetData t) => t.TargetIndex));
							}
							else
							{
								conditions.IncludeIdList.Add(int.Parse(filterId[j]));
							}
						}
					}
				}
			}
			conditions.CharaTypeList.AddRange(RegisterTool.GetCardTypeList(skillCardFilter));
			if (_extractType == ExtractType.LastTribe)
			{
				conditions.Tribe = _extractString;
			}
			else if (skillCardFilter is SkillTribeFilter)
			{
				SkillTribeFilter skillTribeFilter = skillCardFilter as SkillTribeFilter;
				conditions.Tribe = RegisterTool.MakeParameterOptionText(skillTribeFilter.OptionText) + (int)skillTribeFilter._type;
			}
			CARD_TYPE charaTypeList = CARD_TYPE.NON;
			if (conditions.CharaTypeList.Count >= 1)
			{
				charaTypeList = conditions.CharaTypeList[conditions.CharaTypeList.Count - 1];
			}
			SPECIAL_LIBRARY sPECIAL_LIBRARY = RegisterTool.CalculationCardSkillType(skillCardFilter, charaTypeList);
			if (sPECIAL_LIBRARY != SPECIAL_LIBRARY.NONE)
			{
				if ((sPECIAL_LIBRARY == SPECIAL_LIBRARY.NOT_WHEN_ACCELERATE && conditions.SpecialLibrary == SPECIAL_LIBRARY.NOT_WHEN_CRYSTALLIZE) || (sPECIAL_LIBRARY == SPECIAL_LIBRARY.NOT_WHEN_CRYSTALLIZE && conditions.SpecialLibrary == SPECIAL_LIBRARY.NOT_WHEN_ACCELERATE))
				{
					sPECIAL_LIBRARY = SPECIAL_LIBRARY.NOT_WHEN_ACCELERATE_AND_WHEN_CRYSTALLIZE;
				}
				conditions.SpecialLibrary = sPECIAL_LIBRARY;
			}
		}
		if (skill.ApplySelectFilter is SkillLimitUpperCountFromNewestFilter && skill.IsTargetInHand())
		{
			BattlePlayerBase battlePlayerBase = null;
			if (skill.ApplyBattlePlayerFilter is SelfBattlePlayerFilter)
			{
				battlePlayerBase = skill.SkillPrm.ownerCard.SelfBattlePlayer;
			}
			else if (skill.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter)
			{
				battlePlayerBase = skill.SkillPrm.ownerCard.OpponentBattlePlayer;
			}
			if (battlePlayerBase != null)
			{
				conditions.HandIdxList.AddRange(battlePlayerBase.HandCardList.Select((BattleCardBase c) => c.Index));
				List<double> item = new List<double> { RegisterLotCardBase.RAND_MAX };
				_fixedRandomList.Add(item);
			}
		}
		if (skill.ApplySelectFilter is SkillCostNoDuplicationRandomSelectFilter)
		{
			conditions.Duplication = Conditions.DuplicationStatusKind.cost;
		}
		else if (skill.ApplySelectFilter is SkillIdNoDuplicationRandomSelectFilter)
		{
			conditions.Duplication = Conditions.DuplicationStatusKind.cardId;
		}
		conditions.IsDiscarded = isDiscarded;
		return conditions;
	}

	public NetworkBattleDefine.NetworkCardPlaceState GetFromPlaceState()
	{
		return FromPlaceState;
	}

	public SkillBase GetLotSkillBase()
	{
		return LotSkillBase;
	}

	public int GetSkillMovementNum()
	{
		return SkillMovementNum;
	}

	public virtual void SettingGroupIndexMsg(RegisterActionBase registerBase)
	{
		registerBase.PrivateGroupIndexMsg = GroupMsgList[0];
	}

	public virtual void SettingGroupIndexMsg(RegisterActionBase registerBase, int index)
	{
		registerBase.PrivateGroupIndexMsg = GroupMsgList[index];
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}

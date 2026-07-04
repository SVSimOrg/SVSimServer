using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkBattleSetupCardEvent
{
	protected BattleManagerBase _battleMgr;

	private RegisterActionManager _registerActionManager;

	private List<RegisterUnapproved> _registerUnapprovedList;

	private List<SkillBase> movementSkillList = new List<SkillBase>();

	protected NetworkBattleSetupBurialRitePlay _networkBattleSetupBurialRitePlay;

	protected NetworkBattleSetupValidateEvent _networkBattleSetupValidateEvent;

	public NetworkBattleData networkBattleData { get; private set; }

	public NetworkBattleSetupCardEvent(BattleManagerBase manager, RegisterActionManager registerCardList, NetworkBattleData data)
	{
		_battleMgr = manager;
		networkBattleData = data;
		_registerActionManager = registerCardList;
		_networkBattleSetupBurialRitePlay = new NetworkBattleSetupBurialRitePlay(this);
		_networkBattleSetupValidateEvent = new NetworkBattleSetupValidateEvent(_battleMgr as NetworkBattleManagerBase, this);
	}

	public virtual void SetupCardEvent(BattleManagerBase mgr, RegisterActionManager actionManager, BattleCardBase card, List<RegisterUnapproved> registerUnapprovedList)
	{
		_registerUnapprovedList = registerUnapprovedList;
		card.OnEvolveEvent += delegate(bool isSkill)
		{
			if (_registerActionManager.Last() is RegisterEvolution registerEvolution && registerEvolution.IsSelf == card.IsPlayer)
			{
				registerEvolution.AddIndex(card.Index);
			}
			else
			{
				_registerActionManager.Add(new RegisterEvolution(card.IsPlayer, card.Index));
			}
			if (!isSkill && !card.SelfBattlePlayer.CheckNotConsumeEpCard(card))
			{
				RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(card.SelfBattlePlayer);
				registerEnhanceTrigger.SettingUseEp();
				_registerActionManager.Add(registerEnhanceTrigger);
			}
		};
		if (!card.IsPlayer)
		{
			card.OnPlay += delegate
			{
				CheckToAddScanList(card, evol: false);
				return NullVfx.GetInstance();
			};
			card.OnEvolveEvent += delegate(bool isSkill)
			{
				if (!isSkill)
				{
					CheckToAddScanList(card, evol: true);
				}
			};
		}
		card.OnFusionEvent += delegate(List<BattleCardBase> ingredientCards)
		{
			_registerActionManager.Add(new RegisterFusion(card, ingredientCards));
		};
		card.OnAttachSkill += SkillEventSetting;
		card.OnCopySkillComplete += SetupCardSkillEvent;
		SetupCardSkillEvent(card);
	}

	public virtual void SetupCardSkillEvent(BattleCardBase card)
	{
		SetupPreAndPostEvolutionSkillEvents(card);
	}

	protected void SetupPreAndPostEvolutionSkillEvents(BattleCardBase card)
	{
		SkillEventSettingList(card, card.Skills);
		SkillEventSettingList(card, card.EvolutionSkills);
	}

	private void SkillEventSettingList(BattleCardBase card, SkillCollectionBase skillCollection)
	{
		foreach (SkillBase item in skillCollection)
		{
			SkillEventSetting(card, item);
			LastSkillEventSetting(item);
		}
	}

	protected virtual void SkillEventSetting(BattleCardBase card, SkillBase skill)
	{
		if (skill is Skill_none && !skill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite))
		{
			return;
		}
		NetworkExecutionInfoCreator networkExecutionInfoCreator = skill._executionInfoCreator as NetworkExecutionInfoCreator;
		bool flag = RegisterValidate.IsValidateCard(skill);
		NetworkBattleReceiver.ReceiveData receiveData = networkBattleData.GetReceiveData();
		skill.OnSkillStart -= RegisterSettingSkillPublicCount;
		skill.OnSkillStart += RegisterSettingSkillPublicCount;
		if (skill is Skill_shortage_deck_win)
		{
			skill.OnSkillEnd -= EventShortageDeckWin;
			skill.OnSkillEnd += EventShortageDeckWin;
		}
		if (RegisterAttach.IsAffiliationChangeCard(skill))
		{
			skill.OnSkillEnd -= Event_AttachSkillToHand;
			skill.OnSkillEnd += Event_AttachSkillToHand;
			skill.OnSkillEnd -= Event_AttachSkillToBoard;
			skill.OnSkillEnd += Event_AttachSkillToBoard;
		}
		SettingConditionValidateCard(skill, card, networkExecutionInfoCreator);
		if (RegisterSkillConditionCheck.IsSkillConditionCheck(skill))
		{
			if (skill.ConditionTargetFilter is SkillTargetDeckSelfFilter && skill is Skill_summon_card)
			{
				skill.OnSkillEnd -= DeckSkillConditionCheck;
				skill.OnSkillEnd += DeckSkillConditionCheck;
			}
			else
			{
				skill.OnSkillStart -= Event_SkillConditionCheck;
				skill.OnSkillStart += Event_SkillConditionCheck;
			}
		}
		else if (RegisterSkillConditionCheck.IsSelectedCardSkillConditionCheck(skill))
		{
			skill.OnSkillStart -= EventSelectedCardConditionCheck;
			skill.OnSkillStart += EventSelectedCardConditionCheck;
		}
		if (IsSettingUnapprovedCard(skill))
		{
			bool flag2 = RegisterValidate.IsOpenMyHandSkill(skill);
			if (flag2)
			{
				if (skill.SkillPrm.ownerCard.IsPlayer)
				{
					skill.OnSkillStart -= Event_RegisterValidate;
					skill.OnSkillStart += Event_RegisterValidate;
				}
				else
				{
					skill.OnSkillStart -= Event_RegisterOpenMyCards;
					skill.OnSkillStart += Event_RegisterOpenMyCards;
				}
			}
			else
			{
				if (IsNotSettingUnapproved(skill))
				{
					return;
				}
				if (skill.IsRandomUntilDrawSkill)
				{
					skill.OnSkillStart -= Event_SettingScanDataOnSkillStart;
					skill.OnSkillStart += Event_SettingScanDataOnSkillStart;
				}
				else if (RegisterScan.IsScanSkill(skill))
				{
					skill.OnInactiveSkill -= Event_SettingScanData;
					skill.OnInactiveSkill += Event_SettingScanData;
				}
				if (skill is Skill_draw || skill is Skill_summon_card)
				{
					if (RegisterScan.IsNotSelectScanSkill(skill))
					{
						skill.OnSkillEnd -= EventSettingNotSelectSkillScanData;
						skill.OnSkillEnd += EventSettingNotSelectSkillScanData;
					}
					else if (RegisterScan.IsScanLastTargetTribeDrawSkill(skill))
					{
						skill.OnSkillEnd -= EventSettingSkillLastTargetTribeDrawScanData;
						skill.OnSkillEnd += EventSettingSkillLastTargetTribeDrawScanData;
					}
				}
				if (IsChoiceTokenDrawSkill(skill))
				{
					skill.OnSkillEnd -= EventChoiceTokenDrawSetting;
					skill.OnSkillEnd += EventChoiceTokenDrawSetting;
				}
				if (RegisterFilter.IsFilterCard(skill))
				{
					if (!card.IsPlayer && RegisterFilter.IsHandAllSelect(skill))
					{
						networkExecutionInfoCreator.SetHandAllSelect();
					}
					else if (!card.IsPlayer && RegisterFilter.IsDeckAllSelect(skill))
					{
						networkExecutionInfoCreator.SetDeckAllSelect();
					}
					if (RegisterFilter.IsFilterCardUnapproved(skill))
					{
						networkExecutionInfoCreator.SetUnapproved();
					}
					skill.OnSkillStart -= Event_RegisterFilter;
					skill.OnSkillStart += Event_RegisterFilter;
					skill.OnSkillEnd -= Event_RegisterFilterSkillEnd;
					skill.OnSkillEnd += Event_RegisterFilterSkillEnd;
					skill.OnSkillStopStart -= Event_RegisterFilterStop;
					skill.OnSkillStopStart += Event_RegisterFilterStop;
					skill.OnSkillStopEnd -= Event_RegisterFilterSkillEnd;
					skill.OnSkillStopEnd += Event_RegisterFilterSkillEnd;
					return;
				}
				if (flag)
				{
					if (NetworkBattleGenericTool.IsBurialRite(skill))
					{
						networkExecutionInfoCreator.SetNotCheckBuriaRiteCondition(value: true);
					}
					else if (!flag2)
					{
						networkExecutionInfoCreator.SetUnapproved();
					}
					if (IsCheckValidateCard())
					{
						skill.OnSkillStart -= Event_RegisterValidate;
						skill.OnSkillStart += Event_RegisterValidate;
					}
				}
				if (!card.IsPlayer && receiveData != null)
				{
					_networkBattleSetupValidateEvent.OpponentPlayerIncludedValidateSkillToNotPlay(skill);
				}
				bool flag3 = SettingUnapprovedRegisterLotEvent(skill, networkExecutionInfoCreator);
				SettingReplaceSkillOption(skill, card, networkExecutionInfoCreator);
				if ((flag || flag3 || skill.ApplyingTargetFilter is SkillTargetSkillDrewCardFilter || skill.ApplyingTargetFilter is SkillTargetSelectedCardsFilter || skill.ApplySelectFilter is SkillSelectIndexFilter || (skill.OnWhenFusion != 0 && skill.ApplyingTargetFilter is SkillTargetSelfFilter) || (skill.OnWhenDraw != 0 && skill is NetworkSkill_attach_skill && skill.ApplyingTargetFilter is SkillTargetHandSelfFilter) || skill.ApplyingTargetFilter is SkillTargetLastTargetFilter) && RegisterAttach.IsAttachUnapprovedCard(skill))
				{
					skill.OnSkillEnd -= Event_AttachSkillToHand;
					skill.OnSkillEnd += Event_AttachSkillToHand;
				}
				if (RegisterAttach.IsPrivateBuffChangeCard(skill))
				{
					skill.OnSkillEnd -= EchoBuffSkillLastTargetCheck_ToAttachSkillToHand;
					skill.OnSkillEnd += EchoBuffSkillLastTargetCheck_ToAttachSkillToHand;
				}
				if (!card.IsPlayer)
				{
					SettingBurialRiteSkillPlayOrNotPlay(skill, networkExecutionInfoCreator);
				}
				if (RegisterValidate.IsSendOpenMyCardsSkill(skill) && skill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction))
				{
					skill.OnSkillEnd -= Event_OpenCardRemoveAfterAction;
					skill.OnSkillEnd += Event_OpenCardRemoveAfterAction;
				}
				CheckApplySelectFilter(skill, networkExecutionInfoCreator);
			}
			return;
		}
		if (RegisterValidate.IsSendOpenMyCardsSkill(skill))
		{
			skill.OnSkillStart -= Event_RegisterOpenMyCards;
			skill.OnSkillStart += Event_RegisterOpenMyCards;
			if (skill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction))
			{
				skill.OnSkillEnd -= Event_OpenCardRemoveAfterAction;
				skill.OnSkillEnd += Event_OpenCardRemoveAfterAction;
			}
			return;
		}
		if (skill is Skill_update_deck { IsOpen: not false })
		{
			skill.OnSkillStart -= Event_RegisterOpenMyCards;
			skill.OnSkillStart += Event_RegisterOpenMyCards;
		}
		if (skill is Skill_token_draw { IsVisibleTarget: false })
		{
			skill.OnSkillEnd -= Event_RegisterOpenMyCardsOnSkillEnd;
			skill.OnSkillEnd += Event_RegisterOpenMyCardsOnSkillEnd;
		}
		if (IsSendUnapprovedList(skill))
		{
			skill.OnSkillStart -= RegisterUnapproved.Event_SetApplyAndFilterIndex;
			skill.OnSkillStart += RegisterUnapproved.Event_SetApplyAndFilterIndex;
			if (!IsNotSettingUnapproved(skill))
			{
				skill.OnSkillEnd -= NetworkBattleGenericTool.Event_SetupPlayerUnapprovedAddEvent;
				skill.OnSkillEnd += NetworkBattleGenericTool.Event_SetupPlayerUnapprovedAddEvent;
			}
		}
		if (RegisterUnapproved.IsEventSettingSkillLastTarget(skill))
		{
			skill.OnSkillEnd -= LastTargetSkillSetupUnapproved;
			skill.OnSkillEnd += LastTargetSkillSetupUnapproved;
		}
		if (skill.OnDisCardStart != 0)
		{
			skill.OnSkillEnd -= Event_SetupPlayerUnapprovedSelfCard;
			skill.OnSkillEnd += Event_SetupPlayerUnapprovedSelfCard;
		}
		if (RegisterAttach.IsPrivateBuffChangeCard(skill))
		{
			skill.OnSkillEnd -= BuffSkillLastTargetCheck_ToAttachSkillToHand;
			skill.OnSkillEnd += BuffSkillLastTargetCheck_ToAttachSkillToHand;
		}
		if (RegisterAttach.IsAttachUnapprovedCard(skill))
		{
			skill.OnSkillEnd -= Event_AttachSkillToHand;
			skill.OnSkillEnd += Event_AttachSkillToHand;
		}
		if (IsCheckValidateCard())
		{
			_networkBattleSetupValidateEvent.SettingPlayerValidateEvent(skill);
		}
	}

	protected void Event_SettingScanData(SkillBase skillBase)
	{
		RegisterScan.OrganizeScanData(skillBase, _registerActionManager, (NetworkBattleManagerBase)_battleMgr);
	}

	protected void Event_SettingScanDataOnSkillStart(SkillBase skillBase, List<BattleCardBase> cards, SkillConditionCheckerOption option)
	{
		RegisterScan.OrganizeScanDataOnSkillStart(skillBase, _registerActionManager, (NetworkBattleManagerBase)_battleMgr, cards);
	}

	protected VfxBase EventSettingNotSelectSkillScanData(SkillBase skillBase, List<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		RegisterScan.OrganizeNotSelectSkillScanData(skillBase, _registerActionManager, (NetworkBattleManagerBase)_battleMgr, cards);
		return NullVfx.GetInstance();
	}

	protected VfxBase EventSettingSkillLastTargetTribeDrawScanData(SkillBase skillBase, List<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		RegisterScan.OrganizeSkillLastTargetTribeDrawScanData(skillBase, _registerActionManager, (NetworkBattleManagerBase)_battleMgr, cards);
		return NullVfx.GetInstance();
	}

	protected void SettingConditionValidateCard(SkillBase skill, BattleCardBase card, NetworkExecutionInfoCreator executionInfo)
	{
		if (card.IsPlayer || !IsSetNotCheckSelectSkillCard())
		{
			return;
		}
		if (NetworkBattleGenericTool.IsBurialRite(skill))
		{
			if (NetworkBattleGenericTool.IsReceiveSelectDataOnBurialRite(_battleMgr, skill))
			{
				executionInfo.SetNotCheckBuriaRiteCondition(value: true);
			}
		}
		else if (IsCheckValidateSkill(skill) && RegisterValidate.IsSetValidateConditionCheckSkill(skill))
		{
			executionInfo.SetValidateConditionCheckSkill();
		}
	}

	protected bool IsNotSettingUnapproved(SkillBase skill)
	{
		if (NetworkBattleGenericTool.IsNeedUnapprovedListSkill(skill) && skill is Skill_cost_change)
		{
			return true;
		}
		if (RegisterFilter.IsFilterCard(skill) && skill is Skill_cost_change)
		{
			return true;
		}
		return false;
	}

	protected bool SettingUnapprovedRegisterLotEvent(SkillBase skill, NetworkExecutionInfoCreator executionInfo)
	{
		bool result = false;
		if (NetworkBattleGenericTool.IsNeedUnapprovedListSkill(skill))
		{
			executionInfo.SetUnapproved();
			result = true;
			skill.OnSkillStart -= SetSkillTargetsConditionCheckUList;
			skill.OnSkillStart += SetSkillTargetsConditionCheckUList;
			skill.OnSkillEnd -= EventRegisterLotCardStart;
			skill.OnSkillEnd += EventRegisterLotCardStart;
			skill.OnSkillEnd -= EventRegisterLotCardSkillEnd;
			skill.OnSkillEnd += EventRegisterLotCardSkillEnd;
		}
		return result;
	}

	public void SetSkillTargetsConditionCheckUList(SkillBase skill, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option)
	{
		List<CardDataModel> unapprovedList = (_battleMgr as NetworkBattleManagerBase).networkBattleData.GetReceiveData().unapprovedList;
		NetworkExecutionInfoCreator networkExec = skill._executionInfoCreator as NetworkExecutionInfoCreator;
		foreach (BattleCardBase carddata in cards)
		{
			CardDataModel cardDataModel;
			if (RegisterSkillConditionCheck.DoesSkillUsePrivateCount(skill) && skill is Skill_powerup)
			{
				int skillConditionCount = RegisterSkillConditionCheck.GetMovementCount(skill);
				cardDataModel = unapprovedList.Find((CardDataModel x) => x.Index == carddata.Index && x.skillMovementNum / skillConditionCount == networkExec.GetSkillMovementNum() && x.publishedActiveSkillCount == skill.PublishedActiveSkillCount);
			}
			else
			{
				cardDataModel = unapprovedList.Find((CardDataModel x) => x.Index == carddata.Index && x.skillMovementNum == networkExec.GetSkillMovementNum() && x.publishedActiveSkillCount == skill.PublishedActiveSkillCount);
			}
			if (cardDataModel == null)
			{
				continue;
			}
			if (skill.ApplyAndFilter.Count > 0)
			{
				bool flag = skill.ApplySelectFilter is SkillRandomSelectFilter && skill.ApplyAndFilter.All((ApplySkillTargetFilterCollection f) => f.CardFilterList.Exists((ISkillCardFilter c) => c is SkillTribeFilter));
				List<ApplySkillTargetFilterCollection> list = skill.ApplyAndFilter;
				if (!(skill.ApplySelectFilter is SkillRandomSelectFilter) && cardDataModel != null && !flag)
				{
					list = RegisterTargetBase.SelectActiveApplyAndFilter(list, cardDataModel.RandomTargetIndex);
				}
				for (int num = 0; num < list.Count; num++)
				{
					SetSkillTargetCondition(skill, list[num].CardFilterList, option);
				}
				continue;
			}
			SetSkillTargetCondition(skill, skill.ApplyCardFilterList, option);
			break;
		}
	}

	public void SetSkillTargetsCondition(SkillBase skill, SkillConditionCheckerOption option)
	{
		if (skill.ApplyAndFilter.Count > 0)
		{
			List<ApplySkillTargetFilterCollection> applyAndFilter = skill.ApplyAndFilter;
			for (int i = 0; i < applyAndFilter.Count; i++)
			{
				SetSkillTargetCondition(skill, applyAndFilter[i].CardFilterList, option);
			}
		}
		else
		{
			SetSkillTargetCondition(skill, skill.ApplyCardFilterList, option);
		}
	}

	private void SetSkillTargetCondition(SkillBase skill, List<ISkillCardFilter> applyCardFilterList, SkillConditionCheckerOption option)
	{
		for (int i = 0; i < applyCardFilterList.Count; i++)
		{
			ISkillCardFilter skillCardFilter = applyCardFilterList.ElementAt(i);
			SkillCardFilterBase skillCardFilterBase = skillCardFilter as SkillCardFilterBase;
			if ((skillCardFilter is SkillParameterCostFilter || skillCardFilter is SkillParameterBaseCostFilter) ? (!RegisterTool.HasTargetOverCostFromFilter(skill)) : (skillCardFilterBase != null))
			{
				string parameterText = skillCardFilterBase.GetParameterText();
				string parameterOptionText = skillCardFilterBase.GetParameterOptionText();
				List<string> list = new List<string>();
				if (parameterText.Contains("min"))
				{
					list.Add("min");
				}
				else if (parameterText.Contains("max") && !parameterText.Contains("max_"))
				{
					list.Add("max");
				}
				if (list.Count == 0 || (parameterOptionText != "=" && parameterOptionText != "<:"))
				{
					List<string> list2 = RegisterTool.MakeParameterOptionTextList(parameterOptionText);
					list2[0] += skill.OptionValue.ParseInt(parameterText);
					list.AddRange(list2);
				}
				option?.SkillTargetConditionList.Add(list);
			}
		}
	}

	private VfxBase EventRegisterLotCardStart(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		if (cards != null)
		{
			NetworkBattleGenericTool.MakeRegisterLotAndRandomAdvance(skillBase, cards, checkerOption);
		}
		return NullVfx.GetInstance();
	}

	private VfxBase EventRegisterLotCardSkillEnd(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		NetworkBattleGenericTool.SettingRegisterTargetGroupAndInsert();
		return NullVfx.GetInstance();
	}

	protected void SettingBurialRiteSkillPlayOrNotPlay(SkillBase skill, NetworkExecutionInfoCreator executionInfo)
	{
		NetworkBattleSetupBurialRitePlay.CheckResult num = _networkBattleSetupBurialRitePlay.JudgeReceiveBurialRiteSkillPlayOrNotPlay(skill);
		if (num == NetworkBattleSetupBurialRitePlay.CheckResult.NotPlay)
		{
			executionInfo.SetNotPlaySkill();
		}
		if (num == NetworkBattleSetupBurialRitePlay.CheckResult.Play)
		{
			executionInfo.SetPlaySkill();
		}
	}

	public bool IsCheckValidateSkill(SkillBase skill)
	{
		if (RegisterValidate.IsValidateCard(skill))
		{
			return true;
		}
		return false;
	}

	private RegisterChoiceAdd MakeRegistChoiceSkillData(SkillBase skillBase)
	{
		SkillBase skillBase2 = skillBase.SkillPrm.ownerCard.Skills.SingleOrDefault((SkillBase s) => s is Skill_choice);
		if (skillBase2 == null)
		{
			return null;
		}
		List<int> list = SkillOptionValue.ParseOptionTokenID(skillBase2.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.card_id, string.Empty)).ToList();
		if (skillBase2.SkillPrm.ownerCard.BaseParameter.IsFoil)
		{
			for (int num = 0; num < list.Count(); num++)
			{
				list[num]++;
			}
		}
		RegisterChoiceAdd registerChoiceAdd = new RegisterChoiceAdd(skillBase2.SkillPrm.ownerCard, isChoice: true, NetworkBattleDefine.NetworkCardPlaceState.None, skillBase2);
		registerChoiceAdd.SettingChoiceCandidates(list);
		_registerActionManager.Add(registerChoiceAdd);
		return registerChoiceAdd;
	}

	private VfxBase EventChoiceTokenDrawSetting(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		RegisterChoiceAdd registerChoiceAdd = MakeRegistChoiceSkillData(skillBase);
		if (registerChoiceAdd == null)
		{
			return NullVfx.GetInstance();
		}
		foreach (RegisterToken item in _registerActionManager.RegisterDataList.FindAll((RegisterActionBase x) => x is RegisterToken && !(x is RegisterChoiceAdd)).ConvertAll((RegisterActionBase x) => x as RegisterToken))
		{
			registerChoiceAdd.SettingChoiceData(item.IndexList, item.ToPlaceState, item.RepeatCount);
			_registerActionManager.Remove(item);
		}
		return NullVfx.GetInstance();
	}

	public static bool IsChoiceTokenDrawSkill(SkillBase skill)
	{
		if ((skill is Skill_token_draw || skill is Skill_update_deck) && skill.IsTargetChoiceSelectSkill)
		{
			return true;
		}
		return false;
	}

	private void LastSkillEventSetting(SkillBase skill)
	{
		skill.OnSkillEnd -= Event_AddSkillMovementNum;
		skill.OnSkillEnd += Event_AddSkillMovementNum;
	}

	protected void CheckApplySelectFilter(SkillBase skill, NetworkExecutionInfoCreator executionInfo)
	{
		if (skill.ApplySelectFilter is SkillUserSelectFilter)
		{
			executionInfo.SetCheckOppoActionData();
		}
	}

	protected virtual bool IsSendUnapprovedList(SkillBase skill)
	{
		return NetworkBattleGenericTool.IsSendUnapprovedList(skill);
	}

	protected virtual bool IsCheckValidateCard()
	{
		return true;
	}

	protected virtual bool IsCheckSkillConditionCard(BattleCardBase card)
	{
		return true;
	}

	protected virtual bool CheckSkillCondition(SkillBase skill)
	{
		if (!RegisterSkillConditionCheck.IsSkillConditionCheck(skill))
		{
			return RegisterSkillConditionCheck.IsSelectedCardSkillConditionCheck(skill);
		}
		return true;
	}

	public virtual bool IsSettingUnapprovedCard(SkillBase skill)
	{
		return IsPrivateTargetSkill(skill);
	}

	protected virtual bool IsPrivateTargetSkill(SkillBase skill)
	{
		if (!skill.SkillPrm.ownerCard.IsPlayer)
		{
			return true;
		}
		if (skill.SkillPrm.ownerCard.IsPlayer && skill.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter && (skill.ApplyingTargetFilter is SkillTargetHandFilter || skill.ApplyingTargetFilter is SkillTargetDeckFilter || skill.ConditionTargetFilter is SkillTargetDeckSelfFilter))
		{
			return true;
		}
		return false;
	}

	protected virtual bool IsSetNotCheckSelectSkillCard()
	{
		return true;
	}

	private VfxBase EventShortageDeckWin(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		_registerActionManager.Add(new RegisterShortageDeckWin(skillBase.SkillPrm.ownerCard.IsPlayer));
		return NullVfx.GetInstance();
	}

	private void Event_RegisterFilterStop(SkillBase skillBase, List<BattleCardBase> cards, SkillProcessor skillProcessor)
	{
		if (!skillBase.SkillPrm.ownerCard.IsSkillLost)
		{
			RegisterFilterSetting(skillBase, cards, isStop: true, new SkillConditionCheckerOption());
		}
	}

	private void Event_RegisterFilter(SkillBase skillBase, List<BattleCardBase> cards, SkillConditionCheckerOption option)
	{
		if (!skillBase.SkillPrm.ownerCard.IsSkillLost)
		{
			RegisterFilterSetting(skillBase, cards, isStop: false, option);
		}
	}

	private void RegisterFilterSetting(SkillBase skillBase, List<BattleCardBase> cards, bool isStop, SkillConditionCheckerOption option)
	{
		bool flag = (skillBase.ApplyingTargetFilter is SkillTargetSkillDrewCardFilter || (skillBase.ApplyingTargetFilter is SkillTargetLoadTargetFilter && skillBase.OnBeforeAttackStart != 0)) && RegisterFilter.IsDetailCheckSkillDrewCard(skillBase);
		if (!RegisterFilter.IsNeedFilter(_battleMgr, isplayer: false, skillBase, cards, isStop) && !flag)
		{
			return;
		}
		bool flag2 = skillBase.ApplyBattlePlayerFilter is SelfBattlePlayerFilter;
		if (!skillBase.SkillPrm.ownerCard.IsPlayer)
		{
			flag2 = !flag2;
		}
		SetSkillTargetsCondition(skillBase, option);
		bool flag3 = skillBase is NetworkSkill_attach_skill && skillBase.ApplyingTargetFilter is NetworkSkillTargetLastTargetFilter;
		if (skillBase.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skillBase.ApplyAndFilter.Count; i++)
			{
				flag3 |= skillBase is NetworkSkill_attach_skill && skillBase.ApplyAndFilter[i].TargetFilter is NetworkSkillTargetLastTargetFilter && _registerActionManager.RegisterDataList.Any((RegisterActionBase x) => x is RegisterFilter);
			}
		}
		RegisterFilter registerFilter = (flag3 ? ((RegisterFilter)_registerActionManager.GetLast((RegisterActionBase x) => x is RegisterFilter)) : new RegisterFilter(_registerActionManager, _battleMgr, flag2, skillBase, cards, isStop, option));
		if (!flag3)
		{
			_registerActionManager.Add(registerFilter);
		}
		List<RegisterActionBase> list = registerFilter.AddSettingExecOrder(skillBase, isStop);
		if (list != null)
		{
			for (int num = 0; num < list.Count; num++)
			{
				_registerActionManager.Add(list[num]);
			}
		}
		_registerActionManager.SetStopAdd(flag: true);
	}

	public VfxBase Event_RegisterFilterSkillEnd(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		_registerActionManager.SetStopAdd(flag: false);
		return NullVfx.GetInstance();
	}

	public void Event_RegisterValidate(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option)
	{
		if (NetworkBattleGenericTool.IsBurialRite(skillBase))
		{
			if (!skillBase.IsInvoked && networkBattleData.GetReceiveData().OpponentTargetDataList != null)
			{
				List<BattleCardBase> opposingCardObjTarget = NetworkBattleGenericTool.GetOpposingCardObjTarget(_battleMgr, networkBattleData.GetReceiveData().OpponentTargetDataList);
				if (opposingCardObjTarget.Count == 0)
				{
					Event_SettingScanData(skillBase);
				}
				else
				{
					AddValidateCard(skillBase, opposingCardObjTarget);
				}
			}
		}
		else if (skillBase.IsRandomUntilDrawSkill && cards.Count() >= 1)
		{
			AddRandomSelectUntilValidateCard(skillBase, cards);
		}
		else if (skillBase.ApplyingTargetFilter is SkillTargetDeckFilter && skillBase.ApplySelectFilter is SkillRandomEachSameBaseCardIdFilter)
		{
			AddCheckDuplicateCard(skillBase, cards);
		}
		else if (cards.Count() >= 1)
		{
			if (RegisterValidate.IsOpenMyHandSkill(skillBase))
			{
				AddValidateOpponentHandCard(cards.ToList());
			}
			else
			{
				AddValidateCard(skillBase, cards.ToList());
			}
		}
		else if (cards.Count() == 0)
		{
			Event_SettingScanData(skillBase);
		}
	}

	private void AddValidateCard(SkillBase skill, List<BattleCardBase> cards)
	{
		RegisterValidate registerValidate = new RegisterValidate();
		registerValidate.AddValidateData(skill);
		_registerActionManager.Add(registerValidate);
		if (skill.IsBurialRite)
		{
			registerValidate.IndexList.AddRange(from c in cards
				where c.DeathTypeInfo.BurialRite
				select c.Index);
		}
		else
		{
			registerValidate.IndexList.AddRange(cards.Select((BattleCardBase c) => c.Index));
		}
	}

	private void AddValidateOpponentHandCard(List<BattleCardBase> cards)
	{
		for (int i = 0; i < cards.Count; i++)
		{
			RegisterValidate registerValidate = new RegisterValidate();
			registerValidate.SetCardIdValidateData(cards[i]);
			_registerActionManager.Add(registerValidate);
		}
	}

	private void AddRandomSelectUntilValidateCard(SkillBase skillBase, IEnumerable<BattleCardBase> cards)
	{
		RegisterValidate registerValidate = new RegisterValidate();
		_registerActionManager.Add(registerValidate);
		registerValidate.AddValidateData(skillBase);
		registerValidate.IndexList.AddRange(from c in cards
			where c != cards.Last()
			select c.Index);
	}

	private void AddCheckDuplicateCard(SkillBase skillBase, IEnumerable<BattleCardBase> cards)
	{
		RegisterValidate registerValidate = new RegisterValidate();
		_registerActionManager.Add(registerValidate);
		registerValidate.AddValidateData(skillBase);
		registerValidate.IndexList.AddRange(cards.Select((BattleCardBase c) => c.Index));
	}

	private VfxBase LastTargetSkillSetupUnapproved(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		List<int> list = new List<int>();
		foreach (RegisterUnapproved registerUnapproved in _registerUnapprovedList)
		{
			list.Add(registerUnapproved.IndexList[0]);
		}
		if (NetworkBattleGenericTool.IsIncludedCard(list, cards))
		{
			return NetworkBattleGenericTool.Event_SetupPlayerUnapprovedAddEvent(skillBase, cards, checkerOption, skillProcessor);
		}
		return NullVfx.GetInstance();
	}

	private VfxBase Event_SetupPlayerUnapprovedSelfCard(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		BattleCardBase ownerCard = skillBase.SkillPrm.ownerCard;
		NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(skillBase.SkillPrm.ownerCard.SelfBattlePlayer, ownerCard.Index);
		NetworkBattleDefine.NetworkCardPlaceState networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Hand;
		_registerUnapprovedList.Add(new RegisterUnapproved(skillBase, ownerCard, networkCardPlaceState, cardPlaceState, 0, isCardId: true));
		return NullVfx.GetInstance();
	}

	private VfxBase DeckSkillConditionCheck(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		Event_SkillConditionCheck(skillBase, cards);
		return NullVfx.GetInstance();
	}

	public void Event_SkillConditionCheck(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		bool flag = false;
		if (_registerActionManager.RegisterDataList.Find((RegisterActionBase x) => x is RegisterSkillConditionCheck && (x as RegisterSkillConditionCheck).IsInvoked == skillBase.IsInvoked) != null)
		{
			flag = true;
		}
		else if (skillBase.OnWhenPlayOtherStart != 0)
		{
			flag = true;
		}
		if (option == null)
		{
			option = new SkillConditionCheckerOption();
		}
		bool flag2 = skillBase.IsLastTargetDiscardOrBanishSkill(option, isOnlyCheckBanish: true);
		List<BattleCardBase> targetCards = (flag2 ? skillBase.SkillPrm.ownerCard.SelfBattlePlayer.SkillBanishCards : skillBase.SkillPrm.ownerCard.SelfBattlePlayer.SkillDiscards);
		foreach (RegisterSkillConditionCheck item in RegisterSkillConditionCheck.CreateList(skillBase.SkillPrm.ownerCard.Index, NetworkBattleGenericTool.GetPublishSkillCount(skillBase), skillBase, option.SelectedCards, option.ProcessSkillList, _registerActionManager.RegisterDataList, targetCards, !flag2 && skillBase.IsLastTargetDiscardOrBanishSkill(option), flag2))
		{
			if (item.ConditionType == RegisterSkillConditionCheck.SkillConditionType.callCount && _registerActionManager.HasSkillConditionCheckCallCount(skillBase))
			{
				continue;
			}
			_registerActionManager.Add(item);
			if (!flag)
			{
				continue;
			}
			List<RegisterActionBase> registerDataList = _registerActionManager.RegisterDataList;
			for (int num = 1; num < registerDataList.Count; num++)
			{
				if (!(registerDataList[num] is RegisterSkillConditionCheck) || item != registerDataList[num])
				{
					continue;
				}
				int num2 = num;
				int index = num;
				for (int num3 = num2 - 1; num3 > 0; num3--)
				{
					RegisterActionBase value = registerDataList[index];
					RegisterActionBase registerActionBase = registerDataList[num3];
					if ((registerActionBase is RegisterStateChangeCard { Skill: var skill } registerStateChangeCard && ((skill != null && skill is Skill_discard && skill.IsUserSelectType) || (num3 > 0 && registerDataList[num3 - 1] is RegisterMetamorphoseData && (registerStateChangeCard.StateCard.TransformInfo.Type == BattleCardBase.TransformType.Accelerate || registerStateChangeCard.StateCard.TransformInfo.Type == BattleCardBase.TransformType.Crystallize || registerStateChangeCard.StateCard.TransformInfo.Type == BattleCardBase.TransformType.Choice)))) || (registerActionBase is RegisterSkillConditionCheck && (skillBase.OnWhenPlayOtherStart == 0 || !skillBase.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => x.Text.Contains(SkillFilterCreator.ContentKeyword.hand.ToString()) && x.Text.Contains(SkillFilterCreator.ContentKeyword.base_card_id.ToString())))))
					{
						break;
					}
					registerDataList[index] = registerActionBase;
					registerDataList[num3] = value;
					index = num3;
				}
			}
		}
	}

	public void EventDiscardOrBanishSkillConditionCheck(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		if (skillBase.IsLastTargetDiscardOrBanishSkill(option) && RegisterSkillConditionCheck.CheckLastTargetFilter(skillBase))
		{
			(_battleMgr as NetworkBattleManagerBase)._networkBattleSetupCardEventBase.Event_SkillConditionCheck(skillBase, cards, option);
		}
	}

	public void EventSelectedCardConditionCheck(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		if (option.SelectedCards.Any((SkillConditionCheckerOption.SkillAndSelectTarget s) => s.SelectSkill.ApplyingTargetFilter is SkillTargetHandFilter || s.SelectSkill.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter || s.SelectSkill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetHandFilter || f.TargetFilter is SkillTargetHandOtherSelfFilter)))
		{
			(_battleMgr as NetworkBattleManagerBase)._networkBattleSetupCardEventBase.Event_SkillConditionCheck(skillBase, cards, option);
		}
	}

	public void EventPreprocessConditionCheck(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		if (RegisterSkillConditionCheck.IsPreprocessConditionCheck(skillBase))
		{
			(_battleMgr as NetworkBattleManagerBase)._networkBattleSetupCardEventBase.Event_SkillConditionCheck(skillBase, cards, option);
		}
	}

	private VfxBase Event_AttachSkillToHand(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		List<BattleCardBase> list = cards.ToList().FindAll((BattleCardBase x) => NetworkBattleGenericTool.GetCardPlaceState(x.SelfBattlePlayer, x.Index) == NetworkBattleDefine.NetworkCardPlaceState.Hand || NetworkBattleGenericTool.GetCardPlaceState(x.SelfBattlePlayer, x.Index) == NetworkBattleDefine.NetworkCardPlaceState.Deck);
		if (list.Count() >= 1)
		{
			if (skillBase.ApplyingTargetFilter is SkillTargetSelectedCardsFilter && (checkerOption == null || checkerOption.SelectedCards.Count <= 0 || !NetworkBattleGenericTool.IsUnapprovedTarget(checkerOption.SelectedCards[0].SelectSkill)))
			{
				return NullVfx.GetInstance();
			}
			_registerActionManager.Add(new RegisterAttach(list, skillBase));
		}
		return NullVfx.GetInstance();
	}

	private VfxBase Event_OpenCardRemoveAfterAction(SkillBase skill, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		_registerActionManager.Add(new RegisterAttach(new List<BattleCardBase> { skill.SkillPrm.ownerCard }, skill));
		return NullVfx.GetInstance();
	}

	private VfxBase Event_AttachSkillToBoard(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		if (skillBase is Skill_change_affiliation && cards.Any((BattleCardBase s) => s.IsInplay))
		{
			_registerActionManager.Add(new RegisterAttach(cards.ToList(), skillBase));
		}
		return NullVfx.GetInstance();
	}

	private bool IsNoSelectlastTargetSkill(SkillBase skill)
	{
		if (skill.ApplyingTargetFilter is SkillTargetLastTargetFilter)
		{
			return !skill.SkillPrm.ownerCard.Skills.Any((SkillBase s) => s.IsUserSelectType);
		}
		return false;
	}

	private VfxBase BuffSkillLastTargetCheck_ToAttachSkillToHand(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		if (IsNoSelectlastTargetSkill(skillBase))
		{
			List<int> list = new List<int>();
			foreach (RegisterUnapproved registerUnapproved in _registerUnapprovedList)
			{
				list.Add(registerUnapproved.IndexList[0]);
			}
			if (!NetworkBattleGenericTool.IsIncludedCard(list, cards))
			{
				return NullVfx.GetInstance();
			}
		}
		return Event_AttachSkillToHand(skillBase, cards, checkerOption, skillProcessor);
	}

	private VfxBase EchoBuffSkillLastTargetCheck_ToAttachSkillToHand(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		if (IsNoSelectlastTargetSkill(skillBase))
		{
			NetworkBattleReceiver.ReceiveData receiveData = networkBattleData.GetReceiveData();
			receiveData.unapprovedList.Count();
			List<int> list = new List<int>();
			foreach (CardDataModel unapproved in receiveData.unapprovedList)
			{
				list.Add(unapproved.Index);
			}
			if (!NetworkBattleGenericTool.IsIncludedCard(list, cards))
			{
				return NullVfx.GetInstance();
			}
		}
		return Event_AttachSkillToHand(skillBase, cards, checkerOption, skillProcessor);
	}

	protected void SettingReplaceSkillOption(SkillBase skill, BattleCardBase card, NetworkExecutionInfoCreator executionInfo, bool isSetReceiveSkillConditionCheck = true)
	{
		if (IsCheckSkillConditionCard(card) && CheckSkillCondition(skill))
		{
			if (isSetReceiveSkillConditionCheck)
			{
				executionInfo.SetReceiveSkillConditionCheck();
			}
			if (RegisterSkillConditionCheck.DoesSkillCallCountUseSkillDrewCard(skill))
			{
				skill.OnBeforeProcess -= ReplaceSkillCallCount;
				skill.OnBeforeProcess += ReplaceSkillCallCount;
			}
			else
			{
				skill.OnSkillStart -= ReplaceSkillOption;
				skill.OnSkillStart += ReplaceSkillOption;
			}
		}
	}

	private bool IsSkillConditionCheckListNullOrEmpty()
	{
		NetworkBattleManagerBase networkBattleManagerBase = _battleMgr as NetworkBattleManagerBase;
		if (networkBattleManagerBase.networkBattleData == null || networkBattleManagerBase.networkBattleData.GetReceiveData() == null || networkBattleManagerBase.networkBattleData.GetReceiveData().SkillConditionCheckList == null || networkBattleManagerBase.networkBattleData.GetReceiveData().SkillConditionCheckList.Count() == 0)
		{
			return true;
		}
		return false;
	}

	protected void ReplaceSkillCallCount(SkillBase skill)
	{
		if (IsSkillConditionCheckListNullOrEmpty())
		{
			return;
		}
		NetworkBattleManagerBase obj = _battleMgr as NetworkBattleManagerBase;
		BattleCardBase ownerCard = skill.SkillPrm.ownerCard;
		List<CardDataModel> list = obj.SearchSkillConditionCheckDataList(skillMovement: (skill._executionInfoCreator as NetworkExecutionInfoCreator).GetSkillMovementNum(), cardIdx: ownerCard.Index, publishSkillCount: NetworkBattleGenericTool.GetPublishSkillCount(skill), skillConditionCount: RegisterSkillConditionCheck.GetMovementCount(skill));
		if (list == null)
		{
			return;
		}
		foreach (CardDataModel item in list)
		{
			if (item.SkillCallCount != -1 && !int.TryParse(skill.CallCountTextValue, out var _))
			{
				skill.CallCountTextValue = item.SkillCallCount.ToString();
			}
		}
	}

	private void ReplaceParameter(SkillBase skill, int count, int optionIndex)
	{
		SkillFilterCreator.ContentKeyword replaceOption = GetReplaceOption(skill, optionIndex);
		if (replaceOption == SkillFilterCreator.ContentKeyword.none)
		{
			return;
		}
		string option = skill.OptionValue.GetOption(replaceOption);
		int num = option.IndexOfAny("*".ToCharArray());
		if (num >= 1)
		{
			string s = option.Substring(0, num);
			int result = 0;
			if (int.TryParse(s, out result))
			{
				count *= result;
			}
		}
		int num2 = option.IndexOfAny("/".ToCharArray());
		if (num2 >= 1)
		{
			string s2 = option.Substring(num2 + 1, option.Length - num2 - 1);
			int result2 = 0;
			if (int.TryParse(s2, out result2))
			{
				count /= result2;
			}
		}
		skill.OptionValue.SettingReplaceIntData(new SkillOptionValue.ReplaceDataOptionValue(replaceOption, count, skill));
	}

	public void ReplaceSkillOption(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		if (IsSkillConditionCheckListNullOrEmpty() || RegisterSkillConditionCheck.IsContainPreprocessLoadOrLastTarget(skillBase))
		{
			return;
		}
		NetworkBattleManagerBase obj = _battleMgr as NetworkBattleManagerBase;
		BattleCardBase ownerCard = skillBase.SkillPrm.ownerCard;
		List<CardDataModel> list = obj.SearchSkillConditionCheckDataList(skillMovement: (skillBase._executionInfoCreator as NetworkExecutionInfoCreator).GetSkillMovementNum(), cardIdx: ownerCard.Index, publishSkillCount: NetworkBattleGenericTool.GetPublishSkillCount(skillBase), skillConditionCount: RegisterSkillConditionCheck.GetMovementCount(skillBase));
		if (list == null || list.Count() < 1)
		{
			return;
		}
		int num = 0;
		foreach (CardDataModel item in list)
		{
			if (item.SkillValueCount != -1)
			{
				ReplaceParameter(skillBase, item.SkillValueCount, num);
			}
			else if (item.SkillValueParameter.HasValue)
			{
				ReplaceParameter(skillBase, item.SkillValueParameter.Value, num);
			}
			num++;
		}
	}

	public void ReplaceLastTargetDiscardOrBanishSkillOption(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		if (skillBase.IsLastTargetDiscardOrBanishSkill(option) && RegisterSkillConditionCheck.CheckLastTargetFilter(skillBase))
		{
			ReplaceSkillOption(skillBase, cards, option);
		}
	}

	public void ReplacePreprocessConditionCheck(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		if (RegisterSkillConditionCheck.IsPreprocessConditionCheck(skillBase))
		{
			ReplaceSkillOption(skillBase, cards, option);
		}
	}

	private SkillFilterCreator.ContentKeyword GetReplaceOption(SkillBase skill, int optionIndex = 0)
	{
		if (skill is Skill_damage)
		{
			return SkillFilterCreator.ContentKeyword.damage;
		}
		if (skill is Skill_powerup)
		{
			int num = skill.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_offense, -1, isRemoveReplaceData: false);
			int num2 = skill.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_life, -1, isRemoveReplaceData: false);
			if (num != -1 && num2 != -1)
			{
				if (optionIndex % 2 == 0)
				{
					return SkillFilterCreator.ContentKeyword.add_offense;
				}
				return SkillFilterCreator.ContentKeyword.add_life;
			}
			if (num != -1)
			{
				return SkillFilterCreator.ContentKeyword.add_offense;
			}
			if (num2 != -1)
			{
				return SkillFilterCreator.ContentKeyword.add_life;
			}
		}
		else
		{
			if (skill is Skill_summon_token)
			{
				return SkillFilterCreator.ContentKeyword.repeat_count;
			}
			if (skill is Skill_heal)
			{
				return SkillFilterCreator.ContentKeyword.healing;
			}
			if (skill is Skill_cost_change)
			{
				if (skill.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add, -1, isRemoveReplaceData: false) != -1)
				{
					return SkillFilterCreator.ContentKeyword.add;
				}
			}
			else if (skill is Skill_pp_modifier)
			{
				if (skill.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_pp, -1, isRemoveReplaceData: false) != -1)
				{
					return SkillFilterCreator.ContentKeyword.add_pp;
				}
			}
			else if (skill is Skill_chant_count_change)
			{
				return SkillFilterCreator.ContentKeyword.gain_chant;
			}
		}
		return SkillFilterCreator.ContentKeyword.none;
	}

	private VfxBase Event_AddSkillMovementNum(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		(skillBase._executionInfoCreator as NetworkExecutionInfoCreator).AddSkillMovementNum();
		if (!movementSkillList.Contains(skillBase))
		{
			movementSkillList.Add(skillBase);
		}
		return NullVfx.GetInstance();
	}

	private void RegisterSettingSkillPublicCount(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption = null)
	{
		_registerActionManager.SettingRecodeSkillCountData(skillBase);
	}

	public void ClearSkillMovement()
	{
		foreach (SkillBase movementSkill in movementSkillList)
		{
			(movementSkill._executionInfoCreator as NetworkExecutionInfoCreator).ClearSkillMovementNum();
		}
		movementSkillList.Clear();
	}

	private void CheckToAddScanList(BattleCardBase card, bool evol)
	{
		if (evol || !card.IsCantActivateFanfare)
		{
			_registerActionManager.SetPlayActionCardBase(card, evol);
		}
	}

	public void AddRegisterActionManager(RegisterActionBase register)
	{
		_registerActionManager.Add(register);
	}

	private void Event_RegisterOpenMyCards(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption option = null)
	{
		if (RegisterValidate.IsSendOpenMyCardsSkill(skillBase))
		{
			_registerActionManager.Add(new RegisterOpenMyCards(new List<BattleCardBase> { skillBase.SkillPrm.ownerCard }));
		}
		else if (skillBase is Skill_update_deck && (skillBase as Skill_update_deck).IsOpen)
		{
			_registerActionManager.Add(new RegisterOpenMyCards(option.SelectedCards.Select((SkillConditionCheckerOption.SkillAndSelectTarget c) => c.SelectCard).ToList()));
		}
		else
		{
			_registerActionManager.Add(new RegisterOpenMyCards(_battleMgr.BattlePlayer.HandCardList, notBuff: true));
		}
	}

	private VfxBase Event_RegisterOpenMyCardsOnSkillEnd(SkillBase skill, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		NetworkSkill_token_draw tokenDrawSkill = skill as NetworkSkill_token_draw;
		if (tokenDrawSkill.TokenModifierList.Count > 0 && tokenDrawSkill.IsInvisibleTarget())
		{
			IEnumerable<BattleCardBase> source;
			if (tokenDrawSkill.DrawList.Count <= 0)
			{
				source = cards;
			}
			else
			{
				IEnumerable<BattleCardBase> drawList = tokenDrawSkill.DrawList;
				source = drawList;
			}
			List<BattleCardBase> list = source.Where((BattleCardBase c) => tokenDrawSkill.TokenModifierList.Contains(c.CardId)).ToList();
			if (list.Count > 0)
			{
				_registerActionManager.Add(new RegisterOpenMyCards(list));
			}
		}
		return NullVfx.GetInstance();
	}
}

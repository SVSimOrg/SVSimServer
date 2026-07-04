using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class NetworkBattleGenericTool
{
	public static NetworkBattleDefine.NetworkCardPlaceState GetCardPlaceState(BattlePlayerBase player, int index)
	{
		NetworkBattleDefine.NetworkCardPlaceState result = NetworkBattleDefine.NetworkCardPlaceState.None;
		if (player.HandCardList != null && player.HandCardList.SingleOrDefault((BattleCardBase c) => c.Index == index) != null)
		{
			result = NetworkBattleDefine.NetworkCardPlaceState.Hand;
		}
		else if (player.DeckCardList != null && player.DeckCardList.SingleOrDefault((BattleCardBase c) => c.Index == index) != null)
		{
			result = NetworkBattleDefine.NetworkCardPlaceState.Deck;
		}
		else if (player.ClassAndInPlayCardList != null && player.ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c.Index == index) != null)
		{
			result = NetworkBattleDefine.NetworkCardPlaceState.Field;
		}
		else if (player.CemeteryList != null && player.CemeteryList.FirstOrDefault((BattleCardBase c) => c.Index == index) != null)
		{
			result = NetworkBattleDefine.NetworkCardPlaceState.Cemetery;
		}
		else if (player.BanishList != null && player.BanishList.FirstOrDefault((BattleCardBase c) => c.Index == index) != null)
		{
			result = NetworkBattleDefine.NetworkCardPlaceState.Banish;
		}
		else if (player.FusionIngredientList != null && player.FusionIngredientList.SingleOrDefault((BattleCardBase c) => c.Index == index) != null)
		{
			result = NetworkBattleDefine.NetworkCardPlaceState.FusionIngredient;
		}
		if (player.UniteList != null && player.UniteList.FirstOrDefault((BattleCardBase c) => c.Index == index) != null)
		{
			result = NetworkBattleDefine.NetworkCardPlaceState.Unite;
		}
		return result;
	}

	public static BattleCardBase GetIndexToCardBase(BattleManagerBase battleManagerBase, BattlePlayerBase player, int index)
	{
		BattleCardBase battleCardBase = null;
		if (player.HandCardList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.HandCardList, index);
		}
		if (battleCardBase == null && player.DeckCardList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.DeckCardList, index);
		}
		if (battleCardBase == null && player.ClassAndInPlayCardList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.ClassAndInPlayCardList, index);
		}
		if (battleCardBase == null && player.CemeteryList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.CemeteryList, index);
		}
		if (battleCardBase == null && player.NecromanceZoneList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.NecromanceZoneList, index);
		}
		if (battleCardBase == null && player.BanishList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.BanishList, index);
		}
		if (battleCardBase == null && player.FusionIngredientList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.FusionIngredientList, index);
		}
		if (battleCardBase == null && player.ClassAndInPlayCardList != null)
		{
			List<BattleCardBase> list = new List<BattleCardBase>(player.ClassAndInPlayCardList);
			if (player.CemeteryList != null)
			{
				list.AddRange(player.CemeteryList);
			}
			if (player.BanishList != null)
			{
				list.AddRange(player.BanishList);
			}
			for (int i = 0; i < list.Count; i++)
			{
				BattleCardBase battleCardBase2 = list[i];
				if (!(battleCardBase2 is NullBattleCard))
				{
					battleCardBase = battleCardBase2.GetOnCards.FirstOrDefault((BattleCardBase c) => c.Index == index);
					if (battleCardBase != null)
					{
						break;
					}
				}
			}
		}
		if (battleCardBase == null && player.UniteList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.UniteList, index);
		}
		if (battleCardBase == null && player.GetOnList != null)
		{
			battleCardBase = battleManagerBase.GetBattleCardIdx(player.GetOnList, index);
		}
		return battleCardBase;
	}

	public static List<BattleCardBase> GetOpposingCardObjTarget(BattleManagerBase battleManagerBase, List<NetworkBattleReceiver.TargetData> actions)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (NetworkBattleReceiver.TargetData action in actions)
		{
			int targetIndex = action.TargetIndex;
			bool isSelf = action.IsSelf;
			BattleCardBase battleCardBase = null;
			battleCardBase = ((!isSelf) ? battleManagerBase.GetBattleCardIdx(battleManagerBase.BattlePlayer.AllCards.ToList(), targetIndex) : GetIndexToCardBase(battleManagerBase, battleManagerBase.BattleEnemy, targetIndex));
			if (battleCardBase != null)
			{
				list.Add(battleCardBase);
			}
		}
		return list;
	}

	public static List<BattleCardBase> LookForActionDataToTargetCard(BattleManagerBase battleManagerBase, List<NetworkBattleReceiver.TargetData> actions)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (NetworkBattleReceiver.TargetData action in actions)
		{
			int targetIndex = action.TargetIndex;
			bool isSelf = action.IsSelf;
			BattleCardBase battleCardBase = null;
			battleCardBase = ((!isSelf) ? GetIndexToCardBase(battleManagerBase, battleManagerBase.BattlePlayer, targetIndex) : battleManagerBase.GetBattleCardIdx(battleManagerBase.BattleEnemy.ClassAndInPlayCardList, targetIndex));
			list.Add(battleCardBase);
		}
		return list;
	}

	public static int GetSkillIndex(SkillBase skill)
	{
		int num = 0;
		SkillCollectionBase skillCollectionBase = skill.SkillPrm.ownerCard.Skills;
		if (skill.OnWhenEvolveStart != 0 || skill.OnWhenEvolveOtherStart != 0 || skill.OnWhenEvolveSelfAndOtherStart != 0 || skill.OnWhenChoiceEvolveStart != 0 || skill.OnWhenEvolveBeforeStart != 0)
		{
			skillCollectionBase = skill.SkillPrm.ownerCard.EvolutionSkills;
		}
		foreach (SkillBase item in skillCollectionBase)
		{
			if (!(item is Skill_none))
			{
				if (item == skill)
				{
					break;
				}
				num++;
			}
		}
		return num;
	}

	public static SkillBase SerchIndexToSkill(SkillCollectionBase skills, int skillIndex)
	{
		int num = 0;
		foreach (SkillBase skill in skills)
		{
			if (!(skill is Skill_none))
			{
				if (num == skillIndex)
				{
					return skill;
				}
				num++;
			}
		}
		return null;
	}

	public static SkillBase SearchCardSkillIndex(BattleCardBase card, int skillIndex, bool isEvol)
	{
		int num = 0;
		SkillCollectionBase skillCollectionBase = card.NormalSkills;
		if (card.IsSpell)
		{
			skillIndex++;
		}
		if (isEvol)
		{
			skillCollectionBase = card.EvolutionSkills;
			if (skillCollectionBase.Count() == 0)
			{
				skillCollectionBase = card.NormalSkills;
			}
		}
		foreach (SkillBase item in skillCollectionBase)
		{
			if (!(item is Skill_none))
			{
				if (num == skillIndex)
				{
					return item;
				}
				num++;
			}
		}
		return null;
	}

	public static string MakeLogCode(SkillBase skill)
	{
		BattleCardBase ownerCard = skill.SkillPrm.ownerCard;
		return string.Concat(string.Concat("" + CardMaster.GetInstanceForBattle().GetCardParameterFromId(ownerCard.CardId).BaseCardId + "|", GetSkillIndex(skill).ToString(), "|"), ownerCard.IsEvolution ? "1" : "0");
	}

	public static bool IsBurialRite(SkillBase skill, bool notCheckPrevious = false)
	{
		if (skill.PreprocessList != null)
		{
			if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessBurialRite))
			{
				return true;
			}
			if (!notCheckPrevious && skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessReferencePrevious) && SkillPreprocessReferencePrevious.GetPreviousSkill(skill.SkillPrm.ownerCard.EvolutionSkills.Contains(skill) ? skill.SkillPrm.ownerCard.EvolutionSkills : skill.SkillPrm.ownerCard.NormalSkills, skill).PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessBurialRite))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsReceiveSelectDataOnBurialRite(BattleManagerBase mgr, SkillBase skill)
	{
		NetworkBattleManagerBase networkBattleManagerBase = mgr as NetworkBattleManagerBase;
		NetworkBattleReceiver.ReceiveData receiveData = networkBattleManagerBase.networkBattleData.GetReceiveData();
		if (receiveData != null)
		{
			if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessReferencePrevious))
			{
				skill = SkillPreprocessReferencePrevious.GetPreviousSkill(skill.SkillPrm.ownerCard.EvolutionSkills.Contains(skill) ? skill.SkillPrm.ownerCard.EvolutionSkills : skill.SkillPrm.ownerCard.NormalSkills, skill);
			}
			int index = skill.SkillPrm.ownerCard.Skills.IndexOf(skill);
			List<NetworkBattleReceiver.TargetData> opponentTargetDataList = receiveData.OpponentTargetDataList;
			if (opponentTargetDataList != null && opponentTargetDataList.Any((NetworkBattleReceiver.TargetData t) => t.SelectSkillIndexList.Contains(index)) && GetOpposingCardObjTarget(networkBattleManagerBase, opponentTargetDataList).Count >= 1)
			{
				return true;
			}
			List<NetworkBattleReceiver.TargetData> playerTargetDataList = receiveData.PlayerTargetDataList;
			if (playerTargetDataList != null && playerTargetDataList.Any((NetworkBattleReceiver.TargetData t) => t.SelectSkillIndexList.Contains(index)) && GetOpposingCardObjTarget(networkBattleManagerBase, playerTargetDataList).Count >= 1)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsOpenChoiceSkillCard(BattleCardBase card, bool isEvolve)
	{
		foreach (SkillBase item in isEvolve ? card.EvolutionSkills : card.Skills)
		{
			if (IsOpenChoiceSkill(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsOpenChoiceSkill(SkillBase skill)
	{
		if (skill.ApplyingTargetFilter is SkillTargetChosenCardsFilter && !(skill is Skill_token_draw) && !(skill is Skill_update_deck))
		{
			return true;
		}
		return false;
	}

	public static bool IsChoiceCard(BattleCardBase card, bool isEvol = false)
	{
		return GetChoiceSkill(card, isEvol) != null;
	}

	public static SkillBase GetChoiceSkill(BattleCardBase card, bool isEvol = false)
	{
		SkillCollectionBase skillCollectionBase = card.Skills;
		if (isEvol)
		{
			skillCollectionBase = card.EvolutionSkills;
		}
		foreach (SkillBase item in skillCollectionBase)
		{
			if (item.IsTargetChoiceSelectSkill)
			{
				return item;
			}
		}
		return null;
	}

	public static bool IsAcceleratedCard(BattleCardBase card)
	{
		for (int i = 0; i < card.Skills.Count(); i++)
		{
			if (card.Skills.Get(i) is Skill_pp_fixeduse { IsMutationFixedUseCost: not false } && card.Skills.Get(i + 1) is Skill_transform { TransformId: not -1 } skill_transform && CardMaster.GetInstanceForBattle().GetCardParameterFromId(skill_transform.TransformId).CharType == CardBasePrm.CharaType.SPELL)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsCrystallizeCard(BattleCardBase card)
	{
		for (int i = 0; i < card.Skills.Count(); i++)
		{
			if (card.Skills.Get(i) is Skill_pp_fixeduse { IsMutationFixedUseCost: not false } && card.Skills.Get(i + 1) is Skill_transform { TransformId: not -1 } skill_transform && (CardMaster.GetInstanceForBattle().GetCardParameterFromId(skill_transform.TransformId).CharType == CardBasePrm.CharaType.CHANT_FIELD || CardMaster.GetInstanceForBattle().GetCardParameterFromId(skill_transform.TransformId).CharType == CardBasePrm.CharaType.FIELD))
			{
				return true;
			}
		}
		return false;
	}

	public static SkillBase GetMutationPpFixedUseSkill(BattleCardBase card)
	{
		foreach (SkillBase skill in card.Skills)
		{
			if (skill is Skill_pp_fixeduse && (skill as Skill_pp_fixeduse).IsMutationOnCost)
			{
				return skill;
			}
		}
		return null;
	}

	public static bool IsIncludedCard(List<int> checkIndexList, IEnumerable<BattleCardBase> cards)
	{
		foreach (BattleCardBase card in cards)
		{
			if (checkIndexList.Contains(card.Index))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetSkillMovementNum(SkillBase skillBase)
	{
		if (skillBase.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork)
		{
			return -1;
		}
		return (skillBase._executionInfoCreator as NetworkExecutionInfoCreator).GetSkillMovementNum();
	}

	public static int GetPublishSkillCount(SkillBase skill)
	{
		int result = -1;
		if (skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork)
		{
			return result;
		}
		if (skill != null)
		{
			result = skill.PublishedActiveSkillCount;
			if (skill.PublishedActiveSkillCount == -1)
			{
				BattleCardBase skillCard = skill.SkillPrm.ownerCard;
				SkillBase skillBase = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.PublishedSkillList.FindAll((SkillBase x) => x.GetType() == skill.GetType() && x.SkillTimingText == skill.SkillTimingText).FindLast((SkillBase x) => x.SkillPrm.ownerCard.Index == skillCard.Index && x.SkillPrm.ownerCard.IsPlayer == skillCard.IsPlayer);
				if (skillBase != null)
				{
					result = skillBase.PublishedActiveSkillCount;
				}
			}
		}
		return result;
	}

	public static bool IsUnapprovedTarget(SkillBase skill)
	{
		bool isOnFusion = skill.OnWhenFusion != 0;
		bool flag = IsUnapprovedTarget(skill.ApplyingTargetFilter, skill, isOnFusion);
		bool flag2 = skill is NetworkSkill_attach_skill && skill.ApplyingTargetFilter is NetworkSkillTargetLastTargetFilter;
		if (skill.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyAndFilter.Count; i++)
			{
				flag |= IsUnapprovedTarget(skill.ApplyAndFilter[i].TargetFilter, skill, isOnFusion);
				flag2 |= skill is NetworkSkill_attach_skill && skill.ApplyAndFilter[i].TargetFilter is NetworkSkillTargetLastTargetFilter;
			}
		}
		if (flag2)
		{
			SkillBase lastTargetSkillReferenceSkill = GetLastTargetSkillReferenceSkill(skill);
			if (lastTargetSkillReferenceSkill != null)
			{
				flag |= IsUnapprovedTarget(lastTargetSkillReferenceSkill);
				flag = flag || lastTargetSkillReferenceSkill is Skill_return_card;
			}
		}
		return flag;
	}

	public static SkillBase GetLastTargetSkillReferenceSkill(SkillBase lastTargetSkill)
	{
		SkillCollectionBase skillCollectionBase = lastTargetSkill.SkillPrm.ownerCard.NormalSkills;
		int num = skillCollectionBase.IndexOf(lastTargetSkill);
		if (num == -1)
		{
			skillCollectionBase = lastTargetSkill.SkillPrm.ownerCard.EvolutionSkills;
			num = skillCollectionBase.IndexOf(lastTargetSkill);
		}
		num--;
		while (num > -1)
		{
			if (skillCollectionBase.ElementAt(num).ApplyingTargetFilter is NetworkSkillTargetLastTargetFilter)
			{
				num--;
				continue;
			}
			return skillCollectionBase.ElementAt(num);
		}
		return null;
	}

	public static bool IsUnapprovedTarget(ISkillTargetFilter skillTargetFilter, SkillBase skill, bool isOnFusion = false)
	{
		if (!(skillTargetFilter is SkillTargetDeckFilter) && !(skillTargetFilter is SkillTargetHandFilter) && !(skillTargetFilter is SkillTargetHandOtherSelfFilter) && !(skillTargetFilter is SkillTargetSkillDrewCardFilter) && !(skillTargetFilter is SkillTargetDiscardCardListFilter) && !(skillTargetFilter is SkillTargetHandBanishedCardListFilter) && !(skillTargetFilter is SkillTargetDeckBanishedCardListFilter) && (!isOnFusion || !(skillTargetFilter is SkillTargetSelfFilter)))
		{
			if (skill.OnWhenDraw != 0 && skill is NetworkSkill_attach_skill)
			{
				return skillTargetFilter is SkillTargetHandSelfFilter;
			}
			return false;
		}
		return true;
	}

	public static bool IsNeedUnapprovedListSkill(SkillBase skill)
	{
		bool flag = RegisterTool.IsSkillRandom(skill);
		if (skill.IsRandomUntilDrawSkill)
		{
			return true;
		}
		if (skill.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyAndFilter.Count; i++)
			{
				if (IsUnapprovedTarget(skill.ApplyAndFilter[i].TargetFilter, skill) && !IsOnlyAllCardFilter(skill.ApplyAndFilter[i].CardFilterList) && flag)
				{
					return true;
				}
			}
		}
		if (!IsUnapprovedTarget(skill))
		{
			return false;
		}
		if (IsOnlyAllCardFilter(skill.ApplyCardFilterList))
		{
			return false;
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	private static bool IsOnlyAllCardFilter(List<ISkillCardFilter> applyCardFilterList)
	{
		for (int i = 0; i < applyCardFilterList.Count; i++)
		{
			if (!(applyCardFilterList[i] is SkillAllCardFilter))
			{
				return false;
			}
		}
		return true;
	}

	public static void SettingRegisterTargetGroupAndInsert()
	{
		List<RegisterActionBase> registerDataList = new List<RegisterActionBase>(); // Pre-Phase-5b: SettingRegisterTargetGroupAndInsert is unreachable headless
		foreach (RegisterLotCardBase item in registerDataList.FindAll((RegisterActionBase x) => x is RegisterLotCardBase).ConvertAll((RegisterActionBase x) => x as RegisterLotCardBase))
		{
			RegisterActionBase registerActionBase = null;
			int num = 0;
			foreach (RegisterActionBase item2 in registerDataList)
			{
				if (item2 is RegisterLotCardBase || item2.PrivateGroupIndexMsg != "")
				{
					continue;
				}
				bool flag = false;
				if (!item2.IsUseLotCard(item))
				{
					continue;
				}
				if (item2 is RegisterStateChangeCard)
				{
					if ((item2 as RegisterStateChangeCard).FromPlaceState == item.GetFromPlaceState())
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (!flag)
				{
					continue;
				}
				if (item2 is RegisterCopyToken)
				{
					item.SettingGroupIndexMsg(item2, num);
					if (item.GroupMsgList.Count > num + 1)
					{
						num++;
					}
				}
				else
				{
					item.SettingGroupIndexMsg(item2);
				}
				if (registerActionBase == null)
				{
					int num2 = registerDataList.IndexOf(item);
					int num3 = registerDataList.IndexOf(item2);
					if (num2 > num3)
					{
						registerActionBase = item2;
					}
				}
			}
			if (registerActionBase != null)
			{
				int index = registerDataList.IndexOf(registerActionBase);
				registerDataList.Remove(item);
				registerDataList.Insert(index, item);
			}
		}
	}

	public static RegisterLotCardBase MakeRegisterLotAndRandomAdvance(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption)
	{
		NetworkBattleManagerBase networkBattleManagerBase = skillBase.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
		List<CardDataModel> unapprovedList = networkBattleManagerBase.networkBattleData.GetReceiveData().unapprovedList;
		NetworkExecutionInfoCreator networkExec = skillBase._executionInfoCreator as NetworkExecutionInfoCreator;
		int num = 0;
		RegisterLotCardBase result = null;
		List<RegisterLotCardBase> list = new List<RegisterLotCardBase>();
		list = networkBattleManagerBase.RegisterActionManager.RegisterDataList.FindAll((RegisterActionBase x) => x is RegisterLotCardBase).ConvertAll((RegisterActionBase x) => x as RegisterLotCardBase);
		for (int num2 = 0; num2 < cards.Count(); num2++)
		{
			CardDataModel unapprovedCard = GetCardDataModel(skillBase, cards.ElementAt(num2), unapprovedList, networkExec);
			list = list.Where((RegisterLotCardBase data) => data.IsAlreadyAddedIndex(unapprovedCard.Index, unapprovedCard.fromState)).ToList();
			if (list.Count == 0)
			{
				break;
			}
		}
		if (skillBase.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.CardFilterList.Any((ISkillCardFilter cf) => cf is SkillLastTargetTribeFilter)))
		{
			networkBattleManagerBase.RegisterActionManager.Add(new RegisterExtract(isSelf: false, skillBase.SkillPrm.ownerCard.SelfBattlePlayer.GetLastTargetCardsList(0), RegisterActionBase.ActionBaseParameter.tribe.ToString(), isBase: false));
		}
		for (int num3 = 0; num3 < cards.Count(); num3++)
		{
			BattleCardBase battleCardBase = cards.ElementAt(num3);
			CardDataModel cardDataModel = GetCardDataModel(skillBase, battleCardBase, unapprovedList, networkExec);
			if (cardDataModel == null)
			{
				continue;
			}
			if (GetCardPlaceState(battleCardBase.SelfBattlePlayer, battleCardBase.Index) == NetworkBattleDefine.NetworkCardPlaceState.Deck)
			{
				networkBattleManagerBase.StableRandomDouble();
				continue;
			}
			bool lotFirstSettingFlag = false;
			RegisterLotCardBase registerLotCardBase = AddLotRegisterData(networkBattleManagerBase, networkBattleManagerBase.RegisterActionManager, battleCardBase, skillBase.SkillPrm.ownerCard, skillBase, cardDataModel.fromState, list, out lotFirstSettingFlag);
			if (lotFirstSettingFlag)
			{
				registerLotCardBase.SettingTargetStatusToSearchSkill(networkBattleManagerBase, skillBase, num, cardDataModel, checkerOption);
			}
			if (RegisterTool.HasTargetOverCostFromFilter(skillBase))
			{
				num++;
			}
			result = registerLotCardBase;
		}
		return result;
	}

	private static CardDataModel GetCardDataModel(SkillBase skillBase, BattleCardBase carddata, List<CardDataModel> unapprovedList, NetworkExecutionInfoCreator networkExec)
	{
		if (RegisterSkillConditionCheck.DoesSkillUsePrivateCount(skillBase) && skillBase is Skill_powerup)
		{
			int skillConditionCount = RegisterSkillConditionCheck.GetMovementCount(skillBase);
			return unapprovedList.Find((CardDataModel x) => x.Index == carddata.Index && x.skillMovementNum / skillConditionCount == networkExec.GetSkillMovementNum() && x.publishedActiveSkillCount == skillBase.PublishedActiveSkillCount);
		}
		return unapprovedList.Find((CardDataModel x) => x.Index == carddata.Index && x.skillMovementNum == networkExec.GetSkillMovementNum() && x.publishedActiveSkillCount == skillBase.PublishedActiveSkillCount);
	}

	public static VfxBase Event_SetupPlayerUnapprovedAddEvent(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		if (cards == null)
		{
			return NullVfx.GetInstance();
		}
		if (skillBase is Skill_summon_card { IsDeckSelfSummon: not false } && cards.Count() > 0 && !cards.ElementAt(0).IsInplay)
		{
			return NullVfx.GetInstance();
		}
		NetworkBattleManagerBase networkBattleManagerBase = skillBase.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
		int skillMovementNum = GetSkillMovementNum(skillBase);
		foreach (BattleCardBase card in cards)
		{
			bool isCardId = false;
			NetworkBattleDefine.NetworkCardPlaceState to = GetCardPlaceState(skillBase.SkillPrm.ownerCard.SelfBattlePlayer, card.Index);
			NetworkBattleDefine.NetworkCardPlaceState networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Deck;
			if (skillBase.OnDisCardStart != 0)
			{
				networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Hand;
				to = NetworkBattleDefine.NetworkCardPlaceState.Cemetery;
			}
			else if (skillBase.ApplyingTargetFilter is SkillTargetDeckFilter)
			{
				networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Deck;
			}
			else if (skillBase.ApplyingTargetFilter is SkillTargetHandFilter || skillBase.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter)
			{
				networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Hand;
			}
			else if (skillBase.IsHaveApplicableTargetFilter<SkillTargetDiscardCardListFilter>() && (card.IsInCemetery || card.IsInNecromanceZone))
			{
				networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Cemetery;
				if (skillBase is Skill_token_draw { IsOpen: "true" } || skillBase is Skill_summon_token)
				{
					isCardId = true;
				}
			}
			else if (skillBase.IsHaveApplicableTargetFilter<SkillTargetFusionIngredientCardsFilter>() && card.IsFusionIngredient)
			{
				networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.FusionIngredient;
			}
			else if (skillBase.ApplyingTargetFilter is SkillTargetSelfFilter && skillBase.ApplyAndFilter.Count > 0)
			{
				for (int i = 0; i < skillBase.ApplyAndFilter.Count; i++)
				{
					if (skillBase.ApplyAndFilter[i].TargetFilter is SkillTargetDeckFilter)
					{
						networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Deck;
					}
					else if (skillBase.ApplyAndFilter[i].TargetFilter is SkillTargetHandFilter || skillBase.ApplyAndFilter[i].TargetFilter is SkillTargetHandOtherSelfFilter)
					{
						networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Hand;
					}
					else if (skillBase.ApplyAndFilter[i].TargetFilter is SkillTargetGameFusionIngredientedCards || skillBase.ApplyAndFilter[i].TargetFilter is SkillTargetDiscardCardListFilter)
					{
						networkCardPlaceState = (card.IsFusionIngredient ? NetworkBattleDefine.NetworkCardPlaceState.FusionIngredient : NetworkBattleDefine.NetworkCardPlaceState.Cemetery);
					}
				}
			}
			if ((skillBase is Skill_banish || skillBase is Skill_discard) && !RegisterValidate.IsDeckRandomEachSkill(skillBase))
			{
				isCardId = true;
			}
			networkBattleManagerBase.RegisterUnapprovedList.Add(new RegisterUnapproved(skillBase, card, networkCardPlaceState, to, skillMovementNum, isCardId));
		}
		return NullVfx.GetInstance();
	}

	public static RegisterLotCardBase AddLotRegisterData(NetworkBattleManagerBase battleMgr, RegisterActionManager registerCardList, BattleCardBase unapprovedCard, BattleCardBase skillCard, SkillBase skillBase, NetworkBattleDefine.NetworkCardPlaceState fromPlace, List<RegisterLotCardBase> sameTargetDataList, out bool lotFirstSettingFlag)
	{
		lotFirstSettingFlag = false;
		RegisterLotCardBase registerLotCardBase = null;
		if (!RegisterTool.HasTargetOverCostFromFilter(skillBase))
		{
			List<RegisterLotCardBase> list = new List<RegisterLotCardBase>();
			list = registerCardList.RegisterDataList.FindAll((RegisterActionBase x) => x is RegisterLotCardBase).ConvertAll((RegisterActionBase x) => x as RegisterLotCardBase);
			if (sameTargetDataList.Count > 0)
			{
				battleMgr.StableRandomDouble();
				return sameTargetDataList[0];
			}
			int skillMovementNum = GetSkillMovementNum(skillBase);
			RegisterLotCardBase registerLotCardBase2 = list.Find((RegisterLotCardBase x) => x.GetLotSkillBase() == skillBase && x.GetSkillMovementNum() == skillMovementNum);
			if (registerLotCardBase2 != null && !skillBase.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => !(f.SelectFilter is SkillSelectAllFilter) && (!(f.SelectFilter is SkillRandomSelectFilter) || f.SelectFilter.CalcCount(skillBase.OptionValue) <= 1)))
			{
				registerLotCardBase2.AddTargetIndexAndPlaceGroup(unapprovedCard.Index);
				registerLotCardBase2.AddRandomList(battleMgr.StableRandomDouble());
				return registerLotCardBase2;
			}
		}
		registerLotCardBase = new RegisterLotCardBase(registerCardList, battleMgr, battleMgr.StableRandomDouble(), unapprovedCard.IsPlayer, fromPlace, unapprovedCard.Index, skillBase);
		if (!RegisterValidate.IsDeckRandomEachSkill(skillBase))
		{
			registerCardList.Add(registerLotCardBase);
		}
		lotFirstSettingFlag = true;
		return registerLotCardBase;
	}

	public static bool IsSendUnapprovedList(SkillBase skill)
	{
		if (IsNeedUnapprovedListSkill(skill) || IsTargetDeckSelf(skill))
		{
			return true;
		}
		if (RegisterFilter.IsFilterCardUnapproved(skill) && RegisterFilter.IsFilterCard(skill))
		{
			return true;
		}
		return false;
	}

	public static bool IsTargetDeckSelf(SkillBase skill)
	{
		if (skill.ConditionTargetFilter is SkillTargetDeckSelfFilter)
		{
			return true;
		}
		return false;
	}
}

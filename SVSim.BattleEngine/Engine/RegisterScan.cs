using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class RegisterScan : RegisterValidate
{
	public enum ScanParameter
	{
		allow,
		excludeIdxList
	}

	public int AllowNum { get; private set; }

	public BattleCardBase ScanCard { get; private set; }

	public SkillBase ScanSkill { get; private set; }

	public RegisterScan(BattleCardBase card, SkillBase skill)
	{
		ScanCard = card;
		ScanSkill = skill;
	}

	public void SetScanTargetIndex(List<BattleCardBase> beforeHandCard)
	{
		base.IndexList = new List<int>();
		foreach (BattleCardBase item in beforeHandCard)
		{
			base.IndexList.Add(item.Index);
		}
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.scan.ToString();
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		if (AllowNum >= 1)
		{
			dictionary.Add(ScanParameter.allow.ToString(), AllowNum);
		}
		return dictionary;
	}

	public void SettingAllowCardNum(int num)
	{
		AllowNum = num;
	}

	public bool IsSameScanData(RegisterScan checkScan)
	{
		List<ValidateData> validateDataList = checkScan.GetValidateDataList();
		if (validateDataList.Count != ValidateDataList.Count)
		{
			return false;
		}
		if (checkScan.AllowNum != AllowNum)
		{
			return false;
		}
		for (int i = 0; i < validateDataList.Count; i++)
		{
			if (validateDataList[i].BaseCost != ValidateDataList[i].BaseCost || validateDataList[i].ChantCount != ValidateDataList[i].ChantCount || !validateDataList[i].CharaTypes.SequenceEqual(ValidateDataList[i].CharaTypes) || validateDataList[i].Clan != ValidateDataList[i].Clan || !validateDataList[i].ExcludeList.SequenceEqual(ValidateDataList[i].ExcludeList) || !validateDataList[i].IncludeList.SequenceEqual(ValidateDataList[i].IncludeList) || validateDataList[i].NowCost != ValidateDataList[i].NowCost || validateDataList[i].Tribe != ValidateDataList[i].Tribe || validateDataList[i].NowAtk != ValidateDataList[i].NowAtk || validateDataList[i].BaseAtk != ValidateDataList[i].BaseAtk)
			{
				return false;
			}
		}
		return true;
	}

	public static void OrganizeScanData(SkillBase scanSkill, RegisterActionManager registerActionManager, NetworkBattleManagerBase battleMgr)
	{
		BattleCardBase playActionCardBase = registerActionManager.PlayActionCardBase;
		if (playActionCardBase == null)
		{
			return;
		}
		List<BattleCardBase> beforeOpponentHandCardList = registerActionManager.BeforeOpponentHandCardList;
		if (beforeOpponentHandCardList == null)
		{
			return;
		}
		beforeOpponentHandCardList.Remove(playActionCardBase);
		BattleCardBase.TransformInformation transformInfo = playActionCardBase.TransformInfo;
		if (transformInfo.Type != BattleCardBase.TransformType.Metamorphose && transformInfo.OriginalCard != null)
		{
			beforeOpponentHandCardList.Remove(transformInfo.OriginalCard);
		}
		List<RegisterScan> list = new List<RegisterScan>();
		NetworkBattleReceiver.ReceiveData receiveData = battleMgr.networkBattleData.GetReceiveData();
		if (scanSkill.IsNeedCheckConditionOnScan() && !scanSkill.IsScanConditionOk)
		{
			return;
		}
		int num = 0;
		if (scanSkill.ApplySelectFilter is SkillUserSelectFilter)
		{
			num++;
		}
		foreach (SkillBase skill in scanSkill.SkillPrm.ownerCard.Skills)
		{
			if (skill.IsScanConditionOk && NetworkBattleGenericTool.IsBurialRite(skill, notCheckPrevious: true))
			{
				num++;
			}
		}
		if (scanSkill is Skill_discard && RegisterSkillConditionCheck.IsSkillConditionCheck(scanSkill) && !RegisterSkillConditionCheck.DoesSkillUsePrivateCount(scanSkill))
		{
			num++;
		}
		if (NetworkBattleGenericTool.GetOpposingCardObjTarget(battleMgr, receiveData.OpponentTargetDataList).Count < num && !receiveData.unapprovedList.Any())
		{
			if (beforeOpponentHandCardList.Count <= 0 || (((NetworkBattleGenericTool.IsBurialRite(scanSkill) && scanSkill.IsScanConditionOk) || scanSkill is Skill_summon_card) && battleMgr.BattleEnemy.ClassAndInPlayCardList.Count >= 6))
			{
				return;
			}
			RegisterScan registerScan = new RegisterScan(playActionCardBase, scanSkill);
			registerScan.SetScanTargetIndex(beforeOpponentHandCardList);
			registerScan.AddValidateData(scanSkill);
			if (scanSkill.PreprocessList.Find((SkillPreprocessBase x) => x is SkillPreprocessBurialRite) != null)
			{
				int num2 = 0;
				foreach (SkillBase skill2 in playActionCardBase.Skills)
				{
					if (skill2.PreprocessList.Find((SkillPreprocessBase x) => x is SkillPreprocessBurialRite) != null)
					{
						num2++;
					}
				}
				if (num2 >= 2)
				{
					registerScan.SettingAllowCardNum(num2 - 1);
				}
			}
			if (list.Count >= 1)
			{
				bool flag = false;
				foreach (RegisterScan item in list)
				{
					if (item.IsSameScanData(registerScan))
					{
						flag = true;
					}
				}
				if (flag)
				{
					return;
				}
			}
			list.Add(registerScan);
		}
		List<RegisterActionBase> registerDataList = registerActionManager.RegisterDataList;
		if (list.Count == 0)
		{
			return;
		}
		foreach (RegisterScan item2 in list)
		{
			int num3 = 0;
			using (IEnumerator<SkillBase> enumerator = item2.ScanCard.Skills.GetEnumerator())
			{
				while (enumerator.MoveNext() && enumerator.Current != item2.ScanSkill)
				{
					num3++;
				}
			}
			if (battleMgr.LethalPublishedActiveSkillCount != -1 && NetworkBattleGenericTool.GetPublishSkillCount(item2.ScanSkill) > battleMgr.LethalPublishedActiveSkillCount && battleMgr.LethalMovementCount == 1)
			{
				break;
			}
			int num4 = 0;
			int num5 = 0;
			foreach (SkillBase skill3 in item2.ScanCard.Skills)
			{
				if (num4 + 1 == num3)
				{
					bool flag2 = false;
					foreach (RegisterActionBase item3 in registerDataList)
					{
						if (item3.EffectSkillPublicCount == NetworkBattleGenericTool.GetPublishSkillCount(skill3) && item3.EffectSkillMovement == 0)
						{
							flag2 = true;
						}
						else if (flag2)
						{
							break;
						}
						num5++;
					}
					break;
				}
				if (num4 - 1 == num3)
				{
					foreach (RegisterActionBase item4 in registerDataList)
					{
						if (item4.EffectSkillPublicCount == NetworkBattleGenericTool.GetPublishSkillCount(skill3) && item4.EffectSkillMovement == 0)
						{
							break;
						}
						num5++;
					}
				}
				num4++;
			}
			registerDataList.Insert(num5, item2);
		}
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}

	public static void OrganizeScanDataOnSkillStart(SkillBase scanSkill, RegisterActionManager registerActionManager, NetworkBattleManagerBase battleMgr, List<BattleCardBase> cards)
	{
		BattleCardBase playActionCardBase = registerActionManager.PlayActionCardBase;
		if (playActionCardBase == null)
		{
			return;
		}
		NetworkBattleReceiver.ReceiveData receiveData = battleMgr.networkBattleData.GetReceiveData();
		if (!receiveData.unapprovedList.Any() || receiveData.unapprovedList.Any((CardDataModel u) => u.Index == -99) || battleMgr.GetBattlePlayer(playActionCardBase.IsPlayer).HandCardList.Count + cards.Count >= 9)
		{
			return;
		}
		if (cards == null || cards.Count == 0)
		{
			string empty = string.Empty;
			empty = empty + "skillIdx" + scanSkill.PublishedActiveSkillCount;
			empty += " uList";
			for (int num = 0; num < receiveData.unapprovedList.Count; num++)
			{
				empty = empty + receiveData.unapprovedList[num].Index + " ";
			}
			empty += " deck";
			List<BattleCardBase> deckCardList = scanSkill.SkillPrm.ownerCard.SelfBattlePlayer.DeckCardList;
			for (int num2 = 0; num2 < deckCardList.Count; num2++)
			{
				empty = empty + deckCardList[num2].Index + " ";
			}
			Debug.LogError(empty);
			LocalLog.AccumulateTraceLog(empty);
		}
		else
		{
			RegisterScan registerScan = new RegisterScan(playActionCardBase, scanSkill);
			registerScan.IndexList.Add(cards.Last().Index);
			ValidateData validateData = new ValidateData();
			validateData.LibraryType.Add(1003);
			registerScan.ValidateDataList.Add(validateData);
			registerActionManager.RegisterDataList.Add(registerScan);
		}
	}

	public static void OrganizeNotSelectSkillScanData(SkillBase skill, RegisterActionManager registerActionManager, NetworkBattleManagerBase battleMgr, List<BattleCardBase> cards)
	{
		SkillBase scanSkill = skill;
		if (skill.ApplyingTargetFilter is SkillTargetLastTargetFilter)
		{
			scanSkill = skill.SkillPrm.ownerCard.Skills.ElementAt(skill.SkillPrm.ownerCard.Skills.IndexOf(skill) - 1);
		}
		BattleCardBase playActionCardBase = registerActionManager.PlayActionCardBase;
		int num = scanSkill.ApplySelectFilter.CalcCount(scanSkill.OptionValue);
		if ((scanSkill.ApplySelectFilter is SkillCostNoDuplicationRandomSelectFilter skillCostNoDuplicationRandomSelectFilter && skillCostNoDuplicationRandomSelectFilter.IsUpperLimit()) || (scanSkill.ApplySelectFilter is SkillIdNoDuplicationRandomSelectFilter skillIdNoDuplicationRandomSelectFilter && skillIdNoDuplicationRandomSelectFilter.IsUpperLimit()))
		{
			num += cards.Count;
		}
		if (RegisterTool.HasTargetOverCostFromFilter(scanSkill))
		{
			num = scanSkill.SkillPrm.ownerCard.SelfBattlePlayer.SkillInfoLastTargets.First().Count();
		}
		if (num == -1)
		{
			if (scanSkill.ApplyAndFilter.Count == 1)
			{
				num = scanSkill.ApplyAndFilter[0].SelectFilter.CalcCount(scanSkill.OptionValue);
			}
			else if (scanSkill.ApplyAndFilter.Count > 1)
			{
				for (int i = 0; i < scanSkill.ApplyFilterCollection.ApplyAndFilter.Count; i++)
				{
					OrganizeMultiFilterScanData(scanSkill, registerActionManager, cards, scanSkill.ApplyFilterCollection.ApplyAndFilter[i].CardFilterList);
				}
				return;
			}
		}
		if (cards != null && cards.Count >= num)
		{
			return;
		}
		RegisterScan registerScan = new RegisterScan(playActionCardBase, scanSkill);
		BattlePlayerBase battlePlayerBase = ((scanSkill.ApplyBattlePlayerFilter is SelfBattlePlayerFilter) ? scanSkill.SkillPrm.ownerCard.SelfBattlePlayer : scanSkill.SkillPrm.ownerCard.OpponentBattlePlayer);
		if (scanSkill.ApplyingTargetFilter is SkillTargetHandFilter)
		{
			registerScan.IndexList.AddRange(battlePlayerBase.HandCardList.Select((BattleCardBase c) => c.Index));
		}
		else
		{
			registerScan.IndexList.AddRange(battlePlayerBase.DeckCardList.Select((BattleCardBase c) => c.Index));
		}
		if (scanSkill.ApplyFilterCollection.ApplyAndFilter.Count > 0)
		{
			for (int num2 = 0; num2 < scanSkill.ApplyFilterCollection.ApplyAndFilter.Count; num2++)
			{
				ValidateData validateData = registerScan.CreateValidateData(scanSkill.ApplyFilterCollection.ApplyAndFilter[num2].CardFilterList, scanSkill);
				if (num > 1 && cards != null)
				{
					validateData.ExcludeIdxList.AddRange(cards.Select((BattleCardBase c) => c.Index));
				}
				registerScan.OrValidateDataList.Add(validateData);
			}
		}
		else
		{
			ValidateData validateData2 = registerScan.CreateValidateData(scanSkill.ApplyCardFilterList, scanSkill);
			if (RegisterTool.HasTargetOverCostFromFilter(scanSkill))
			{
				if (num > 0)
				{
					List<CardDataModel> unapprovedList = battleMgr.networkBattleData.GetReceiveData().unapprovedList;
					NetworkExecutionInfoCreator networkExec = scanSkill._executionInfoCreator as NetworkExecutionInfoCreator;
					CardDataModel unapprovedCard = unapprovedList.Find((CardDataModel x) => x.skillCardIndex == scanSkill.SkillPrm.ownerCard.Index && x.skillMovementNum == networkExec.GetSkillMovementNum() && x.publishedActiveSkillCount == scanSkill.PublishedActiveSkillCount);
					List<IReadOnlyBattleCardInfo> list = scanSkill.SkillPrm.ownerCard.SelfBattlePlayer.SkillInfoLastTargets.First().ToList();
					if (unapprovedCard != null)
					{
						list = list.Where((IReadOnlyBattleCardInfo c) => !unapprovedCard.skillKeyCardIdxList.Contains(c.Index)).ToList();
					}
					string text = "gt";
					for (int num3 = 0; num3 < list.Count; num3++)
					{
						if (num3 > 0)
						{
							text += ",";
						}
						text += list[num3].Cost;
					}
					validateData2.NowCost = text;
				}
			}
			else if (num > 1 && cards != null)
			{
				if (scanSkill.ApplySelectFilter is SkillCostNoDuplicationRandomSelectFilter)
				{
					string text2 = "ne";
					for (int num4 = 0; num4 < cards.Count; num4++)
					{
						if (num4 > 0)
						{
							text2 += ",";
						}
						text2 += cards[num4].LastCost;
					}
					validateData2.NowCost = text2;
				}
				else
				{
					validateData2.ExcludeIdxList.AddRange(cards.Select((BattleCardBase c) => c.Index));
				}
			}
			registerScan.ValidateDataList.Add(validateData2);
		}
		registerActionManager.RegisterDataList.Add(registerScan);
	}

	public static void OrganizeSkillLastTargetTribeDrawScanData(SkillBase skill, RegisterActionManager registerActionManager, NetworkBattleManagerBase battleMgr, List<BattleCardBase> cards)
	{
		BattleCardBase playActionCardBase = registerActionManager.PlayActionCardBase;
		int num = -1;
		for (int i = 0; i < skill.ApplyAndFilter.Count; i++)
		{
			num = skill.ApplyAndFilter[i].SelectFilter.CalcCount(skill.OptionValue);
		}
		if (cards == null || cards.Count < num)
		{
			battleMgr.networkBattleData.GetReceiveData();
			RegisterScan registerScan = new RegisterScan(playActionCardBase, skill);
			BattlePlayerBase battlePlayerBase = ((skill.ApplyBattlePlayerFilter is SelfBattlePlayerFilter) ? skill.SkillPrm.ownerCard.SelfBattlePlayer : skill.SkillPrm.ownerCard.OpponentBattlePlayer);
			registerScan.IndexList.AddRange(battlePlayerBase.DeckCardList.Select((BattleCardBase c) => c.Index));
			ValidateData validateData = new ValidateData();
			validateData.Tribe = "eqv1";
			registerScan.OrValidateDataList.Add(validateData);
			registerActionManager.RegisterDataList.Add(registerScan);
		}
	}

	public static void OrganizeMultiFilterScanData(SkillBase skill, RegisterActionManager registerActionManager, List<BattleCardBase> cards, List<ISkillCardFilter> cardFilterList)
	{
		SkillParameterCostFilter skillParameterCostFilter = cardFilterList.First((ISkillCardFilter f) => f is SkillParameterCostFilter) as SkillParameterCostFilter;
		int cost = skill.OptionValue.ParseInt(skillParameterCostFilter.GetParameterText());
		if (!cards.Any((BattleCardBase c) => c.LastCost == cost || c.LastCost == -1))
		{
			RegisterScan registerScan = new RegisterScan(registerActionManager.PlayActionCardBase, skill);
			BattlePlayerBase battlePlayerBase = ((skill.ApplyBattlePlayerFilter is SelfBattlePlayerFilter) ? skill.SkillPrm.ownerCard.SelfBattlePlayer : skill.SkillPrm.ownerCard.OpponentBattlePlayer);
			registerScan.IndexList.AddRange(battlePlayerBase.DeckCardList.Select((BattleCardBase c) => c.Index));
			ValidateData item = registerScan.CreateValidateData(cardFilterList, skill);
			registerScan.ValidateDataList.Add(item);
			registerActionManager.RegisterDataList.Add(registerScan);
		}
	}

	public static bool IsScanSkill(SkillBase skill)
	{
		if (RegisterValidate.IsValidateCard(skill) && !(skill is Skill_fusion) && (!NetworkBattleGenericTool.IsTargetDeckSelf(skill) || !RegisterValidate.IsDeckParamVariable(skill)) && !RegisterValidate.IsDeckRandomEachSkill(skill) && !RegisterValidate.IsOpenMyHandSkill(skill) && !skill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessReferencePrevious))
		{
			return true;
		}
		if (skill is Skill_discard && RegisterSkillConditionCheck.IsSkillConditionCheck(skill) && !RegisterSkillConditionCheck.DoesSkillUsePrivateCount(skill))
		{
			return true;
		}
		return false;
	}

	public static bool IsNotSelectScanSkill(SkillBase skill)
	{
		if (skill.ApplyingTargetFilter is SkillTargetDeckSelfFilter)
		{
			return false;
		}
		if (skill.ApplySelectFilter is SkillUserSelectFilter)
		{
			return false;
		}
		if (skill.ApplyFilterCollection.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyFilterCollection.ApplyAndFilter.Count; i++)
			{
				if (IsScanDrawSkillFilter(skill.ApplyFilterCollection.ApplyAndFilter[i].CardFilterList))
				{
					return true;
				}
			}
		}
		else if (IsScanDrawSkillFilter(skill.ApplyCardFilterList))
		{
			return true;
		}
		return false;
	}

	private static bool IsScanDrawSkillFilter(List<ISkillCardFilter> cardFilterList)
	{
		for (int i = 0; i < cardFilterList.Count; i++)
		{
			ISkillCardFilter skillCardFilter = cardFilterList[i];
			if (skillCardFilter is SkillClanFilter)
			{
				return true;
			}
			if (skillCardFilter is SkillTribeFilter)
			{
				return true;
			}
			if (skillCardFilter is SkillParameterOffenseFilter || skillCardFilter is SkillParameterBaseOffenseFilter)
			{
				return true;
			}
			if (skillCardFilter is SkillParameterLifeFilter || skillCardFilter is SkillParameterBaseLifeFilter)
			{
				return true;
			}
			if (skillCardFilter is SkillParameterCostFilter || skillCardFilter is SkillParameterBaseCostFilter)
			{
				return true;
			}
			if (skillCardFilter is SkillParameterIdFilter)
			{
				return true;
			}
			if (skillCardFilter is SkillAbilitySpellChargeFilter || skillCardFilter is SkillAbilityDestroyWhiteRitualFilter || skillCardFilter is SkillAbilityWhenDestroyFilter || skillCardFilter is SkillAbilitySuperSkyboundArtFilter || skillCardFilter is SkillAbilityEnhanceFilter || skillCardFilter is SkillAbilityWhenPlayFilter || skillCardFilter is SkillAbilityAccelerateFilter || skillCardFilter is SkillAbilityCrystallizeFilter || skillCardFilter is SkillAbilityNecromanceFilter || skillCardFilter is SkillAbilityFusionFilter)
			{
				return true;
			}
			if (RegisterTool.GetCardTypeList(skillCardFilter).Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsScanLastTargetTribeDrawSkill(SkillBase skill)
	{
		if (skill.ApplyFilterCollection.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyFilterCollection.ApplyAndFilter.Count; i++)
			{
				List<ISkillCardFilter> cardFilterList = skill.ApplyFilterCollection.ApplyAndFilter[i].CardFilterList;
				for (int j = 0; j < cardFilterList.Count; j++)
				{
					if (cardFilterList[j] is SkillLastTargetTribeFilter)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}

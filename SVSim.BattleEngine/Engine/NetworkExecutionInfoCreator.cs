using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class NetworkExecutionInfoCreator : ExecutionInfoCreatorBase
{
	private NetworkBattleManagerBase _networkBattleMgr;

	private IEnumerable<BattleCardBase> replaceCards;

	private bool _validateSkillCheckFlag;

	private bool _playSkill;

	private bool _notPlaySkill;

	private bool _isReceiveSkillConditionCheck;

	private bool _isUnapprovedSkill;

	private bool _isAllHandCardTarget;

	private bool _isAllDeckCardTarget;

	private bool _isUseUListOnlySelfTurnWhenAdmin;

	private bool _isCheckOppoActionData;

	private int _skillMovementNum;

	private bool _notNetwrokConditionCheck;

	public bool IsNotCheckBuriaRiteCondition { get; private set; }

	public NetworkExecutionInfoCreator(SkillBase skill)
		: base(skill)
	{
		_networkBattleMgr = (NetworkBattleManagerBase)skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr;
	}

	public override bool CheckScanCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		return base.CheckCondition(playerInfoPair, option, isPrePlay);
	}

	public override bool CheckCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay, bool isSkipTarget = false)
	{
		bool result = base.CheckCondition(playerInfoPair, option, isPrePlay, isSkipTarget);
		if (_notNetwrokConditionCheck)
		{
			return result;
		}
		NetworkBattleManagerBase networkBattleManagerBase = _skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
		if (_playSkill)
		{
			return true;
		}
		if (_notPlaySkill)
		{
			return false;
		}
		if (_isCheckOppoActionData && networkBattleManagerBase.networkBattleData.GetReceiveData().OpponentTargetDataList.Count() == 0)
		{
			if (_skill.ApplyingTargetFilter is SkillTargetInPlayFilter || _skill.ApplyingTargetFilter is SkillTargetInPlayOtherSelfFilter || _skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessNecromance))
			{
				return false;
			}
			return result;
		}
		if (IsNotCheckBuriaRiteCondition)
		{
			if (NetworkBattleGenericTool.IsReceiveSelectDataOnBurialRite(networkBattleManagerBase, _skill))
			{
				return true;
			}
			return false;
		}
		if (_validateSkillCheckFlag)
		{
			return networkBattleManagerBase.GetValidateTargetSkillIndexList().Contains(NetworkBattleGenericTool.GetSkillIndex(_skill));
		}
		if (networkBattleManagerBase == null)
		{
			return false;
		}
		if (IsUnapprovedSkill())
		{
			NetworkBattleReceiver.ReceiveData receiveData = networkBattleManagerBase.networkBattleData.GetReceiveData();
			if (receiveData != null)
			{
				BattleCardBase owner = _skill.SkillPrm.ownerCard;
				SkillCollectionBase skills = owner.Skills;
				bool flag = false;
				if (skills.Any((SkillBase s) => s.ConditionTargetFilter is SkillTargetDeckSelfFilter) && owner.IsInDeck)
				{
					CardDataModel cardDataModel = receiveData.unapprovedList.Find((CardDataModel x) => x.Index == owner.Index);
					if (cardDataModel == null)
					{
						return false;
					}
					_skill.SetPublishedActiveSkillCount(cardDataModel.publishedActiveSkillCount);
					flag = true;
				}
				NetworkBattleManagerBase networkBattleManagerBase2 = _skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
				if (RegisterValidate.IsDeckParamVariable(_skill))
				{
					return true;
				}
				if (networkBattleManagerBase2.IsContainUnapprovedSkill(_skill, _skill.SkillPrm.ownerCard.Index, NetworkBattleGenericTool.GetPublishSkillCount(_skill), _skillMovementNum))
				{
					if (flag || !_skill.IsOnceCallTiming)
					{
						return result;
					}
					return true;
				}
				if ((_skill.IsUserSelectType || _skill.IsBurialRite) && ((_networkBattleMgr.GameMgr.IsWatchBattle && _skill.SkillPrm.ownerCard.IsPlayer) || _networkBattleMgr.GameMgr.IsAdminWatch))
				{
					return result;
				}
				if (RegisterSkillConditionCheck.IsSkillConditionCheck(_skill, _networkBattleMgr.GameMgr.IsAdmin))
				{
					return false;
				}
			}
		}
		if (_isReceiveSkillConditionCheck)
		{
			if (networkBattleManagerBase.IsSkillConditionCheckSkill(_skill.SkillPrm.ownerCard.Index))
			{
				return networkBattleManagerBase.IsReceivedSkillConditionCheck(_skillMovementNum, _skill);
			}
			if (RegisterSkillConditionCheck.IsHighlander(_skill.ConditionFilterCollection) || RegisterSkillConditionCheck.IsHighlanderPreprocessConditionCheck(_skill))
			{
				bool num = networkBattleManagerBase.networkBattleData.GetReceiveData().GetReceiveCardList().Any((CardDataModel c) => c.IsHighlander);
				bool flag2 = (RegisterSkillConditionCheck.IsHighlander(_skill.ConditionFilterCollection) ? _skill.ConditionFilterCollection : (_skill.ConditionFilterCollection.ConditionCheckerFilterList.ElementAt(0) as NetworkSkillPreprocessConditionCheck).GetConditionSkillFilterCollection()).VariableCompareFilter.Any((SkillVariableComareFilter x) => x.Compare == "=");
				return num == flag2;
			}
			return false;
		}
		if (!_skill.SkillPrm.ownerCard.IsPlayer && ((_skill.OnWhenDraw != 0 && _skill.SkillPrm.ownerCard.Skills.Any((SkillBase s) => s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard))) || _skill.OnDisCardStart != 0))
		{
			if (!networkBattleManagerBase.networkBattleData.GetReceiveData().GetReceiveCardList().Any((CardDataModel c) => c.IsOpen && c.Index == _skill.SkillPrm.ownerCard.Index))
			{
				return false;
			}
			return result;
		}
		if (!_skill.SkillPrm.ownerCard.IsPlayer && RegisterValidate.IsSendOpenMyCardsSkill(_skill) && _skill.OnSelfTurnEndStart != 0)
		{
			if (!networkBattleManagerBase.networkBattleData.GetReceiveData().GetReceiveCardList().Any((CardDataModel c) => c.Index == _skill.SkillPrm.ownerCard.Index))
			{
				return false;
			}
			return result;
		}
		return result;
	}

	public override List<BattleCardBase> GetSelectableCards(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isSkipForceSelect = false, List<BattleCardBase> selectedCards = null)
	{
		if (_validateSkillCheckFlag)
		{
			IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = _skill.ApplyBattlePlayerFilter.Filtering(playerInfoPair);
			IEnumerable<IReadOnlyBattleCardInfo> enumerable = _skill.FilteringByTargetFilter(playerInfoPair, option);
			int i = 0;
			for (int count = _skill.ApplyCustomSelectFilterList.Count; i < count; i++)
			{
				enumerable = _skill.ApplyCustomSelectFilterList[i].Filtering(enumerable, battlePlayerInfos, option);
			}
			return enumerable.Cast<BattleCardBase>().ToList();
		}
		return (from c in base.GetSelectableCards(playerInfoPair, option, isSkipForceSelect, selectedCards)
			select (!_networkBattleMgr.GameMgr.IsAdminWatch && !_networkBattleMgr.GameMgr.IsReplayBattle && !c.IsPlayer && !c.IsInplay && !c.Card.IsInplay && !_skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetTurnPlayCardsOtherSelfFilter) && !_skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.BattlePlayerFilter is OpponentBattlePlayerFilter && f.TargetFilter is SkillTargetGamePlayCardsOtherSelfFilter)) ? c.Card : c).ToList();
	}

	public override IEnumerable<BattleCardBase> CalcApplyTargets(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, ref int targetCount, bool isCheckInHand = false)
	{
		bool flag = false;
		NetworkBattleManagerBase networkBattleManagerBase = _skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
		if (_isCheckOppoActionData && networkBattleManagerBase.networkBattleData.GetReceiveData().OpponentTargetDataList.Count() == 0)
		{
			flag = ((!(_skill.ApplyingTargetFilter is SkillTargetInPlayFilter) && !(_skill.ApplyingTargetFilter is SkillTargetInPlayOtherSelfFilter)) ? true : false);
		}
		if (flag)
		{
			return new List<BattleCardBase>();
		}
		if (_isAllHandCardTarget)
		{
			return _skill.SkillPrm.selfBattlePlayer.HandCardList;
		}
		if (_isAllDeckCardTarget)
		{
			return _skill.SkillPrm.selfBattlePlayer.DeckCardList;
		}
		if (option.SelectedCards.Count < 0 || !option.SelectedCards.Any((SkillConditionCheckerOption.SkillAndSelectTarget s) => s.SelectSkill == _skill && s.SelectCard != null))
		{
			if (IsUnapprovedSkill() && !isCheckInHand)
			{
				List<BattleCardBase> list = CalculationUnapprovedCardList(NetworkBattleGenericTool.GetPublishSkillCount(_skill));
				_skill.CallOnCalcApplyTargets(_skill, list);
				return list;
			}
			IEnumerable<BattleCardBase> selectableCards = _skill.GetSelectableCards(playerInfoPair, option);
			return _skill.ApplySelectFilter.Filtering(selectableCards, _skill.OptionValue, option);
		}
		return IfNeededSelectCardCheck(playerInfoPair, option);
	}

	public override VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>> FixedSkillApplyTarget(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, ref int targetCount)
	{
		VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>> vfxWith;
		if ((_skill.SkillPrm.ownerCard.IsPlayer && (!_networkBattleMgr.GameMgr.IsWatchBattle || _networkBattleMgr.IsRecovery)) || _networkBattleMgr.GameMgr.IsReplayBattle)
		{
			IEnumerable<BattleCardBase> source = CalcApplyTargets(playerInfoPair, option, ref targetCount);
			vfxWith = NotIndependentCardFiltering(source.ToList());
			return new VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>>(vfxWith.Vfx, SkillAllowTargetFiltering(vfxWith.Value_1), vfxWith.Value_2);
		}
		UnknownReplaceCard(NetworkBattleGenericTool.GetPublishSkillCount(_skill));
		IEnumerable<BattleCardBase> replaceCard = GetReplaceCard();
		IEnumerable<BattleCardBase> enumerable;
		if (replaceCard != null && replaceCard.Count() >= 1)
		{
			enumerable = GetReplaceCard().ToList();
		}
		else
		{
			enumerable = CalcApplyTargets(playerInfoPair, option, ref targetCount);
			if (_skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase networkBattleManagerBase && targetCount > 0)
			{
				networkBattleManagerBase.RecoveryRecordSkillTarget(enumerable);
				enumerable = networkBattleManagerBase.RecoverySkillTarget(enumerable, targetCount);
			}
		}
		ClearReplaceCard();
		vfxWith = NotIndependentCardFiltering(enumerable.ToList());
		return new VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>>(vfxWith.Vfx, SkillAllowTargetFiltering(vfxWith.Value_1), vfxWith.Value_2);
	}

	private void UnknownReplaceCard(int publishedActiveCount)
	{
		if (_skill is Skill_summon_card)
		{
			replaceCards = null;
			SetReplaceCards(CalculationUnapprovedCardList(publishedActiveCount));
		}
	}

	public IEnumerable<BattleCardBase> GetReplaceCard()
	{
		return replaceCards;
	}

	public void SetReplaceCards(IEnumerable<BattleCardBase> target)
	{
		replaceCards = target;
	}

	public void ClearReplaceCard()
	{
		replaceCards = null;
	}

	public void SetReceiveSkillConditionCheck()
	{
		_isReceiveSkillConditionCheck = true;
	}

	public void SetUnapproved()
	{
		_isUnapprovedSkill = true;
	}

	private bool IsUnapprovedSkill()
	{
		if (_networkBattleMgr.GameMgr.IsWatchBattle)
		{
			if (_isUseUListOnlySelfTurnWhenAdmin)
			{
				if (_isUnapprovedSkill)
				{
					return _skill.SkillPrm.ownerCard.SelfBattlePlayer.IsSelfTurn;
				}
				return false;
			}
			return _isUnapprovedSkill;
		}
		if (_isUnapprovedSkill && _networkBattleMgr.BattleEnemy.IsSelfTurn)
		{
			return true;
		}
		return false;
	}

	public void SetHandAllSelect()
	{
		_isAllHandCardTarget = true;
	}

	public void SetDeckAllSelect()
	{
		_isAllDeckCardTarget = true;
	}

	public void SetValidateConditionCheckSkill()
	{
		_validateSkillCheckFlag = true;
	}

	public void SetNotCheckBuriaRiteCondition(bool value)
	{
		IsNotCheckBuriaRiteCondition = value;
	}

	public void SetPlaySkill()
	{
		_playSkill = true;
	}

	public void SetNotPlaySkill()
	{
		_notPlaySkill = true;
	}

	public void SetCheckOppoActionData()
	{
		_isCheckOppoActionData = true;
	}

	public void AddSkillMovementNum()
	{
		_skillMovementNum++;
	}

	public void ClearSkillMovementNum()
	{
		_skillMovementNum = 0;
	}

	public int GetSkillMovementNum()
	{
		return _skillMovementNum;
	}

	private List<BattleCardBase> CalculationUnapprovedCardList(int skillIndex)
	{
		BattlePlayerBase selfBattlePlayer = _skill.SkillPrm.ownerCard.SelfBattlePlayer;
		return (selfBattlePlayer.BattleMgr as NetworkBattleManagerBase).GetUnapprovedCardObj(selfBattlePlayer, _skill.SkillPrm.ownerCard.Index, skillIndex, _skillMovementNum, _skill);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_summon_token : SkillBaseSummon
{

	private ApplySkillTargetFilterCollection _filters;

	protected SummonedCardsList _summonedCardsList;

	protected BattlePlayerBase _summonPlayer;

	public override bool IsTargetIndicate => false;

	public bool IsReanimate
	{
		get
		{
			if (base.ApplyCardFilterList.FirstOrDefault((ISkillCardFilter f) => f is SkillParameterCostFilter) is SkillParameterCostFilter skillParameterCostFilter && skillParameterCostFilter.GetParameterOptionText() == "<:=")
			{
				return base.ApplyingTargetFilter is SkillTargetDestroyedCardListFilter;
			}
			return false;
		}
	}

	public Skill_summon_token(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		ValueWithOperator valueWithOperator = base.OptionValue.GetValueWithOperator(SkillFilterCreator.ContentKeyword.base_card_id);
		if (valueWithOperator != null && valueWithOperator.Operator == "!=")
		{
			string text = valueWithOperator.Value;
			_filters = new ApplySkillTargetFilterCollection();
			if (text.First() == '{')
			{
				text = text.Remove(0, 1);
				text = text.Remove(text.Length - 1, 1);
			}
			string[] array = text.Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				SkillFilterCreator.SetupTarget(_filters, array[i], skillPrm.ownerCard, this);
			}
		}
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		_summonedCardsList = null;
		InitSummonParameter();
		base.SkillPrm.ownerCard.SelfBattlePlayer.SkillSummonedCards = new List<BattleCardBase>();
		IEnumerable<int> enumerable = null;
		List<int> excludeIds = null;
		if (_filters != null)
		{
			excludeIds = (from BattleCardBase c in _filters.Filtering(new BattlePlayerReadOnlyInfoPair(base.SkillPrm.ownerCard.SelfBattlePlayer, base.SkillPrm.ownerCard.OpponentBattlePlayer), new SkillConditionCheckerOption(), base.OptionValue)
				select c.BaseParameter.BaseCardId).ToList();
			if (IsMakeFoil)
			{
				excludeIds = CardID2Foil(excludeIds).ToList();
			}
		}
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.random_count, -1);
		int num2;
		if (base.OptionValue.HasInfoByName(SkillFilterCreator.ContentKeyword.repeat_count))
		{
			num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.repeat_count);
			if (num2 == -1)
			{
				return NullVfxWithLoading.GetInstance();
			}
		}
		else
		{
			num2 = -1;
		}
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.summon_token, "_OPT_NULL_");
		string option = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.summon_side, "null");
		string option2 = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.effect, "NONE");
		bool flag = num >= 0 && !base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast;
		if (parameter.targetCards.Count() > 0)
		{
			enumerable = GetTargetId(parameter.targetCards, IsMakeFoil);
		}
		else if ("_OPT_NULL_" != text)
		{
			enumerable = SkillOptionValue.ParseOptionTokenID(text);
			if (IsMakeFoil)
			{
				enumerable = CardID2Foil(enumerable);
			}
		}
		if (excludeIds != null)
		{
			enumerable = enumerable.Where((int id) => !excludeIds.Contains(id));
		}
		if (flag)
		{
			bool flag2 = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_random_distinct, "null") == "true";
			if (flag2)
			{
				enumerable = enumerable.Where((int id) => !base.SkillPrm.ownerCard.SkillApplyInformation.RandomSelectedCardList.Any((BattleCardBase c) => c.CardId == id));
				if (enumerable.Count() == 0)
				{
					return NullVfxWithLoading.GetInstance();
				}
				if (base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.reset_random_distinct_by, "NONE") == "self_turn_end")
				{
					SetOnLoseEvent(base.SkillPrm.ownerCard, null, null);
				}
			}
			enumerable = SkillRandomSelectFilter.Filtering(num, enumerable, base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr, flag2);
		}
		if (enumerable == null || enumerable.Count() == 0 || enumerable.FirstOrDefault() == 0 || num2 == 0)
		{
			return NullVfxWithLoading.GetInstance();
		}
		if (num2 > 0)
		{
			List<int> list = new List<int>();
			for (int num3 = 0; num3 < enumerable.Count(); num3++)
			{
				for (int num4 = 0; num4 < num2; num4++)
				{
					list.Add(enumerable.ElementAt(num3));
				}
			}
			enumerable = list;
		}
		_summonPlayer = ((option == SkillFilterCreator.ContentKeyword.me.ToStringCustom() || option == "null") ? base.SkillPrm.selfBattlePlayer : base.SkillPrm.opponentBattlePlayer);
		_summonedCardsList = CreateSummonedCardsList(parameter.targetCards, enumerable, _summonPlayer, option);
		CallOnSummonTokenCards(option2 != "NONE");
		List<BattleCardBase> list2 = _summonedCardsList.summonedCards.ToList();
		if (IsBattleLog && list2.Count > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillSummon(list2, this);
		}
		BattlePlayerBase.SummonInfo summonInfo = new BattlePlayerBase.SummonInfo(_summonPlayer.IsPlayer, _summonedCardsList, SUMMON_TYPE.TOKEN, IsReanimate);
		BattlePlayerBase selfBattlePlayer = base.SkillPrm.selfBattlePlayer;
		SkillProcessor skillProcessor = parameter.skillProcessor;
		BattlePlayerBase.SummonInfo summonInfo2 = summonInfo;
		VfxWithLoadingSequential vfxWithLoadingToRegister = selfBattlePlayer.CardManagement(null, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.SUMMON, isRandom: false, null, null, this, summonInfo2) as VfxWithLoadingSequential;
		AddLastTarget(parameter, _summonedCardsList.ToList());
		base.SkillPrm.selfBattlePlayer.UpdateHandCardsPlayability();
		VfxWithLoading vfxWithLoadingToRegister2 = CreateSummonCardAnimation(base.SkillPrm.ownerCard.IsPlayer, _summonedCardsList, option2 != "NONE");
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoadingToRegister2);
		vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoadingToRegister);
		if (flag)
		{
			foreach (BattleCardBase summonedCards in _summonedCardsList)
			{
				base.SkillPrm.ownerCard.SkillApplyInformation.AddRandomSelectedCard(summonedCards);
			}
		}
		return vfxWithLoadingSequential;
	}

	protected override VfxWithLoading CreateSummonCardAnimation(bool isPlayer, SummonedCardsList summonedCardsList, bool isOwnerEffect = false)
	{
		VfxWithLoading vfxWithLoadingToRegister = CreateSummonSkillEffect(base.SkillPrm, summonedCardsList, isOwnerEffect);
		bool isSeSysSummonLandingDuplicateCheck = Global.SeSysSummonLandingDuplicateCheckId.Contains(base.SkillPrm.ownerCard.BaseParameter.BaseCardId);
		StartPickMultiCardVfx vfxToRegister = new StartPickMultiCardVfx(summonedCardsList, base.SkillPrm.resourceMgr, isPlayer, isToken: true, _isIgnoreVoice, _isRandomVoice, isGetoff: false, _isEvoVoice, _voiceWaitTime, null, isSeSysSummonLandingDuplicateCheck);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoadingToRegister);
		vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		return vfxWithLoadingSequential;
	}

	protected VfxWithLoading CreateSummonSkillEffect(SkillParameter skillPrm, SummonedCardsList summonedCardsList, bool isOwnerEffect)
	{
		if (!isOwnerEffect)
		{
			return CreateSkillEffect(skillPrm.resourceMgr, summonedCardsList, isFollowInHand: false, addToLastOperation: true);
		}
		return CreateOwnerSummonSkillEffect(skillPrm.resourceMgr, summonedCardsList);
	}

	private VfxWithLoading CreateOwnerSummonSkillEffect(IBattleResourceMgr resourceMgr, SummonedCardsList summonedCardsList)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		foreach (SummonedCardsList.CardEffectPair cardEffectPair in summonedCardsList.cardEffectPairList)
		{
			BattleCardBase targetCard = cardEffectPair.card;
			VfxWithLoading vfxWithLoading = CreateSingleVfx(resourceMgr, () => targetCard.BattleCardView.CardWrapObject.transform.position, targetCard.AsIEnumerable(), addToLastOperation: true);
			cardEffectPair.summonEffect = vfxWithLoading.MainVfx;
			vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		}
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnEffect(base.SkillPrm.buildInfo, isFollowInHand: false, isTargetPosition: true, addToLastOperation: true);
		return vfxWithLoadingSequential;
	}

	private IEnumerable<int> CardID2Foil(IEnumerable<int> cardIDs)
	{
		CardMaster cardMaster = CardMaster.GetInstanceForBattle();
		return from id in cardIDs
			select cardMaster.GetCardParameterFromId(id) into param
			select param.FoilCardId;
	}

	private IEnumerable<int> GetTargetId(IEnumerable<BattleCardBase> targetCards, bool isFoil)
	{
		if (isFoil)
		{
			return targetCards.Select((BattleCardBase card) => (!card.IsChoiceEvolutionCard) ? card.BaseParameter.FoilCardId : (card.BaseParameter.BaseCardId + 1));
		}
		return targetCards.Select((BattleCardBase card) => (!card.IsChoiceEvolutionCard) ? card.BaseParameter.NormalCardId : card.BaseParameter.BaseCardId);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		Func<SkillProcessor, VfxBase> resetFunction = null;
		resetFunction = delegate
		{
			targetCard.SkillApplyInformation.RandomSelectedCardList.Clear();
			BattlePlayerBase selfBattlePlayer2 = targetCard.SelfBattlePlayer;
			selfBattlePlayer2.OnTurnEndSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Remove(selfBattlePlayer2.OnTurnEndSkillAfter, resetFunction);
			return NullVfx.GetInstance();
		};
		BattlePlayerBase selfBattlePlayer = targetCard.SelfBattlePlayer;
		selfBattlePlayer.OnTurnEndSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Combine(selfBattlePlayer.OnTurnEndSkillAfter, resetFunction);
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			BattlePlayerBase selfBattlePlayer2 = card.SelfBattlePlayer;
			selfBattlePlayer2.OnTurnEndSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Remove(selfBattlePlayer2.OnTurnEndSkillAfter, resetFunction);
			card.SkillApplyInformation.RandomSelectedCardList.Clear();
			return NullVfx.GetInstance();
		};
	}

	public override void AddIndividualIdSkillBuffLog(Skill_attach_skill attachSkill, BattleCardBase target)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.selfBattlePlayer, base.SkillPrm.opponentBattlePlayer);
		SkillCollectionBase.SetupOptionValue(base.OptionValue, playerInfoPair, base.SkillPrm.ownerCard, this);
		int num = SkillOptionValue.ParseOptionTokenID(base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.summon_token, "_OPT_NULL_")).FirstOrDefault();
		BuffInfo buffInfo = new BuffInfo(num, num, this);
		buffInfo.IsReserveTokenDrawSkill = true;
		target.AddBuffInfo(buffInfo);
		attachSkill.OnIndividualIdSkillStop = (Action)Delegate.Combine(attachSkill.OnIndividualIdSkillStop, (Action)delegate
		{
			target.RemoveBuffInfo(buffInfo);
			if (target.IsClass)
			{
				UpdateClassBuffIfActive(target);
			}
		});
	}

	protected virtual void CallOnSummonTokenCards(bool isOwnerEffect)
	{
	}
}

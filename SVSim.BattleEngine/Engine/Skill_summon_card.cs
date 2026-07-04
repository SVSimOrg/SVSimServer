using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_summon_card : SkillBaseSummon
{
	protected SummonedCardsList _summonedCardsList;

	public bool IsDeckSelfSummon => base.IsDeckSelfSkill;

	public Skill_summon_card(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		InitSummonParameter();
		base.SkillPrm.ownerCard.SelfBattlePlayer.SkillSummonedCards = new List<BattleCardBase>();
		int count = base.SkillPrm.selfBattlePlayer.DeckCardList.Count;
		SUMMON_TYPE sUMMON_TYPE = SUMMON_TYPE.DESTROYED;
		foreach (BattleCardBase card in parameter.targetCards)
		{
			if (base.SkillPrm.selfBattlePlayer.DeckCardList.Any((BattleCardBase p) => p == card))
			{
				sUMMON_TYPE = SUMMON_TYPE.DECK;
				break;
			}
			if (base.SkillPrm.selfBattlePlayer.HandCardList.Any((BattleCardBase p) => p == card))
			{
				sUMMON_TYPE = SUMMON_TYPE.HAND;
				break;
			}
		}
		if (sUMMON_TYPE == SUMMON_TYPE.DESTROYED && !parameter.targetCards.Any((BattleCardBase c) => c.IsDead))
		{
			return NullVfxWithLoading.GetInstance();
		}
		_summonedCardsList = CreateSummonedCardsList(parameter.targetCards);
		if (!_summonedCardsList.Any())
		{
			return NullVfxWithLoading.GetInstance();
		}
		if (IsCantSummon())
		{
			return NullVfxWithLoading.GetInstance();
		}
		CallOnSummonCards();
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillSummon(_summonedCardsList.summonedCards.ToList(), this);
		}
		BattlePlayerBase.SummonInfo summonInfo = new BattlePlayerBase.SummonInfo(base.SkillPrm.ownerCard.IsPlayer, _summonedCardsList, sUMMON_TYPE, isReanimate: false, base.IsDeckSelfSkill);
		BattlePlayerBase selfBattlePlayer = base.SkillPrm.selfBattlePlayer;
		SkillProcessor skillProcessor = parameter.skillProcessor;
		BattlePlayerBase.SummonInfo summonInfo2 = summonInfo;
		VfxWithLoadingSequential vfxWithLoadingSequential = selfBattlePlayer.CardManagement(null, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.SUMMON, isRandom: false, null, null, this, summonInfo2) as VfxWithLoadingSequential;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		switch (sUMMON_TYPE)
		{
		case SUMMON_TYPE.HAND:
			foreach (BattleCardBase summonedCards in _summonedCardsList)
			{
				parallelVfxPlayer.Register(RemoveHandCardFromViewVfx(summonedCards, base.SkillPrm.selfBattlePlayer, isBurialRite: false));
				parallelVfxPlayer.Register(summonedCards.StopSpellCharge());
				summonedCards.BattleCardView.SetNormalLabelEnable(isEnable: true);
			}
			break;
		case SUMMON_TYPE.DECK:
			if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsReplayBattle || !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsNetworkBattle)
			{
				for (int num = 0; num < _summonedCardsList.summonedCards.Count(); num++)
				{
					_summonedCardsList.summonedCards.ElementAt(num).BattleCardView.SetNormalLabelEnable(isEnable: true);
				}
			}
			base.SkillPrm.selfBattlePlayer.CallOnChangeDeckAfterEvent(count, parameter.skillProcessor, _summonedCardsList.summonedCards.ToList());
			break;
		}
		VfxWithLoading vfxWithLoading = CreateSummonCardAnimation(base.SkillPrm.ownerCard.IsPlayer, _summonedCardsList);
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create(vfxWithLoading.MainVfx);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (IsDeckSelfSummon)
		{
			BattleCardBase battleCardBase = _summonedCardsList.summonedCards.First();
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			parallelVfxPlayer2.Register(NullVfx.GetInstance());
		}
		VfxWithLoadingSequential vfxWithLoadingSequential2 = VfxWithLoadingSequential.Create(vfxWithLoadingSequential.LoadingVfx, parallelVfxPlayer, sequentialVfxPlayer, parallelVfxPlayer2, vfxWithLoadingSequential.MainVfx);
		vfxWithLoadingSequential2.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		return vfxWithLoadingSequential2;
	}

	private SummonedCardsList CreateSummonedCardsList(IEnumerable<BattleCardBase> summonedCards)
	{
		int num = base.SkillPrm.selfBattlePlayer.ClassAndInPlayCardList.Count;
		SummonedCardsList summonedCardsList = new SummonedCardsList();
		foreach (BattleCardBase summonedCard in summonedCards)
		{
			if (num < 6)
			{
				summonedCardsList.AddCardToSummonedCards(summonedCard, !IsDeckSelfSummon);
				num++;
			}
		}
		return summonedCardsList;
	}

	protected override VfxWithLoading CreateSummonCardAnimation(bool isPlayer, SummonedCardsList summonedCardsList, bool isOwnerEffect = false)
	{
		VfxWithLoading vfxWithLoadingToRegister = CreateSummonSkillEffect(base.SkillPrm, summonedCardsList);
		StartPickMultiCardVfx vfxToRegister = new StartPickMultiCardVfx(summonedCardsList, base.SkillPrm.resourceMgr, isPlayer, isToken: true, _isIgnoreVoice, _isRandomVoice, isGetoff: false, _isEvoVoice, _voiceWaitTime);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoadingToRegister);
		vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		return vfxWithLoadingSequential;
	}

	protected VfxWithLoading CreateSummonSkillEffect(SkillParameter skillPrm, SummonedCardsList summonedCardsList)
	{
		IBattleResourceMgr resourceMgr = skillPrm.resourceMgr;
		if (skillPrm.buildInfo._effectTargetType == EffectMgr.TargetType.SINGLE)
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
		return CreateSkillEffect(resourceMgr, summonedCardsList, isFollowInHand: false, addToLastOperation: true);
	}

	public static VfxBase RemoveHandCardFromViewVfx(BattleCardBase targetHandCard, BattlePlayerBase self, bool isBurialRite)
	{
		return InstantVfx.Create(delegate
		{
			self.BattleView.HandView.RemoveCardFromView(targetHandCard.BattleCardView, 0.1f);
			iTween.Stop(targetHandCard.BattleCardView.GameObject);
			targetHandCard.BattleCardView.GameObject.SetActive(value: false);
			targetHandCard.BattleCardView.HideCanPlayEffect();
			targetHandCard.BattleCardView.ResetCardView(targetHandCard.BaseParameter);
			List<int> burialRiteOrDiscardCardHandIndexList = targetHandCard.SelfBattlePlayer.BurialRiteOrDiscardCardHandIndexList;
			if (isBurialRite && burialRiteOrDiscardCardHandIndexList.Count > 0)
			{
				int num = burialRiteOrDiscardCardHandIndexList[0];
				burialRiteOrDiscardCardHandIndexList.RemoveAt(0);
				for (int i = 0; i < burialRiteOrDiscardCardHandIndexList.Count; i++)
				{
					if (num < burialRiteOrDiscardCardHandIndexList[i])
					{
						burialRiteOrDiscardCardHandIndexList[i]--;
					}
				}
			}
		});
	}

	public bool IsCantSummon()
	{
		List<Skill_cant_summon.CantSummonInfo> cantSummonList = base.SkillPrm.ownerCard.SelfBattlePlayer.Class.SkillApplyInformation.CantSummonList;
		for (int i = 0; i < cantSummonList.Count; i++)
		{
			if (cantSummonList[i] == Skill_cant_summon.CantSummonInfo.DeckSelf && base.IsDeckSelfSkill)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void CallOnSummonCards()
	{
	}
}

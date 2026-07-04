using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_getoff : SkillBaseSummon
{
	public override bool IsTargetIndicate => false;

	public Skill_getoff(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		InitSummonParameter();
		IEnumerable<int> targetId = GetTargetId(base.SkillPrm.ownerCard.GetOnCards, IsMakeFoil);
		if (targetId == null || targetId.Count() == 0)
		{
			return NullVfxWithLoading.GetInstance();
		}
		SummonedCardsList summonedCardsList = CreateSummonedCardsList(base.SkillPrm.ownerCard.GetOnCards, targetId, base.SkillPrm.selfBattlePlayer, "null", isGetoff: true);
		List<BattleCardBase> list = summonedCardsList.summonedCards.ToList();
		if (IsBattleLog && list.Count > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGetOff(this, list);
		}
		BattlePlayerBase.SummonInfo summonInfo = new BattlePlayerBase.SummonInfo(base.SkillPrm.ownerCard.IsPlayer, summonedCardsList, SUMMON_TYPE.TOKEN);
		VfxWithLoading vfxWithLoadingToRegister = (VfxWithLoading)base.SkillPrm.selfBattlePlayer.CardManagement(null, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.GETOFF, isRandom: false, null, base.SkillPrm.ownerCard, this, summonInfo);
		AddLastTarget(parameter, summonedCardsList.ToList());
		base.SkillPrm.selfBattlePlayer.UpdateHandCardsPlayability();
		VfxWithLoading vfxWithLoadingToRegister2 = CreateSummonCardAnimation(base.SkillPrm.ownerCard.IsPlayer, summonedCardsList, isOwnerEffect: true);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoadingToRegister2);
		vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoadingToRegister);
		return vfxWithLoadingSequential;
	}

	protected override VfxWithLoading CreateSummonCardAnimation(bool isPlayer, SummonedCardsList summonedCardsList, bool isOwnerEffect = false)
	{
		VfxWithLoading vfxWithLoadingToRegister = CreateSummonSkillEffect(base.SkillPrm, summonedCardsList, isOwnerEffect);
		StartPickMultiCardVfx vfxToRegister = new StartPickMultiCardVfx(summonedCardsList, base.SkillPrm.resourceMgr, isPlayer, isToken: true, _isIgnoreVoice, _isRandomVoice, isGetoff: true);
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

	private IEnumerable<int> GetTargetId(IEnumerable<BattleCardBase> targetCards, bool isFoil)
	{
		if (isFoil)
		{
			return targetCards.Select((BattleCardBase card) => card.BaseParameter.FoilCardId);
		}
		return targetCards.Select((BattleCardBase card) => card.BaseParameter.NormalCardId);
	}
}

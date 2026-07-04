using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_fusion_metamorphose : Skill_metamorphose
{

	public override bool IsShowSideLogSkillType => false;

	public Skill_fusion_metamorphose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		IEnumerable<BattleCardBase> enumerable = parameter.targetCards.Where((BattleCardBase c) => c.IsInHand);
		List<MetamorphoseCardPair> list = new List<MetamorphoseCardPair>();
		base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.random_count, -1);
		List<BattleCardBase> inplayTargets = enumerable.Where((BattleCardBase c) => c.IsInplay).ToList();
		if (_metamorphoseCardId == -1)
		{
			return NullVfxWithLoading.GetInstance();
		}
		if (!enumerable.Any())
		{
			return NullVfxWithLoading.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer3 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer4 = ParallelVfxPlayer.Create();
		float num = 0f;
		int num2 = 0;
		foreach (BattleCardBase item in enumerable)
		{
			BattleCardBase originalCard = item.Card;
			BattleCardBase metamorphosedCard;
			if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsNetworkBattle && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork && !base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsReplayBattle)
			{
				metamorphosedCard = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.MetamorphoseCard(_metamorphoseCardId, originalCard.IsPlayer, originalCard.Index, this, isFusion: true);
			}
			else
			{
				metamorphosedCard = originalCard.SelfBattlePlayer.CreateCard(_metamorphoseCardId, originalCard.Index);
			}
			if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
			{
				metamorphosedCard.BattleCardView.CardWrapObject.SetActive(value: false);
			}
			originalCard.MetamorphoseCard = metamorphosedCard;
			metamorphosedCard.TransformInfo = new BattleCardBase.TransformInformation(BattleCardBase.TransformType.Metamorphose, originalCard);
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				metamorphosedCard.BattleCardView.GameObject.SetActive(value: true);
				metamorphosedCard.BattleCardView.CardWrapObject.SetActive(value: false);
				metamorphosedCard.BattleCardView.Collider.enabled = false;
				metamorphosedCard.SetOnDraw(draw: false);
			}));
			parallelVfxPlayer2.Register(base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.LoadCardResources(new List<BattleCardBase> { metamorphosedCard }));
			parallelVfxPlayer3.Register(originalCard.Metamorphose(parameter.skillProcessor));
			MetamorphoseCardPair metamorphoseCardPair = new MetamorphoseCardPair(originalCard, metamorphosedCard);
			list.Add(metamorphoseCardPair);
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery && (originalCard.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch))
			{
				VfxBase canNotTouchCardVfx = NullVfx.GetInstance();
				SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(canNotTouchCardVfx);
				_canNotTouchCardVfxList.Add(canNotTouchCardVfx);
			}
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				base.SkillPrm.ownerCard.SelfBattlePlayer.BattleView.PlayQueueView.ReplaceCard(metamorphosedCard.BattleCardView, originalCard.BattleCardView);
			}));
			sequentialVfxPlayer.Register(metamorphosedCard.SelfBattlePlayer.ReplaceInHand(originalCard, metamorphosedCard, parameter.skillProcessor));
			InstantVfx instantVfx = InstantVfx.Create(delegate
			{
				base.SkillPrm.selfBattlePlayer.BattleView.HandView.ReplaceCardInView(originalCard.BattleCardView, metamorphosedCard.BattleCardView);
				MotionUtils.SetLayerAll(metamorphosedCard.BattleCardView.GameObject, 31);
			});
			originalCard.BattleCardView.HideCanPlayEffect();
			VfxBase vfxBase = Skill_metamorphose.SetupMetamorphosedCardTransformVfx(originalCard, metamorphosedCard);
			ParallelVfxPlayer parallelVfxPlayer5 = ParallelVfxPlayer.Create(instantVfx, InstantVfx.Create(delegate
			{
				metamorphosedCard.BattleCardView.Transform.position = originalCard.BattleCardView.Transform.position;
				metamorphosedCard.BattleCardView.Transform.rotation = originalCard.BattleCardView.Transform.rotation;
				metamorphosedCard.BattleCardView.Transform.localScale = originalCard.BattleCardView.Transform.localScale;
				Skill_metamorphose.SwapMetamorphosedCardMesh(originalCard, metamorphosedCard);
			}));
			GameObject effectGameObject = null;
			VfxBase loadingVfx = NullVfx.GetInstance();
			VfxBase playEffectAndSeVfx = NullVfx.GetInstance();
			VfxWithLoading vfxWithLoading = VfxWithLoading.Create(loadingVfx, playEffectAndSeVfx);
			parallelVfxPlayer2.Register(vfxWithLoading.LoadingVfx);
			base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnEffect(base.SkillPrm.buildInfo);
			SequentialVfxPlayer morphVfx = SequentialVfxPlayer.Create(vfxBase, vfxWithLoading.MainVfx, WaitVfx.Create(0.5f), parallelVfxPlayer5);
			sequentialVfxPlayer.Register(Skill_metamorphose.AttachMetamorphoseCardToOriginalCardVfx(originalCard, metamorphosedCard));
			sequentialVfxPlayer.Register(WaitVfx.Create(num));
			num += 0.1f;
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			sequentialVfxPlayer.Register(Skill_metamorphose.EnableMetamorphosedCardColliderVfx(metamorphosedCard));
			sequentialVfxPlayer.Register(TouchableUpdateVfx(originalCard.IsPlayer));
			parallelVfxPlayer4.Register(sequentialVfxPlayer);
			num2++;
		}
		IncreaseSkillMetamorphoseCount(inplayTargets);
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillMetamorphose(list.ToList(), this, IsTargetInOpponentHand(), isFusion: true);
		}
		VfxBase vfxBase2 = parallelVfxPlayer2;
		vfxBase2 = UIManager.GetInstance().CreateNowLoadingVfx(parallelVfxPlayer2);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(parallelVfxPlayer, parallelVfxPlayer3, parallelVfxPlayer4);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxBase2);
		return vfxWithLoadingSequential;
	}

	public bool IsShowFusionMetamorphoseFrameEffect(IEnumerable<BattleCardBase> selectedCards)
	{
		if (selectedCards.Count() == 0)
		{
			return false;
		}
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.ownerCard.SelfBattlePlayer, base.SkillPrm.ownerCard.OpponentBattlePlayer);
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.FusionIngredientCards = BattlePlayerBase.ConvertToSkillInfoCollection(selectedCards);
		return CheckCondition(playerInfoPair, skillConditionCheckerOption, isPrePlay: true);
	}
}

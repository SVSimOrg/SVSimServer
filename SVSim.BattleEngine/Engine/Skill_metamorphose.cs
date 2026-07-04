using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_metamorphose : SkillBaseSummon
{
	public class MetamorphoseCardPair
	{
		public BattleCardBase OldCard;

		public BattleCardBase NewCard;

		public MetamorphoseCardPair(BattleCardBase oldCard, BattleCardBase newCard)
		{
			OldCard = oldCard;
			NewCard = newCard;
		}
	}

	protected List<VfxBase> _canNotTouchCardVfxList = new List<VfxBase>();

	protected int _metamorphoseCardId;

	public Skill_metamorphose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		_metamorphoseCardId = GetMetamorphoseCardId();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		IEnumerable<BattleCardBase> enumerable = parameter.targetCards.Where((BattleCardBase c) => c.IsInHand || c.IsInplay);
		List<MetamorphoseCardPair> list = new List<MetamorphoseCardPair>();
		List<BattleCardBase> inplayTargets = enumerable.Where((BattleCardBase c) => c.IsInplay).ToList();
		if (_metamorphoseCardId == -1)
		{
			return NullVfxWithLoading.GetInstance();
		}
		if (!enumerable.Any())
		{
			if (IsTargetInOpponentHand())
			{
				BattleLogManager.GetInstance().AddLogSkillMetamorphose(list.ToList(), this, isTargetInOpponentHand: true);
			}
			return NullVfxWithLoading.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer3 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer4 = ParallelVfxPlayer.Create();
		float num = 0f;
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		int num2 = 0;
		foreach (BattleCardBase item in enumerable)
		{
			BattleCardBase originalCard = item.Card;
			BattleCardBase metamorphosedCard;
			if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsNetworkBattle && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork && !base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsReplayBattle)
			{
				metamorphosedCard = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.MetamorphoseCard(_metamorphoseCardId, originalCard.IsPlayer, originalCard.Index, this);
			}
			else
			{
				metamorphosedCard = originalCard.SelfBattlePlayer.CreateCard(_metamorphoseCardId, originalCard.Index);
				if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
				{
					metamorphosedCard.BattleCardView.CardWrapObject.SetActive(value: false);
				}
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
			list2.Add(metamorphosedCard);
			parallelVfxPlayer3.Register(originalCard.Metamorphose(parameter.skillProcessor));
			MetamorphoseCardPair metamorphoseCardPair = new MetamorphoseCardPair(originalCard, metamorphosedCard);
			list.Add(metamorphoseCardPair);
			if (originalCard.IsInHand)
			{
				SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
				if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery && (originalCard.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch))
				{
					VfxBase canNotTouchCardVfx = NullVfx.GetInstance();
					SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(canNotTouchCardVfx);
					_canNotTouchCardVfxList.Add(canNotTouchCardVfx);
				}
				sequentialVfxPlayer.Register(metamorphosedCard.SelfBattlePlayer.ReplaceInHand(originalCard, metamorphosedCard, parameter.skillProcessor));
				InstantVfx instantVfx = InstantVfx.Create(delegate
				{
					base.SkillPrm.selfBattlePlayer.BattleView.HandView.ReplaceCardInView(originalCard.BattleCardView, metamorphosedCard.BattleCardView);
				});
				if (originalCard.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
				{
					originalCard.BattleCardView.HideCanPlayEffect();
					VfxBase vfxBase = SetupMetamorphosedCardTransformVfx(originalCard, metamorphosedCard);
					ParallelVfxPlayer parallelVfxPlayer5 = ParallelVfxPlayer.Create(instantVfx, metamorphosedCard.BattleCardView.ShowHandCardInfo(), InstantVfx.Create(delegate
					{
						metamorphosedCard.BattleCardView.Transform.position = originalCard.BattleCardView.Transform.position;
						metamorphosedCard.BattleCardView.Transform.rotation = originalCard.BattleCardView.Transform.rotation;
						metamorphosedCard.BattleCardView.Transform.localScale = originalCard.BattleCardView.Transform.localScale;
						SwapMetamorphosedCardMesh(originalCard, metamorphosedCard);
					}));
					GameObject effectGameObject = null;
					VfxBase loadingVfx = NullVfx.GetInstance();
					VfxBase mainVfx = NullVfx.GetInstance();
					VfxWithLoading vfxWithLoading = VfxWithLoading.Create(loadingVfx, mainVfx);
					parallelVfxPlayer2.Register(vfxWithLoading.LoadingVfx);
					base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnEffect(base.SkillPrm.buildInfo, isFollowInHand: false, isTargetPosition: false, addToLastOperation: true);
					SequentialVfxPlayer morphVfx = SequentialVfxPlayer.Create(vfxBase, vfxWithLoading.MainVfx, parallelVfxPlayer5);
					sequentialVfxPlayer.Register(AttachMetamorphoseCardToOriginalCardVfx(originalCard, metamorphosedCard));
					sequentialVfxPlayer.Register(WaitVfx.Create(num));
					num += 0.1f;
					sequentialVfxPlayer.Register(NullVfx.GetInstance());
				}
				else
				{
					sequentialVfxPlayer.Register(SequentialVfxPlayer.Create(SetupMetamorphosedCardTransformVfx(originalCard, metamorphosedCard), NullVfx.GetInstance(), InstantVfx.Create(delegate
					{
						SwapMetamorphosedCardMesh(originalCard, metamorphosedCard);
					}), instantVfx));
				}
				sequentialVfxPlayer.Register(EnableMetamorphosedCardColliderVfx(metamorphosedCard));
				sequentialVfxPlayer.Register(TouchableUpdateVfx(originalCard.IsPlayer));
				parallelVfxPlayer4.Register(sequentialVfxPlayer);
			}
			else if (originalCard.IsInplay)
			{
				ParallelVfxPlayer parallelVfxPlayer6 = ParallelVfxPlayer.Create();
				parallelVfxPlayer6.Register(SetupMetamorphosedCardTransformVfx(originalCard, metamorphosedCard));
				parallelVfxPlayer6.Register(originalCard.SkillApplyInformation.AllSkillEffectStop());
				parallelVfxPlayer6.Register(metamorphosedCard.SelfBattlePlayer.ReplaceInPlay(originalCard, metamorphosedCard, parameter.skillProcessor, isMetamorphose: true));
				parallelVfxPlayer6.Register(metamorphosedCard.SetUpInplay());
				metamorphosedCard.SelfBattlePlayer.GameInplayMetamorphoseCards.Add(metamorphosedCard);
				metamorphosedCard.BattleCardView.GameObject.transform.rotation = Quaternion.identity;
				VfxWithLoading vfxWithLoading2 = CreateSkillEffect(base.SkillPrm.resourceMgr, new BattleCardBase[1] { metamorphosedCard }, isFollowInHand: false, addToLastOperation: true, num2 > 0);
				num2++;
				parallelVfxPlayer2.Register(vfxWithLoading2.LoadingVfx);
				parallelVfxPlayer6.Register(SequentialVfxPlayer.Create(NullVfx.GetInstance(), EnableMetamorphosedCardColliderVfx(metamorphosedCard), metamorphosedCard.BattleCardView.InitializeBattleCardIcon(metamorphosedCard, metamorphosedCard.Skills)));
				originalCard.FlagCardAsDestroyedBySkill();
				parallelVfxPlayer6.Register(originalCard.RemoveFromInPlay());
				parallelVfxPlayer4.Register(parallelVfxPlayer6);
			}
		}
		parallelVfxPlayer2.Register(base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.LoadCardResources(list2));
		IncreaseSkillMetamorphoseCount(inplayTargets);
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillMetamorphose(list.ToList(), this, IsTargetInOpponentHand());
		}
		BattleCardBase newCard = list.First().NewCard;
		bool num3 = !newCard.IsPlayer && newCard.IsInHand;
		VfxBase vfxToRegister = parallelVfxPlayer2;
		if (!num3)
		{
			vfxToRegister = UIManager.GetInstance().CreateNowLoadingVfx(parallelVfxPlayer2);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(parallelVfxPlayer, parallelVfxPlayer3, parallelVfxPlayer4);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxToRegister);
		return vfxWithLoadingSequential;
	}

	public static VfxBase SetupMetamorphosedCardTransformVfx(BattleCardBase originalCard, BattleCardBase metamorphosedCard)
	{
		return InstantVfx.Create(delegate
		{
			Transform transform = metamorphosedCard.BattleCardView.Transform;
			Transform transform2 = originalCard.BattleCardView.Transform;
			transform.position = transform2.position;
			transform.rotation = transform2.rotation;
			transform.parent = transform2.parent;
			transform.localScale = transform2.localScale;
			transform.SetSiblingIndex(transform2.GetSiblingIndex());
		});
	}

	protected VfxBase TouchableUpdateVfx(bool isPlayer)
	{
		if (isPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
		{
			return InstantVfx.Create(delegate
			{
				if (_canNotTouchCardVfxList.Count > 0)
				{
					_canNotTouchCardVfxList.RemoveAt(0);
				}
			});
		}
		return NullVfx.GetInstance();
	}

	public static VfxBase AttachMetamorphoseCardToOriginalCardVfx(BattleCardBase originalCard, BattleCardBase metamorphosedCard)
	{
		return InstantVfx.Create(delegate
		{
			Transform transform = metamorphosedCard.BattleCardView.Transform;
			Transform transform2 = (transform.parent = originalCard.BattleCardView.Transform);
			transform.position = transform2.position;
			transform.rotation = transform2.rotation;
			transform.localScale = Vector3.one;
		});
	}

	public static VfxBase EnableMetamorphosedCardColliderVfx(BattleCardBase metamorphosedCard)
	{
		return InstantVfx.Create(delegate
		{
			metamorphosedCard.BattleCardView.Collider.enabled = true;
		});
	}

	public static void SwapMetamorphosedCardMesh(BattleCardBase originalCard, BattleCardBase metamorphosedCard)
	{
		originalCard.BattleCardView.GameObject.SetActive(value: false);
		originalCard.BattleCardView.Transform.SetParent(originalCard.SelfBattlePlayer.BattleView.BanishParent.transform);
		metamorphosedCard.BattleCardView.CardTemplate.CardWrapObjTemp.gameObject.SetActive(value: true);
	}

	public int GetMetamorphoseCardId()
	{
		int num = -1;
		string text = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.metamorphose, "_OPT_NULL_");
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is SingleBattleMgr)
		{
			text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.metamorphose, "_OPT_NULL_");
		}
		if (text != "_OPT_NULL_")
		{
			num = SkillOptionValue.ParseOptionTokenID(text).First();
		}
		if (IsMakeFoil)
		{
			num = CardMaster.GetInstanceForBattle().GetCardParameterFromId(num).FoilCardId;
		}
		return num;
	}

	protected void IncreaseSkillMetamorphoseCount(List<BattleCardBase> inplayTargets)
	{
		if (inplayTargets.Any())
		{
			BattlePlayerBase battlePlayer = base.SkillPrm.ownerCard.SelfBattlePlayer;
			TurnAndIntValue turnAndIntValue = battlePlayer.GameSkillMetamorphoseCountList.FirstOrDefault((TurnAndIntValue t) => t.IsSelfTurn == battlePlayer.IsSelfTurn && t.Turn == battlePlayer.Turn);
			if (turnAndIntValue != null)
			{
				turnAndIntValue.Increment();
			}
			else
			{
				battlePlayer.GameSkillMetamorphoseCountList.Add(new TurnAndIntValue(1, battlePlayer.Turn, battlePlayer.IsSelfTurn));
			}
		}
	}
}

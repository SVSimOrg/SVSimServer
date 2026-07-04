using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 27 of 47 methods unrun in baseline
//   Type: Wizard.Battle.ActionProcessor
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle;

public class ActionProcessor
{
	private readonly BattlePlayerPair _pair;

	public Action OnPlayComplete;

	public Action OnEvolutionComplete;

	public Action OnAttackComplete;

	public Action OnFusionComplete;

	public Action<BattleCardBase, BattleCardBase, int, int> OnAttackDamageComplete;

	public Action<BattleCardBase> OnAttackProcessComplete;

	public Action<BattleCardBase, BattleCardBase> OnAttackStart;

	public static readonly Vector3 ACCELERATE_POSITION_OFFSET = new Vector3(0f, 0f, -0.03f);

	public event Action<BattleCardBase, BattleCardBase, IEnumerable<BattleCardBase>> OnBeforePlayCard;

	public event Action<BattleCardBase, BattleCardBase, List<int>> OnBeforeChosenPlayCard;

	public event Action<BattleCardBase, IEnumerable<BattleCardBase>, bool> OnBeforeBurialRitePlayCard;

	public event Func<BattleCardBase, VfxBase> OnAfterPlayCard;

	public event Func<VfxBase> OnBeforeAttack;

	public event Func<VfxBase> OnBeforeAttackSkillComplete;

	public event Func<BattleCardBase, BattleCardBase, bool, VfxBase> OnAfterAttack;

	public event Action<BattleCardBase, BattleCardBase, IEnumerable<BattleCardBase>> OnBeforeEvolution;

	public event Action<BattleCardBase, BattleCardBase, List<int>> OnBeforeChosenEvolution;

	public event Action<BattleCardBase> OnJustBeforeEvolution;

	public event Action<BattleCardBase> OnRightAfterEvolution;

	public event Func<BattleCardBase, VfxBase> OnAfterEvolution;

	public event Action<BattleCardBase, IEnumerable<BattleCardBase>> OnBeforeFusion;

	public event Func<BattleCardBase, VfxBase> OnAfterFusion;

	public event Action<BattleCardBase, int, bool> OnTransform;

	public event Action<SkillBase> OnSpecialAccelerate;

	public ActionProcessor(BattlePlayerPair pair)
	{
		_pair = pair;
	}

	private void GetSelectTargetBySelectList(SkillBase selectSkill, List<BattleCardBase> selectList, List<SkillConditionCheckerOption.SkillAndSelectTarget> selectTargets, ref int skillIndex)
	{
		BattlePlayerPair playerInfoPair = new BattlePlayerPair(selectSkill.SkillPrm.ownerCard.SelfBattlePlayer, selectSkill.SkillPrm.ownerCard.OpponentBattlePlayer);
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		int skillSelectCount = selectSkill.GetSkillSelectCount();
		int num = 1;
		if (skillSelectCount >= 2)
		{
			NetworkBattleManagerBase networkBattleManagerBase = selectSkill.SkillPrm.selfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
			num = ((selectSkill.SkillPrm.selfBattlePlayer.IsPlayer || networkBattleManagerBase == null || _pair.Self.BattleMgr.GameMgr.IsAdminWatch || !RegisterValidate.IsValidateCard(selectSkill)) ? Math.Min(skillSelectCount, selectSkill.GetSelectableCards(playerInfoPair, option, isSkipForceSelect: true).Count()) : (from n in networkBattleManagerBase.GetValidateTargetSkillIndexList()
				where n == NetworkBattleGenericTool.GetSkillIndex(selectSkill)
				select n).Count());
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			if (skillIndex >= selectList.Count)
			{
				break;
			}
			selectTargets.Add(new SkillConditionCheckerOption.SkillAndSelectTarget(selectList[skillIndex], selectSkill));
			skillIndex++;
		}
	}

	private VfxBase SetSkillConditionCheckeroptionSelectCards(BattleCardBase card, SkillConditionCheckerOption option, IEnumerable<BattleCardBase> cards, bool isEvolve, List<int> selectChoiceCardIdList, bool isFusion = false, bool isChoiceBrave = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SkillCollectionBase skillCollectionBase = (isEvolve ? card.EvolutionSkills : card.Skills);
		bool isChoiceTransform = false;
		List<BattleCardBase> list = null;
		if (cards != null)
		{
			list = cards.ToList();
		}
		if (!isFusion && skillCollectionBase.Any((SkillBase s) => s is Skill_transform))
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(card.SelfBattlePlayer, card.OpponentBattlePlayer);
			CardMaster instanceForBattle = CardMaster.GetInstanceForBattle();
			for (int num = 1; num < skillCollectionBase.Count(); num++)
			{
				if (skillCollectionBase.Get(num) is Skill_transform skill_transform)
				{
					Skill_pp_fixeduse skill_pp_fixeduse = skillCollectionBase.Get(num - 1) as Skill_pp_fixeduse;
					if (false || (skill_transform.TransformId != -1 && skill_pp_fixeduse != null && skill_pp_fixeduse.IsMutationFixedUseCost && skill_pp_fixeduse.CheckCondition(playerInfoPair, option, isPrePlay: true) && card.GetAccelerateOrCrystallizeTransformSkill() != null))
					{
						BattleCardBase.TransformType transformType = ((skill_transform.OnWhenAccelerate != 0) ? BattleCardBase.TransformType.Accelerate : BattleCardBase.TransformType.Crystallize);
						sequentialVfxPlayer.Register(SwapTransformCard(card, card.BaseParameter.IsFoil ? instanceForBattle.GetCardParameterFromId(skill_transform.TransformId).FoilCardId : skill_transform.TransformId, option, transformType, selectChoiceCardIdList, list));
						this.OnSpecialAccelerate.Call(skill_pp_fixeduse);
						this.OnSpecialAccelerate.Call(skill_transform);
					}
					if (skillCollectionBase.Get(num - 1) is Skill_choice skill_choice && skill_choice.CheckCondition(playerInfoPair, option, isPrePlay: true))
					{
						isChoiceTransform = true;
					}
				}
			}
		}
		if (cards == null && selectChoiceCardIdList == null)
		{
			return sequentialVfxPlayer;
		}
		SkillBase skillBase = card.GetSelectTypeSkill(isEvolve, isFusion: false, isRegister: false, isEvolutionSimpleProcessor: false, isChoiceCheck: true).FirstOrDefault((SkillBase s) => s is Skill_choice);
		int num2 = 1;
		if (skillBase != null)
		{
			num2 = SetupChoiceCard(card, option, selectChoiceCardIdList, skillBase, list, isChoiceTransform, sequentialVfxPlayer, isChoiceBrave);
		}
		if (list != null && list.Count > 0)
		{
			BattleCardBase battleCardBase = card;
			if (option.PlayedCard != null && card != option.PlayedCard)
			{
				battleCardBase = option.PlayedCard;
			}
			IEnumerable<SkillBase> selectTypeSkill = battleCardBase.GetSelectTypeSkill(isEvolve, isFusion: false, isRegister: true);
			int num3 = selectTypeSkill.Where((SkillBase s) => s.IsBurialRite).Count();
			List<SkillBase> list2 = selectTypeSkill.Where((SkillBase s) => !(s is Skill_none)).ToList();
			int skillIndex = num3;
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				if (list2[num4].IsChoiceType)
				{
					num4 += num2 - 1;
				}
				else if (list2[num4].IsBurialRite)
				{
					if (list2[num4].IsUserSelectType)
					{
						GetSelectTargetBySelectList(list2[num4], list, option.SelectedCards, ref skillIndex);
					}
				}
				else
				{
					GetSelectTargetBySelectList(list2[num4], list, option.SelectedCards, ref skillIndex);
				}
			}
			for (int num5 = 0; num5 < num3; num5++)
			{
				option.BurialRiteCards.Add(list[num5]);
			}
			if (num3 > 0)
			{
				card.SkillApplyInformation.ClearLastBurialRiteCardList();
				card.SkillApplyInformation.AddLastBurialRiteCardList(list);
			}
		}
		return sequentialVfxPlayer;
	}

	private int SetupChoiceCard(BattleCardBase card, SkillConditionCheckerOption option, List<int> selectChoiceCardIdList, SkillBase selectChoiceSkill, List<BattleCardBase> selectList, bool isChoiceTransform, SequentialVfxPlayer vfx, bool isChoiceBrave)
	{
		int num = 1;
		if (option.PlayedCard == null)
		{
			option.PlayedCard = card;
		}
		int num2 = -1;
		if (selectChoiceSkill.ApplySelectFilter is SkillChoiceSelectFilter)
		{
			num = ((SkillChoiceSelectFilter)selectChoiceSkill.ApplySelectFilter).CalcCount(selectChoiceSkill.OptionValue);
		}
		if (selectChoiceCardIdList != null && selectChoiceCardIdList.Count >= 1)
		{
			foreach (int selectChoiceCardId in selectChoiceCardIdList)
			{
				num2 = selectChoiceCardId;
				option.ChosenCards.Add(num2);
			}
		}
		else if (selectList != null && selectList.Count > 0)
		{
			for (int i = 0; i < num; i++)
			{
				BattleCardBase battleCardBase = selectList[i];
				num2 = battleCardBase.BaseParameter.CardId;
				option.ChosenCards.Add(num2);
				if (isChoiceTransform || isChoiceBrave)
				{
					selectList.Remove(battleCardBase);
				}
			}
		}
		if (num2 >= 0 && isChoiceTransform)
		{
			vfx.Register(SwapTransformCard(card, num2, option, BattleCardBase.TransformType.Choice, selectChoiceCardIdList, selectList));
		}
		return num;
	}

	private VfxBase SwapTransformCard(BattleCardBase originalCard, int transformCardID, SkillConditionCheckerOption option, BattleCardBase.TransformType transformType, List<int> selectChoiceCardIdList, List<BattleCardBase> selectList)
	{
		bool isMutation = transformType == BattleCardBase.TransformType.Accelerate || transformType == BattleCardBase.TransformType.Crystallize;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
		BattleCardBase transformCard = originalCard.SelfBattlePlayer.CreateCard(transformCardID, originalCard.Index);
		transformCard.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
		{
			transformCard.BattleCardView.GameObject.SetActive(value: false);
		}));
		sequentialVfxPlayer2.Register(InstantVfx.Create(delegate
		{
			if (!_pair.Self.BattleMgr.IsRecovery)
			{
				Transform transform = transformCard.BattleCardView.Transform;
				Transform transform2 = originalCard.BattleCardView.Transform;
				transform.position = transform2.position;
				transform.rotation = transform2.rotation;
				if (_pair.Self.BattleMgr.GameMgr.IsWatchBattle || _pair.Self.BattleMgr.GameMgr.IsReplayBattle)
				{
					transform.parent = originalCard.SelfBattlePlayer.BattleView.HandDeck.transform;
				}
				else
				{
					transform.parent = transform2.parent;
				}
				transform.localScale = transform2.localScale;
				transform.SetSiblingIndex(transform2.GetSiblingIndex());
				if (!isMutation)
				{
					transformCard.BattleCardView.CardTemplate.DynamicSetupMaterials(transformCard, _pair.Self.BattleMgr.BattleResourceMgr);
					originalCard.BattleCardView.GameObject.SetActive(value: false);
					transformCard.BattleCardView.GameObject.SetActive(value: true);
				}
			}
		}));
		if (isMutation)
		{
			sequentialVfxPlayer.Register(sequentialVfxPlayer2);
		}
		else
		{
			transformCard.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(sequentialVfxPlayer2);
		}
		transformCard.SetOnDraw(draw: false);
		originalCard.MetamorphoseCard = transformCard;
		transformCard.TransformInfo = new BattleCardBase.TransformInformation(transformType, originalCard);
		option.PlayedCard = transformCard;
		option.SummonedCard = transformCard;
		SkillBase skillBase = transformCard.GetSelectTypeSkill().FirstOrDefault((SkillBase s) => s is Skill_choice);
		if (skillBase != null)
		{
			SetupChoiceCard(transformCard, option, selectChoiceCardIdList, skillBase, selectList, isChoiceTransform: false, sequentialVfxPlayer, isChoiceBrave: false);
		}
		this.OnTransform.Call(originalCard, transformCardID, !isMutation);
		return sequentialVfxPlayer;
	}

	private VfxBase SetupPlayCard(BattleCardBase originalCard, BattleCardBase playCard, SkillProcessor skillProcessor, bool isChoiceBrave)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		bool isMutation = originalCard.CheckConditionFixedUseCost(isPrePlay: true) && originalCard.CalcFixedUseCost(originalCard.SelfBattlePlayer.Pp) < originalCard.Cost;
		GameMgr gameMgr = _pair.Self.BattleMgr.GameMgr;
		// IsAdminWatch is const-false in headless — the isAdminWatch local was only used at the
		// `(isAdminWatch && isMutation) || isChoiceBrave` argument which collapses to isChoiceBrave.
		BattleManagerBase battleMgr = _pair.Self.BattleMgr;
		if (!isChoiceBrave)
		{
			sequentialVfxPlayer.Register(playCard.SelfBattlePlayer.ReplaceInHand(originalCard, playCard, skillProcessor));
		}
		if (isMutation)
		{
			sequentialVfxPlayer.Register(battleMgr.LoadCardResources(new List<BattleCardBase> { playCard }));
		}
		if (!battleMgr.IsRecovery)
		{
			playCard.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
			{
				originalCard.SelfBattlePlayer.BattleView.HandView.RemoveCardFromView(originalCard.BattleCardView, 0.3f);
			}));
		}
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			if (!battleMgr.IsRecovery)
			{
				playCard.SelfBattlePlayer.BattleView.PlayQueueView.RemoveCardFromView(originalCard.BattleCardView, isChoiceBrave);
				if (isMutation)
				{
					playCard.BattleCardView.CardTemplate.DynamicSetupMaterials(playCard, battleMgr.BattleResourceMgr);
				}
			}
		}));
		if (!isMutation)
		{
			sequentialVfxPlayer.Register(battleMgr.OperateMgr.InitSetCard(playCard, playCard.SelfBattlePlayer.IsPlayer, isSelect: false, isRecovery: false, isChoiceSelect: true, isAccelerateSelect: false, registerDirectlyToVfxManager: true, isFusionWait: false, isChoiceBrave));
		}
		else
		{
			bool isChoice = playCard.GetSelectTypeSkill().Any((SkillBase s) => s is Skill_choice);
			sequentialVfxPlayer.Register(playCard.SelfBattlePlayer.BattleView.PlayQueueView.InstantAddCardToViewVfx(playCard.BattleCardView, forceCardIntoPlayQueue: false, isChoice));
			if (isMutation)
			{
				sequentialVfxPlayer.Register(InstantVfx.Create(delegate
				{
					Vector3 position = originalCard.BattleCardView.GameObject.transform.position;
					position += ACCELERATE_POSITION_OFFSET;
					Quaternion rot = originalCard.BattleCardView.GameObject.transform.rotation;
					if (!playCard.IsSpell && playCard.SelfBattlePlayer.BattleView.PlayQueueView.IsCardInQueue(playCard.BattleCardView))
					{
						Vector3 localEulerAngles = originalCard.BattleCardView.GameObject.transform.localEulerAngles;
						rot = Quaternion.Euler(localEulerAngles.x, 0f, localEulerAngles.z);
					}
					gameMgr.GetEffectMgr().Start(playCard.IsSpell ? EffectMgr.EffectType.CMN_CARD_ACCELERATE_1 : EffectMgr.EffectType.CMN_CARD_CRYSTALLIZE_1, playCard.IsSpell ? position : originalCard.BattleCardView.GameObject.transform.position, rot, 31);
				}));
				sequentialVfxPlayer.Register(WaitVfx.Create(playCard.IsSpell ? 0.2f : 0.2f));
				sequentialVfxPlayer.Register(InstantVfx.Create(delegate
				{
					CardTemplate cardTemplate = originalCard.BattleCardView.CardTemplate;
					cardTemplate.SetEffectStyle(UILabel.Effect.None);
					if (originalCard is UnitBattleCard)
					{
						cardTemplate.NormalAtkLabelTemp.effectStyle = UILabel.Effect.None;
						cardTemplate.NormalLifeLabelTemp.effectStyle = UILabel.Effect.None;
					}
				}));
				sequentialVfxPlayer.Register(WaitVfx.Create(playCard.IsSpell ? 0.2f : 0f));
			}
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				if (!battleMgr.IsRecovery && isMutation)
				{
					Transform transform = playCard.BattleCardView.Transform;
					Transform transform2 = originalCard.BattleCardView.Transform;
					if (playCard.IsSpell || playCard.SelfBattlePlayer.BattleView.PlayQueueView.IsCardInQueue(playCard.BattleCardView))
					{
						MotionUtils.SetLayerAll(playCard.BattleCardView.CardWrapObject, 31);
						playCard.BattleCardView.Transform.SetParent(battleMgr.CutInContainer.transform, worldPositionStays: false);
					}
					if (playCard.IsSpell)
					{
						transform.position = transform2.position;
						transform.localScale = transform2.localScale;
						originalCard.BattleCardView.Transform.SetParent(originalCard.SelfBattlePlayer.BattleView.BanishParent.transform);
						originalCard.BattleCardView.GameObject.SetActive(value: false);
						playCard.BattleCardView.GameObject.SetActive(value: true);
						playCard.BattleCardView.ShowInHandFrameEffect(enable: true, HandCardFrameEffectType.LIGHT_BLUE);
						originalCard.BattleCardView.CardTemplate.SetEffectStyle(UILabel.Effect.None);
					}
					else
					{
						transform.position = transform2.position;
						transform.localScale = transform2.localScale;
						originalCard.BattleCardView.Transform.SetParent(originalCard.SelfBattlePlayer.BattleView.BanishParent.transform);
						originalCard.BattleCardView.GameObject.SetActive(value: false);
						playCard.BattleCardView.GameObject.SetActive(value: true);
						originalCard.BattleCardView.Transform.SetParent(originalCard.SelfBattlePlayer.BattleView.BanishParent.transform);
					}
				}
			}));
			if (isMutation)
			{
				sequentialVfxPlayer.Register(WaitVfx.Create(playCard.IsSpell ? 0.2f : 0.3f));
				if (!playCard.IsSpell)
				{
					sequentialVfxPlayer.Register(InstantVfx.Create(delegate
					{
						originalCard.BattleCardView.GameObject.SetActive(value: false);
						MotionUtils.SetLayerAll(playCard.BattleCardView.CardWrapObject, 10);
					}));
				}
			}
		}
		return sequentialVfxPlayer;
	}

	public VfxBase PlayCard(BattleCardBase card, IEnumerable<BattleCardBase> selectedCards, List<int> selectChoiceId = null, bool isChoiceBrave = false)
	{
		if (selectedCards != null)
		{
			foreach (BattleCardBase selectedCard in selectedCards)
			{
				selectedCard.SelfBattlePlayer.AddLastTargetCardsList(selectedCard);
			}
		}
		SetIndividualId(card.NormalSkills);
		if (card.NormalSkills.Any((SkillBase s) => s.HasIndividualId))
		{
			card.SelfBattlePlayer.BattleMgr.IncrementIndividualId();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SkillProcessor skillProcessor = new SkillProcessor();
		BattleCardBase battleCardBase = card;
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.PlayedCard = card;
		skillConditionCheckerOption.SummonedCard = card;
		if (isChoiceBrave)
		{
			card.SelfBattlePlayer.IsAlreadyChoiceBraveInThisTurn = true;
			card.SelfBattlePlayer.BattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
			card.SelfBattlePlayer.BattleView.UpdateChoiceBraveActivatingEffect(isActivating: false);
			int cardId = ((selectChoiceId != null && selectChoiceId.Count > 0) ? selectChoiceId[0] : selectedCards.First().CardId);
			BattleCardBase selectSkillCard = (CardMaster.IsChoiceBraveCardCheck(card.CardId) ? card : ((selectedCards != null && selectedCards.Count() > 1 && (card.IsPlayer || _pair.Self.BattleMgr.GameMgr.IsAdminWatch)) ? selectedCards.First() : null));
			skillConditionCheckerOption.PlayedCard = card.SelfBattlePlayer.BattleMgr.ReplaceChoiceBraveCard(card, cardId, selectSkillCard);
		}
		sequentialVfxPlayer.Register(SetSkillConditionCheckeroptionSelectCards(card, skillConditionCheckerOption, selectedCards, isEvolve: false, selectChoiceId, isFusion: false, isChoiceBrave));
		if (battleCardBase != skillConditionCheckerOption.PlayedCard)
		{
			if (battleCardBase.SkillApplyInformation.SkillRandomArray == null)
			{
				skillConditionCheckerOption.PlayedCard.SkillApplyInformation.GiveSkillRandomArray(battleCardBase.SkillApplyInformation.SkillRandomArray);
			}
			battleCardBase = skillConditionCheckerOption.PlayedCard;
			sequentialVfxPlayer.Register(SetupPlayCard(card, battleCardBase, skillProcessor, isChoiceBrave));
			_pair.Self.BattleMgr.PSideLogControl.RemoveAllLog();
		}
		battleCardBase.SetPlayedTurnNow();
		int count = skillConditionCheckerOption.ChosenCards.Count;
		int num = selectedCards?.Count() ?? 0;
		if (count > 0 && num - count < 0)
		{
			List<BattleCardBase> list = ((selectedCards != null) ? selectedCards.ToList() : new List<BattleCardBase>());
			foreach (int chosenCard in skillConditionCheckerOption.ChosenCards)
			{
				BattleCardBase item = _pair.Self.BattleMgr.CreateTransformCardRegisterVfx(skillConditionCheckerOption.PlayedCard, chosenCard, skillConditionCheckerOption.PlayedCard.IsPlayer);
				list.Insert(0, item);
			}
			selectedCards = list;
		}
		this.OnBeforePlayCard.Call(card, battleCardBase, selectedCards);
		this.OnBeforeChosenPlayCard.Call(card, battleCardBase, skillConditionCheckerOption.ChosenCards);
		if (skillConditionCheckerOption.BurialRiteCards != null && skillConditionCheckerOption.BurialRiteCards.Count > 0)
		{
			this.OnBeforeBurialRitePlayCard.Call(battleCardBase, selectedCards, arg3: false);
		}
		bool isEnhance = battleCardBase.CheckConditionFixedUseCost(isPrePlay: true);
		VfxWith<SkillProcessor.ProcessInfo> vfxWith = card.PlayChoiceCard(skillProcessor, skillConditionCheckerOption);
		VfxWith<SkillProcessor.ProcessInfo> vfxWith2 = battleCardBase.PlayCard(skillProcessor, skillConditionCheckerOption, isInplayGeneration: false, card);
		VfxBase vfx = battleCardBase.SelfBattlePlayer.StartSkillWhenChangeInplay(null, new List<BattleCardBase> { battleCardBase }, skillProcessor);
		skillProcessor.Register(vfxWith.Value);
		skillProcessor.Register(vfxWith2.Value);
		VfxBase vfx2 = _pair.Self.StartSkillWhenPlayOtherEnhanceAndAccelerateAndCrystallize(battleCardBase, isEnhance, skillProcessor);
		VfxBase vfx3 = _pair.Self.StartSkillWhenSummonOther(battleCardBase, skillProcessor);
		VfxBase vfx4 = skillProcessor.Process(_pair);
		if (card != battleCardBase && card.Skills.HaveBeforeChoiceSkill())
		{
			BattleLogManager.GetInstance().AddLogPlayAsChoiceTransform(battleCardBase);
		}
		VfxBase vfxBase = battleCardBase.FinishWhenPlaySkill();
		VfxBase allFuncVfxResults = this.OnAfterPlayCard.GetAllFuncVfxResults(battleCardBase);
		BattleUIContainer battleUIContainer = _pair.Self.BattleMgr.BattleUIContainer;
		bool isSelfTurn = _pair.Self.BattleMgr.BattlePlayer.IsSelfTurn;
		if (isSelfTurn)
		{
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				battleUIContainer.PlayerCardPlaying = true;
				if (battleUIContainer.IsEnableMenu())
				{
					battleUIContainer.DisableMenu();
				}
			}));
		}
		sequentialVfxPlayer.Register(vfxWith2.Vfx);
		sequentialVfxPlayer.Register(vfx);
		sequentialVfxPlayer.Register(battleCardBase.StopSpellCharge());
		sequentialVfxPlayer.Register(vfx2);
		sequentialVfxPlayer.Register(vfx3);
		sequentialVfxPlayer.Register(vfx4);
		if (vfxBase != null)
		{
			sequentialVfxPlayer.Register(vfxBase);
		}
		if (allFuncVfxResults.IsVfxNonEmpty())
		{
			sequentialVfxPlayer.Register(allFuncVfxResults);
		}
		Action handleBattleMenu = delegate
		{
		};
		if (isSelfTurn)
		{
			handleBattleMenu = delegate
			{
				battleUIContainer.PlayerCardPlaying = false;
				if (_pair.Self.BattleMgr.BattlePlayer.BattleView.PlayQueueView.QueueCount() == 0 && !battleUIContainer.IsEnableMenu())
				{
					battleUIContainer.RequestEnableMenuWhenTouchable();
				}
			};
		}
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			BattlePlayer battlePlayer = _pair.Self.BattleMgr.BattlePlayer;
			handleBattleMenu();
			int num2 = battlePlayer.BattleView.PlayQueueView.QueueCount();
			bool isSelecting = battlePlayer.BattleView.IsSelecting;
			if (num2 == 0 && !isSelecting && !battlePlayer.IsDuringChoiceBrave)
			{
				battlePlayer.BattleView.ClearSelectCardList();
			}
		}));
		OnPlayComplete.Call();
		return sequentialVfxPlayer;
	}

	private void SetIndividualId(SkillCollectionBase skills)
	{
		for (int i = 0; i < skills.Count(); i++)
		{
			skills.ElementAt(i).InitSetIndividualId();
		}
	}

	public VfxBase Attack(IBattleCardUniqueID attackCardId, IBattleCardUniqueID targetCardId)
	{
		BattleCardBase attackCard = _pair.Self.InPlayCards.SingleOrDefault((BattleCardBase c) => c.EquelsID(attackCardId));
		IfNullCardThenException(attackCard, attackCardId, "場");
		if (attackCard.SkillApplyInformation.IsQuick)
		{
			attackCard.SelfBattlePlayer.GameQuickAttackCards.Add(attackCard);
		}
		BattleCardBase battleCardBase = ((targetCardId.IsPlayer == _pair.Self.IsPlayer) ? _pair.Self.ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c.EquelsID(targetCardId)) : _pair.Opponent.ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c.EquelsID(targetCardId)));
		IfNullCardThenException(battleCardBase, targetCardId, "場");
		attackCard.BattleCardView._inPlayFrameEffect.HideFrameEffect();
		OnAttackStart.Call(attackCard, battleCardBase);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SkillProcessor skillProcessor = new SkillProcessor();
		SkillCollectionBase skills = attackCard.Skills;
		foreach (SkillBase item in skills)
		{
			if (item.IsBeforAttackSkill)
			{
				VfxWith<SkillProcessor.ProcessInfo> vfxWith = skills.CreateBeforeAttackInfo(item, attackCard, battleCardBase, skillProcessor, new BattlePlayerPair(attackCard.SelfBattlePlayer, attackCard.OpponentBattlePlayer));
				skillProcessor.Register(vfxWith.Value);
				sequentialVfxPlayer.Register(vfxWith.Vfx);
				if (vfxWith.Value != null)
				{
					attackCard.SelfBattlePlayer.PredictionWarningCards.Add(attackCard);
				}
			}
			if (item.IsWhenFightSkill)
			{
				VfxWith<SkillProcessor.ProcessInfo> vfxWith2 = skills.CreateBeforeFightInfo(item, attackCard, battleCardBase, skillProcessor, new BattlePlayerPair(attackCard.SelfBattlePlayer, attackCard.OpponentBattlePlayer));
				skillProcessor.Register(vfxWith2.Value);
				sequentialVfxPlayer.Register(vfxWith2.Vfx);
				if (vfxWith2.Value != null)
				{
					attackCard.SelfBattlePlayer.PredictionWarningCards.Add(attackCard);
				}
			}
		}
		SkillCollectionBase skills2 = battleCardBase.Skills;
		foreach (SkillBase item2 in skills2)
		{
			if (item2.IsWhenFightSkill)
			{
				VfxWith<SkillProcessor.ProcessInfo> vfxWith3 = skills2.CreateBeforeFightInfo(item2, attackCard, battleCardBase, skillProcessor, new BattlePlayerPair(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer));
				skillProcessor.Register(vfxWith3.Value);
				sequentialVfxPlayer.Register(vfxWith3.Vfx);
				if (vfxWith3.Value != null)
				{
					attackCard.SelfBattlePlayer.PredictionWarningCards.Add(attackCard);
				}
			}
		}
		foreach (BattleCardBase item3 in from c in _pair.Self.ClassAndInPlayCardList.Concat(_pair.Opponent.ClassAndInPlayCardList)
			where c.Skills._skillTimingInfo.IsBeforeAttackSelfAndOther
			select c)
		{
			SkillCollectionBase skills3 = item3.Skills;
			foreach (SkillBase item4 in skills3)
			{
				if (item4.IsBeforeAttackSelfAndOtherSkill)
				{
					VfxWith<SkillProcessor.ProcessInfo> vfxWith4 = skills3.CreateBeforeAttackSelfAndOtherInfo(item4, attackCard, battleCardBase, skillProcessor, new BattlePlayerPair(item3.SelfBattlePlayer, item3.OpponentBattlePlayer));
					skillProcessor.Register(vfxWith4.Value);
					sequentialVfxPlayer.Register(vfxWith4.Vfx);
					if (vfxWith4.Value != null)
					{
						item3.SelfBattlePlayer.PredictionWarningCards.Add(item3);
					}
				}
			}
		}
		VfxBase allFuncVfxResults = this.OnBeforeAttack.GetAllFuncVfxResults();
		sequentialVfxPlayer.Register(allFuncVfxResults);
		sequentialVfxPlayer.Register(attackCard.SelfBattlePlayer.BattleView.PrepareCardsForAttackSequenceVfx(attackCard.BattleCardView, battleCardBase.BattleCardView));
		VfxBase vfx = skillProcessor.Process(_pair);
		sequentialVfxPlayer.Register(vfx);
		VfxBase allFuncVfxResults2 = this.OnBeforeAttackSkillComplete.GetAllFuncVfxResults();
		sequentialVfxPlayer.Register(allFuncVfxResults2);
		BattleCardBase battleCardBase2 = (attackCard.IsDead ? null : attackCard.GetDamageReflectionTarget(isSkillDamage: false));
		int damage = battleCardBase.DamageCalculationAtkTypeBeAttacked.Damage;
		int num = attackCard.CalculateFinalDamageAmount(damage);
		BattleCardBase battleCardBase3 = (battleCardBase.IsDead ? null : battleCardBase.GetDamageReflectionTarget(isSkillDamage: false));
		int damage2 = attackCard.DamageCalculationAtkTypeAttack.Damage;
		int num2 = battleCardBase.CalculateFinalDamageAmount(damage2);
		OnAttackDamageComplete.Call(attackCard, battleCardBase, num2, num);
		bool flag = !attackCard.IsDead && !battleCardBase.IsDead && !attackCard.SelfBattlePlayer.Class.IsDead && !battleCardBase.SelfBattlePlayer.Class.IsDead && attackCard.IsInplay && battleCardBase.IsInplay;
		if (flag)
		{
			VfxBase vfx2 = attackCard.StartAttack(battleCardBase, _pair);
			sequentialVfxPlayer.Register(vfx2);
		}
		else
		{
			if (attackCard.SkillApplyInformation.IsSneak)
			{
				sequentialVfxPlayer.Register(attackCard.SkillApplyInformation.FourceDepriveSneak());
			}
			attackCard.AttackableCount--;
			if (!attackCard.Attackable)
			{
				sequentialVfxPlayer.Register(InstantVfx.Create(delegate
				{
					attackCard.BattleCardView._inPlayFrameEffect.HideFrameEffect();
				}));
			}
			AttackSelectControl attackSelectControl = attackCard.SelfBattlePlayer.BattleView.AttackSelectControl;
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			if (!attackCard.IsDead && attackCard.IsInplay)
			{
				IBattleCardView battleCardView = attackCard.BattleCardView;
				parallelVfxPlayer.Register(attackSelectControl.ResetCardAfterAttack(battleCardView));
			}
			if (!battleCardBase.IsDead && battleCardBase.IsInplay)
			{
				IBattleCardView battleCardView2 = battleCardBase.BattleCardView;
				parallelVfxPlayer.Register(attackSelectControl.ResetCardAfterAttack(battleCardView2));
			}
			sequentialVfxPlayer.Register(parallelVfxPlayer);
		}
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			attackCard.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
		}));
		if (_pair.Self.BattleMgr.GameMgr.IsWatchBattle || _pair.Self.BattleMgr.GameMgr.IsReplayBattle)
		{
			bool num3 = attackCard.SelfBattlePlayer.BattleMgr.DetailMgr.DetailPanelControl.IsShow && attackCard.SelfBattlePlayer.BattleMgr.DetailMgr.DetailPanelControl._card == attackCard;
			bool flag2 = attackCard.SelfBattlePlayer.IsSelfTurn && attackCard.IsPlayer && attackCard.IsInplay && attackCard.AttackableCount <= 0;
			if (num3 && flag2)
			{
				sequentialVfxPlayer.Register(attackCard.BattleCardView.ShowAttackFinished());
			}
		}
		OnAttackProcessComplete.Call(attackCard);
		if (flag)
		{
			SkillProcessor skillProcessor2 = new SkillProcessor();
			attackCard.SelfBattlePlayer.StartSkillWhenDamageSelfAndOther(null, (battleCardBase3 != null) ? new List<BattleCardBase> { battleCardBase3 } : null, skillProcessor2, damage2, num2);
			if (battleCardBase is UnitBattleCard)
			{
				attackCard.SelfBattlePlayer.StartSkillWhenDamageSelfAndOther(null, (battleCardBase2 != null) ? new List<BattleCardBase> { battleCardBase2 } : null, skillProcessor2, damage, num);
			}
			for (int num4 = 0; num4 < attackCard.Skills.Count(); num4++)
			{
				SkillBase skillBase = attackCard.Skills.ElementAt(num4);
				if (skillBase.OnAfterAttackStart != 0)
				{
					SkillProcessor.ProcessInfo info = attackCard.Skills.CreateAfterAttackInfo(skillBase, attackCard, battleCardBase, skillProcessor2, new BattlePlayerPair(attackCard.SelfBattlePlayer, attackCard.OpponentBattlePlayer));
					skillProcessor2.Register(info);
				}
				if (skillBase.OnAfterFightStart != 0)
				{
					SkillProcessor.ProcessInfo info2 = attackCard.Skills.CreateAfterFightInfo(skillBase, attackCard, battleCardBase, skillProcessor2, new BattlePlayerPair(attackCard.SelfBattlePlayer, attackCard.OpponentBattlePlayer));
					skillProcessor2.Register(info2);
				}
			}
			for (int num5 = 0; num5 < battleCardBase.Skills.Count(); num5++)
			{
				SkillBase skillBase2 = battleCardBase.Skills.ElementAt(num5);
				if (skillBase2.OnAfterFightStart != 0)
				{
					SkillProcessor.ProcessInfo info3 = battleCardBase.Skills.CreateAfterFightInfo(skillBase2, attackCard, battleCardBase, skillProcessor2, new BattlePlayerPair(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer));
					skillProcessor2.Register(info3);
				}
			}
			foreach (BattleCardBase item5 in from c in _pair.Self.ClassAndInPlayCardList.Concat(_pair.Opponent.ClassAndInPlayCardList)
				where c.Skills._skillTimingInfo.IsAfterAttackSelfAndOther
				select c)
			{
				SkillCollectionBase skills4 = item5.Skills;
				foreach (SkillBase item6 in skills4)
				{
					if (item6.IsAfterAttackSelfAndOtherSkill)
					{
						VfxWith<SkillProcessor.ProcessInfo> vfxWith5 = skills4.CreateAfterAttackSelfAndOtherInfo(item6, attackCard, battleCardBase, skillProcessor2, new BattlePlayerPair(item5.SelfBattlePlayer, item5.OpponentBattlePlayer));
						skillProcessor2.Register(vfxWith5.Value);
						sequentialVfxPlayer.Register(vfxWith5.Vfx);
						if (vfxWith5.Value != null)
						{
							item5.SelfBattlePlayer.PredictionWarningCards.Add(item5);
						}
					}
				}
			}
			VfxBase vfx3 = skillProcessor2.Process(_pair);
			sequentialVfxPlayer.Register(vfx3);
		}
		VfxBase allFuncVfxResults3 = this.OnAfterAttack.GetAllFuncVfxResults(attackCard, battleCardBase, flag);
		sequentialVfxPlayer.Register(allFuncVfxResults3);
		OnAttackComplete.Call();
		return sequentialVfxPlayer;
	}

	public VfxBase Evolution(BattleCardBase card, IEnumerable<BattleCardBase> selectedCards, List<int> selectChoiceId = null)
	{
		BattleCardBase battleCardBase = card;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (_pair.Self.BattleMgr.BattlePlayer.Class.IsDead || _pair.Self.BattleMgr.BattleEnemy.Class.IsDead || card.IsEvolution || !card.SelfBattlePlayer.IsSelfTurn)
		{
			return NullVfx.GetInstance();
		}
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		sequentialVfxPlayer.Register(SetSkillConditionCheckeroptionSelectCards(card, option, selectedCards, isEvolve: true, selectChoiceId));
		SkillProcessor skillProcessor = new SkillProcessor();
		if (option.PlayedCard != null && card.EvolutionSkills.Any((SkillBase s) => s is Skill_transform))
		{
			battleCardBase = option.PlayedCard;
			sequentialVfxPlayer.Register(battleCardBase.SelfBattlePlayer.ReplaceInPlay(card, battleCardBase, skillProcessor));
			sequentialVfxPlayer.Register(battleCardBase.SetUpInplay());
			BattlePlayerPair playerInfoPair = new BattlePlayerPair(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer);
			sequentialVfxPlayer.Register(battleCardBase.Skills.RegisterAndProcessWhenChangeInplayImmediateInfo(playerInfoPair));
			battleCardBase.Skills.CreateAndRegisterWhenChangeInplayInfo(new List<BattleCardBase> { battleCardBase }, skillProcessor, playerInfoPair);
			sequentialVfxPlayer.Register(card.RemoveFromInPlay());
		}
		BattlePlayerReadOnlyInfoPair pair = new BattlePlayerReadOnlyInfoPair(card.SelfBattlePlayer, card.OpponentBattlePlayer);
		SkillBase skillBase = card.EvolutionSkills.FirstOrDefault((SkillBase s) => s.OnWhenEvolveBeforeStart != 0 && s is Skill_evolve_to_other && s.CheckCondition(pair, option, isPrePlay: true));
		if (skillBase != null)
		{
			IBattleResourceMgr battleResourceMgr = card.SelfBattlePlayer.BattleMgr.BattleResourceMgr;
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			UnitBattleCardView unitBattleCardView = card.BattleCardView as UnitBattleCardView;
			parallelVfxPlayer.Register(battleResourceMgr.DecrementEffectBattleRefCount(card.BaseParameter.AtkEffectParameter.GetEffectPath(isEvolve: false)));
			parallelVfxPlayer.Register(battleResourceMgr.DecrementEffectBattleRefCount(card.BaseParameter.AtkEffectParameter.GetEffectPath(isEvolve: true)));
			if (option.ChosenCards.Count > 0)
			{
				card.UpdateBuildInfoAndSkillCollection(option.ChosenCards[0], isFoil: false);
			}
			else
			{
				card.UpdateBuildInfoAndSkillCollection(skillBase.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.card_id, -1), card.BaseParameter.IsFoil);
			}
			card.BattleCardView.InitializeVoiceInfo(card.CardId);
			for (int num = 0; num < card.NormalSkills.Count(); num++)
			{
				card.NormalSkills.ElementAt(num).SetInductionVoiceIndex();
			}
			for (int num2 = 0; num2 < card.EvolutionSkills.Count(); num2++)
			{
				card.EvolutionSkills.ElementAt(num2).SetInductionVoiceIndex();
			}
			if (unitBattleCardView != null)
			{
				parallelVfxPlayer.Register(unitBattleCardView.LoadAttackEffect(card.BaseParameter.AtkEffectParameter, isEvolve: false));
				parallelVfxPlayer.Register(unitBattleCardView.LoadAttackEffect(card.BaseParameter.AtkEffectParameter, isEvolve: true));
			}
			sequentialVfxPlayer.Register(parallelVfxPlayer);
		}
		if (option.ChosenCards.Count > 0 && selectedCards.Count() <= 0)
		{
			List<BattleCardBase> list = selectedCards.ToList();
			foreach (int chosenCard in option.ChosenCards)
			{
				BattleCardBase item = _pair.Self.BattleMgr.CreateTransformCardRegisterVfx(card, chosenCard, option.PlayedCard.IsPlayer);
				list.Add(item);
			}
			selectedCards = list;
		}
		SetIndividualId(card.EvolutionSkills);
		if (card.EvolutionSkills.Any((SkillBase s) => s.HasIndividualId))
		{
			card.SelfBattlePlayer.BattleMgr.IncrementIndividualId();
		}
		this.OnBeforeEvolution.Call(card, battleCardBase, selectedCards);
		this.OnBeforeChosenEvolution.Call(card, battleCardBase, option.ChosenCards);
		if (option.BurialRiteCards != null && option.BurialRiteCards.Count > 0)
		{
			this.OnBeforeBurialRitePlayCard.Call(battleCardBase, selectedCards, arg3: true);
		}
		this.OnJustBeforeEvolution.Call(battleCardBase);
		VfxBase vfx = battleCardBase.Evolution(isSkill: false, skillProcessor, option);
		sequentialVfxPlayer.Register(vfx);
		this.OnRightAfterEvolution.Call(battleCardBase);
		VfxBase vfx2 = skillProcessor.Process(_pair);
		sequentialVfxPlayer.Register(vfx2);
		VfxBase allFuncVfxResults = this.OnAfterEvolution.GetAllFuncVfxResults(battleCardBase);
		sequentialVfxPlayer.Register(allFuncVfxResults);
		OnEvolutionComplete.Call();
		return sequentialVfxPlayer;
	}

	public VfxBase Fusion(BattleCardBase fusionCard, List<BattleCardBase> selectedCards)
	{
		BattleManagerBase ins = _pair.Self.BattleMgr;
		if (ins.BattlePlayer.Class.IsDead || ins.BattleEnemy.Class.IsDead || fusionCard.IsEvolution || !fusionCard.SelfBattlePlayer.IsSelfTurn)
		{
			return NullVfx.GetInstance();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		this.OnBeforeFusion.Call(fusionCard, selectedCards);
		sequentialVfxPlayer.Register(SetSkillConditionCheckeroptionSelectCards(fusionCard, option, selectedCards, isEvolve: false, new List<int>(), isFusion: true));
		sequentialVfxPlayer.Register(fusionCard.SelfBattlePlayer.CardManagement(fusionCard, new SkillProcessor(), BattlePlayerBase.CARD_MANAGEMENT.FUSION_MATERIAL, isRandom: false, selectedCards));
		VfxBase allFuncVfxResults = this.OnAfterFusion.GetAllFuncVfxResults(fusionCard);
		sequentialVfxPlayer.Register(allFuncVfxResults);
		OnFusionComplete.Call();
		return sequentialVfxPlayer;
	}

	private static void IfNullCardThenException(BattleCardBase card, IBattleCardUniqueID cardId, string positionName)
	{
		if (card == null)
		{
			throw new ArgumentException("カード " + cardId.GetName() + " は" + positionName + "に見つかりませんでした");
		}
	}

	public static IEnumerable<BattleCardBase> GetSkillUserSelectableTargets(SkillBase skill, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option = null, List<BattleCardBase> selectedCards = null)
	{
		if (skill == null || !skill.DoesSkillFulfillActivationConditions(playerInfoPair, ignoreHandSkill: true, isPrePlay: true))
		{
			return null;
		}
		if (option == null)
		{
			option = new SkillConditionCheckerOption();
		}
		IEnumerable<BattleCardBase> selectableCards = skill.GetSelectableCards(playerInfoPair, option, isSkipForceSelect: false, selectedCards);
		if (!selectableCards.Any())
		{
			selectableCards = skill.GetSelectableCards(playerInfoPair, option, isSkipForceSelect: true, selectedCards);
		}
		if (!selectableCards.Any())
		{
			return null;
		}
		return selectableCards;
	}

	public static IEnumerable<BattleCardBase> GetSkillUserSelectableTargets(SkillCollectionBase skills, BattlePlayerReadOnlyInfoPair playerInfoPair, ref SkillBase actSkill)
	{
		IEnumerable<SkillBase> source = skills.Where(delegate(SkillBase s)
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair2 = new BattlePlayerReadOnlyInfoPair(s.SkillPrm.ownerCard.SelfBattlePlayer, s.SkillPrm.ownerCard.OpponentBattlePlayer);
			return s.IsUserSelectType && s.CheckCondition(playerInfoPair2, new SkillConditionCheckerOption(), isPrePlay: true);
		});
		if (source.Count() > 0)
		{
			actSkill = ((source.Count() > 1) ? source.ToList().Last() : source.ToList().First());
		}
		return GetSkillUserSelectableTargets(actSkill, playerInfoPair);
	}
}

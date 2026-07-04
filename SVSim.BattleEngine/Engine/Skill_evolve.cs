using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_evolve : SkillBase
{

	protected List<BattleCardBase> _evolvedBySkill = new List<BattleCardBase>();

	public Skill_evolve(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (parameter.targetCards.Where((BattleCardBase c) => !c.IsEvolution).Count() == 0)
		{
			return NullVfxWithLoading.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer3 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer4 = ParallelVfxPlayer.Create();
		_evolvedBySkill = parameter.targetCards.Where((BattleCardBase battleCardBase2) => battleCardBase2.CanEvolution(isSkill: true, isSelfBattlePlayer: true) && battleCardBase2.IsInplay).ToList();
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillEvolution(_evolvedBySkill, this);
		}
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnBeforeSkillEvolve(base.SkillPrm.ownerCard, _evolvedBySkill);
		float num = 0f;
		int num2 = 0;
		foreach (BattleCardBase card in parameter.targetCards)
		{
			if (!card.CanEvolution(isSkill: true, isSelfBattlePlayer: true) || !card.IsInplay)
			{
				continue;
			}
			Action enableAttackControlAction = null;
			Action enableAttackTargettingMovementAction = null;
			base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
			{
				if (card.IsPlayer)
				{
					enableAttackControlAction = card.ForceAttackOff();
				}
				else
				{
					enableAttackTargettingMovementAction = card.BattleCardView._attackTargetSelectInfo.DisableAttackTargettingMovement();
				}
			}));
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create(WaitVfx.Create(num));
			BattleCardBase battleCardBase = card;
			VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, battleCardBase.AsIEnumerable(), isFollowInHand: false, addToLastOperation: true, num2 > 0);
			parallelVfxPlayer.Register(vfxWithLoading.LoadingVfx);
			sequentialVfxPlayer.Register(vfxWithLoading.MainVfx);
			bool evolveMeWhenAttack = (base.IsBeforAttackSkill && battleCardBase == base.SkillPrm.ownerCard) || (base.IsBeforeAttackSelfAndOtherSkill && battleCardBase != base.SkillPrm.ownerCard);
			if (evolveMeWhenAttack)
			{
				base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnEvolveMeWhenAttack(battleCardBase);
			}
			sequentialVfxPlayer.Register(card.Evolution(isSkill: true, parameter.skillProcessor, new SkillConditionCheckerOption()));
			parallelVfxPlayer3.Register(sequentialVfxPlayer);
			parallelVfxPlayer2.Register(TurnOffAttackFrameVfx(card));
			parallelVfxPlayer4.Register(ToggleOffCantAttackVfx(card, enableAttackControlAction, enableAttackTargettingMovementAction));
			num += 0.05f;
			num2++;
		}
		RegisterEvolveTriggerSkill(parameter.skillProcessor, parameter.targetCards);
		VfxBase vfxToRegister = UIManager.GetInstance().CreateNowLoadingVfx(parallelVfxPlayer);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(parallelVfxPlayer2, parallelVfxPlayer3, parallelVfxPlayer4);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxToRegister);
		return vfxWithLoadingSequential;
	}

	private VfxBase TurnOffAttackFrameVfx(BattleCardBase evolvedCard)
	{
		return InstantVfx.Create(delegate
		{
			evolvedCard.BattleCardView._inPlayFrameEffect.HideFrameEffect();
		});
	}

	private VfxBase ToggleOffCantAttackVfx(BattleCardBase evolvedCard, Action enableAttackControlAction, Action enableAttackTargettingMovementAction)
	{
		return InstantVfx.Create(delegate
		{
			enableAttackControlAction.Call();
			enableAttackTargettingMovementAction.Call();
		});
	}

	private void RegisterEvolveTriggerSkill(SkillProcessor skillProcessor, IEnumerable<BattleCardBase> targets)
	{
		List<BattleCardBase> list = base.SkillPrm.selfBattlePlayer.HandCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0)).ToList();
		list.AddRange(base.SkillPrm.opponentBattlePlayer.HandCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0)));
		list.AddRange(base.SkillPrm.selfBattlePlayer.ClassAndInPlayCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0)));
		list.AddRange(base.SkillPrm.opponentBattlePlayer.ClassAndInPlayCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0)));
		int i;
		for (i = 0; i < list.Count; i++)
		{
			if (targets.Any((BattleCardBase t) => t != list[i]) && list[i].Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0))
			{
				skillProcessor.Register(list[i].Skills.CreateWhenEvolveOtherInfo(targets.ToList(), skillProcessor, new BattlePlayerReadOnlyInfoPair(list[i].SelfBattlePlayer, list[i].OpponentBattlePlayer)));
			}
			if (list[i].Skills.Any((SkillBase s) => s.OnWhenEvolveSelfAndOtherStart != 0))
			{
				skillProcessor.Register(list[i].Skills.CreateWhenEvolveSelfAndOtherInfo(targets.ToList(), skillProcessor, new BattlePlayerReadOnlyInfoPair(list[i].SelfBattlePlayer, list[i].OpponentBattlePlayer)));
			}
		}
	}

	protected override bool IsSkipInduction(SkillProcessor.ProcessCallType callType)
	{
		if (!base.IsSkipInduction(callType))
		{
			if (base.ApplyingTargetFilter is SkillTargetSelfFilter && base.ApplyAndFilter.All((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetSelfFilter))
			{
				return base.SkillPrm.ownerCard.IsEvolution;
			}
			return false;
		}
		return true;
	}
}

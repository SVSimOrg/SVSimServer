using System.Collections.Generic;
using UnityEngine;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessOpenCard : SkillPreprocessBase
{
	private readonly BattleCardBase _ownerCard;

	public SkillPreprocessOpenCard(BattleCardBase ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		skill.SetAndAddPublishedActiveSkillCount();
		if (IsOpenCardAfterSkill(skill))
		{
			return NullVfx.GetInstance();
		}
		return CreateOpenCardVfx(skill);
	}

	private bool IsOpenCardAfterSkill(SkillBase skill)
	{
		if (skill.ApplyingTargetFilter is SkillTargetHandSelfFilter)
		{
			if (!(skill is Skill_cost_change) && !(skill is Skill_powerup) && !(skill is Skill_power_down))
			{
				return skill is Skill_power_modifier;
			}
			return true;
		}
		return false;
	}

	public VfxBase CreateOpenCardVfx(SkillBase skill)
	{
		BattleCardBase ownerCard = _ownerCard;
		if (!_ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
		{
			BattleLogManager.GetInstance().AddLogOpenCard(_ownerCard);
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.Add(ownerCard);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (ownerCard.SelfBattlePlayer.IsPlayer)
		{
			if (!IsOpenCardAfterSkill(skill))
			{
				_ownerCard.SelfBattlePlayer.CallOnOpenCard(_ownerCard);
			}
			Transform transform = ownerCard.BattleCardView.GameObject.transform;
			sequentialVfxPlayer.Register(VfxWithLoadingSequential.Create());
		}
		else
		{
			_ownerCard.SelfBattlePlayer.CallOnOpenCard(_ownerCard);
			if (ownerCard.BattleCardView != null)
			{
				List<int> costList = ownerCard.BattleCardView.GetUseCostList(ownerCard.Cost, useNomalCost: true);
				bool isInHand = ownerCard.IsInHand;
				sequentialVfxPlayer.Register(InstantVfx.Create(delegate
				{
					ownerCard.BattleCardView.UpdateCost(costList, isGenerateInHand: true, playEffect: true, isInHand);
				}));
			}
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			if (ownerCard == ownerCard.LastDrawOpenCard)
			{
				sequentialVfxPlayer.Register(ownerCard.SelfBattlePlayer.BattleView.HandView.ShuffleHand());
			}
		}
		return sequentialVfxPlayer;
	}
}

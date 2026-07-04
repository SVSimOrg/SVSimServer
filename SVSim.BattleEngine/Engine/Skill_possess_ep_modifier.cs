using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_possess_ep_modifier : SkillBase
{
	public static readonly int EP_NONE = -1;

	public Skill_possess_ep_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast)
		{
			return NullVfxWithLoading.GetInstance();
		}
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_eptotal, 0);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_ep, 0);
		int num3 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_ep, EP_NONE);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			int currentEpCount = targetCard.SelfBattlePlayer.CurrentEpCount;
			if (num2 != 0)
			{
				parallelVfxPlayer.Register(AddEp(parameter.skillProcessor, num2, targetCard.SelfBattlePlayer));
			}
			else if (num != 0)
			{
				parallelVfxPlayer.Register(AddEpTotal(parameter.skillProcessor, num, targetCard.SelfBattlePlayer));
			}
			else if (num3 != EP_NONE)
			{
				parallelVfxPlayer.Register(SetEp(num3, targetCard.SelfBattlePlayer));
			}
			int currentEpCount2 = targetCard.SelfBattlePlayer.CurrentEpCount;
			int epTotal = targetCard.SelfBattlePlayer.EpTotal;
			parallelVfxPlayer.Register(NullVfx.GetInstance());
			if (IsBattleLog)
			{
				BattleLogManager.GetInstance().AddLogSkillSetEP(currentEpCount2, targetCard.SelfBattlePlayer.Class, this);
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards, isFollowInHand: false, addToLastOperation: true));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	protected virtual VfxBase AddEp(SkillProcessor skillProcessor, int add, BattlePlayerBase battlePlayer)
	{
		AddToEpWithFloor(skillProcessor, add, battlePlayer);
		if (battlePlayer.CurrentEpCount > battlePlayer.EpTotal)
		{
			battlePlayer.SetCurrentEpCount(battlePlayer.EpTotal);
		}
		return NullVfx.GetInstance();
	}

	private VfxBase AddEpTotal(SkillProcessor skillProcessor, int add, BattlePlayerBase battlePlayer)
	{
		AddToEpWithFloor(skillProcessor, add, battlePlayer);
		int currentEpCount = battlePlayer.CurrentEpCount;
		return UpdateUsableAndMaxEp(battlePlayer, currentEpCount);
	}

	protected virtual VfxBase SetEp(int set, BattlePlayerBase battlePlayer)
	{
		if (set < 0)
		{
			return NullVfx.GetInstance();
		}
		int currentEpCount = battlePlayer.CurrentEpCount;
		battlePlayer.SetCurrentEpCount(set);
		return UpdateUsableAndMaxEp(battlePlayer, currentEpCount);
	}

	private void AddToEpWithFloor(SkillProcessor skillProcessor, int amountToAdd, BattlePlayerBase battlePlayer)
	{
		if (amountToAdd < 0)
		{
			amountToAdd *= -1;
			if (battlePlayer.CurrentEpCount < amountToAdd)
			{
				amountToAdd = battlePlayer.CurrentEpCount;
			}
			battlePlayer.UseEpCount(amountToAdd);
			battlePlayer.StartSkillWhenUseEpSelfAndOther(skillProcessor);
		}
		else
		{
			battlePlayer.AddCurrentEpCount(amountToAdd);
		}
	}

	private VfxBase UpdateUsableAndMaxEp(BattlePlayerBase battlePlayer, int oldMaxEpAmount)
	{
		if (battlePlayer.CurrentEpCount > battlePlayer.EpTotal)
		{
			if (battlePlayer.EpTotal == BattleManagerBase.FIRST_PLAYER_EP_NUM)
			{
				battlePlayer.EpTotal = BattleManagerBase.SECOND_PLAYER_EP_NUM;
				battlePlayer.SetCurrentEpCount(BattleManagerBase.SECOND_PLAYER_EP_NUM);
				return battlePlayer.StatusPanelControl.PlayIncreaseMaxEpAnimation(oldMaxEpAmount, battlePlayer.CurrentEpCount);
			}
			battlePlayer.SetCurrentEpCount(battlePlayer.EpTotal);
		}
		return NullVfx.GetInstance();
	}
}

using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_pp_modifier : SkillBase
{
	public static readonly int PP_NONE = -1;

	protected int _decreaseTurnPp = PP_NONE;

	public Skill_pp_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_pptotal, 0);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_pptotal, PP_NONE);
		int num3 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_pp, 0);
		int num4 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_pp, PP_NONE);
		int num5 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_pp, PP_NONE);
		_decreaseTurnPp = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.decrease_turn_pp, PP_NONE);
		bool isBattleLog = IsBattleLog;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		BattleManagerBase ins = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr;
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattleCardBase battleCardBase = targetCard;
			BattlePlayerBase selfBattlePlayer = battleCardBase.SelfBattlePlayer;
			int ppTotal = battleCardBase.SelfBattlePlayer.PpTotal;
			int pp = battleCardBase.SelfBattlePlayer.Pp;
			if (num != 0)
			{
				selfBattlePlayer.AddPpTotal(num, isUpdatePp: false, parameter.skillProcessor, base.SkillPrm.ownerCard, bySkill: true);
				parallelVfxPlayer.Register(NullVfx.GetInstance());
			}
			else if (num2 != PP_NONE)
			{
				base.SkillPrm.selfBattlePlayer.SetPpTotal(num2, isUpdatePp: false, parameter.skillProcessor);
				parallelVfxPlayer.Register(NullVfx.GetInstance());
			}
			if (num3 != 0)
			{
				AddPp(selfBattlePlayer, num3);
				targetCard.SkillApplyInformation.AddPp(num3, ins.CurrentTurn, ins.BattlePlayer.IsSelfTurn);
				selfBattlePlayer.StartSkillWhenPpHealing(parameter.skillProcessor);
			}
			else if (num4 > PP_NONE)
			{
				AddPp(selfBattlePlayer, -num4);
				selfBattlePlayer.AddGameUsedPp(num4);
			}
			else if (num5 != PP_NONE)
			{
				SetPp(selfBattlePlayer, num5);
			}
			if (_decreaseTurnPp != PP_NONE)
			{
				BuffInfo buffInfo = AddBuffInfoIfNeeded(battleCardBase);
				BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, _decreaseTurnPp, "", null, 0L);
				base.buffInfoContainer.Add(buffInfoContainer);
				battleCardBase.SkillApplyInformation.GiveDecreaseTurnStartPP(_decreaseTurnPp);
				SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			}
			parallelVfxPlayer.Register(NullVfx.GetInstance());
			int ppTotal2 = selfBattlePlayer.PpTotal;
			int pp2 = selfBattlePlayer.Pp;
			selfBattlePlayer.UpdateHandCardsPlayability();
			if (isBattleLog)
			{
				if (ppTotal2 - ppTotal != 0 || num3 != 0)
				{
					bool flag = ppTotal2 - ppTotal != 0;
					int changePP = (flag ? (ppTotal2 - ppTotal) : (pp2 - pp));
					BattleLogManager.GetInstance().AddLogSkillChangePP(battleCardBase, changePP, ppTotal, flag, this);
				}
				else if (num4 > 0)
				{
					BattleLogManager.GetInstance().AddLogSkillChangePP(battleCardBase, -num4, ppTotal, isTotal: false, this);
				}
			}
		}
		if (isBattleLog && _decreaseTurnPp != PP_NONE)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.DecreaseTurnPP, _decreaseTurnPp);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveDecreaseTurnStartPP(item._intValue);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			if (item._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(item._targetCard);
			}
			parallelVfxPlayer.Register(vfx);
		}
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	protected virtual void AddPp(BattlePlayerBase player, int add)
	{
		player.Pp += add;
		if (player.Pp < 0)
		{
			player.Pp = 0;
		}
		else if (player.Pp > 10)
		{
			player.Pp = 10;
		}
		if (player.PpTotal < player.Pp)
		{
			player.Pp = player.PpTotal;
		}
	}

	private void SetPp(BattlePlayerBase player, int set)
	{
		player.Pp = set;
		if (player.PpTotal < player.Pp)
		{
			player.Pp = player.PpTotal;
		}
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.ForceDepriveDecreaseTurnStartPP();
		};
	}
}

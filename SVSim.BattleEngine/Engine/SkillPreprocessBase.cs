using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public abstract class SkillPreprocessBase : ISkillConditionChecker
{
	public string BanId { get; protected set; } = string.Empty;

	public abstract bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false);

	public virtual bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck, bool isRegidentStop)
	{
		if (this is SkillPreprocessConditionCheck)
		{
			return IsRight(playerInfoPair, option, PreexecutionCheck, isRegidentStop);
		}
		return IsRight(playerInfoPair, option, PreexecutionCheck);
	}

	public virtual bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public abstract VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption);

	protected Func<BattlePlayerReadOnlyInfoPair, int, int, bool> GetConditionJudgeFunc(string condition)
	{
		string[] list = condition.Split('=');
		switch (list[0])
		{
		case "resonance":
			if (list.Length > 1 && list[1] == "true")
			{
				return (BattlePlayerReadOnlyInfoPair pairInfo, int callCount, int turnEndCount) => pairInfo.ReadOnlySelf.SkillInfoDeckCards.Count() % 2 == 0;
			}
			return (BattlePlayerReadOnlyInfoPair pairInfo, int callCount, int turnEndCount) => pairInfo.ReadOnlySelf.SkillInfoDeckCards.Count() % 2 == 1;
		case "call_count":
			return (BattlePlayerReadOnlyInfoPair pairInfo, int callCount, int turnEndCount) => callCount >= ((list.Length > 1) ? int.Parse(list[1]) : 0);
		case "turn_end_count":
		case "op_turn_end_count":
		case "me_or_op_turn_end_count":
			return (BattlePlayerReadOnlyInfoPair pairInfo, int callCount, int turnEndCount) => turnEndCount >= ((list.Length > 1) ? int.Parse(list[1]) : 0);
		default:
			return (BattlePlayerReadOnlyInfoPair pairInfo, int callCount, int turnEndCount) => true;
		}
	}

	protected string[] DisassemblySpecialString(string str)
	{
		str = str.Replace("(", string.Empty).Replace(")", string.Empty);
		return str.Split('&');
	}

	protected void SkillIconInitialize(ParallelVfxPlayer vfx, List<BattleCardBase> inPlayCards)
	{
		// Route through the first inPlayCard's mgr (all cards in the pair share a mgr); no-op when
		// the list is empty (nothing to initialize) which matches the pre-cull "no mgr = no icon" state.
		if (inPlayCards.Count > 0 && !inPlayCards[0].SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			for (int i = 0; i < inPlayCards.Count; i++)
			{
				BattleCardBase battleCardBase = inPlayCards[i];
				vfx.Register(battleCardBase.BattleCardView.BattleCardIconAnimations.Initialize(battleCardBase, battleCardBase.Skills));
			}
		}
	}

	public virtual void Clone(SkillPreprocessBase source, SkillBase Skill)
	{
	}

	protected VfxBase StopSkill(SkillBase skill, SkillProcessor skillProcessor)
	{
		VfxWithLoading result = skill.Stop(skillProcessor);
		skill.SetIsResidentSkillStartFlag(flg: false);
		return result;
	}
}

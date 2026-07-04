using System.Collections.Generic;
using System.Linq;
using Cute;
using Wizard.Battle.Card;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public class AIOperationProcessor
{
	private BattleManagerBase _battleMgr;

	private EnemyAI _ai;

	public AIOperationProcessor(BattleManagerBase mgr, EnemyAI ai)
	{
		_battleMgr = mgr;
		_ai = ai;
	}

	public VfxBase AIPlayCard(AISituationInfo situation)
	{
		_ai.BestPlayPtnRecord = null;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		VfxBase emote = _ai.GetEmote(AIEmoteCmdType.ON_CARD_PLAY_ALLY, situation);
		sequentialVfxPlayer.Register(emote);
		AIRealBattleCardSearcher.SearchBattleCardFromSituation(_ai.PlayerPair, situation, out var actor, out var targetList);
		AISelectedTargetInfoSet selectedTargets = situation.SelectedTargets;
		if (targetList.IsNotNullOrEmpty() && !selectedTargets.HasChoiceTarget)
		{
			SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
			for (int i = 0; i < targetList.Count; i++)
			{
				BattleCardBase battleCardBase = targetList[i];
				if (!battleCardBase.IsClass && !battleCardBase.BattleCardView.IsLoadResorces)
				{
					sequentialVfxPlayer2.Register(WaitVfx.Create(0.07f));
					sequentialVfxPlayer2.Register(battleCardBase.LoadResource());
					sequentialVfxPlayer2.Register(WaitVfx.Create(0.2f));
				}
			}
			sequentialVfxPlayer.Register(sequentialVfxPlayer2);
		}
		BattleCardBase battleCardBase2 = actor;
		int num = 0;
		if (actor.HasSkillAccelerate || actor.HasSkillCrystallize)
		{
			int num2 = CalcFixedUseCostTransformId(actor, actor.SelfBattlePlayer.Pp);
			if (num2 > 0)
			{
				battleCardBase2 = _battleMgr.CreateTransformCardRegisterVfx(actor, num2, actor.IsPlayer);
			}
		}
		else if (actor.Skills.CheckWhenPlayChoice && actor.Skills.Count() > 1)
		{
			SkillBase skillBase = actor.Skills.Get(0);
			SkillBase skillBase2 = actor.Skills.Get(1);
			if (skillBase.IsChoiceType && skillBase2 is Skill_transform)
			{
				battleCardBase2 = targetList[0];
				num = 1;
			}
		}
		BattlePlayerPair battlePlayerPair = _ai.PlayerPair.VirtualClone(CloneActualFlags.All);
		battlePlayerPair.Self.AddCurrentTrunPlayCount(1);
		BattleCardBase battleCardBase3 = battlePlayerPair.Self.HandCardList.Where((BattleCardBase c) => c.Index == actor.Index && c.CardId == actor.CardId && c.IsPlayer == actor.IsPlayer).FirstOrDefault();
		if (battleCardBase3 != null)
		{
			battlePlayerPair.Self.HandCardList.Remove(battleCardBase3);
		}
		List<SkillBase> checkSkillList = GetCheckSkillList(battleCardBase2.Skills, battlePlayerPair);
		if (CheckSelectTypeSkillTargetCount(actor, checkSkillList, targetList, num, "[PlayCard]") || targetList != null)
		{
			CheckSelectTypeSkillTarget(actor, battlePlayerPair, checkSkillList, targetList, num, "[PlayCard]");
		}
		if (!targetList.IsNotNullOrEmpty() || !targetList.Any((BattleCardBase c) => c is IVirtualBattleCard) || !actor.Skills.HaveChoiceTransformSkill())
		{
			sequentialVfxPlayer.Register(_battleMgr.OperateMgr.InitSetCard(actor, actor.IsPlayer, targetList.IsNotNullOrEmpty()));
		}
		sequentialVfxPlayer.Register(_battleMgr.OperateMgr.PlayCard(actor, actor.IsPlayer, targetList));
		return sequentialVfxPlayer;
	}

	public VfxBase AIEvolutionCard(AISituationInfo situation)
	{
		AIRealBattleCardSearcher.SearchBattleCardFromSituation(_ai.PlayerPair, situation, out var actor, out var targetList);
		if (!actor.CanEvolution(isSkill: false, isSelfBattlePlayer: true))
		{
			AIConsoleUtility.LogError("Evolution Condition Error!!!");
		}
		if (actor.EvolutionSkills.HasEvolutionSkillWithSelection)
		{
			List<SkillBase> checkSkillList = GetCheckSkillList(actor.EvolutionSkills, _ai.PlayerPair);
			CheckSelectTypeSkillTarget(actor, _ai.PlayerPair, checkSkillList, targetList, 0, "[Evolution]");
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		VfxBase emote = _ai.GetEmote(AIEmoteCmdType.ON_ALLY_EVOLUTION, situation);
		sequentialVfxPlayer.Register(emote);
		if (emote != NullVfx.GetInstance())
		{
			_ai.EmoteQuery.OnCardPlayEmotion();
		}
		sequentialVfxPlayer.Register(_battleMgr.OperateMgr.EvolutionCard(actor, actor.IsPlayer, targetList));
		return sequentialVfxPlayer;
	}

	public VfxBase AIFusionCard(AISituationInfo situation)
	{
		AIRealBattleCardSearcher.SearchBattleCardFromSituation(_ai.PlayerPair, situation, out var actor, out var targetList);
		SkillBase item = actor.Skills.FirstOrDefault((SkillBase s) => s is Skill_fusion);
		CheckSelectTypeSkillTarget(actor, _ai.PlayerPair, new List<SkillBase> { item }, targetList, 0, "[Fusion]");
		return _battleMgr.OperateMgr.FusionCard(actor, actor.IsPlayer, targetList);
	}

	private List<SkillBase> GetCheckSkillList(SkillCollectionBase skillCollection, BattlePlayerPair playerPair)
	{
		List<SkillBase> list = new List<SkillBase>();
		IEnumerable<SkillBase> selectTypeSkill = skillCollection.GetSelectTypeSkill(isEvolve: true, isFusion: false, isActivateFanfare: false, playerPair);
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		foreach (SkillBase item in skillCollection.Where((SkillBase s) => s.IsChoiceType && s.CheckCondition(playerPair, option, isPrePlay: true)))
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (SkillBase item2 in selectTypeSkill)
		{
			if (!list.Contains(item2))
			{
				list.Add(item2);
			}
		}
		return list;
	}

	private void CheckSelectTypeSkillTarget(BattleCardBase actor, BattlePlayerPair playerPair, IEnumerable<SkillBase> skills, List<BattleCardBase> targetList, int selectTargetNumber, string checkTimingName)
	{
		int num = selectTargetNumber;
		bool flag = false;
		foreach (SkillBase skill in skills)
		{
			if (skill.IsBurialRite)
			{
				BattleCardBase card = targetList[num];
				if (!actor.SelfBattlePlayer.HandCardList.Any((BattleCardBase c) => c.Index == card.Index && c.IsPlayer == card.IsPlayer))
				{
					string message = actor.BaseParameter.CardName + checkTimingName + "BURIAL_RITE Target not in Hand list Error!!!";
					ThrowErrorMessage(message);
				}
				else if (!card.IsInHand || !card.IsUnit)
				{
					string message2 = actor.BaseParameter.CardName + checkTimingName + "BURIAL_RITE Wrong targeted selection criteria Error!!!";
					ThrowErrorMessage(message2);
				}
				flag = true;
				num++;
			}
			if (skill.ApplySelectFilter is SkillRandomSelectFilter || skill.ApplySelectFilter is SkillSelectAllFilter || (!skill.IsChoiceType && skill.ApplySelectFilter is SkillSelectNullFilter))
			{
				continue;
			}
			IEnumerable<BattleCardBase> selectableCards = skill.GetSelectableCards(playerPair, new SkillConditionCheckerOption());
			int skillSelectCount = skill.GetSkillSelectCount();
			int num2 = targetList.Count - num;
			skillSelectCount = ((skillSelectCount > num2) ? num2 : skillSelectCount);
			List<BattleCardBase> list = new List<BattleCardBase>(targetList.GetRange(num, skillSelectCount));
			num += skillSelectCount;
			if (list.Count > 1)
			{
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					BattleCardBase card2 = list[num3];
					List<BattleCardBase> list2 = list.Where((BattleCardBase c) => c.Index == card2.Index && c.IsPlayer == card2.IsPlayer).ToList();
					if (list2.Count > 1)
					{
						string message3 = $"{actor.BaseParameter.CardName}{checkTimingName}Duplicate {list2.Count} selected card Error!!!";
						ThrowErrorMessage(message3);
					}
				}
			}
			int num4 = 0;
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				BattleCardBase card3 = list[num5];
				if (!selectableCards.Any((BattleCardBase c) => c.Index == card3.Index && c.IsPlayer == card3.IsPlayer))
				{
					if (num4 < selectableCards.Count() && !flag)
					{
						targetList[num5] = selectableCards.ElementAt(num4);
						num4++;
					}
					string message4 = actor.BaseParameter.CardName + checkTimingName + "Target not in selection list Error!!!";
					ThrowErrorMessage(message4);
				}
			}
		}
	}

	private bool CheckSelectTypeSkillTargetCount(BattleCardBase actor, IEnumerable<SkillBase> skills, List<BattleCardBase> targetList, int selectCardCount, string checkTimingName)
	{
		string text = checkTimingName + actor.BaseParameter.CardName + "\n";
		int num = selectCardCount;
		foreach (SkillBase skill in skills)
		{
			if (skill.IsBurialRite)
			{
				selectCardCount++;
				num++;
				text += "BurialRite1回";
				if (skill is Skill_none)
				{
					continue;
				}
			}
			if (!(skill.ApplySelectFilter is SkillRandomSelectFilter) && !(skill.ApplySelectFilter is SkillSelectAllFilter) && (!skill.IsBurialRite || skill.ApplySelectFilter is SkillUserSelectFilter) && (skill.IsChoiceType || !(skill.ApplySelectFilter is SkillSelectNullFilter)))
			{
				BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(actor.SelfBattlePlayer, actor.OpponentBattlePlayer);
				SkillConditionCheckerOption option = new SkillConditionCheckerOption();
				if (skill.CheckCondition(playerInfoPair, option, isPrePlay: true))
				{
					int skillSelectCount = skill.GetSkillSelectCount();
					selectCardCount += skillSelectCount;
					num += ((!actor.IsUnit && !actor.IsField) ? 1 : 0);
					text += $"{skill}{skillSelectCount}回";
				}
			}
		}
		if (selectCardCount <= 0)
		{
			return false;
		}
		bool flag = false;
		if (targetList == null || targetList.Count <= 0)
		{
			if (num <= 0)
			{
				return false;
			}
			text += "\n選択されていません";
			flag = true;
		}
		else
		{
			int count = targetList.Count;
			int num2 = count - selectCardCount;
			if (num2 < 0 && count < num)
			{
				text += "\n少なく選択されています";
				flag = true;
			}
			if (num2 > 0)
			{
				text += $"\n{num2}回多く選択されています";
				flag = true;
			}
		}
		if (flag)
		{
			ThrowErrorMessage(text);
		}
		return flag;
	}

	public int CalcFixedUseCostTransformId(BattleCardBase actor, int currentPp)
	{
		int num = -1;
		int cost = actor.Cost;
		int result = -1;
		for (int i = 0; i < actor.Skills.Count(); i++)
		{
			if (!(actor.Skills.ElementAt(i) is Skill_pp_fixeduse skill_pp_fixeduse) || currentPp < skill_pp_fixeduse._fixedUsePP)
			{
				continue;
			}
			bool num2 = skill_pp_fixeduse.CheckCondition(_ai.PlayerPair, new SkillConditionCheckerOption(), isPrePlay: true);
			bool flag = num2 && num < skill_pp_fixeduse._fixedUsePP;
			bool flag2 = num2 && skill_pp_fixeduse._fixedUsePP < cost && currentPp < cost;
			if (flag || flag2)
			{
				if (actor.Skills.ElementAt(i + 1) is Skill_transform skill_transform)
				{
					result = skill_transform.TransformId;
				}
				num = skill_pp_fixeduse._fixedUsePP;
			}
		}
		return result;
	}

	private void ThrowErrorMessage(string message)
	{
		AIConsoleUtility.LogError(message);
	}
}

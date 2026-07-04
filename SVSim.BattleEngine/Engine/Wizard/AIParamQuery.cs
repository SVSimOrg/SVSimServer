using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle;

namespace Wizard;

public class AIParamQuery
{

	public EnemyAI enemyAI;

	public AIStyleQuery styleQuery;

	private int classID;

	private AI_LOGIC_LV logicLv = AI_LOGIC_LV.STRONG;

	private AIDeckAcccessor deck;

	public AIParamQuery(EnemyAI ai)
	{
		enemyAI = ai;
	}

	public void SetUp(AIStyleQuery _styleQuery)
	{
		styleQuery = _styleQuery;
	}

	public void SetDeck(int _classID, AI_LOGIC_LV _logicLv, AIDeckData _curDeck, AIDeckData _commonDic, AIDeckData _allyCommonDic)
	{
		classID = _classID;
		logicLv = _logicLv;
		deck = new AIDeckAcccessor(_curDeck, _commonDic, _allyCommonDic);
	}

	public AICardData SearchAICardData(AIVirtualCard card)
	{
		if (!card.IsLeader && deck != null)
		{
			return deck.SearchCardData(card.BaseId, card.IsAlly);
		}
		return null;
	}

	public int GetClassID()
	{
		return classID;
	}

	public AI_LOGIC_LV GetLogicLv()
	{
		return logicLv;
	}

	public bool IsEnabledTag(AIVirtualCard card, AIVirtualField field, AIPlayTagType type, List<int> playPtn, AISituationInfo situation)
	{
		int tagCount = GetTagCount(card);
		if (tagCount <= 0)
		{
			return false;
		}
		for (int i = 0; i < tagCount; i++)
		{
			AIPlayTag tag = GetTag(card, i);
			if (tag.Type == type && tag.CheckCondition(card, playPtn, card.SelfField, situation))
			{
				return true;
			}
		}
		return false;
	}

	public int GetTagCount(AIVirtualCard owner, AICardData data = null)
	{
		if (deck == null)
		{
			return 0;
		}
		AICardData aICardData = null;
		if (!owner.IsLeader)
		{
			aICardData = ((data == null) ? deck.SearchCardData(owner.BaseId, enemyAI.IsAllyCard(owner)) : data);
		}
		int num = 0;
		if (aICardData != null && aICardData.TagList != null && !owner.IsSkillLost)
		{
			if (owner.IsRobbedLastword)
			{
				for (int i = 0; i < aICardData.TagList.Count; i++)
				{
					if (!aICardData.TagList[i].IsLastwordTag())
					{
						num++;
					}
				}
			}
			else
			{
				num += aICardData.TagList.Count;
			}
		}
		return num;
	}

	public AIPlayTag GetTag(AIVirtualCard owner, int index, AICardData data = null)
	{
		AICardData aICardData = null;
		if (!owner.IsLeader)
		{
			aICardData = ((data == null) ? deck.SearchCardData(owner.BaseId, enemyAI.IsAllyCard(owner)) : data);
		}
		int num = 0;
		if (aICardData != null && aICardData.TagList != null && !owner.IsSkillLost)
		{
			if (owner.IsRobbedLastword)
			{
				for (int i = 0; i < aICardData.TagList.Count; i++)
				{
					if (!aICardData.TagList[i].IsLastwordTag())
					{
						if (num == index)
						{
							return aICardData.TagList[i];
						}
						num++;
					}
				}
			}
			else
			{
				num = aICardData.TagList.Count;
				if (aICardData.TagList.Count > index)
				{
					return aICardData.TagList[index];
				}
			}
		}
		return null;
	}

	public bool IsRobbedLastword(BattleCardBase card)
	{
		for (int i = 0; i < card.BuffInfoList.Count; i++)
		{
			BuffInfo buffInfo = card.BuffInfoList[i];
			if (buffInfo.PreviousOwner == null && buffInfo.SkillFrom is Skill_rob_skill)
			{
				return true;
			}
		}
		return false;
	}

	public float EvaluateBattlePlayerPair(AIVirtualField field, bool useStyle = true)
	{
		float num = 0f;
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.IsOnField)
			{
				float num2 = aIVirtualCard.EvaluateValueOnField(EnemyAI.EmptyPlayPtn, null, useStyle);
				num += num2;
			}
		}
		for (int j = 0; j < field.EnemyInplayCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = field.EnemyInplayCards[j];
			if (!aIVirtualCard2.IsDead && aIVirtualCard2.IsOnField)
			{
				float num3 = aIVirtualCard2.EvaluateValueOnField(EnemyAI.EmptyPlayPtn, null, useStyle);
				num -= num3;
			}
		}
		return num;
	}

	public IEnumerable<BattleCardBase> RemoveDuplicatedCards(IEnumerable<BattleCardBase> targetCards, IEnumerable<BattleCardBase> filterCards)
	{
		if (!targetCards.IsNotNullOrEmpty() || !filterCards.IsNotNullOrEmpty())
		{
			yield break;
		}
		foreach (BattleCardBase targetCard in targetCards)
		{
			bool flag = false;
			foreach (BattleCardBase filterCard in filterCards)
			{
				if (filterCard != null && targetCard.IsPlayer == filterCard.IsPlayer && targetCard.Index == filterCard.Index)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				yield return targetCard;
			}
		}
	}

	public bool IsBanishSkill(SkillBase skill)
	{
		if (!(skill is Skill_banish) && !(skill is Skill_destroy) && !(skill is Skill_damage) && !(skill is Skill_return_card))
		{
			return skill is Skill_metamorphose;
		}
		return true;
	}

	public float CalcHandCardOverflowPenalty(AIVirtualField field, List<int> playPtn, float penalty, ref int dstHandCount)
	{
		float num = 0f;
		for (int i = 0; i < playPtn.Count; i++)
		{
			int index = playPtn[i];
			AIVirtualCard aIVirtualCard = field.AllyHandCards[index];
			int handPlusCount = aIVirtualCard.GetHandPlusCount(playPtn);
			_ = aIVirtualCard.IsSpell;
			dstHandCount--;
			dstHandCount += handPlusCount;
			if (dstHandCount > 9)
			{
				num += penalty * (float)(dstHandCount - 9);
				dstHandCount = 9;
			}
		}
		for (int j = 0; j < field.AllyInplayCards.Count; j++)
		{
			AIVirtualCard tagOwner = field.AllyInplayCards[j];
			dstHandCount += tagOwner.GetHandPlusCount(playPtn);
		}
		if (dstHandCount > 8)
		{
			num += penalty * (float)(dstHandCount - 8);
		}
		return num;
	}

	public int CalcAllyUnitTotalDamage()
	{
		int num = 0;
		foreach (BattleCardBase inPlayCard in enemyAI.ALLY.InPlayCards)
		{
			if (inPlayCard.IsUnit)
			{
				int num2 = inPlayCard.MaxLife - inPlayCard.Life;
				num += num2;
			}
		}
		return num;
	}

	public int GetEnemyGuardiansCount()
	{
		int num = 0;
		foreach (BattleCardBase inPlayCard in enemyAI.OPPONENT.InPlayCards)
		{
			if (inPlayCard.SkillApplyInformation.IsGuard)
			{
				num++;
			}
		}
		return num;
	}

	public IEnumerable<BattleCardBase> GetSelectSkillTargetCandidates(BattleCardBase card, BattlePlayerReadOnlyInfoPair pair)
	{
		SkillBase actSkill = null;
		return ActionProcessor.GetSkillUserSelectableTargets(card.Skills, pair, ref actSkill);
	}

	public bool IsDestroyBeforeAttack(BattleCardBase card)
	{
		if (card.IsUnit)
		{
			return card.Skills.Any((SkillBase skill) => skill is Skill_destroy && skill.IsBeforAttackSkill);
		}
		return false;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> GetBrokenAll(BattleCardBase card)
	{
		SkillTargetDestroyedCardListFilter skillTargetDestroyedCardListFilter = new SkillTargetDestroyedCardListFilter();
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		IBattlePlayerReadOnlyInfo[] battlePlayerInfos = new IBattlePlayerReadOnlyInfo[1] { card.SelfBattlePlayer };
		return skillTargetDestroyedCardListFilter.Filtering(battlePlayerInfos, option);
	}

	public IEnumerable<BattleCardBase> GetSelectableCards(SkillBase skill, BattlePlayerPair pair, bool isSkipForceSelect = false)
	{
		if (skill.IsUserSelectType || skill.IsChoiceType)
		{
			SkillConditionCheckerOption option = new SkillConditionCheckerOption();
			return skill.GetSelectableCards(pair, option, isSkipForceSelect);
		}
		return new List<BattleCardBase>();
	}

	public static List<T> AddElementToList<T>(T element, List<T> container, bool isBlockDuplicate = false)
	{
		if (container == null)
		{
			container = new List<T>();
		}
		if (!isBlockDuplicate || !container.Contains(element))
		{
			container.Add(element);
		}
		return container;
	}

	public static List<T> AddRangeToList<T>(List<T> elements, List<T> container, bool isBlockDuplicate = false)
	{
		if (elements == null || elements.Count <= 0)
		{
			return container;
		}
		if (container == null)
		{
			container = new List<T>();
		}
		for (int i = 0; i < elements.Count; i++)
		{
			T item = elements[i];
			if (!isBlockDuplicate || !container.Contains(item))
			{
				container.Add(item);
			}
		}
		return container;
	}

	public static List<T> CloneList<T>(List<T> src)
	{
		if (src == null)
		{
			return null;
		}
		return new List<T>(src);
	}
}

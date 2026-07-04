using System.Collections.Generic;

namespace Wizard;

public static class AIPlayOnSkillUtility
{
	public static int GetLifePenaltyOnPlay(BattleCardBase card, EnemyAI enemyAI)
	{
		int num = 0;
		IEnumerable<SkillBase> selfInjurySkillsOnPlay = GetSelfInjurySkillsOnPlay(card, enemyAI);
		if (selfInjurySkillsOnPlay == null)
		{
			return num;
		}
		foreach (SkillBase item in selfInjurySkillsOnPlay)
		{
			num += GetSelfInjurySkillDamage(card, item, isNecromance: false, enemyAI);
		}
		return num;
	}

	private static IEnumerable<SkillBase> GetSelfInjurySkillsOnPlay(BattleCardBase card, EnemyAI enemyAI)
	{
		foreach (SkillBase skill in card.Skills)
		{
			if (!(skill is Skill_damage) || skill.IsWhenPlaySkill)
			{
				continue;
			}
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(enemyAI.ALLY, enemyAI.OPPONENT);
			if (!skill.CheckConditionAI(playerInfoPair, new SkillConditionCheckerOption(), isPrePlay: true))
			{
				continue;
			}
			IEnumerable<BattleCardBase> selectableCards = skill.GetSelectableCards(playerInfoPair, new SkillConditionCheckerOption());
			foreach (BattleCardBase item in selectableCards)
			{
				if (item == enemyAI.ALLY.Class)
				{
					yield return skill;
					break;
				}
			}
		}
	}

	private static int GetSelfInjurySkillDamage(BattleCardBase card, SkillBase skill, bool isNecromance, EnemyAI enemyAI)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(enemyAI.ALLY, enemyAI.OPPONENT);
		SkillCollectionBase.SetupOptionValue(skill.OptionValue, playerInfoPair, card, skill);
		return skill.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.damage, 0);
	}
}

using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIPlayerLifeSimulationUtility
{
	public static int GetAllSelfTurnDamageCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType durationType)
	{
		return GetSelfTurnDamageCountAtPlayed(tagOwner, field, playPtn, durationType) + GetSelfTurnDamageCountAtPlayPtn(tagOwner, field, playPtn);
	}

	public static int GetSelfTurnDamageCountAtPlayed(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType durationType)
	{
		int num = 0;
		if (!tagOwner.IsAlly)
		{
			_ = field.EnemyBattlePlayer;
		}
		else
		{
			_ = field.AllyBattlePlayer;
		}
		return durationType switch
		{
			AIScriptTokenArgType.TURN => field.AllyDamageCountInTurn, 
			AIScriptTokenArgType.GAME => tagOwner.IsAlly ? field.AllyDamageCountInGame : field.EnemyDamageCountInGame, 
			_ => num, 
		};
	}

	public static int GetSelfTurnDamageCountAtPlayPtn(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn)
	{
		if (!tagOwner.IsAlly)
		{
			return 0;
		}
		int num = 0;
		IEnumerable<SkillBase> enumerable = null;
		for (int i = 0; i < playPtn.Count; i++)
		{
			enumerable = GetSelfInjurySkillsOnPlay(field.AllyHandCards[playPtn[i]], field);
			if (enumerable != null)
			{
				num += enumerable.Count();
			}
		}
		return num;
	}

	public static int GetSelfTurnDamageCountAtBeforePlayPtn(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType durationType)
	{
		if (!tagOwner.IsAlly)
		{
			return 0;
		}
		int num = 0;
		IEnumerable<SkillBase> enumerable = null;
		for (int i = 0; i < playPtn.Count && !tagOwner.IsSameCard(field.AllyHandCards[playPtn[i]]); i++)
		{
			enumerable = GetSelfInjurySkillsOnPlay(field.AllyHandCards[playPtn[i]], field);
			if (enumerable != null)
			{
				num += enumerable.Count();
			}
		}
		return num + GetSelfTurnDamageCountAtPlayed(tagOwner, field, playPtn, durationType);
	}

	public static IEnumerable<SkillBase> GetSelfInjurySkillsOnPlay(AIVirtualCard card, AIVirtualField field)
	{
		foreach (SkillBase battleSkill in card.BattleSkills)
		{
			if (!(battleSkill is Skill_damage) || !battleSkill.IsWhenPlaySkill)
			{
				continue;
			}
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(field.AllyBattlePlayer, field.EnemyBattlePlayer);
			if (!battleSkill.CheckConditionAI(playerInfoPair, new SkillConditionCheckerOption(), isPrePlay: true))
			{
				continue;
			}
			IEnumerable<BattleCardBase> selectableCards = battleSkill.GetSelectableCards(playerInfoPair, new SkillConditionCheckerOption());
			foreach (BattleCardBase item in selectableCards)
			{
				if (item == field.AllyBattlePlayer.Class)
				{
					yield return battleSkill;
					break;
				}
			}
		}
	}
}

using System.Collections.Generic;
using System.Linq;

public class RegisterTool
{
	public enum OrderListParameter
	{
		add,
		move,
		metamorphose,
		alter,
		trigger,
		target,
		playerParam,
		exTurn,
		specialWin,
		deckOut,
		validate,
		evolution,
		skillConditionCheck,
		scan,
		reverseDeckOut,
		playCount,
		extract,
		count,
		openMyCards
	}

	public static string MakeCostFromSkillDestroyed(BattleManagerBase mgr, bool isPlayer, int destroyCount, CardDataModel unapprovedCard)
	{
		if (unapprovedCard == null /* Pre-Phase-5b: IsWatchBattle const-false */)
		{
			return "";
		}
		return "gt" + NetworkBattleGenericTool.GetIndexToCardBase(mgr, mgr.GetBattlePlayer(isPlayer), unapprovedCard.skillKeyCardIdxList[destroyCount]).Cost;
	}

	public static string MakeParameterOptionText(string text)
	{
		switch (text)
		{
		case "=":
			return "eq";
		case "!=":
			return "ne";
		case "<=":
		case "<:=":
			return "le";
		case ">=":
		case ">:=":
			return "ge";
		case "<":
		case "<:":
			return "lt";
		case ">":
		case ">:":
			return "gt";
		default:
			return string.Empty;
		}
	}

	public static List<string> MakeParameterOptionTextList(string text)
	{
		List<string> list = new List<string>();
		list.Add(MakeParameterOptionText(text));
		switch (text)
		{
		case "<:":
		case ">:":
		case "<:=":
		case ">:=":
			list.Add("max");
			break;
		}
		return list;
	}

	public static bool IsSkillRandom(SkillBase skill)
	{
		if (skill.ApplyAndFilter.Count > 0)
		{
			for (int i = 0; i < skill.ApplyAndFilter.Count; i++)
			{
				ApplySkillTargetFilterCollection applySkillTargetFilterCollection = skill.ApplyAndFilter[i];
				if (IsRandomSelectFilter(applySkillTargetFilterCollection.SelectFilter) || HasRandomCustomSelectFilter(applySkillTargetFilterCollection.ApplyCustomSelectFilterList))
				{
					return true;
				}
			}
		}
		if (IsRandomSelectFilter(skill.ApplySelectFilter) || HasRandomCustomSelectFilter(skill.ApplyCustomSelectFilterList))
		{
			return true;
		}
		return false;
	}

	private static bool IsRandomSelectFilter(ISkillSelectFilter skillSelectFilter)
	{
		if (!(skillSelectFilter is SkillRandomSelectFilter) && !(skillSelectFilter is SkillRandomSelectUntilFilter) && !(skillSelectFilter is SkillIdNoDuplicationRandomSelectFilter) && !(skillSelectFilter is SkillCostNoDuplicationRandomSelectFilter))
		{
			return skillSelectFilter is SkillRandomEachSameBaseCardIdFilter;
		}
		return true;
	}

	private static bool HasRandomCustomSelectFilter(List<ISkillCustomSelectFilter> customSelectFilterList)
	{
		for (int i = 0; i < customSelectFilterList.Count; i++)
		{
			if (IsRamdomCustomSelectFilter(customSelectFilterList[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsRamdomCustomSelectFilter(ISkillCustomSelectFilter customSelectFilter)
	{
		return customSelectFilter is SkillTargetOverCostFromLastTargetFilter;
	}

	public static bool HasTargetOverCostFromFilter(SkillBase skill)
	{
		if (skill.ApplyCustomSelectFilterList != null && skill.ApplyCustomSelectFilterList.Any((ISkillCustomSelectFilter x) => x is SkillTargetOverCostFromLastTargetFilter))
		{
			return true;
		}
		return false;
	}

	public static bool IsSkillFilterEffect(SkillBase skill)
	{
		if (skill.ApplyCardFilterList.Count == 1 && skill.ApplyCardFilterList.First() is SkillAllCardFilter)
		{
			return false;
		}
		return true;
	}

	public static List<RegisterActionBase.CARD_TYPE> GetCardTypeList(ISkillCardFilter cardFilter)
	{
		List<RegisterActionBase.CARD_TYPE> list = new List<RegisterActionBase.CARD_TYPE>();
		if (cardFilter is SkillUnitFilter)
		{
			list.Add(RegisterActionBase.CARD_TYPE.FOLLOWER);
		}
		if (cardFilter is SkillSpellFilter)
		{
			list.Add(RegisterActionBase.CARD_TYPE.SPELL);
		}
		if (cardFilter is SkillFieldFilter)
		{
			list.Add(RegisterActionBase.CARD_TYPE.NOT_COUNT_AMULET);
			list.Add(RegisterActionBase.CARD_TYPE.COUNT_AMULET);
		}
		if (cardFilter is SkillChantFieldFilter)
		{
			list.Add(RegisterActionBase.CARD_TYPE.COUNT_AMULET);
		}
		if (cardFilter is SkillNotChantFieldFilter)
		{
			list.Add(RegisterActionBase.CARD_TYPE.NOT_COUNT_AMULET);
		}
		if (cardFilter is SkillUnitAndAllFieldFilter)
		{
			list.Add(RegisterActionBase.CARD_TYPE.FOLLOWER);
			list.Add(RegisterActionBase.CARD_TYPE.NOT_COUNT_AMULET);
			list.Add(RegisterActionBase.CARD_TYPE.COUNT_AMULET);
		}
		if (cardFilter is SkillSpellAndFieldFilter)
		{
			list.Add(RegisterActionBase.CARD_TYPE.SPELL);
			list.Add(RegisterActionBase.CARD_TYPE.NOT_COUNT_AMULET);
			list.Add(RegisterActionBase.CARD_TYPE.COUNT_AMULET);
		}
		return list;
	}

	public static RegisterActionBase.SPECIAL_LIBRARY CalculationCardSkillType(ISkillCardFilter cardFilter, RegisterActionBase.CARD_TYPE CharaTypeList)
	{
		if (cardFilter is SkillAbilitySpellChargeFilter)
		{
			if ((cardFilter as SkillAbilitySpellChargeFilter).GetOptionParameter() != "!=")
			{
				if (CharaTypeList == RegisterActionBase.CARD_TYPE.SPELL)
				{
					return RegisterActionBase.SPECIAL_LIBRARY.SPELL_CHARGE_SPELL;
				}
				return RegisterActionBase.SPECIAL_LIBRARY.SPELL_CHARGE_ALL;
			}
			return RegisterActionBase.SPECIAL_LIBRARY.SPELL_NOT_SPELL_CHARGE;
		}
		if (cardFilter is SkillAbilityDestroyWhiteRitualFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.EARTH_RITUAL;
		}
		if (cardFilter is SkillAbilityWhiteRitualFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.EARTH_MARK;
		}
		if (cardFilter is SkillAbilityWhenDestroyFilter)
		{
			if (!(cardFilter as SkillAbilityWhenDestroyFilter).IsOperaterEqual())
			{
				return RegisterActionBase.SPECIAL_LIBRARY.NOT_WHEN_DESTROY;
			}
			return RegisterActionBase.SPECIAL_LIBRARY.WHEN_DESTROY;
		}
		if (cardFilter is SkillAbilityUnionBurstFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.UNION_BURST;
		}
		if (cardFilter is SkillAbilitySuperSkyboundArtFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.SUPER_SKYBOUND_ART;
		}
		if (cardFilter is SkillAbilityWhenPlayFilter)
		{
			if ((cardFilter as SkillAbilityWhenPlayFilter).IsOperaterEqual())
			{
				return RegisterActionBase.SPECIAL_LIBRARY.WHEN_PLAY;
			}
			return RegisterActionBase.SPECIAL_LIBRARY.NOT_WHEN_PLAY;
		}
		if (cardFilter is SkillAbilityEnhanceFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.ENHANCE;
		}
		if (cardFilter is SkillAbilityAccelerateFilter)
		{
			if (!(cardFilter as SkillAbilityAccelerateFilter).IsOperaterEqual())
			{
				return RegisterActionBase.SPECIAL_LIBRARY.NOT_WHEN_ACCELERATE;
			}
			return RegisterActionBase.SPECIAL_LIBRARY.WHEN_ACCELERATE;
		}
		if (cardFilter is SkillAbilityCrystallizeFilter)
		{
			if (!(cardFilter as SkillAbilityCrystallizeFilter).IsOperaterEqual())
			{
				return RegisterActionBase.SPECIAL_LIBRARY.NOT_WHEN_CRYSTALLIZE;
			}
			return RegisterActionBase.SPECIAL_LIBRARY.WHEN_CRYSTALLIZE;
		}
		if (cardFilter is SkillAbilityReanimateFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.REANIMATE;
		}
		if (cardFilter is SkillAbilityNecromanceFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.NECROMANCE;
		}
		if (cardFilter is SkillAbilityWhenEvolveFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.WHEN_EVOLVE;
		}
		if (cardFilter is SkillAbilityFusionFilter)
		{
			return RegisterActionBase.SPECIAL_LIBRARY.FUSION;
		}
		return RegisterActionBase.SPECIAL_LIBRARY.NONE;
	}
}

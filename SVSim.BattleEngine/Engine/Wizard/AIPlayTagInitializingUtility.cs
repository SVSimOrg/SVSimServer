using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wizard.Battle.UI;

namespace Wizard;

public static class AIPlayTagInitializingUtility
{
	private static readonly AIScriptTokenArgType[] LegalClassType = new AIScriptTokenArgType[9]
	{
		AIScriptTokenArgType.NEUTRAL,
		AIScriptTokenArgType.ELF,
		AIScriptTokenArgType.ROYAL,
		AIScriptTokenArgType.WITCH,
		AIScriptTokenArgType.DRAGON,
		AIScriptTokenArgType.NECROMANCER,
		AIScriptTokenArgType.VAMPIRE,
		AIScriptTokenArgType.BISHOP,
		AIScriptTokenArgType.NEMESIS
	};

	private static readonly AIScriptTokenArgType[] LegalTribeType = new AIScriptTokenArgType[16]
	{
		AIScriptTokenArgType.LEGION,
		AIScriptTokenArgType.LORD,
		AIScriptTokenArgType.LEVIN,
		AIScriptTokenArgType.WHITE_RITUAL,
		AIScriptTokenArgType.MANARIA,
		AIScriptTokenArgType.ARTIFACT,
		AIScriptTokenArgType.FOOD,
		AIScriptTokenArgType.LOOT,
		AIScriptTokenArgType.MACHINE,
		AIScriptTokenArgType.NATURE,
		AIScriptTokenArgType.BANQUET,
		AIScriptTokenArgType.HERO,
		AIScriptTokenArgType.ARMED,
		AIScriptTokenArgType.HELLBOUND,
		AIScriptTokenArgType.SCHOOL,
		AIScriptTokenArgType.CHESS
	};

	private static AIScriptTokenArgType[] LegalTokenSideType = new AIScriptTokenArgType[4]
	{
		AIScriptTokenArgType.BOTH,
		AIScriptTokenArgType.ALLY,
		AIScriptTokenArgType.OPPONENT,
		AIScriptTokenArgType.SELECTED_TARGET_SIDE
	};

	public static List<string> SplitTagText(string text)
	{
		List<string> list = new List<string>();
		int num = -1;
		int num2 = 0;
		bool flag = true;
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case '{':
				if (flag)
				{
					if (i > 0)
					{
						list.Add(text.Substring(0, i - 2));
					}
					flag = false;
				}
				num2++;
				if (num == -1)
				{
					num = i;
				}
				break;
			case '}':
				num2--;
				if (num2 == 0 && num < i && num >= 0)
				{
					list.Add(text.Substring(num, i + 1 - num));
					num = -1;
				}
				break;
			}
		}
		return list;
	}

	public static string TrimAttachTagArgument(string text)
	{
		string pattern = "(^{ | }$|}$)";
		return Regex.Replace(text, pattern, "");
	}

	public static AIPlayTag CreateAIPlayTagFromWords(string type, string arg, string cond)
	{
		AIPlayTagAsset aIPlayTagAsset = new AIPlayTagAsset();
		aIPlayTagAsset.Type = TrimAttachTagArgument(type);
		aIPlayTagAsset.Arg = TrimAttachTagArgument(arg);
		aIPlayTagAsset.Condition = TrimAttachTagArgument(cond);
		AIPlayTag aIPlayTag = new AIPlayTag();
		if (!aIPlayTag.InitFromTextAsset(aIPlayTagAsset))
		{
			return null;
		}
		return aIPlayTag;
	}

	public static AIScriptTokenArgType CreateSingleArgType(AIPolishConvertedExpression expression, AIScriptTokenArgType[] legalTypes = null)
	{
		if (!(expression.TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken))
		{
			AIConsoleUtility.LogError("CreateSingleArgType error!! arg = null!!!!!");
			return AIScriptTokenArgType.NONE;
		}
		if (legalTypes != null && Array.IndexOf(legalTypes, aIScriptArgumentToken.ArgumentType) < 0)
		{
			AIConsoleUtility.LogError("Argument is not legalTypes!! argType == " + aIScriptArgumentToken.ArgumentType);
			return aIScriptArgumentToken.ArgumentType;
		}
		return aIScriptArgumentToken.ArgumentType;
	}

	public static List<AIPlayTag> CloneTagList(List<AIPlayTag> src)
	{
		return AIParamQuery.CloneList(src);
	}

	public static bool IsInitOfFilterSet(AIPolishConvertedExpression arg)
	{
		if (arg.TokenList != null && arg.TokenList.Count > 0 && arg.TokenList[0] is AIScriptArgumentToken { IsNot: false } aIScriptArgumentToken)
		{
			int argumentType = (int)aIScriptArgumentToken.ArgumentType;
			if (argumentType > 83 && argumentType < 106)
			{
				return true;
			}
		}
		return false;
	}

	public static CardBasePrm.ClanType CreateClassType(AIPolishConvertedExpression expr)
	{
		return ConvertTokenArgTypeToClanType(CreateSingleArgType(expr, LegalClassType));
	}

	public static CardBasePrm.TribeType CreateTribeType(AIPolishConvertedExpression expr)
	{
		return AITribeSimulationUtility.ConvertTokenArgTypeToTribeType(CreateSingleArgType(expr, LegalTribeType));
	}

	public static CardBasePrm.ClanType ConvertTokenArgTypeToClanType(AIScriptTokenArgType tokenArgType)
	{
		return tokenArgType switch
		{
			AIScriptTokenArgType.NEUTRAL => CardBasePrm.ClanType.ALL, 
			AIScriptTokenArgType.ELF => CardBasePrm.ClanType.MIN, 
			AIScriptTokenArgType.ROYAL => CardBasePrm.ClanType.ROYAL, 
			AIScriptTokenArgType.WITCH => CardBasePrm.ClanType.WITCH, 
			AIScriptTokenArgType.DRAGON => CardBasePrm.ClanType.DRAGON, 
			AIScriptTokenArgType.NECROMANCER => CardBasePrm.ClanType.NECRO, 
			AIScriptTokenArgType.VAMPIRE => CardBasePrm.ClanType.VAMPIRE, 
			AIScriptTokenArgType.BISHOP => CardBasePrm.ClanType.BISHOP, 
			AIScriptTokenArgType.NEMESIS => CardBasePrm.ClanType.NEMESIS, 
			_ => CardBasePrm.ClanType.NONE, 
		};
	}

	public static AIPlayTag CreateTurnStartSubtractCountdownTagForCountdownAmulet()
	{
		AIPlayTagAsset aIPlayTagAsset = new AIPlayTagAsset();
		aIPlayTagAsset.Type = "turnStartSubtractCountdown";
		aIPlayTagAsset.Arg = "SELF ; ALL_SELECT ; 1 ; ALLY";
		aIPlayTagAsset.Condition = "";
		AIPlayTag aIPlayTag = new AIPlayTag();
		aIPlayTag.InitFromTextAsset(aIPlayTagAsset);
		return aIPlayTag;
	}

	public static AIPlayTag CreateBasicSkillTag(AIScriptTokenArgType skillType)
	{
		if (!GiveSkillTagCollection.IsGiveSkillManagedSkillType(skillType))
		{
			AIConsoleUtility.LogError($"AIPlayTagInitializingUtility.CreateBasicSkilltag() error!! skillType:{skillType} is invalid!!");
			return null;
		}
		AIPlayTagAsset aIPlayTagAsset = new AIPlayTagAsset();
		aIPlayTagAsset.Type = "giveSkill";
		aIPlayTagAsset.Arg = skillType.ToString();
		aIPlayTagAsset.Condition = "";
		AIPlayTag aIPlayTag = new AIPlayTag();
		aIPlayTag.InitFromTextAsset(aIPlayTagAsset);
		return aIPlayTag;
	}

	public static bool TryCreateTokenSideType(AIPolishConvertedExpression expression, out AIScriptTokenArgType sideType)
	{
		if (expression.TokenList.Count > 1)
		{
			sideType = AIScriptTokenArgType.NONE;
			return false;
		}
		if (!(expression.TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken) || Array.IndexOf(LegalTokenSideType, aIScriptArgumentToken.ArgumentType) < 0)
		{
			sideType = AIScriptTokenArgType.NONE;
			return false;
		}
		sideType = aIScriptArgumentToken.ArgumentType;
		return true;
	}

	public static bool TryCreateSelectType(AIPolishConvertedExpression expression, AIScriptTokenArgType[] legalSelectTypes, out AIScriptTokenArgType selectType)
	{
		if (expression.TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken && Array.IndexOf(legalSelectTypes, aIScriptArgumentToken.ArgumentType) >= 0)
		{
			selectType = aIScriptArgumentToken.ArgumentType;
			return true;
		}
		selectType = AIScriptTokenArgType.ALL_SELECT;
		return false;
	}

	public static AIScriptTokenArgType GetDamageTypeFromExprList(AIPolishConvertedExpression expression, out bool isDamageTypeDefinedByMaster)
	{
		isDamageTypeDefinedByMaster = false;
		if (expression.TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken && Array.IndexOf(AIBarrierSimulationUtility.LegalDamageType, aIScriptArgumentToken.ArgumentType) >= 0)
		{
			isDamageTypeDefinedByMaster = true;
			return aIScriptArgumentToken.ArgumentType;
		}
		return AIScriptTokenArgType.ALL_DAMAGE;
	}

	public static List<AIScriptTokenArgType> InitializeStopTimingList(AIPolishConvertedExpression stopTimingExpresstion)
	{
		List<AIScriptTokenArgType> list = new List<AIScriptTokenArgType>();
		List<AIScriptTokenBase> tokenList = stopTimingExpresstion.TokenList;
		if (tokenList == null || tokenList.Count <= 0)
		{
			list.Add(AIScriptTokenArgType.NONE);
			return list;
		}
		if (tokenList.Count == 1)
		{
			list.Add(CreateSingleArgType(stopTimingExpresstion, AIBarrierSimulationUtility.LegalStopTimingList));
		}
		else
		{
			for (int i = 0; i < tokenList.Count; i++)
			{
				AIScriptTokenBase aIScriptTokenBase = tokenList[i];
				if ((!(aIScriptTokenBase is AIScriptOperatorSymbolToken) || aIScriptTokenBase.Type != AIScriptTokenType.OR) && aIScriptTokenBase is AIScriptArgumentToken { ArgumentType: var argumentType } && Array.IndexOf(AIBarrierSimulationUtility.LegalStopTimingList, argumentType) >= 0)
				{
					list.Add(argumentType);
				}
			}
		}
		if (list.Count <= 0)
		{
			list.Add(AIScriptTokenArgType.NONE);
		}
		return list;
	}

	public static List<AIPolishConvertedExpression> GetFilterExpressionList(List<AIPolishConvertedExpression> exprList, int nonFilterFirstOffset)
	{
		int num = exprList.Count - nonFilterFirstOffset;
		if (num > 0)
		{
			return exprList.GetRange(0, num);
		}
		return null;
	}

	public static CantAttackType CreateBanAttackType(AIPolishConvertedExpression expression)
	{
		if (expression.TokenList == null || expression.TokenList.Count <= 0)
		{
			return CantAttackType.Null;
		}
		if (!(expression.TokenList[0] is AIScriptArgumentToken { ArgumentType: var argumentType } aIScriptArgumentToken))
		{
			return CantAttackType.Null;
		}
		switch (argumentType)
		{
		case AIScriptTokenArgType.ALL:
			return CantAttackType.All;
		case AIScriptTokenArgType.FOLLOWER:
			return CantAttackType.Unit;
		case AIScriptTokenArgType.CLASS:
			return CantAttackType.Class;
		case AIScriptTokenArgType.GUARD:
			if (aIScriptArgumentToken.IsNot)
			{
				return CantAttackType.NotHasGuard;
			}
			return CantAttackType.Null;
		default:
			return CantAttackType.Null;
		}
	}
}

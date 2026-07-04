using System;

namespace Wizard;

public static class AIScriptParser
{
	public static int ParseInt(string num)
	{
		if (num == "")
		{
			return 0;
		}
		try
		{
			return int.Parse(num);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static AIScriptTokenBase ConvertWordToToken(string word)
	{
		if (float.TryParse(word, out var result))
		{
			return new AIScriptNumericToken(result);
		}
		switch (word)
		{
		case "+":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.PLUS);
		case "-":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.MINUS);
		case "*":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.MULTI);
		case "/":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.DIV);
		case "%":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.REMAIN);
		case "(":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.LEFT_BLACKET);
		case ")":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.RIGHT_BLACKET);
		case ",":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.COMMA);
		case ">":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.MORE_THAN);
		case ">=":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.MORE_EQUAL);
		case "<":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.LESS_THAN);
		case "<=":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.LESS_EQUAL);
		case "==":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.EQUAL);
		case "max":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.MAX);
		case "min":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.MIN);
		case "|":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.OR);
		case "&":
			return new AIScriptOperatorSymbolToken(AIScriptTokenType.AND);
		default:
		{
			bool isNot = false;
			if (word.StartsWith("!"))
			{
				isNot = true;
				word = word.Substring(1);
			}
			return word switch
			{
				"NOW_TURN" => new AIScriptVariableToken(AIScriptTokenVariableType.NOW_TURN), 
				"COUNTDOWN" => new AIScriptVariableToken(AIScriptTokenVariableType.COUNTDOWN), 
				"REINCARNATION_MAX" => new AIScriptVariableToken(AIScriptTokenVariableType.REINCARNATION_MAX), 
				"SPELLBOOST" => new AIScriptVariableToken(AIScriptTokenVariableType.SPELLBOOST), 
				"FIELD_SPACE" => new AIScriptVariableToken(AIScriptTokenVariableType.FIELD_SPACE), 
				"ALLY_UNIT_MIN" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_UNIT_MIN), 
				"ALLY_UNIT_MAX" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_UNIT_MAX), 
				"ALLY_ATTACKABLE_ATK_MAX" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_ATTACKABLE_ATK_MAX), 
				"ALLY_NON_ATTACKABLE_ATK_MAX" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_NON_ATTACKABLE_ATK_MAX), 
				"ALLY_MAX_ATTACKABLE_LIFE" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_MAX_ATTACKABLE_LIFE), 
				"ENEMY_UNIT_MIN" => new AIScriptVariableToken(AIScriptTokenVariableType.ENEMY_UNIT_MIN), 
				"ENEMY_UNIT_MAX" => new AIScriptVariableToken(AIScriptTokenVariableType.ENEMY_UNIT_MAX), 
				"HAND_COUNT_E" => new AIScriptVariableToken(AIScriptTokenVariableType.HAND_COUNT_E), 
				"DECK_COUNT_E" => new AIScriptVariableToken(AIScriptTokenVariableType.DECK_COUNT_E), 
				"PLAYPTN_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.PLAYPTN_COUNT), 
				"MEMBER_ATK_SUM" => new AIScriptVariableToken(AIScriptTokenVariableType.MEMBER_ATK_SUM), 
				"MEMBER_MAX_LIFE" => new AIScriptVariableToken(AIScriptTokenVariableType.MEMBER_MAX_LIFE), 
				"MEMBER_LIFE_SUM" => new AIScriptVariableToken(AIScriptTokenVariableType.MEMBER_LIFE_SUM), 
				"INPLAY_ATTACK_SUM_TO_LEADER" => new AIScriptVariableToken(AIScriptTokenVariableType.INPLAY_ATTACK_SUM_TO_LEADER), 
				"SUMMON_DRUNKEN_ATK_MAX" => new AIScriptVariableToken(AIScriptTokenVariableType.SUMMON_DRUNKEN_ATK_MAX), 
				"ATTACK_TARGET_ATK_MAX" => new AIScriptVariableToken(AIScriptTokenVariableType.ATTACK_TARGET_ATK_MAX), 
				"ENEMY_MIN_ATK" => new AIScriptVariableToken(AIScriptTokenVariableType.ENEMY_MIN_ATK), 
				"ENEMY_MAX_ATK" => new AIScriptVariableToken(AIScriptTokenVariableType.ENEMY_MAX_ATK), 
				"ENEMY_ATK_SUM" => new AIScriptVariableToken(AIScriptTokenVariableType.ENEMY_ATK_SUM), 
				"ALLY_INPLAY_MAX_ATK" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_INPLAY_MAX_ATK), 
				"GRAVE_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.GRAVE_COUNT), 
				"ALLY_MAX_PP" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_MAX_PP), 
				"ENEMY_MAX_PP" => new AIScriptVariableToken(AIScriptTokenVariableType.ENEMY_MAX_PP), 
				"REST_PP" => new AIScriptVariableToken(AIScriptTokenVariableType.REST_PP), 
				"NOW_REST_PP" => new AIScriptVariableToken(AIScriptTokenVariableType.NOW_REST_PP), 
				"ALLY_EP" => new AIScriptVariableToken(AIScriptTokenVariableType.ALLY_EP), 
				"ENEMY_EP" => new AIScriptVariableToken(AIScriptTokenVariableType.ENEMY_EP), 
				"OWN_ATK" => new AIScriptVariableToken(AIScriptTokenVariableType.OWN_ATK), 
				"OWN_LIFE" => new AIScriptVariableToken(AIScriptTokenVariableType.OWN_LIFE), 
				"OWN_COST" => new AIScriptVariableToken(AIScriptTokenVariableType.OWN_COST), 
				"OWN_BASE_ATK" => new AIScriptVariableToken(AIScriptTokenVariableType.OWN_BASE_ATK), 
				"OWN_BASE_LIFE" => new AIScriptVariableToken(AIScriptTokenVariableType.OWN_BASE_LIFE), 
				"KILLER_ATTACK_VALUE" => new AIScriptVariableToken(AIScriptTokenVariableType.KILLER_ATTACK_VALUE), 
				"ENHANCE_COST" => new AIScriptVariableToken(AIScriptTokenVariableType.ENHANCE_COST), 
				"PLAY_ACTOR_ENHANCE_COST" => new AIScriptVariableToken(AIScriptTokenVariableType.PLAY_ACTOR_ENHANCE_COST), 
				"ACCELERATE_COST" => new AIScriptVariableToken(AIScriptTokenVariableType.ACCELERATE_COST), 
				"CHOICE_TRANSFORM_COST" => new AIScriptVariableToken(AIScriptTokenVariableType.CHOICE_TRANSFORM_COST), 
				"BASE_SKILL_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.BASE_SKILL_COUNT), 
				"LAST_LIFE" => new AIScriptVariableToken(AIScriptTokenVariableType.LAST_LIFE), 
				"MAX_ATTACKABLE_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.MAX_ATTACKABLE_COUNT), 
				"HAND_MIN_COST" => new AIScriptVariableToken(AIScriptTokenVariableType.HAND_MIN_COST), 
				"HAND_MAX_COST" => new AIScriptVariableToken(AIScriptTokenVariableType.HAND_MAX_COST), 
				"LEADER_DEF_LIFE" => new AIScriptVariableToken(AIScriptTokenVariableType.LEADER_DEF_LIFE), 
				"LEADER_CURRENT_LIFE" => new AIScriptVariableToken(AIScriptTokenVariableType.LEADER_CURRENT_LIFE), 
				"UNION_BURST_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.UNION_BURST_COUNT), 
				"CONSUME_EP" => new AIScriptVariableToken(AIScriptTokenVariableType.CONSUME_EP), 
				"LAST_HEAL_AMOUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.LAST_HEAL_AMOUNT), 
				"JUST_BEFORE_TURN_DAMAGE" => new AIScriptVariableToken(AIScriptTokenVariableType.JUST_BEFORE_TURN_DAMAGE), 
				"RALLY_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.RALLY_COUNT), 
				"SKYBOUND_ART_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.SKYBOUND_ART_COUNT), 
				"SUPER_SKYBOUND_ART_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.SUPER_SKYBOUND_ART_COUNT), 
				"USED_EP_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.USED_EP_COUNT), 
				"USED_PP_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.USED_PP_COUNT), 
				"EARTH_RITE_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.EARTH_RITE_COUNT), 
				"NECROMANCE_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.NECROMANCE_COUNT), 
				"USED_STACK_COUNT" => new AIScriptVariableToken(AIScriptTokenVariableType.USED_STACK_COUNT), 
				"DEFAULT_DAMAGE" => new AIScriptVariableToken(AIScriptTokenVariableType.DEFAULT_DAMAGE), 
				"NEXT_PLAY_PRIORITY" => new AIScriptVariableToken(AIScriptTokenVariableType.NEXT_PLAY_PRIORITY), 
				"NECROMANCED_COUNT_IN_GAME" => new AIScriptVariableToken(AIScriptTokenVariableType.NECROMANCED_COUNT_IN_GAME), 
				"IS_EVO_TURN" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_EVO_TURN), 
				"IS_ABLE_EVO" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ABLE_EVO), 
				"IS_F_ADV" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_F_ADV), 
				"IS_F_DISADV" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_F_DISADV), 
				"IS_AWAKE" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_AWAKE), 
				"IS_BERSERK" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_BERSERK), 
				"IS_TO_BE_BERSERK" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_TO_BE_BERSERK), 
				"IS_ON_FIELD" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ON_FIELD), 
				"IS_IN_HAND" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_IN_HAND), 
				"IS_IN_PLAYPTN" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_IN_PLAYPTN), 
				"IS_OWNER_TURN" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_OWNER_TURN), 
				"IS_ALLY" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ALLY), 
				"IS_ENEMY" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ENEMY), 
				"IS_ALLY_FIRST" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ALLY_FIRST), 
				"IS_SNEAK" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_SNEAK), 
				"IS_EVOLVED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_EVOLVED), 
				"IS_KILLER" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_KILLER), 
				"IS_MEDUSA" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_MEDUSA), 
				"IS_GUARD" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_GUARD), 
				"IS_RUSH" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_RUSH), 
				"IS_PLAYOUT_ATTACKER" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_PLAYOUT_ATTACKER), 
				"IS_ENHANCED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ENHANCED), 
				"IS_ACCELERATE" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ACCELERATE), 
				"IS_CRYSTALIZE" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_CRYSTALIZE), 
				"IS_ATTACK_LEADER" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ATTACK_LEADER), 
				"IS_FANFARE" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_FANFARE), 
				"IS_SKILL_REMOVED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_SKILL_REMOVED), 
				"IS_BARBAROSSA" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_BARBAROSSA), 
				"IS_RESONANCE" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_RESONANCE), 
				"IS_LETHAL" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_LETHAL), 
				"IS_USED_EVO" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_USED_EVO), 
				"IS_FIRST_TURN" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_FIRST_TURN), 
				"IS_DAMAGED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_DAMAGED), 
				"IS_DELAY_HEAL" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_DELAY_HEAL), 
				"IS_NOT_ATTACK_YET" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_NOT_ATTACK_YET), 
				"IS_EVOLVING" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_EVOLVING), 
				"IS_LEADER" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_LEADER), 
				"IS_GET_ON" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_GET_ON), 
				"IS_SKILL_SUMMONED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_SKILL_SUMMONED), 
				"IS_UNTOUCHABLE" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_UNTOUCHABLE), 
				"IS_FORCE_TARGETING" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_FORCE_TARGETING), 
				"IS_QUICK" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_QUICK), 
				"IS_NOT_BE_ATTACKED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_NOT_BE_ATTACKED), 
				"IS_IN_SIMULATION" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_IN_SIMULATION), 
				"IS_IGNORE_GUARD" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_IGNORE_GUARD), 
				"IS_ATTACKED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ATTACKED), 
				"IS_ONEMORELASTWORD_TAGGED" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_ONEMORELASTWORD_TAGGED), 
				"IS_DRAIN" => new AIScriptVariableToken(AIScriptTokenVariableType.IS_DRAIN), 
				"MEMBER_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.MEMBER_COUNT, 1), 
				"INPLAY_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.INPLAY_COUNT, 1), 
				"PLAYOUT_ATTACKER_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.PLAYOUT_ATTACKER_COUNT, 1), 
				"BEFORE_PLAYPTN_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.BEFORE_PLAYPTN_COUNT, 1), 
				"PLAY_TOKEN_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.PLAY_TOKEN_COUNT, 1), 
				"EVO_COUNT_IN_GAME" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVO_COUNT_IN_GAME, 1), 
				"EVO_COUNT_IN_PREVIOUS_TURN" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVO_COUNT_IN_PREVIOUS_TURN, 1), 
				"HAND_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.HAND_COUNT, 1), 
				"HAND_NAME_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.HAND_NAME_COUNT, 0), 
				"SKILL_COUNT_FROM_ID" => new AIScriptFunctionToken(AIScriptTokenFuncType.SKILL_COUNT_FROM_ID, 2), 
				"TAG_COUNT_FROM_ID" => new AIScriptFunctionToken(AIScriptTokenFuncType.TAG_COUNT_FROM_ID, 2), 
				"DECK_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.DECK_COUNT, 1), 
				"EVAL_RANDOM_MULTI_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_MULTI_DAMAGE, 2), 
				"EVAL_RANDOM_MULTI_SELECT_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_MULTI_SELECT_DAMAGE, 3), 
				"EVAL_RANDOM_MULTI_DAMAGE_MAX" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_MULTI_DAMAGE_MAX, 0), 
				"EVAL_INSTANT_ATTACK" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_INSTANT_ATTACK, 3), 
				"EVAL_RUSH" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RUSH, 3), 
				"EVAL_TARGETING_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_DAMAGE, 2), 
				"EVAL_TARGETING_AND_RANDOM_MULTI_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_AND_RANDOM_MULTI_DAMAGE, 0), 
				"EVAL_TARGETING_DESTROY" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_DESTROY, 4), 
				"EVAL_TARGETING_OTHER_DESTROY" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_OTHER_DESTROY, 4), 
				"EVAL_TARGETING_MULTI_DESTROY" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_MULTI_DESTROY, 0), 
				"EVAL_TARGETING_BOUNCE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_BOUNCE, 4), 
				"EVAL_TARGETING_BANISH" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_BANISH, 4), 
				"EVAL_TARGETING_HEAL" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_HEAL, 0), 
				"EVAL_TARGETING_METAMORPHOSE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_METAMORPHOSE, 0), 
				"EVAL_TARGETING_BUFF" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_TARGETING_BUFF, 0), 
				"EVAL_ECHO_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ECHO_DAMAGE, 2), 
				"EVAL_ALL_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_DAMAGE, 2), 
				"EVAL_ALL_MULTI_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_MULTI_DAMAGE, 3), 
				"EVAL_ALL_DESTROY" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_DESTROY, 4), 
				"EVAL_ALL_BOUNCE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_BOUNCE, 4), 
				"EVAL_ALL_METAMORPHOSE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_METAMORPHOSE, 0), 
				"EVAL_ALL_BUFF" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_BUFF, 0), 
				"EVAL_RANDOM_BUFF" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_BUFF, 0), 
				"EVAL_ALL_HEAL" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_HEAL, 0), 
				"EVAL_ALL_BANISH" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ALL_BANISH, 0), 
				"EVAL_RANDOM_DESTROY" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_DESTROY, 3), 
				"EVAL_RANDOM_BOUNCE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_BOUNCE, 1), 
				"EVAL_RANDOM_BANISH" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_BANISH, 0), 
				"EVAL_RANDOM_METAMORPHOSE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_RANDOM_METAMORPHOSE, 0), 
				"EVAL_LEADER_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_LEADER_DAMAGE, 2), 
				"EVAL_LEADER_HEAL" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_LEADER_HEAL, 2), 
				"EVAL_REANIMATE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_REANIMATE, 1), 
				"EVAL_DIVIDED_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_DIVIDED_DAMAGE, 1), 
				"FORCED_EXCHANGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.FORCED_EXCHANGE, 1), 
				"LIFE" => new AIScriptFunctionToken(AIScriptTokenFuncType.LIFE, 1), 
				"ATTACK" => new AIScriptFunctionToken(AIScriptTokenFuncType.ATTACK, 1), 
				"POW" => new AIScriptFunctionToken(AIScriptTokenFuncType.POW, 2), 
				"CEILING" => new AIScriptFunctionToken(AIScriptTokenFuncType.CEILING, 1), 
				"FLOOR" => new AIScriptFunctionToken(AIScriptTokenFuncType.FLOOR, 1), 
				"EVAL_COUNTDOWN" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_CHANT_COUNT_CHANGE, 1), 
				"EVAL_ALL_COUNTDOWN" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_CHANT_COUNT_CHANGE_ALL, 1), 
				"IS_CLASH_TARGET" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_CLASH_TARGET, 1), 
				"IS_ATTACK_TARGET" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_ATTACK_TARGET, 1), 
				"IS_SELECTED_TARGET" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_SELECTED_TARGET, 1), 
				"IS_NEXT_PLAY" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_NEXT_PLAY, 1), 
				"DECK_NAME_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.DECK_NAME_COUNT, 1), 
				"BROKEN_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.BROKEN_COUNT, 1), 
				"BROKEN_NAME_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.BROKEN_NAME_COUNT, 1), 
				"BROKEN_COST_SUM" => new AIScriptFunctionToken(AIScriptTokenFuncType.BROKEN_COST_SUM, 1), 
				"IS_TRIBE" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_TRIBE, 1), 
				"PLAYED_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.PLAYED_COUNT, 1), 
				"PLAYED_COUNT_IN_GAME" => new AIScriptFunctionToken(AIScriptTokenFuncType.PLAYED_COUNT_IN_GAME, 1), 
				"PLAYED_COUNT_IN_PREVIOUS_TURN" => new AIScriptFunctionToken(AIScriptTokenFuncType.PLAYED_COUNT_IN_PREVIOUS_TURN, 1), 
				"PLAYED_NAME_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.PLAYED_NAME_COUNT, 1), 
				"RANDOM" => new AIScriptFunctionToken(AIScriptTokenFuncType.RANDOM, 1), 
				"IS_REANIMATE" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_REANIMATE, 1), 
				"DAMAGE_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.DAMAGE_COUNT, 1), 
				"HEAL_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.HEAL_COUNT, 1), 
				"TOTAL_DAMAGE" => new AIScriptFunctionToken(AIScriptTokenFuncType.TOTAL_DAMAGE, 0), 
				"FUSION_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.FUSION_COUNT, 0), 
				"NOW_FUSION_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.NOW_FUSION_COUNT, 0), 
				"FUSION_COUNT_AT_ONCE" => new AIScriptFunctionToken(AIScriptTokenFuncType.FUSION_COUNT_AT_ONCE, 0), 
				"FUSION_NAME_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.FUSION_NAME_COUNT, 0), 
				"IS_SKILL_OCCURRED" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_SKILL_OCCURRED, 0), 
				"SKILL_ACTIVATE_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.SKILL_ACTIVATE_COUNT, 0), 
				"HAND_MAX_ATTACK" => new AIScriptFunctionToken(AIScriptTokenFuncType.HAND_MAX_ATTACK, 0), 
				"DECK_MAX_COST" => new AIScriptFunctionToken(AIScriptTokenFuncType.DECK_MAX_COST, 0), 
				"INPLAY_LARGEST_LIFE" => new AIScriptFunctionToken(AIScriptTokenFuncType.INPLAY_LARGEST_LIFE, 0), 
				"LEADER_MAX_LIFE" => new AIScriptFunctionToken(AIScriptTokenFuncType.LEADER_MAX_LIFE, 0), 
				"IS_DISCARD_TARGET" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_DISCARD_TARGET, 0), 
				"DISCARD_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.DISCARD_COUNT, 0), 
				"BOUNCE_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.BOUNCE_COUNT, 0), 
				"EMOTE_PLAY_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.EMOTE_PLAY_COUNT, 0), 
				"DRAW_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.DRAW_COUNT, 0), 
				"MEMBER_MAX_ATK" => new AIScriptFunctionToken(AIScriptTokenFuncType.MEMBER_MAX_ATK, 0), 
				"LEAVE_NAME_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.LEAVE_NAME_COUNT, 1), 
				"DESTROYED_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.DESTROYED_COUNT, 1), 
				"IS_BURIAL_RITE" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_BURIAL_RITE, 0), 
				"BURIAL_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.BURIAL_COUNT, 1), 
				"RESONANCE_START_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.RESONANCE_START_COUNT, 2), 
				"SUMMON_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.SUMMON_COUNT, 1), 
				"BUFF_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.BUFF_COUNT, 0), 
				"IS_HOLDING_BATTLE_SKILL" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_HOLDING_BATTLE_SKILL, 1), 
				"IS_LEADER_HOLDING_BATTLE_SKILL" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_LEADER_HOLDING_BATTLE_SKILL, 1), 
				"STACK_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.STACK_COUNT, 0), 
				"LEAVE_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.LEAVE_COUNT, 0), 
				"OWN_DESTROY_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.OWN_DESTROY_COUNT, 0), 
				"IS_SELECTABLE" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_SELECTABLE, 0), 
				"IS_ENEMY_AI_ID" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_ENEMY_AI_ID, 0), 
				"ADD_HAND_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.ADD_HAND_COUNT, 0), 
				"BASE_COST" => new AIScriptFunctionToken(AIScriptTokenFuncType.BASE_COST, 0), 
				"RECEIVED_DAMAGE_SUM" => new AIScriptFunctionToken(AIScriptTokenFuncType.RECEIVED_DAMAGE_SUM, 0), 
				"IS_PLAYER_CHARA_ID" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_PLAYER_CHARA_ID, 0), 
				"FUSION_COUNT_IN_GAME" => new AIScriptFunctionToken(AIScriptTokenFuncType.FUSION_COUNT_IN_GAME, 0), 
				"FUSION_NAME_COUNT_IN_GAME" => new AIScriptFunctionToken(AIScriptTokenFuncType.FUSION_NAME_COUNT_IN_GAME, 0), 
				"IS_PLAYER_ABILITY_ID" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_PLAYER_ABILITY_ID, 0), 
				"BANISH_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.BANISH_COUNT, 0), 
				"HAND_BANISH_COUNT" => new AIScriptFunctionToken(AIScriptTokenFuncType.HAND_BANISH_COUNT, 0), 
				"IS_BOTH_CLASS" => new AIScriptFunctionToken(AIScriptTokenFuncType.IS_BOTH_CLASS, 0), 
				"EVAL_ATTACK_REMOVE" => new AIScriptFunctionToken(AIScriptTokenFuncType.EVAL_ATTACK_REMOVE, 0), 
				"ADDED_DECK_COUNT_IN_GAME" => new AIScriptFunctionToken(AIScriptTokenFuncType.ADDED_DECK_COUNT_IN_GAME, 0), 
				"NONE" => new AIScriptArgumentToken(AIScriptTokenArgType.NONE, isNot), 
				"LEGION" => new AIScriptArgumentToken(AIScriptTokenArgType.LEGION, isNot), 
				"LORD" => new AIScriptArgumentToken(AIScriptTokenArgType.LORD, isNot), 
				"WHITE_RITUAL" => new AIScriptArgumentToken(AIScriptTokenArgType.WHITE_RITUAL, isNot), 
				"ARTIFACT" => new AIScriptArgumentToken(AIScriptTokenArgType.ARTIFACT, isNot), 
				"MANARIA" => new AIScriptArgumentToken(AIScriptTokenArgType.MANARIA, isNot), 
				"MACHINE" => new AIScriptArgumentToken(AIScriptTokenArgType.MACHINE, isNot), 
				"FOOD" => new AIScriptArgumentToken(AIScriptTokenArgType.FOOD, isNot), 
				"LEVIN" => new AIScriptArgumentToken(AIScriptTokenArgType.LEVIN, isNot), 
				"NATURE" => new AIScriptArgumentToken(AIScriptTokenArgType.NATURE, isNot), 
				"BANQUET" => new AIScriptArgumentToken(AIScriptTokenArgType.BANQUET, isNot), 
				"HERO" => new AIScriptArgumentToken(AIScriptTokenArgType.HERO, isNot), 
				"ARMED" => new AIScriptArgumentToken(AIScriptTokenArgType.ARMED, isNot), 
				"LOOT" => new AIScriptArgumentToken(AIScriptTokenArgType.LOOT, isNot), 
				"HELLBOUND" => new AIScriptArgumentToken(AIScriptTokenArgType.HELLBOUND, isNot), 
				"SCHOOL" => new AIScriptArgumentToken(AIScriptTokenArgType.SCHOOL, isNot), 
				"CHESS" => new AIScriptArgumentToken(AIScriptTokenArgType.CHESS, isNot), 
				"ANY_TRIBE" => new AIScriptArgumentToken(AIScriptTokenArgType.ANY_TRIBE, isNot), 
				"ALL" => new AIScriptArgumentToken(AIScriptTokenArgType.ALL, isNot), 
				"SPELL" => new AIScriptArgumentToken(AIScriptTokenArgType.SPELL, isNot), 
				"SPELL_CARD_TYPE" => new AIScriptArgumentToken(AIScriptTokenArgType.SPELL_CARD_TYPE, isNot), 
				"ALL_SPELLBOOST" => new AIScriptArgumentToken(AIScriptTokenArgType.ALL_SPELLBOOST, isNot), 
				"FIELD" => new AIScriptArgumentToken(AIScriptTokenArgType.FIELD, isNot), 
				"CHANT_FIELD" => new AIScriptArgumentToken(AIScriptTokenArgType.CHANT_FIELD, isNot), 
				"CLASS" => new AIScriptArgumentToken(AIScriptTokenArgType.CLASS, isNot), 
				"ALLY_CLASS" => new AIScriptArgumentToken(AIScriptTokenArgType.ALLY_CLASS, isNot), 
				"ENEMY_CLASS" => new AIScriptArgumentToken(AIScriptTokenArgType.ENEMY_CLASS, isNot), 
				"ACCELERATE" => new AIScriptArgumentToken(AIScriptTokenArgType.ACCELERATE, isNot), 
				"CRYSTALIZE" => new AIScriptArgumentToken(AIScriptTokenArgType.CRYSTALIZE, isNot), 
				"LASTWORD" => new AIScriptArgumentToken(AIScriptTokenArgType.LASTWORD, isNot), 
				"KILLER" => new AIScriptArgumentToken(AIScriptTokenArgType.KILLER, isNot), 
				"SNEAK" => new AIScriptArgumentToken(AIScriptTokenArgType.SNEAK, isNot), 
				"MEDUSA" => new AIScriptArgumentToken(AIScriptTokenArgType.MEDUSA, isNot), 
				"QUICK" => new AIScriptArgumentToken(AIScriptTokenArgType.QUICK, isNot), 
				"RUSH" => new AIScriptArgumentToken(AIScriptTokenArgType.RUSH, isNot), 
				"DRAIN" => new AIScriptArgumentToken(AIScriptTokenArgType.DRAIN, isNot), 
				"GUARD" => new AIScriptArgumentToken(AIScriptTokenArgType.GUARD, isNot), 
				"UNTOUCHABLE" => new AIScriptArgumentToken(AIScriptTokenArgType.UNTOUCHABLE, isNot), 
				"FORCE_TARGETING" => new AIScriptArgumentToken(AIScriptTokenArgType.FORCE_TARGETING, isNot), 
				"UNBANISHABLE" => new AIScriptArgumentToken(AIScriptTokenArgType.UNBANISHABLE, isNot), 
				"IGNORE_GUARD" => new AIScriptArgumentToken(AIScriptTokenArgType.IGNORE_GUARD, isNot), 
				"DAMAGE_CLIP" => new AIScriptArgumentToken(AIScriptTokenArgType.DAMAGE_CLIP, isNot), 
				"DAMAGE_CUT" => new AIScriptArgumentToken(AIScriptTokenArgType.DAMAGE_CUT, isNot), 
				"SKILL_DAMAGE" => new AIScriptArgumentToken(AIScriptTokenArgType.SKILL_DAMAGE, isNot), 
				"ALL_DAMAGE" => new AIScriptArgumentToken(AIScriptTokenArgType.ALL_DAMAGE, isNot), 
				"ATTACK_DAMAGE" => new AIScriptArgumentToken(AIScriptTokenArgType.ATTACK_DAMAGE, isNot), 
				"SPELL_DAMAGE" => new AIScriptArgumentToken(AIScriptTokenArgType.SPELL_DAMAGE, isNot), 
				"NOT_BE_ATTACKED" => new AIScriptArgumentToken(AIScriptTokenArgType.NOT_BE_ATTACKED, isNot), 
				"UNION_BURST" => new AIScriptArgumentToken(AIScriptTokenArgType.UNION_BURST, isNot), 
				"NO_SKILL" => new AIScriptArgumentToken(AIScriptTokenArgType.NO_SKILL, isNot), 
				"CRYSTALIZE_HOLDER" => new AIScriptArgumentToken(AIScriptTokenArgType.CRYSTALIZE_HOLDER, isNot), 
				"ELF" => new AIScriptArgumentToken(AIScriptTokenArgType.ELF, isNot), 
				"ROYAL" => new AIScriptArgumentToken(AIScriptTokenArgType.ROYAL, isNot), 
				"WITCH" => new AIScriptArgumentToken(AIScriptTokenArgType.WITCH, isNot), 
				"DRAGON" => new AIScriptArgumentToken(AIScriptTokenArgType.DRAGON, isNot), 
				"NECROMANCER" => new AIScriptArgumentToken(AIScriptTokenArgType.NECROMANCER, isNot), 
				"VAMPIRE" => new AIScriptArgumentToken(AIScriptTokenArgType.VAMPIRE, isNot), 
				"BISHOP" => new AIScriptArgumentToken(AIScriptTokenArgType.BISHOP, isNot), 
				"NEMESIS" => new AIScriptArgumentToken(AIScriptTokenArgType.NEMESIS, isNot), 
				"WHEN_PLAY" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_PLAY, isNot), 
				"WHEN_DESTROY" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_DESTROY, isNot), 
				"WHEN_ATTACK" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_ATTACK, isNot), 
				"WHEN_CLASH" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_CLASH, isNot), 
				"WHEN_EVO" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_EVO, isNot), 
				"WHEN_TURNEND" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_TURNEND, isNot), 
				"WHEN_ALLY_TURNEND" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_ALLY_TURNEND, isNot), 
				"WHEN_OPPONENT_TURNEND" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_OPPONENT_TURNEND, isNot), 
				"WHEN_ALLY_TURNSTART" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_ALLY_TURNSTART, isNot), 
				"WHEN_OPPONENT_TURNSTART" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_OPPONENT_TURNSTART, isNot), 
				"WHEN_NEXT_TURNEND" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_NEXT_TURNEND, isNot), 
				"WHEN_SUMMON" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_SUMMON, isNot), 
				"WHEN_HEAL" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_HEAL, isNot), 
				"WHEN_LEAVE" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_LEAVE, isNot), 
				"FIRST_TURN" => new AIScriptArgumentToken(AIScriptTokenArgType.FIRST_TURN, isNot), 
				"EVOLVED" => new AIScriptArgumentToken(AIScriptTokenArgType.EVOLVED, isNot), 
				"ATTACKABLE" => new AIScriptArgumentToken(AIScriptTokenArgType.ATTACKABLE, isNot), 
				"CANT_ATTACK" => new AIScriptArgumentToken(AIScriptTokenArgType.CANT_ATTACK, isNot), 
				"PREVIOUS_TURN_ATTACKED" => new AIScriptArgumentToken(AIScriptTokenArgType.PREVIOUS_TURN_ATTACKED, isNot), 
				"ATTACKED" => new AIScriptArgumentToken(AIScriptTokenArgType.ATTACKED, isNot), 
				"CONSUME_EP_ZERO" => new AIScriptArgumentToken(AIScriptTokenArgType.CONSUME_EP_ZERO, isNot), 
				"ALLY" => new AIScriptArgumentToken(AIScriptTokenArgType.ALLY, isNot), 
				"ENEMY" => new AIScriptArgumentToken(AIScriptTokenArgType.OPPONENT, isNot), 
				"BOTH" => new AIScriptArgumentToken(AIScriptTokenArgType.BOTH, isNot), 
				"SELECTED_TARGET_SIDE" => new AIScriptArgumentToken(AIScriptTokenArgType.SELECTED_TARGET_SIDE, isNot), 
				"RANDOM_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.RANDOM_SELECT, isNot), 
				"ALL_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.ALL_SELECT, isNot), 
				"TARGET_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.TARGET_SELECT, isNot), 
				"SECOND_TARGET_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.SECOND_TARGET_SELECT, isNot), 
				"RANDOM_MULTI_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.RANDOM_MULTI_SELECT, isNot), 
				"REVERSE_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.REVERSE_TARGET, isNot), 
				"FIRST_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.FIRST_SELECT, isNot), 
				"DIVIDED_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.DIVIDED_SELECT, isNot), 
				"OLDEST_SELECT" => new AIScriptArgumentToken(AIScriptTokenArgType.OLDEST_SELECT, isNot), 
				"FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.FOLLOWER, isNot), 
				"FOLLOWER_CARD_TYPE" => new AIScriptArgumentToken(AIScriptTokenArgType.FOLLOWER_CARD_TYPE, isNot), 
				"AMULET" => new AIScriptArgumentToken(AIScriptTokenArgType.AMULET, isNot), 
				"NOT_COUNTDOWN_AMULET" => new AIScriptArgumentToken(AIScriptTokenArgType.NOT_COUNTDOWN_AMULET, isNot), 
				"DAMAGED_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.DAMAGED_FOLLOWER, isNot), 
				"NO_DAMAGED_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.NO_DAMAGED_FOLLOWER, isNot), 
				"EVOLVED_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.EVOLVED_FOLLOWER, isNot), 
				"BUFFED_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.BUFFED_FOLLOWER, isNot), 
				"OTHER_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.OTHER_FOLLOWER, isNot), 
				"SUMMON_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.SUMMON_FOLLOWER, isNot), 
				"SUMMON_AMULET" => new AIScriptArgumentToken(AIScriptTokenArgType.SUMMON_AMULET, isNot), 
				"LATEST_SUMMON_CARD" => new AIScriptArgumentToken(AIScriptTokenArgType.LATEST_SUMMON_CARD, isNot), 
				"LATEST_DRAW_CARD" => new AIScriptArgumentToken(AIScriptTokenArgType.LATEST_DRAW_CARD, isNot), 
				"ROMELIA_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.ROMELIA_TARGET, isNot), 
				"MIN_ATTACK" => new AIScriptArgumentToken(AIScriptTokenArgType.MIN_ATTACK, isNot), 
				"MAX_ATTACK" => new AIScriptArgumentToken(AIScriptTokenArgType.MAX_ATTACK, isNot), 
				"MIN_COST" => new AIScriptArgumentToken(AIScriptTokenArgType.MIN_COST, isNot), 
				"MAX_COST" => new AIScriptArgumentToken(AIScriptTokenArgType.MAX_COST, isNot), 
				"ALLY_AMULET" => new AIScriptArgumentToken(AIScriptTokenArgType.ALLY_AMULET, isNot), 
				"LIFE_INF" => new AIScriptArgumentToken(AIScriptTokenArgType.LIFE_INF, isNot), 
				"ATK_INF" => new AIScriptArgumentToken(AIScriptTokenArgType.ATK_INF, isNot), 
				"COST_INF" => new AIScriptArgumentToken(AIScriptTokenArgType.COST_INF, isNot), 
				"LIFE_SUP" => new AIScriptArgumentToken(AIScriptTokenArgType.LIFE_SUP, isNot), 
				"ATK_EQL" => new AIScriptArgumentToken(AIScriptTokenArgType.ATK_EQL, isNot), 
				"LIFE_EQL" => new AIScriptArgumentToken(AIScriptTokenArgType.LIFE_EQL, isNot), 
				"ATK_SUP" => new AIScriptArgumentToken(AIScriptTokenArgType.ATK_SUP, isNot), 
				"COST_SUP" => new AIScriptArgumentToken(AIScriptTokenArgType.COST_SUP, isNot), 
				"COST_EQL" => new AIScriptArgumentToken(AIScriptTokenArgType.COST_EQL, isNot), 
				"BASE_COST_INF" => new AIScriptArgumentToken(AIScriptTokenArgType.BASE_COST_INF, isNot), 
				"BASE_COST_SUP" => new AIScriptArgumentToken(AIScriptTokenArgType.BASE_COST_SUP, isNot), 
				"BASE_COST_EQL" => new AIScriptArgumentToken(AIScriptTokenArgType.BASE_COST_EQL, isNot), 
				"COUNTDOWN_EQL" => new AIScriptArgumentToken(AIScriptTokenArgType.COUNTDOWN_EQL, isNot), 
				"PLAY_COUNT_EQL" => new AIScriptArgumentToken(AIScriptTokenArgType.PLAY_COUNT_EQL, isNot), 
				"CLASH_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.CLASH_TARGET, isNot), 
				"BANISHED_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.BANISHED_TARGET, isNot), 
				"GETOFF_CARD" => new AIScriptArgumentToken(AIScriptTokenArgType.GETOFF_CARD, isNot), 
				"ATTACKER" => new AIScriptArgumentToken(AIScriptTokenArgType.ATTACKER, isNot), 
				"EVOLVER" => new AIScriptArgumentToken(AIScriptTokenArgType.EVOLVER, isNot), 
				"PLAYED_CARD" => new AIScriptArgumentToken(AIScriptTokenArgType.PLAYED_CARD, isNot), 
				"TEMP" => new AIScriptArgumentToken(AIScriptTokenArgType.TEMP, isNot), 
				"PERM" => new AIScriptArgumentToken(AIScriptTokenArgType.PERM, isNot), 
				"TURN" => new AIScriptArgumentToken(AIScriptTokenArgType.TURN, isNot), 
				"GAME" => new AIScriptArgumentToken(AIScriptTokenArgType.GAME, isNot), 
				"DESTROY" => new AIScriptArgumentToken(AIScriptTokenArgType.DESTROY, isNot), 
				"BANISH" => new AIScriptArgumentToken(AIScriptTokenArgType.BANISH, isNot), 
				"TOKEN_DRAW" => new AIScriptArgumentToken(AIScriptTokenArgType.TOKEN_DRAW, isNot), 
				"SELF" => new AIScriptArgumentToken(AIScriptTokenArgType.SELF, isNot), 
				"NEUTRAL" => new AIScriptArgumentToken(AIScriptTokenArgType.NEUTRAL, isNot), 
				"WHEN_DAMAGED" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_DAMAGED, isNot), 
				"USE_MIN" => new AIScriptArgumentToken(AIScriptTokenArgType.USE_MIN, isNot), 
				"IGNORE_IN_BATTLE" => new AIScriptArgumentToken(AIScriptTokenArgType.IGNORE_IN_BATTLE, isNot), 
				"IGNORE_IN_FUSION" => new AIScriptArgumentToken(AIScriptTokenArgType.IGNORE_IN_FUSION, isNot), 
				"IN_HAND" => new AIScriptArgumentToken(AIScriptTokenArgType.IN_HAND, isNot), 
				"IN_PLAY" => new AIScriptArgumentToken(AIScriptTokenArgType.IN_PLAY, isNot), 
				"LAST_DRAW_CARD" => new AIScriptArgumentToken(AIScriptTokenArgType.LAST_DRAW_CARD, isNot), 
				"NEWER" => new AIScriptArgumentToken(AIScriptTokenArgType.NEWER, isNot), 
				"OLDEST_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.OLDEST_FOLLOWER, isNot), 
				"OTHER_OLDEST_HAND_CARD_TYPE" => new AIScriptArgumentToken(AIScriptTokenArgType.OTHER_OLDEST_HAND_CARD_TYPE, isNot), 
				"CHOICED_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.CHOICED_TARGET, isNot), 
				"SELECTED_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.SELECTED_TARGET, isNot), 
				"SECOND_SELECTED_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.SECOND_SELECTED_TARGET, isNot), 
				"PLAYED" => new AIScriptArgumentToken(AIScriptTokenArgType.PLAYED, isNot), 
				"PLAYPTN" => new AIScriptArgumentToken(AIScriptTokenArgType.PLAYPTN, isNot), 
				"BEFORE_PLAYPTN" => new AIScriptArgumentToken(AIScriptTokenArgType.BEFORE_PLAYPTN, isNot), 
				"NOW" => new AIScriptArgumentToken(AIScriptTokenArgType.NOW, isNot), 
				"DESTRUCTIBLE" => new AIScriptArgumentToken(AIScriptTokenArgType.DESTRUCTIBLE, isNot), 
				"TRIGGER" => new AIScriptArgumentToken(AIScriptTokenArgType.TRIGGER, isNot), 
				"REAL_SKILL_TARGET" => new AIScriptArgumentToken(AIScriptTokenArgType.REAL_SKILL_TARGET, isNot), 
				"CANDIDATE" => new AIScriptArgumentToken(AIScriptTokenArgType.CANDIDATE, isNot), 
				"ENHANCED" => new AIScriptArgumentToken(AIScriptTokenArgType.ENHANCED, isNot), 
				"LEAST_VALUE" => new AIScriptArgumentToken(AIScriptTokenArgType.LEAST_VALUE, isNot), 
				"ALLY_ATTACK_FOLLOWER" => new AIScriptArgumentToken(AIScriptTokenArgType.ALLY_ATTACK_FOLLOWER, isNot), 
				"ADD" => new AIScriptArgumentToken(AIScriptTokenArgType.ADD, isNot), 
				"SET" => new AIScriptArgumentToken(AIScriptTokenArgType.SET, isNot), 
				"SELECTED_TARGET_ID" => new AIScriptArgumentToken(AIScriptTokenArgType.SELECTED_TARGET_ID, isNot), 
				"DEFAULT_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.DEFAULT_LOGIC, isNot), 
				"DESTROY_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.DESTROY_LOGIC, isNot), 
				"DAMAGE_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.DAMAGE_LOGIC, isNot), 
				"BANISH_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.BANISH_LOGIC, isNot), 
				"BOUNCE_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.BOUNCE_LOGIC, isNot), 
				"METAMORPHOSE_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.METAMORPHOSE_LOGIC, isNot), 
				"MAX_ATTACK_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.MAX_ATTACK_LOGIC, isNot), 
				"TYRANT_ORDER_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.TYRANT_ORDER_LOGIC, isNot), 
				"REVERSE_DISCARD_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.REVERSE_DISCARD_LOGIC, isNot), 
				"WHITEFROST_WHISPER_LOGIC" => new AIScriptArgumentToken(AIScriptTokenArgType.WHITEFROST_WHISPER_LOGIC, isNot), 
				"FILTER_END" => new AIScriptArgumentToken(AIScriptTokenArgType.FILTER_END, isNot), 
				"NEXT_PLAY" => new AIScriptArgumentToken(AIScriptTokenArgType.NEXT_PLAY, isNot), 
				"WHEN_PLAY_DAMAGE" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_PLAY_DAMAGE, isNot), 
				"WHEN_PLAY_DESTROY" => new AIScriptArgumentToken(AIScriptTokenArgType.WHEN_PLAY_DESTROY, isNot), 
				"FIRST_SUMMON_FOLLOWER_IN_PLAYPTN" => new AIScriptArgumentToken(AIScriptTokenArgType.FIRST_SUMMON_FOLLOWER_IN_PLAYPTN, isNot), 
				"DESTROYED_CARD" => new AIScriptArgumentToken(AIScriptTokenArgType.DESTROYED_CARD, isNot), 
				"DESTROYED_IN_CURRENT_TURN" => new AIScriptArgumentToken(AIScriptTokenArgType.DESTROYED_IN_CURRENT_TURN, isNot), 
				_ => ConvertWordWithSpecialMarkToToken(word, isNot), 
			};
		}
		}
	}

	public static AIScriptTokenBase ConvertWordWithSpecialMarkToToken(string word, bool isNot)
	{
		char c = word[0];
		string text = word.Substring(1);
		switch (c)
		{
		case '@':
		{
			int result = 0;
			if (int.TryParse(text, out result))
			{
				return new AIScriptIDToken(result, isNot);
			}
			break;
		}
		case '#':
			return new AIScriptTextToken(text, isNot);
		}
		return null;
	}

	public static AIPlayTagType ConvertWordToTagType(string word)
	{
		switch (word)
		{
		case "playBonus":
			return AIPlayTagType.PlayBonus;
		case "playBonusRate":
			return AIPlayTagType.PlayBonusRate;
		case "fanfareBonus":
			return AIPlayTagType.FanfareBonus;
		case "ignoreFanfareBonus":
			return AIPlayTagType.IgnoreFanfareBonus;
		case "ignoreBreak":
			return AIPlayTagType.IgnoreBreak;
		case "evoBonus":
			return AIPlayTagType.EvoBonus;
		case "memberEvoBonus":
			return AIPlayTagType.MemberEvoBonus;
		case "enemyEvoBonus":
			return AIPlayTagType.EnemyEvoBonus;
		case "battleBonus":
			return AIPlayTagType.BattleBonus;
		case "battleBonusRate":
			return AIPlayTagType.BattleBonusRate;
		case "clashBonus":
			return AIPlayTagType.ClashBonus;
		case "clashDestroy":
			return AIPlayTagType.ClashDestroy;
		case "clashHeal":
			return AIPlayTagType.ClashHeal;
		case "clashToken":
			return AIPlayTagType.ClashToken;
		case "clashBanish":
			return AIPlayTagType.ClashBanish;
		case "clashSpellboost":
			return AIPlayTagType.ClashSpellboost;
		case "afterClashHeal":
			return AIPlayTagType.AfterClashHeal;
		case "attackDamage":
			return AIPlayTagType.AttackDamage;
		case "attackBreakDamage":
			return AIPlayTagType.AttackBreakDamage;
		case "attackBreakEvo":
			return AIPlayTagType.AttackBreakEvo;
		case "attackBreakRecoverPp":
			return AIPlayTagType.AttackBreakRecoverPp;
		case "attackBreakAttackTwice":
			return AIPlayTagType.AttackBreakAttackTwice;
		case "attackHeal":
			return AIPlayTagType.AttackHeal;
		case "attackAttackableCount":
			return AIPlayTagType.AttackAttackableCount;
		case "attackQuick":
			return AIPlayTagType.AttackQuick;
		case "afterAttackHeal":
			return AIPlayTagType.AfterAttackHeal;
		case "attackBonus":
			return AIPlayTagType.AttackBonus;
		case "clashDamage":
			return AIPlayTagType.ClashDamage;
		case "afterClashDamage":
			return AIPlayTagType.AfterClashDamage;
		case "clashRemoveSkill":
			return AIPlayTagType.ClashRemoveSkill;
		case "attackRemoveSkill":
			return AIPlayTagType.AttackRemoveSkill;
		case "playptnBonus":
			return AIPlayTagType.PlayptnBonus;
		case "priority":
			return AIPlayTagType.Priority;
		case "playoutAtkB":
			return AIPlayTagType.PlayoutAttackBonus;
		case "playoutDamageB":
			return AIPlayTagType.PlayoutDamageBonus;
		case "allyPlayoutDamageBonus":
			return AIPlayTagType.AllyPlayoutDamageBonus;
		case "playoutNextTurn":
			return AIPlayTagType.PlayoutNextTurn;
		case "costBonus":
			return AIPlayTagType.CostBonus;
		case "playLimit":
			return AIPlayTagType.PlayLimit;
		case "mlgKeep":
			return AIPlayTagType.MulliganKeep;
		case "mlgChange":
			return AIPlayTagType.MulliganChange;
		case "handPlus":
			return AIPlayTagType.HandPlus;
		case "playPlus":
			return AIPlayTagType.PlayPlus;
		case "evoHandPlus":
			return AIPlayTagType.EvoHandPlus;
		case "noNormalEvo":
			return AIPlayTagType.NoNormalEvo;
		case "evoEvo":
			return AIPlayTagType.EvoEvo;
		case "evoDamage":
			return AIPlayTagType.EvoDamage;
		case "break":
			return AIPlayTagType.Break;
		case "otherBreakBonus":
			return AIPlayTagType.OtherBreakBonus;
		case "otherBanishBonus":
			return AIPlayTagType.OtherBanishBonus;
		case "otherLeaveBonus":
			return AIPlayTagType.OtherLeaveBonus;
		case "lastwordDestroy":
			return AIPlayTagType.LastwordDestroy;
		case "breakDamage":
			return AIPlayTagType.BreakDamage;
		case "breakRecoverAttackableCount":
			return AIPlayTagType.BreakRecoverAttackableCount;
		case "lastwordDamage":
			return AIPlayTagType.LastwordDamage;
		case "lastwordHeal":
			return AIPlayTagType.LastwordHeal;
		case "lastwordMetamorphose":
			return AIPlayTagType.LastwordMetamorphose;
		case "lastwordBanish":
			return AIPlayTagType.LastwordBanish;
		case "lastwordDraw":
			return AIPlayTagType.LastwordDraw;
		case "lastwordAddDeck":
			return AIPlayTagType.LastwordAddDeck;
		case "lastwordSetStatus":
			return AIPlayTagType.LastwordSetStatus;
		case "turnEndDestroy":
			return AIPlayTagType.TurnEndDestroy;
		case "turnEndBanish":
			return AIPlayTagType.TurnEndBanish;
		case "turnEndDamage":
			return AIPlayTagType.TurnEndDamage;
		case "turnEndHeal":
			return AIPlayTagType.TurnEndHeal;
		case "turnEndSetLeaderMaxLife":
			return AIPlayTagType.TurnEndSetLeaderMaxLife;
		case "turnEndDiscard":
			return AIPlayTagType.TurnEndDiscard;
		case "turnEndBuff":
			return AIPlayTagType.TurnEndBuff;
		case "turnEndSubtractCountdown":
			return AIPlayTagType.TurnEndSubtractCountdown;
		case "turnEndToken":
			return AIPlayTagType.TurnEndToken;
		case "turnEndBounce":
			return AIPlayTagType.TurnEndBounce;
		case "turnEndAddDeck":
			return AIPlayTagType.TurnEndAddDeck;
		case "turnEndEvo":
			return AIPlayTagType.TurnEndEvo;
		case "turnEndDraw":
			return AIPlayTagType.TurnEndDraw;
		case "turnEndShield":
			return AIPlayTagType.TurnEndShield;
		case "turnEndDamageClip":
			return AIPlayTagType.TurnEndDamageClip;
		case "turnEndDamageCut":
			return AIPlayTagType.TurnEndDamageCut;
		case "turnEndGuard":
			return AIPlayTagType.TurnEndGuard;
		case "turnEndBanAttack":
			return AIPlayTagType.TurnEndBanAttack;
		case "playSkip":
			return AIPlayTagType.PlaySkip;
		case "playSkipWithEvo":
			return AIPlayTagType.PlaySkipWithEvo;
		case "playSkipIfEvo":
			return AIPlayTagType.PlaySkipIfEvo;
		case "playSkipWithAction":
			return AIPlayTagType.PlaySkipWithAction;
		case "playSkipWithActionIfEvo":
			return AIPlayTagType.PlaySkipWithActionIfEvo;
		case "target":
			return AIPlayTagType.Target;
		case "ignoreTarget":
			return AIPlayTagType.IgnoreTarget;
		case "memBattleB":
			return AIPlayTagType.MemberBattleBonus;
		case "memberBattleBonusRate":
			return AIPlayTagType.MemberBattleBonusRate;
		case "enemyBattleB":
			return AIPlayTagType.EnemyBattleBonus;
		case "enemyBattleBonusRate":
			return AIPlayTagType.EnemyBattleBonusRate;
		case "allyPlayB":
			return AIPlayTagType.AllyPlayBonus;
		case "enemyPlayBonus":
			return AIPlayTagType.EnemyPlayBonus;
		case "reincarnation":
			return AIPlayTagType.ReincarnationSimulation;
		case "attackBuff":
			return AIPlayTagType.AttackBuff;
		case "attackHandBuff":
			return AIPlayTagType.AttackHandBuff;
		case "attackDestroy":
			return AIPlayTagType.AttackDestroy;
		case "attackBanish":
			return AIPlayTagType.AttackBanish;
		case "attackEvo":
			return AIPlayTagType.AttackEvo;
		case "afterAttackEvo":
			return AIPlayTagType.AfterAttackEvo;
		case "afterAttackDraw":
			return AIPlayTagType.AfterAttackDraw;
		case "afterAttackBanish":
			return AIPlayTagType.AfterAttackBanish;
		case "breakBuff":
			return AIPlayTagType.BreakBuff;
		case "breakSetLeaderMaxLife":
			return AIPlayTagType.BreakSetLeaderMaxLife;
		case "summonBuff":
			return AIPlayTagType.SummonBuff;
		case "summonEvo":
			return AIPlayTagType.SummonEvo;
		case "summonRush":
			return AIPlayTagType.SummonRush;
		case "summonQuick":
			return AIPlayTagType.SummonQuick;
		case "summonDamage":
			return AIPlayTagType.SummonDamage;
		case "summonBanish":
			return AIPlayTagType.SummonBanish;
		case "summonHeal":
			return AIPlayTagType.SummonHeal;
		case "summonDestroy":
			return AIPlayTagType.SummonDestroy;
		case "summonBanAttack":
			return AIPlayTagType.SummonBanAttack;
		case "otherSummonDamage":
			return AIPlayTagType.OtherSummonDamage;
		case "otherSummonRush":
			return AIPlayTagType.OtherSummonRush;
		case "otherSummonGuard":
			return AIPlayTagType.OtherSummonGuard;
		case "otherSummonQuick":
			return AIPlayTagType.OtherSummonQuick;
		case "otherSummonKiller":
			return AIPlayTagType.OtherSummonKiller;
		case "otherSummonDrain":
			return AIPlayTagType.OtherSummonDrain;
		case "otherSummonBanish":
			return AIPlayTagType.OtherSummonBanish;
		case "otherSummonHeal":
			return AIPlayTagType.OtherSummonHeal;
		case "otherSummonBuff":
			return AIPlayTagType.OtherSummonBuff;
		case "otherSummonEvo":
			return AIPlayTagType.OtherSummonEvo;
		case "otherSummonDamageCut":
			return AIPlayTagType.OtherSummonDamageCut;
		case "otherSummonDamageClip":
			return AIPlayTagType.OtherSummonDamageClip;
		case "otherSummonSubtractCountdown":
			return AIPlayTagType.OtherSummonSubtractCountdown;
		case "otherSummonAddCemetery":
			return AIPlayTagType.OtherSummonAddCemetery;
		case "otherSummonUntouchable":
			return AIPlayTagType.OtherSummonUntouchable;
		case "otherSummonDestroy":
			return AIPlayTagType.OtherSummonDestory;
		case "otherSummonDraw":
			return AIPlayTagType.OtherSummonDraw;
		case "attackByLife":
			return AIPlayTagType.AttackByLife;
		case "attackToken":
			return AIPlayTagType.AttackToken;
		case "puppetAttack":
			return AIPlayTagType.PuppetAttack;
		case "noSkipAttack":
			return AIPlayTagType.NoSkipAttack;
		case "evoToken":
			return AIPlayTagType.EvoToken;
		case "playDestroy":
			return AIPlayTagType.PlayDestroy;
		case "playDamage":
			return AIPlayTagType.PlayDamage;
		case "fanfareDamage":
			return AIPlayTagType.FanfareDamage;
		case "fanfareDestroy":
			return AIPlayTagType.FanfareDestroy;
		case "playMetamorphose":
			return AIPlayTagType.PlayMetamorphose;
		case "fanfareMetamorphose":
			return AIPlayTagType.FanfareMetamorphose;
		case "playHandMetamorphose":
			return AIPlayTagType.PlayHandMetamorphose;
		case "fanfareHandMetamorphose":
			return AIPlayTagType.FanfareHandMetamorphose;
		case "playHeal":
			return AIPlayTagType.PlayHeal;
		case "fanfareHeal":
			return AIPlayTagType.FanfareHeal;
		case "playBanish":
			return AIPlayTagType.PlayBanish;
		case "fanfareBanish":
			return AIPlayTagType.FanfareBanish;
		case "playBounce":
			return AIPlayTagType.PlayBounce;
		case "fanfareBounce":
			return AIPlayTagType.FanfareBounce;
		case "playSubtractCountdown":
			return AIPlayTagType.PlaySubtractCountdown;
		case "fanfareSubtractCountdown":
			return AIPlayTagType.FanfareSubtractCountdown;
		case "fanfareRecoverAttackableCount":
			return AIPlayTagType.FanfareRecoverAttackableCount;
		case "fanfareSneak":
			return AIPlayTagType.FanfareSneak;
		case "playSneak":
			return AIPlayTagType.PlaySneak;
		case "fanfareQuick":
			return AIPlayTagType.FanfareQuick;
		case "playQuick":
			return AIPlayTagType.PlayQuick;
		case "fanfareRush":
			return AIPlayTagType.FanfareRush;
		case "playRush":
			return AIPlayTagType.PlayRush;
		case "fanfareGuard":
			return AIPlayTagType.FanfareGuard;
		case "playGuard":
			return AIPlayTagType.PlayGuard;
		case "fanfareDrain":
			return AIPlayTagType.FanfareDrain;
		case "playDrain":
			return AIPlayTagType.PlayDrain;
		case "fanfareKiller":
			return AIPlayTagType.FanfareKiller;
		case "playKiller":
			return AIPlayTagType.PlayKiller;
		case "fanfareUntouchable":
			return AIPlayTagType.FanfareUntouchable;
		case "playUntouchable":
			return AIPlayTagType.PlayUntouchable;
		case "playBonusInSimulation":
			return AIPlayTagType.PlayBonusInSimulation;
		case "fanfareBonusInSimulation":
			return AIPlayTagType.FanfareBonusInSimulation;
		case "fanfareForceTargeting":
			return AIPlayTagType.FanfareForceTargeting;
		case "breakFirst":
			return AIPlayTagType.BreakFirst;
		case "breakLast":
			return AIPlayTagType.BreakLast;
		case "bounceBonus":
			return AIPlayTagType.BounceBonus;
		case "banishBonus":
			return AIPlayTagType.BanishBonus;
		case "breakBeforePlay":
			return AIPlayTagType.BreakBeforePlay;
		case "emoOnPlay":
			return AIPlayTagType.EmoteOnPlay;
		case "emoOnEvo":
			return AIPlayTagType.EmoteOnEvo;
		case "emoOnAtk":
			return AIPlayTagType.EmoteOnAtk;
		case "emoOnDestroy":
			return AIPlayTagType.EmoteOnDestroy;
		case "forceEmoOnDestroy":
			return AIPlayTagType.ForceEmoteOnDestroy;
		case "emoOnTurnEnd":
			return AIPlayTagType.EmoteOnTurnEnd;
		case "clashKiller":
			return AIPlayTagType.ClashKiller;
		case "playAttachTag":
			return AIPlayTagType.PlayAttachTag;
		case "fanfareAttachTag":
			return AIPlayTagType.FanfareAttachTag;
		case "playCopyTag":
			return AIPlayTagType.PlayCopyTag;
		case "fanfareCopyTag":
			return AIPlayTagType.FanfareCopyTag;
		case "attackAttachTag":
			return AIPlayTagType.AttackAttachTag;
		case "lastwordAttachTag":
			return AIPlayTagType.LastwordAttachTag;
		case "evoAttachTag":
			return AIPlayTagType.EvoAttachTag;
		case "summonAttachTag":
			return AIPlayTagType.SummonAttachTag;
		case "otherSummonAttachTag":
			return AIPlayTagType.OtherSummonAttachTag;
		case "breakAttachTag":
			return AIPlayTagType.BreakAttachTag;
		case "necromanceAttachTag":
			return AIPlayTagType.NecromanceAttachTag;
		case "turnEndAttachTag":
			return AIPlayTagType.TurnEndAttachTag;
		case "healAttachTag":
			return AIPlayTagType.HealAttachTag;
		case "fanfareAttachStyle":
			return AIPlayTagType.FanfareAttachStyle;
		case "fanfareToken":
			return AIPlayTagType.FanfareToken;
		case "lastwordToken":
			return AIPlayTagType.LastwordToken;
		case "playToken":
			return AIPlayTagType.PlayToken;
		case "addCardToPlayoutPlayPtn":
			return AIPlayTagType.AddCardToPlayoutPlayPtn;
		case "cantBeAttacked":
			return AIPlayTagType.CantBeAttacked;
		case "necromance":
			return AIPlayTagType.Necromance;
		case "earthRite":
			return AIPlayTagType.EarthRite;
		case "burialRite":
			return AIPlayTagType.BurialRite;
		case "enhance":
			return AIPlayTagType.Enhance;
		case "accelerate":
			return AIPlayTagType.Accelerate;
		case "crystalize":
			return AIPlayTagType.Crystalize;
		case "choiceTransform":
			return AIPlayTagType.ChoiceTransform;
		case "removeSkill":
			return AIPlayTagType.RemoveSkill;
		case "oneMoreLastword":
			return AIPlayTagType.OneMoreLastword;
		case "attackableClass":
			return AIPlayTagType.AttackableClass;
		case "clashBuff":
			return AIPlayTagType.ClashBuff;
		case "playptnBaseStatsRate":
			return AIPlayTagType.PlayptnBaseStatsRate;
		case "playRecoverPp":
			return AIPlayTagType.PlayRecoverPP;
		case "fanfareRecoverPp":
			return AIPlayTagType.FanfareRecoverPp;
		case "buffRecoverPp":
			return AIPlayTagType.BuffRecoverPP;
		case "condChoice":
			return AIPlayTagType.CondChoice;
		case "evoChoice":
			return AIPlayTagType.EvoChoice;
		case "playChoice":
			return AIPlayTagType.PlayChoice;
		case "fanfareChoice":
			return AIPlayTagType.FanfareChoice;
		case "plagueCity":
			return AIPlayTagType.PlagueCity;
		case "breakDestroy":
			return AIPlayTagType.BreakDestroy;
		case "evolvedAttackable":
			return AIPlayTagType.EvolvedAttackable;
		case "evolvedAttackableCount":
			return AIPlayTagType.EvolvedAttackableCount;
		case "evolvedSkill":
			return AIPlayTagType.EvolvedSkill;
		case "evoDestroy":
			return AIPlayTagType.EvoDestroy;
		case "evoBuff":
			return AIPlayTagType.EvoBuff;
		case "evoHeal":
			return AIPlayTagType.EvoHeal;
		case "evoBanish":
			return AIPlayTagType.EvoBanish;
		case "evoSubtractCountdown":
			return AIPlayTagType.EvoSubtractCountdown;
		case "evoSetLeaderMaxLife":
			return AIPlayTagType.EvoSetLeaderMaxLife;
		case "evoMetamorphose":
			return AIPlayTagType.EvoMetamorphose;
		case "evoHandMetamorphose":
			return AIPlayTagType.EvoHandMetamorphose;
		case "evoBounce":
			return AIPlayTagType.EvoBounce;
		case "evoRush":
			return AIPlayTagType.EvoRush;
		case "evoQuick":
			return AIPlayTagType.EvoQuick;
		case "evoGuard":
			return AIPlayTagType.EvoGuard;
		case "evoKiller":
			return AIPlayTagType.EvoKiller;
		case "evoDrain":
			return AIPlayTagType.EvoDrain;
		case "evoShield":
			return AIPlayTagType.EvoShield;
		case "evoDamageCut":
			return AIPlayTagType.EvoDamageCut;
		case "evoReanimate":
			return AIPlayTagType.EvoReanimate;
		case "evoAddDeck":
			return AIPlayTagType.EvoAddDeck;
		case "healDamage":
			return AIPlayTagType.HealDamage;
		case "healBuff":
			return AIPlayTagType.HealBuff;
		case "healToken":
			return AIPlayTagType.HealToken;
		case "healHeal":
			return AIPlayTagType.HealHeal;
		case "healEvo":
			return AIPlayTagType.HealEvo;
		case "turnEndMetamorphose":
			return AIPlayTagType.TurnEndMetamorphose;
		case "leaveBonus":
			return AIPlayTagType.LeaveBonus;
		case "leaveToken":
			return AIPlayTagType.LeaveToken;
		case "leaveHeal":
			return AIPlayTagType.LeaveHeal;
		case "leaveDamage":
			return AIPlayTagType.LeaveDamage;
		case "leaveAttachTag":
			return AIPlayTagType.LeaveAttachTag;
		case "leaveBanish":
			return AIPlayTagType.LeaveBanish;
		case "lastwordBuff":
			return AIPlayTagType.LastwordBuff;
		case "firstEvo":
			return AIPlayTagType.FirstEvo;
		case "lastwordReanimate":
			return AIPlayTagType.LastwordReanimate;
		case "reanimateBonus":
			return AIPlayTagType.ReanimateBonus;
		case "reanimateEvo":
			return AIPlayTagType.ReanimateEvo;
		case "playDraw":
			return AIPlayTagType.PlayDraw;
		case "handBonus":
			return AIPlayTagType.HandBonus;
		case "fusion":
			return AIPlayTagType.Fusion;
		case "playReanimate":
			return AIPlayTagType.PlayReanimate;
		case "fanfareReanimate":
			return AIPlayTagType.FanfareReanimate;
		case "modifyHeal":
			return AIPlayTagType.ModifyHeal;
		case "evoRecoverPp":
			return AIPlayTagType.EvoRecoverPp;
		case "fusionBonus":
			return AIPlayTagType.FusionBonus;
		case "fusionDraw":
			return AIPlayTagType.FusionDraw;
		case "noInstantAttack":
			return AIPlayTagType.NoInstantAttack;
		case "giveSkill":
			return AIPlayTagType.GiveSkill;
		case "playDiscard":
			return AIPlayTagType.PlayDiscard;
		case "fanfareDiscard":
			return AIPlayTagType.FanfareDiscard;
		case "discardedBonus":
			return AIPlayTagType.DiscardedBonus;
		case "attackDiscard":
			return AIPlayTagType.AttackDiscard;
		case "discardedToken":
			return AIPlayTagType.DiscardedToken;
		case "discardDamage":
			return AIPlayTagType.DiscardDamage;
		case "breakHeal":
			return AIPlayTagType.BreakHeal;
		case "buffDamage":
			return AIPlayTagType.BuffDamage;
		case "buffHeal":
			return AIPlayTagType.BuffHeal;
		case "buffBuff":
			return AIPlayTagType.BuffBuff;
		case "buffEvo":
			return AIPlayTagType.BuffEvo;
		case "buffDestroy":
			return AIPlayTagType.BuffDestroy;
		case "buffToken":
			return AIPlayTagType.BuffToken;
		case "buffDraw":
			return AIPlayTagType.BuffDraw;
		case "buffShield":
			return AIPlayTagType.BuffShield;
		case "discardHeal":
			return AIPlayTagType.DiscardHeal;
		case "evoDiscard":
			return AIPlayTagType.EvoDiscard;
		case "allyDiscardBonus":
			return AIPlayTagType.AllyDiscardBonus;
		case "buffRush":
			return AIPlayTagType.BuffRush;
		case "banishAttachTag":
			return AIPlayTagType.BanishAttachTag;
		case "otherBanishToken":
			return AIPlayTagType.OtherBanishToken;
		case "otherBanishAddCemetery":
			return AIPlayTagType.OtherBanishAddCemetery;
		case "getOn":
			return AIPlayTagType.GetOn;
		case "getOnBanish":
			return AIPlayTagType.GetOnBanish;
		case "getOnDamage":
			return AIPlayTagType.GetOnDamage;
		case "getOnEvo":
			return AIPlayTagType.GetOnEvo;
		case "getOffMetamorphose":
			return AIPlayTagType.GetOffMetamorphose;
		case "getOffEvo":
			return AIPlayTagType.GetOffEvo;
		case "rallyCountPlus":
			return AIPlayTagType.RallyCountPlus;
		case "playActivateCount":
			return AIPlayTagType.PlayActivateCount;
		case "attackActivateCount":
			return AIPlayTagType.AttackActivateCount;
		case "breakActivateCount":
			return AIPlayTagType.BreakActivateCount;
		case "banishActivateCount":
			return AIPlayTagType.BanishActivateCount;
		case "damagedActivateCount":
			return AIPlayTagType.DamagedActivateCount;
		case "buffActivateCount":
			return AIPlayTagType.BuffActivateCounnt;
		case "turnEndActivateCount":
			return AIPlayTagType.TurnEndActivateCount;
		case "healActivateCount":
			return AIPlayTagType.HealActivateCount;
		case "evoActivateCount":
			return AIPlayTagType.EvoActivateCount;
		case "necromanceActivateCount":
			return AIPlayTagType.NecromanceActivateCount;
		case "summonActivateCount":
			return AIPlayTagType.SummonActivateCount;
		case "bounceDamage":
			return AIPlayTagType.BounceDamage;
		case "playBuff":
			return AIPlayTagType.PlayBuff;
		case "fanfareBuff":
			return AIPlayTagType.FanfareBuff;
		case "playHandBuff":
			return AIPlayTagType.PlayHandBuff;
		case "fanfareHandBuff":
			return AIPlayTagType.FanfareHandBuff;
		case "playSetMaxStatus":
			return AIPlayTagType.PlaySetMaxStatus;
		case "fanfareSetMaxStatus":
			return AIPlayTagType.FanfareSetMaxStatus;
		case "playSetLeaderMaxLife":
			return AIPlayTagType.PlaySetLeaderMaxLife;
		case "resonanceDamage":
			return AIPlayTagType.ResonanceDamage;
		case "resonanceHeal":
			return AIPlayTagType.ResonanceHeal;
		case "resonanceKiller":
			return AIPlayTagType.ResonanceKiller;
		case "forceBerserk":
			return AIPlayTagType.ForceBerserk;
		case "setAITribe":
			return AIPlayTagType.SetAITribe;
		case "playSpellboost":
			return AIPlayTagType.PlaySpellboost;
		case "fanfareSpellboost":
			return AIPlayTagType.FanfareSpellboost;
		case "playAddCemetery":
			return AIPlayTagType.PlayAddCemetery;
		case "necromanceAddCemetery":
			return AIPlayTagType.NecromanceAddCemetery;
		case "fanfareAddCemetery":
			return AIPlayTagType.FanfareAddCemetery;
		case "playBanAttack":
			return AIPlayTagType.PlayBanAttack;
		case "fanfareBanAttack":
			return AIPlayTagType.FanfareBanAttack;
		case "playIgnoreGuard":
			return AIPlayTagType.PlayIgnoreGuard;
		case "fanfareIgnoreGuard":
			return AIPlayTagType.FanfareIgnoreGuard;
		case "turnStartAttachTag":
			return AIPlayTagType.TurnStartAttachTag;
		case "turnStartSubtractCountdown":
			return AIPlayTagType.TurnStartSubtractCountdown;
		case "turnStartDamageCut":
			return AIPlayTagType.TurnStartDamageCut;
		case "turnStartShield":
			return AIPlayTagType.TurnStartShield;
		case "turnStartDamage":
			return AIPlayTagType.TurnStartDamage;
		case "generateTag":
			return AIPlayTagType.GenerateTag;
		case "evoTokenDraw":
			return AIPlayTagType.EvoTokenDraw;
		case "evoAttackableCount":
			return AIPlayTagType.EvoAttackableCount;
		case "evoChangeCost":
			return AIPlayTagType.EvoChangeCost;
		case "playTokenDraw":
			return AIPlayTagType.PlayTokenDraw;
		case "fanfareTokenDraw":
			return AIPlayTagType.FanfareTokenDraw;
		case "playSummonHandCard":
			return AIPlayTagType.PlaySummonHandCard;
		case "fanfareSummonHandCard":
			return AIPlayTagType.FanfareSummonHandCard;
		case "playChangeClass":
			return AIPlayTagType.PlayChangeClass;
		case "fanfareChangeClass":
			return AIPlayTagType.FanfareChangeClass;
		case "playChangeTribe":
			return AIPlayTagType.PlayChangeTribe;
		case "fanfareChangeTribe":
			return AIPlayTagType.FanfareChangeTribe;
		case "playChangeCost":
			return AIPlayTagType.PlayChangeCost;
		case "fanfareChangeCost":
			return AIPlayTagType.FanfareChangeCost;
		case "playSelect":
			return AIPlayTagType.PlaySelect;
		case "fanfareSelect":
			return AIPlayTagType.FanfareSelect;
		case "playHandSelect":
			return AIPlayTagType.PlayHandSelect;
		case "fanfareHandSelect":
			return AIPlayTagType.FanfareHandSelect;
		case "evoHandSelect":
			return AIPlayTagType.EvoHandSelect;
		case "removeByDestroy":
			return AIPlayTagType.RemoveByDestroy;
		case "evoHandBuff":
			return AIPlayTagType.EvoHandBuff;
		case "playNotBeAttacked":
			return AIPlayTagType.PlayNotBeAttacked;
		case "fanfareNotBeAttacked":
			return AIPlayTagType.FanfareNotBeAttacked;
		case "playAttackableCount":
			return AIPlayTagType.PlayAttackableCount;
		case "fanfareAttackableCount":
			return AIPlayTagType.FanfareAttackableCount;
		case "breakAddStack":
			return AIPlayTagType.BreakAddStack;
		case "changeInplayImmediateRemoveByBanish":
			return AIPlayTagType.ChangeInplayImmediateRemoveByBanish;
		case "changeInplayImmediateRemoveByDestroy":
			return AIPlayTagType.ChangeInplayImmediateRemoveByDestroy;
		case "changeInplayCannotPlay":
			return AIPlayTagType.ChangeInplayCannotPlay;
		case "changeInplayCannotAttack":
			return AIPlayTagType.ChangeInplayCannotAttack;
		case "changeInplayAttachTag":
			return AIPlayTagType.ChangeInplayAttachTag;
		case "changeInplayImmediateShield":
			return AIPlayTagType.ChangeInplayImmediateShield;
		case "changeInplayImmediateDamageCut":
			return AIPlayTagType.ChangeInplayImmediateDamageCut;
		case "changeInplayImmediateDamageClip":
			return AIPlayTagType.ChangeInplayImmediateDamageClip;
		case "changeInplayImmediateLifeLowerLimit":
			return AIPlayTagType.ChangeInplayImmediateLifeLowerLimit;
		case "changeInplayImmediateUntouchable":
			return AIPlayTagType.ChangeInplayImmediateUntouchable;
		case "changeInplayImmediateIndestructible":
			return AIPlayTagType.ChangeInplayImmediateIndestructible;
		case "changePpTotalBuff":
			return AIPlayTagType.ChangePpTotalBuff;
		case "changeInplayImmediateDamageModifier":
			return AIPlayTagType.ChangeInplayImmediateDamageModifier;
		case "evoSetStatus":
			return AIPlayTagType.EvoSetStatus;
		case "playRemoveSkill":
			return AIPlayTagType.PlayRemoveSkill;
		case "fanfareRemoveSkill":
			return AIPlayTagType.FanfareRemoveSkill;
		case "playModifyConsumeEp":
			return AIPlayTagType.PlayModifyConsumeEp;
		case "fanfareModifyConsumeEp":
			return AIPlayTagType.FanfareModifyConsumeEp;
		case "playEvo":
			return AIPlayTagType.PlayEvo;
		case "fanfareEvo":
			return AIPlayTagType.FanfareEvo;
		case "otherPlayEvo":
			return AIPlayTagType.OtherPlayEvo;
		case "otherEnhanceEvo":
			return AIPlayTagType.OtherEnhanceEvo;
		case "otherPlayDamage":
			return AIPlayTagType.OtherPlayDamage;
		case "otherPlayDestroy":
			return AIPlayTagType.OtherPlayDestroy;
		case "otherPlayBounce":
			return AIPlayTagType.OtherPlayBounce;
		case "otherPlayBuff":
			return AIPlayTagType.OtherPlayBuff;
		case "otherPlayToken":
			return AIPlayTagType.OtherPlayToken;
		case "otherPlayRemoveTag":
			return AIPlayTagType.OtherPlayRemoveTag;
		case "otherPlayQuick":
			return AIPlayTagType.OtherPlayQuick;
		case "attackSubtractCountdown":
			return AIPlayTagType.AttackSubtractCountdown;
		case "damagedBonus":
			return AIPlayTagType.DamagedBonus;
		case "damagedBuff":
			return AIPlayTagType.DamagedBuff;
		case "damagedDamage":
			return AIPlayTagType.DamagedDamage;
		case "damagedToken":
			return AIPlayTagType.DamagedToken;
		case "damagedHeal":
			return AIPlayTagType.DamagedHeal;
		case "damagedCantUnderAttack":
			return AIPlayTagType.DamagedCantUnderAttack;
		case "otherDamagedDamage":
			return AIPlayTagType.OtherDamagedDamage;
		case "otherDamagedHeal":
			return AIPlayTagType.OtherDamagedHeal;
		case "otherDamagedSetLeaderMaxLife":
			return AIPlayTagType.OtherDamagedSetLeaderMaxLife;
		case "otherDamagedSubtractCountdown":
			return AIPlayTagType.OtherDamagedSubtractCountdown;
		case "otherDamagedBanish":
			return AIPlayTagType.OtherDamagedBanish;
		case "otherAttackBuff":
			return AIPlayTagType.OtherAttackBuff;
		case "otherAttackDamage":
			return AIPlayTagType.OtherAttackDamage;
		case "otherAttackHeal":
			return AIPlayTagType.OtherAttackHeal;
		case "otherAttackToken":
			return AIPlayTagType.OtherAttackToken;
		case "otherEvoBuff":
			return AIPlayTagType.OtherEvoBuff;
		case "otherAttackAttachTag":
			return AIPlayTagType.OtherAttackAttachTag;
		case "otherAttackRemoveTag":
			return AIPlayTagType.OtherAttackRemoveTag;
		case "otherEvoDamage":
			return AIPlayTagType.OtherEvoDamage;
		case "otherEvoBanish":
			return AIPlayTagType.OtherEvoBanish;
		case "otherEvoSubtractCountdown":
			return AIPlayTagType.OtherEvoSubtractCountdown;
		case "otherEvoEvo":
			return AIPlayTagType.OtherEvoEvo;
		case "otherEvoToken":
			return AIPlayTagType.OtherEvoToken;
		case "otherEvoShield":
			return AIPlayTagType.OtherEvoShield;
		case "selfAndOtherEvoDamage":
			return AIPlayTagType.SelfAndOtherEvoDamage;
		case "selfAndOtherEvoAttachTag":
			return AIPlayTagType.SelfAndOtherEvoAttachTag;
		case "selfAndOtherEvoShield":
			return AIPlayTagType.SelfAndOtherEvoShield;
		case "selfAndOtherEvoDestroy":
			return AIPlayTagType.SelfAndOtherEvoDestroy;
		case "selfAndOtherEvoBounce":
			return AIPlayTagType.SelfAndOtherEvoBounce;
		case "selfAndOtherEvoToken":
			return AIPlayTagType.SelfAndOtherEvoToken;
		case "selfAndOtherEvoDraw":
			return AIPlayTagType.SelfAndOtherEvoDraw;
		case "selfAndOtherEvoAddCemetery":
			return AIPlayTagType.SelfAndOtherEvoAddCemetery;
		case "selfAndOtherEvoHeal":
			return AIPlayTagType.SelfAndOtherEvoHeal;
		case "selfAndOtherEvoTokenDraw":
			return AIPlayTagType.SelfAndOtherEvoTokenDraw;
		case "attackKiller":
			return AIPlayTagType.AttackKiller;
		case "lastwordEvo":
			return AIPlayTagType.LastwordEvo;
		case "lastwordRemoveSkill":
			return AIPlayTagType.LastwordRemoveSkill;
		case "lastwordAddCemetery":
			return AIPlayTagType.LastwordAddCemetery;
		case "lastwordDamageClip":
			return AIPlayTagType.LastwordDamageClip;
		case "lastwordShield":
			return AIPlayTagType.LastwordShield;
		case "evolveToOther":
			return AIPlayTagType.EvolveToOther;
		case "playShield":
			return AIPlayTagType.PlayShield;
		case "fanfareShield":
			return AIPlayTagType.FanfareShield;
		case "playDamageCut":
			return AIPlayTagType.PlayDamageCut;
		case "fanfareDamageCut":
			return AIPlayTagType.FanfareDamageCut;
		case "playDamageClip":
			return AIPlayTagType.PlayDamageClip;
		case "fanfareDamageClip":
			return AIPlayTagType.FanfareDamageClip;
		case "otherPlayRecoverPp":
			return AIPlayTagType.OtherPlayRecoverPp;
		case "attackShield":
			return AIPlayTagType.AttackShield;
		case "clashShield":
			return AIPlayTagType.ClashShield;
		case "attackDamageClip":
			return AIPlayTagType.AttackDamageClip;
		case "clashDamageClip":
			return AIPlayTagType.ClashDamageClip;
		case "clashRemoveTag":
			return AIPlayTagType.ClashRemoveTag;
		case "otherLeaveDamage":
			return AIPlayTagType.OtherLeaveDamage;
		case "otherLeaveToken":
			return AIPlayTagType.OtherLeaveToken;
		case "turnEndRemoveTag":
			return AIPlayTagType.TurnEndRemoveTag;
		case "otherPlayAttachTag":
			return AIPlayTagType.OtherPlayAttachTag;
		case "buffBonus":
			return AIPlayTagType.BuffBonus;
		case "necromanceDamage":
			return AIPlayTagType.NecromanceDamage;
		case "necromanceHeal":
			return AIPlayTagType.NecromanceHeal;
		case "fanfareRemoveGuard":
			return AIPlayTagType.FanfareRemoveGuard;
		case "attackRemoveTag":
			return AIPlayTagType.AttackRemoveTag;
		case "forceImmediateAttack":
			return AIPlayTagType.ForceImmediateAttack;
		case "stack":
			return AIPlayTagType.Stack;
		case "evoAddStack":
			return AIPlayTagType.EvoAddStack;
		case "fusionMetamorphose":
			return AIPlayTagType.FusionMetamorphose;
		case "lastwordSubtractCountdown":
			return AIPlayTagType.LastwordSubtractCountdown;
		case "attackSetStatus":
			return AIPlayTagType.AttackSetStatus;
		case "playAddDeck":
			return AIPlayTagType.PlayAddDeck;
		case "fanfareAddDeck":
			return AIPlayTagType.FanfareAddDeck;
		case "attackAddDeck":
			return AIPlayTagType.AttackAddDeck;
		case "breakRecoverPp":
			return AIPlayTagType.BreakRecoverPp;
		case "choiceBrave":
			return AIPlayTagType.ChoiceBrave;
		default:
			if (!string.IsNullOrEmpty(word))
			{
				AIConsoleUtility.LogError("AICardData Unknown tag : " + word);
			}
			return AIPlayTagType.None;
		}
	}
}

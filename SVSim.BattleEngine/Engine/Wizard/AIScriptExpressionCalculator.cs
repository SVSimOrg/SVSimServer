using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public class AIScriptExpressionCalculator
{
	public static float CalculateExpression(List<AIScriptTokenBase> iPolish, List<int> playPtn, AIVirtualField field, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		if (iPolish == null || iPolish.Count <= 0)
		{
			return 0f;
		}
		if (iPolish.Count == 1 && iPolish[0] is AIScriptNumericToken)
		{
			return iPolish[0].Value;
		}
		if (playPtn == null)
		{
			playPtn = EnemyAI.EmptyPlayPtn;
		}
		return Calculation(iPolish, playPtn, field, tagOwner, situation);
	}

	private static float Calculation(List<AIScriptTokenBase> expr, List<int> playPtn, AIVirtualField field, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		Stack<AIScriptTokenBase> stack = new Stack<AIScriptTokenBase>();
		int count = expr.Count;
		for (int i = 0; i < count; i++)
		{
			AIScriptTokenBase aIScriptTokenBase = expr[i];
			if (aIScriptTokenBase is AIScriptNumericToken || aIScriptTokenBase is AIScriptArgumentToken || aIScriptTokenBase is AIScriptIDToken)
			{
				stack.Push(aIScriptTokenBase);
			}
			else if (aIScriptTokenBase is AIScriptVariableToken)
			{
				AIScriptTokenBase aIScriptTokenBase2 = CalculateVariableToken(aIScriptTokenBase, playPtn, field, tagOwner, situation);
				if (aIScriptTokenBase2 != null)
				{
					stack.Push(aIScriptTokenBase2);
				}
			}
			else if (aIScriptTokenBase is AIScriptFunctionToken)
			{
				int argCount = ((AIScriptFunctionToken)aIScriptTokenBase).ArgCount;
				List<AIScriptTokenBase> list = new List<AIScriptTokenBase>();
				for (int j = 0; j < argCount; j++)
				{
					AIScriptTokenBase item = stack.Pop();
					list.Add(item);
				}
				stack.Push(CalculateFunctionToken(list, aIScriptTokenBase as AIScriptFunctionToken, field, playPtn, tagOwner, situation));
			}
			else
			{
				float value = stack.Pop().Value;
				float value2 = stack.Pop().Value;
				float value3 = CalculateByOperatorSymbol(aIScriptTokenBase.Type, value2, value);
				stack.Push(new AIScriptNumericToken(value3));
			}
		}
		if (stack.Count > 1 || stack.Count <= 0)
		{
			return 0f;
		}
		return stack.Pop().Value;
	}

	public static float CalculateByOperatorSymbol(AIScriptTokenType symbol, float left, float right)
	{
		switch (symbol)
		{
		case AIScriptTokenType.PLUS:
			return left + right;
		case AIScriptTokenType.MINUS:
			return left - right;
		case AIScriptTokenType.MULTI:
			return left * right;
		case AIScriptTokenType.DIV:
			return left / right;
		case AIScriptTokenType.REMAIN:
			return left % right;
		case AIScriptTokenType.MORE_THAN:
			if (!(left > right))
			{
				return 0f;
			}
			return 1f;
		case AIScriptTokenType.MORE_EQUAL:
			if (!(left >= right))
			{
				return 0f;
			}
			return 1f;
		case AIScriptTokenType.LESS_THAN:
			if (!(left < right))
			{
				return 0f;
			}
			return 1f;
		case AIScriptTokenType.LESS_EQUAL:
			if (!(left <= right))
			{
				return 0f;
			}
			return 1f;
		case AIScriptTokenType.EQUAL:
			if (left != right)
			{
				return 0f;
			}
			return 1f;
		case AIScriptTokenType.MAX:
			return Mathf.Max(left, right);
		case AIScriptTokenType.MIN:
			return Mathf.Min(left, right);
		case AIScriptTokenType.OR:
			if (!(left > 0f) && !(right > 0f))
			{
				return 0f;
			}
			return 1f;
		case AIScriptTokenType.AND:
			if (!(left > 0f) || !(right > 0f))
			{
				return 0f;
			}
			return 1f;
		default:
			return 0f;
		}
	}

	public static AIScriptTokenBase CalculateVariableToken(AIScriptTokenBase token, List<int> playPtn, AIVirtualField field, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		_ = token is AIScriptVariableToken;
		switch (((AIScriptVariableToken)token).VariableType)
		{
		case AIScriptTokenVariableType.NOW_TURN:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.AllyTurnCount : field.EnemyTurnCount);
		case AIScriptTokenVariableType.COUNTDOWN:
			return new AIScriptNumericToken(tagOwner.ChantCount);
		case AIScriptTokenVariableType.REINCARNATION_MAX:
			return new AIScriptNumericToken(AIReincarnationUtility.EvalMaxReincarnationBonus(tagOwner, playPtn));
		case AIScriptTokenVariableType.SPELLBOOST:
			return new AIScriptNumericToken(tagOwner.SpellboostCount);
		case AIScriptTokenVariableType.FIELD_SPACE:
			return new AIScriptNumericToken(field.GetPredictiveAllyFieldSpace());
		case AIScriptTokenVariableType.ALLY_UNIT_MIN:
			return new AIScriptNumericToken(field.CalcMinEvaluateValue(playPtn, isAlly: true));
		case AIScriptTokenVariableType.ALLY_UNIT_MAX:
			return new AIScriptNumericToken(field.CalcMaxEvaluateValue(playPtn, isAlly: true));
		case AIScriptTokenVariableType.ALLY_ATTACKABLE_ATK_MAX:
			return new AIScriptNumericToken(field.GetMaxAllyAttackableUnitAttack(playPtn, tagOwner.IsAlly));
		case AIScriptTokenVariableType.ALLY_NON_ATTACKABLE_ATK_MAX:
			return new AIScriptNumericToken(field.GetMaxAllyNonAttackableUnitAttack(playPtn));
		case AIScriptTokenVariableType.ALLY_MAX_ATTACKABLE_LIFE:
			return new AIScriptNumericToken(field.GetAllyMaxAttackableLife(tagOwner.IsAlly));
		case AIScriptTokenVariableType.ENEMY_UNIT_MIN:
			return new AIScriptNumericToken(field.CalcMinEvaluateValue(EnemyAI.EmptyPlayPtn, isAlly: false));
		case AIScriptTokenVariableType.ENEMY_UNIT_MAX:
			return new AIScriptNumericToken(field.CalcMaxEvaluateValue(EnemyAI.EmptyPlayPtn, isAlly: false));
		case AIScriptTokenVariableType.HAND_COUNT_E:
			return new AIScriptNumericToken(field.AI.OPPONENT.HandCardList.Count);
		case AIScriptTokenVariableType.DECK_COUNT_E:
			return new AIScriptNumericToken(field.AI.PlayerPair.Opponent.DeckCardList.Count);
		case AIScriptTokenVariableType.PLAYPTN_COUNT:
		{
			int allPlayedCountInTurn = field.PlayedCardContainer.GetAllPlayedCountInTurn(tagOwner.IsAlly);
			if (playPtn != null)
			{
				return new AIScriptNumericToken(playPtn.Count + allPlayedCountInTurn + AIEvaluateBonusFromOhterUtility.GetPlayPlusCount(field, playPtn));
			}
			return new AIScriptNumericToken(allPlayedCountInTurn);
		}
		case AIScriptTokenVariableType.MEMBER_MAX_LIFE:
			return new AIScriptNumericToken(field.GetMemberMaxLife(playPtn, tagOwner.IsAlly));
		case AIScriptTokenVariableType.MEMBER_ATK_SUM:
			return new AIScriptNumericToken(field.GetMemberAtkSum(playPtn, tagOwner.IsAlly));
		case AIScriptTokenVariableType.MEMBER_LIFE_SUM:
			return new AIScriptNumericToken(field.GetMemberLifeSum(playPtn, tagOwner.IsAlly));
		case AIScriptTokenVariableType.INPLAY_ATTACK_SUM_TO_LEADER:
			return new AIScriptNumericToken(field.GetInplayAttackSumToLeader(tagOwner.IsAlly));
		case AIScriptTokenVariableType.ATTACK_TARGET_ATK_MAX:
			return new AIScriptNumericToken(field.GetAttackTargetMaxAtk(tagOwner));
		case AIScriptTokenVariableType.SUMMON_DRUNKEN_ATK_MAX:
			return new AIScriptNumericToken(field.GetSummonDrunkenAtkMax(tagOwner));
		case AIScriptTokenVariableType.ENEMY_MIN_ATK:
			return new AIScriptNumericToken(field.GetEnemyInPlayMinAttack());
		case AIScriptTokenVariableType.ENEMY_MAX_ATK:
			return new AIScriptNumericToken(field.GetEnemyInPlayMaxAttack());
		case AIScriptTokenVariableType.ENEMY_ATK_SUM:
			return new AIScriptNumericToken(field.GetInplayAttackSumToLeader(isAlly: false));
		case AIScriptTokenVariableType.ALLY_INPLAY_MAX_ATK:
			return new AIScriptNumericToken(field.GetAllyInplayMaxAttack());
		case AIScriptTokenVariableType.GRAVE_COUNT:
			return new AIScriptNumericToken(field.VirtualCemetery.GetCemeteryCount(tagOwner.IsAlly));
		case AIScriptTokenVariableType.ALLY_MAX_PP:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.AllyPpTotal : field.EnemyPpTotal);
		case AIScriptTokenVariableType.ENEMY_MAX_PP:
			return new AIScriptNumericToken((!tagOwner.IsAlly) ? field.AllyPpTotal : field.EnemyPpTotal);
		case AIScriptTokenVariableType.REST_PP:
			return new AIScriptNumericToken((tagOwner != null) ? field.AI.PlayPtnRecorder.GetRestPp(playPtn, field) : 0);
		case AIScriptTokenVariableType.NOW_REST_PP:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.AllyPp : field.EnemyPp);
		case AIScriptTokenVariableType.ALLY_EP:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.AllyEvolutionCount : field.EnemyEvolutionCount);
		case AIScriptTokenVariableType.ENEMY_EP:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.EnemyEvolutionCount : field.AllyEvolutionCount);
		case AIScriptTokenVariableType.OWN_ATK:
			return new AIScriptNumericToken(tagOwner.Attack);
		case AIScriptTokenVariableType.OWN_LIFE:
			return new AIScriptNumericToken(tagOwner.Life);
		case AIScriptTokenVariableType.OWN_COST:
			return new AIScriptNumericToken(tagOwner.Cost);
		case AIScriptTokenVariableType.OWN_BASE_ATK:
			return new AIScriptNumericToken(tagOwner.DefaultAttack);
		case AIScriptTokenVariableType.OWN_BASE_LIFE:
			return new AIScriptNumericToken(tagOwner.DefaultLife);
		case AIScriptTokenVariableType.KILLER_ATTACK_VALUE:
			return new AIScriptNumericToken(tagOwner.EvalKillerAttackValue(playPtn, situation));
		case AIScriptTokenVariableType.ENHANCE_COST:
			return new AIScriptNumericToken(AIEnhanceUtility.GetEnhanceCost(tagOwner, field, playPtn));
		case AIScriptTokenVariableType.PLAY_ACTOR_ENHANCE_COST:
			if (situation == null || situation.ActionType != AIOperationType.PLAY || situation.Actor == null)
			{
				return new AIScriptNumericToken(-1f);
			}
			return new AIScriptNumericToken(AIEnhanceUtility.GetEnhanceCost(situation.Actor, field, playPtn));
		case AIScriptTokenVariableType.ACCELERATE_COST:
			return new AIScriptNumericToken(AIAccelerateUtility.GetAccelerateCost(tagOwner, field, playPtn));
		case AIScriptTokenVariableType.CHOICE_TRANSFORM_COST:
			return new AIScriptNumericToken(tagOwner.GetChoiceTransformCost(field, playPtn));
		case AIScriptTokenVariableType.BASE_SKILL_COUNT:
			return new AIScriptNumericToken(tagOwner.GetBaseSkillCount());
		case AIScriptTokenVariableType.LAST_LIFE:
			return new AIScriptNumericToken(tagOwner.LastLife);
		case AIScriptTokenVariableType.MAX_ATTACKABLE_COUNT:
			return new AIScriptNumericToken(tagOwner.MaxAttackableCount);
		case AIScriptTokenVariableType.HAND_MIN_COST:
			if (tagOwner != null && !tagOwner.IsAlly)
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(field.GetHandMinCost(tagOwner));
		case AIScriptTokenVariableType.HAND_MAX_COST:
			if (!tagOwner.IsAlly)
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(field.GetHandMaxCost(tagOwner));
		case AIScriptTokenVariableType.UNION_BURST_COUNT:
		{
			int num3 = 0;
			if (tagOwner.HasUnionBurst)
			{
				num3 = tagOwner.UnionBurstCount;
			}
			return new AIScriptNumericToken(num3);
		}
		case AIScriptTokenVariableType.CONSUME_EP:
			return new AIScriptNumericToken((!tagOwner.IsNotConsumeEp) ? 1 : 0);
		case AIScriptTokenVariableType.LAST_HEAL_AMOUNT:
			return new AIScriptNumericToken(tagOwner.LastHealAmount);
		case AIScriptTokenVariableType.JUST_BEFORE_TURN_DAMAGE:
			return new AIScriptNumericToken(field.JustBeforeTurnLeaderDamage);
		case AIScriptTokenVariableType.RALLY_COUNT:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.AllyRallyCount : field.EnemyRallyCount);
		case AIScriptTokenVariableType.SKYBOUND_ART_COUNT:
		{
			int num2 = 0;
			if (tagOwner.HasSkyboundArt)
			{
				num2 = tagOwner.SkyboundArtCount;
			}
			return new AIScriptNumericToken(num2);
		}
		case AIScriptTokenVariableType.SUPER_SKYBOUND_ART_COUNT:
		{
			int num = 0;
			if (tagOwner.HasSuperSkyboundArt)
			{
				num = tagOwner.SuperSkyboundArtCount;
			}
			return new AIScriptNumericToken(num);
		}
		case AIScriptTokenVariableType.USED_EP_COUNT:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.UsedEpCount : 0);
		case AIScriptTokenVariableType.USED_PP_COUNT:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.UsedPpCount : 0);
		case AIScriptTokenVariableType.EARTH_RITE_COUNT:
			return new AIScriptNumericToken(AIGetPreprocessInformationUtility.GetEarthRiteCount(tagOwner, situation));
		case AIScriptTokenVariableType.NECROMANCE_COUNT:
			return new AIScriptNumericToken(AIGetPreprocessInformationUtility.GetNecromanceCount(tagOwner, situation));
		case AIScriptTokenVariableType.USED_STACK_COUNT:
			return new AIScriptNumericToken(tagOwner.IsAlly ? field.AllyGameUsedStackCount : field.EnemyGameUsedStackCount);
		case AIScriptTokenVariableType.DEFAULT_DAMAGE:
			return new AIScriptNumericToken(situation.GetCurrentProcessDefaultDamage());
		case AIScriptTokenVariableType.NEXT_PLAY_PRIORITY:
		{
			AISinglePlayptnRecord bestPlayPtnRecord = field.AI.BestPlayPtnRecord;
			if (bestPlayPtnRecord == null)
			{
				AIConsoleUtility.LogError("NEXT_PLAY_PRIORITY: BestPlayPtnRecord is null! Wrong use timing.");
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(bestPlayPtnRecord.FirstPlayCardPriority);
		}
		case AIScriptTokenVariableType.NECROMANCED_COUNT_IN_GAME:
			return new AIScriptNumericToken(field.GetNecromanceCountInGame(situation, tagOwner.IsAlly));
		case AIScriptTokenVariableType.IS_EVO_TURN:
			return new AIScriptNumericToken((field.AI.ALLY.EvolveWaitTurnCount <= 0) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ABLE_EVO:
			return new AIScriptNumericToken(tagOwner.IsAbleEvolution() ? 1f : 0f);
		case AIScriptTokenVariableType.IS_F_ADV:
			return new AIScriptNumericToken((field.AI.FieldAdvantage >= 0f) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_F_DISADV:
			return new AIScriptNumericToken((field.AI.FieldAdvantage < 0f) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_AWAKE:
			return new AIScriptNumericToken(SkillConditionAwake.IsAwake(tagOwner.IsAlly ? field.AllyPpTotal : field.EnemyPpTotal) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_BERSERK:
			return new AIScriptNumericToken(AIBerserkUtility.IsToBeBerserk(field, null, situation, tagOwner.IsAlly) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_TO_BE_BERSERK:
			return new AIScriptNumericToken(AIBerserkUtility.IsToBeBerserk(field, playPtn, situation, tagOwner.IsAlly) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ON_FIELD:
			return new AIScriptNumericToken(tagOwner.IsOnField ? 1f : 0f);
		case AIScriptTokenVariableType.IS_IN_HAND:
			return new AIScriptNumericToken(tagOwner.IsInHand ? 1f : 0f);
		case AIScriptTokenVariableType.IS_IN_PLAYPTN:
			return new AIScriptNumericToken(playPtn.Contains(field.AllyHandCards.FindIndex((AIVirtualCard c) => c.CardIndex == tagOwner.CardIndex)) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_OWNER_TURN:
			return new AIScriptNumericToken(tagOwner.IsSelfTurn ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ALLY:
			return new AIScriptNumericToken(tagOwner.IsAlly ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ENEMY:
			return new AIScriptNumericToken(tagOwner.IsAlly ? 0f : 1f);
		case AIScriptTokenVariableType.IS_ALLY_FIRST:
			return new AIScriptNumericToken(field.AI.IsAllyFirst ? 1f : 0f);
		case AIScriptTokenVariableType.IS_SNEAK:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.SNEAK, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_KILLER:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.KILLER, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_MEDUSA:
			return new AIScriptNumericToken(tagOwner.IsDestroyWhenAttack ? 1f : 0f);
		case AIScriptTokenVariableType.IS_EVOLVED:
			return new AIScriptNumericToken((tagOwner.IsUseEvo || tagOwner.IsEvolution) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_EVOLVING:
			return new AIScriptNumericToken(tagOwner.IsUseEvo ? 1f : 0f);
		case AIScriptTokenVariableType.IS_GUARD:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.GUARD, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_RUSH:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.RUSH, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_PLAYOUT_ATTACKER:
			return new AIScriptNumericToken(AttackSelectControl.IsAttackPossible(tagOwner, tagOwner.SelfField.EnemyClass, tagOwner.BaseCard.OpponentBattlePlayer) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ENHANCED:
			return new AIScriptNumericToken(tagOwner.IsEnhanced(field, playPtn, situation) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ACCELERATE:
			return new AIScriptNumericToken(tagOwner.IsAccelerated(field, playPtn, situation) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_CRYSTALIZE:
			return new AIScriptNumericToken(tagOwner.IsCrystalize(field, playPtn, situation) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ATTACK_LEADER:
			if (situation != null && situation is AIVirtualAttackInfo aIVirtualAttackInfo)
			{
				return new AIScriptNumericToken((aIVirtualAttackInfo.AttackTarget != null && aIVirtualAttackInfo.AttackTarget.IsLeader) ? 1f : 0f);
			}
			return new AIScriptNumericToken(0f);
		case AIScriptTokenVariableType.IS_FANFARE:
			return new AIScriptNumericToken(field.IsEnableIgnoreFanfareBonus(playPtn) ? 0f : 1f);
		case AIScriptTokenVariableType.IS_SKILL_REMOVED:
			return new AIScriptNumericToken(tagOwner.IsSkillLost ? 1f : 0f);
		case AIScriptTokenVariableType.IS_BARBAROSSA:
			return new AIScriptNumericToken(field.AI.IsOpponentBarbarossaDestroyed ? 1f : 0f);
		case AIScriptTokenVariableType.IS_RESONANCE:
			return new AIScriptNumericToken(field.IsResonance(tagOwner.IsAlly) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_LETHAL:
			return new AIScriptNumericToken(field.AI.IsLethal(tagOwner, field, playPtn, situation) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_USED_EVO:
			return new AIScriptNumericToken(field.AI.ALLY.NowTurnEvol ? 0f : 1f);
		case AIScriptTokenVariableType.IS_FIRST_TURN:
			return new AIScriptNumericToken(tagOwner.IsFirstTurn ? 1f : 0f);
		case AIScriptTokenVariableType.IS_DAMAGED:
			return new AIScriptNumericToken((tagOwner.MaxLife - tagOwner.Life > 0) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_DELAY_HEAL:
			return new AIScriptNumericToken(tagOwner.IsDelayHeal(field, situation) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_NOT_ATTACK_YET:
			return new AIScriptNumericToken(tagOwner.IsNotAttackYet ? 1f : 0f);
		case AIScriptTokenVariableType.IS_LEADER:
			return new AIScriptNumericToken(tagOwner.IsLeader ? 1f : 0f);
		case AIScriptTokenVariableType.IS_GET_ON:
			return new AIScriptNumericToken(tagOwner.IsGetOn ? 1f : 0f);
		case AIScriptTokenVariableType.IS_SKILL_SUMMONED:
			return new AIScriptNumericToken((tagOwner.IsOnField && tagOwner.IsSkillSummoned) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_UNTOUCHABLE:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.UNTOUCHABLE, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_FORCE_TARGETING:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.FORCE_TARGETING, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_QUICK:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.QUICK, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_NOT_BE_ATTACKED:
			return new AIScriptNumericToken(((situation != null && situation.ActionType == AIOperationType.ATTACK) ? tagOwner.IsCantUnderAttack(field.ParamQuery, situation.Actor, playPtn, field) : tagOwner.IsCantUnderAnyAttack()) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_IN_SIMULATION:
			return new AIScriptNumericToken(field.AI.IsInVirutalSimulation ? 1f : 0f);
		case AIScriptTokenVariableType.IS_IGNORE_GUARD:
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(tagOwner, AIScriptTokenArgType.IGNORE_GUARD, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ATTACKED:
			return new AIScriptNumericToken(tagOwner.IsAttacked ? 1f : 0f);
		case AIScriptTokenVariableType.IS_ONEMORELASTWORD_TAGGED:
			return new AIScriptNumericToken(AIOneMoreLastwordUtility.IsHoldingOneMoreLastword(tagOwner, field) ? 1f : 0f);
		case AIScriptTokenVariableType.IS_DRAIN:
			return new AIScriptNumericToken(tagOwner.IsDrain ? 1f : 0f);
		default:
			return null;
		}
	}

	public static AIScriptTokenBase CalculateFunctionToken(List<AIScriptTokenBase> argList, AIScriptFunctionToken funcToken, AIVirtualField field, List<int> playPtn, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		if (argList.Count != funcToken.ArgCount)
		{
			return null;
		}
		AIVirtualCard aIVirtualCard = null;
		AIVirtualCard aIVirtualCard2 = null;
		if (situation is AIVirtualAttackInfo aIVirtualAttackInfo)
		{
			aIVirtualCard2 = aIVirtualAttackInfo.AttackTarget;
			aIVirtualCard = ((situation.Actor.IsAlly != tagOwner.IsAlly) ? aIVirtualAttackInfo.Actor : aIVirtualAttackInfo.AttackTarget);
		}
		switch (funcToken.FuncType)
		{
		case AIScriptTokenFuncType.MEMBER_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.MultipleFiltering(field.GetFieldCountCardList(playPtn), argList, tagOwner, playPtn, situation)?.Count ?? 0);
		case AIScriptTokenFuncType.POW:
		{
			float value2 = argList[1].Value;
			float value3 = argList[0].Value;
			return new AIScriptNumericToken(Mathf.Pow(value2, value3));
		}
		case AIScriptTokenFuncType.CEILING:
			return new AIScriptNumericToken(Mathf.Ceil(argList[0].Value));
		case AIScriptTokenFuncType.FLOOR:
			return new AIScriptNumericToken(Mathf.Floor(argList[0].Value));
		case AIScriptTokenFuncType.EVAL_RANDOM_MULTI_DAMAGE:
		{
			argList.Reverse();
			AIRandomMultiDamageEvaluator.CreateEvalRandomDamageArgList(argList, out var filters, out var damage6, out var count6);
			return new AIScriptNumericToken(AIRandomMultiDamageEvaluator.EvaluateRandomDamageAverage(tagOwner, field, playPtn, situation, filters, damage6, count6));
		}
		case AIScriptTokenFuncType.EVAL_RANDOM_MULTI_DAMAGE_MAX:
		{
			argList.Reverse();
			AIRandomMultiDamageEvaluator.CreateEvalRandomDamageArgList(argList, out var filters2, out var damage7, out var count7);
			return new AIScriptNumericToken(AIRandomMultiDamageEvaluator.EvaluateRandomDamageMax(tagOwner, field, playPtn, filters2, situation, damage7, count7));
		}
		case AIScriptTokenFuncType.EVAL_INSTANT_ATTACK:
		{
			int attack2 = (int)argList[2].Value;
			int life2 = (int)argList[1].Value;
			int count5 = (int)argList[0].Value;
			return new AIScriptNumericToken(AIInstantAttackUtility.EvalInstantAttack(attack2, life2, count5, playPtn, tagOwner, situation));
		}
		case AIScriptTokenFuncType.EVAL_RUSH:
		{
			int attack = (int)argList[2].Value;
			int life = (int)argList[1].Value;
			int count4 = (int)argList[0].Value;
			bool isRush = true;
			return new AIScriptNumericToken(AIInstantAttackUtility.EvalInstantAttack(attack, life, count4, playPtn, tagOwner, situation, isRush));
		}
		case AIScriptTokenFuncType.INPLAY_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.MultipleFiltering(field.CardListSet.BothInplayCards, argList, tagOwner, playPtn, situation)?.Count ?? 0);
		case AIScriptTokenFuncType.PLAYOUT_ATTACKER_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIPlayoutAttackerCountUtility.GetPlayoutAttackerCount(field, tagOwner, playPtn, argList, situation));
		case AIScriptTokenFuncType.BEFORE_PLAYPTN_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIPlayPtnUtility.GetBeforePlayPtnCount(argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.PLAY_TOKEN_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIPlayTokenSimulationUtility.GetPlayTokenCount(argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.EVO_COUNT_IN_GAME:
			return new AIScriptNumericToken(field.GetEvolutionCountInGame(tagOwner, ((AIScriptArgumentToken)argList[0]).ArgumentType));
		case AIScriptTokenFuncType.EVO_COUNT_IN_PREVIOUS_TURN:
			return new AIScriptNumericToken(field.GetEvolutionCountInPreviousTurn(tagOwner, ((AIScriptArgumentToken)argList[0]).ArgumentType));
		case AIScriptTokenFuncType.HAND_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIHandCountUtility.GetHandCount(field, argList, tagOwner, situation, playPtn));
		case AIScriptTokenFuncType.HAND_NAME_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIHandCountUtility.GetHandNameCount(field, argList, tagOwner, situation, playPtn));
		case AIScriptTokenFuncType.SKILL_COUNT_FROM_ID:
		{
			int iD6 = ((AIScriptIDToken)argList[0]).ID;
			argList.RemoveAt(0);
			argList.Reverse();
			return new AIScriptNumericToken(AISkillCountFromIdUtility.GetSkillCountFromID(argList, iD6, tagOwner, playPtn, situation));
		}
		case AIScriptTokenFuncType.TAG_COUNT_FROM_ID:
		{
			int iD5 = ((AIScriptIDToken)argList[0]).ID;
			argList.RemoveAt(0);
			argList.Reverse();
			return new AIScriptNumericToken(AITagCountFromIdUtility.GetTagCountFromId(tagOwner, iD5, argList, field, playPtn, situation));
		}
		case AIScriptTokenFuncType.EVAL_CHANT_COUNT_CHANGE:
		{
			int amount2 = (int)argList[0].Value;
			return new AIScriptNumericToken(AICountdownChangeUtility.EvalChantCountChange(tagOwner, amount2, playPtn, isSelect: true, situation));
		}
		case AIScriptTokenFuncType.EVAL_CHANT_COUNT_CHANGE_ALL:
		{
			int amount = (int)argList[0].Value;
			return new AIScriptNumericToken(AICountdownChangeUtility.EvalChantCountChange(tagOwner, amount, playPtn, isSelect: false, situation));
		}
		case AIScriptTokenFuncType.EVAL_ALL_DAMAGE:
		{
			List<AIScriptTokenBase> list19 = new List<AIScriptTokenBase>();
			for (int num25 = argList.Count - 1; num25 > 0; num25--)
			{
				list19.Add(argList[num25]);
			}
			int damage8 = (int)argList[0].Value;
			return new AIScriptNumericToken(AIDamageSimulationUtility.EvalMultiAllDamage(tagOwner, field, list19, playPtn, situation, damage8, 1));
		}
		case AIScriptTokenFuncType.EVAL_TARGETING_DAMAGE:
		{
			List<AIScriptTokenBase> list12 = new List<AIScriptTokenBase>();
			for (int num18 = argList.Count - 1; num18 > 0; num18--)
			{
				list12.Add(argList[num18]);
			}
			int damage3 = (int)argList[0].Value;
			return new AIScriptNumericToken(AIDamageSimulationUtility.EvalTargetingDamage(tagOwner, field, list12, playPtn, situation, damage3));
		}
		case AIScriptTokenFuncType.EVAL_TARGETING_AND_RANDOM_MULTI_DAMAGE:
		{
			if (argList.Count < 5)
			{
				return new AIScriptNumericToken(0f);
			}
			new List<AIScriptTokenBase>();
			int num12 = 0;
			argList.Reverse();
			for (int num13 = 1; num13 < argList.Count; num13++)
			{
				if (argList[num13] is AIScriptNumericToken)
				{
					num12 = num13;
					break;
				}
			}
			List<AIScriptTokenBase> range3 = argList.GetRange(0, num12);
			List<AIScriptTokenBase> range4 = argList.GetRange(num12 + 1, argList.Count - num12 - 3);
			int targetDamage = (int)argList[num12].Value;
			int randomDamage = (int)argList[argList.Count - 2].Value;
			int randomDamageCount = (int)argList[argList.Count - 1].Value;
			return new AIScriptNumericToken(AIDamageSimulationUtility.EvalTargetingDamageAndRandomMultiSelectDamage(tagOwner, field, range3, range4, playPtn, situation, targetDamage, randomDamage, randomDamageCount));
		}
		case AIScriptTokenFuncType.EVAL_ALL_MULTI_DAMAGE:
		{
			List<AIScriptTokenBase> list2 = new List<AIScriptTokenBase>();
			for (int num3 = argList.Count - 2; num3 > 0; num3--)
			{
				list2.Add(argList[num3]);
			}
			int damage = (int)argList[1].Value;
			int damageCount = (int)argList[0].Value;
			return new AIScriptNumericToken(AIDamageSimulationUtility.EvalMultiAllDamage(tagOwner, field, list2, playPtn, situation, damage, damageCount));
		}
		case AIScriptTokenFuncType.EVAL_ALL_DESTROY:
			argList.Reverse();
			return new AIScriptNumericToken(AIDestroySimulationUtility.EvalAllDestroy(argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.EVAL_TARGETING_DESTROY:
			argList.Reverse();
			return new AIScriptNumericToken(AIDestroySimulationUtility.EvalTargetingDestroy(argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.EVAL_TARGETING_OTHER_DESTROY:
		{
			AIScriptTokenBase aIScriptTokenBase = argList[0];
			argList.RemoveAt(0);
			int select_count = 0;
			if (aIScriptTokenBase is AIScriptNumericToken)
			{
				select_count = (int)(aIScriptTokenBase as AIScriptNumericToken).Value;
			}
			argList.Reverse();
			int num4 = 0;
			int num5 = 1;
			while (argList != null && num5 < argList.Count)
			{
				if (argList != null && argList.Count > 0 && argList[num5] is AIScriptArgumentToken aIScriptArgumentToken6 && aIScriptArgumentToken6.IsSideArgType())
				{
					num4 = num5;
				}
				num5++;
			}
			List<AIScriptTokenBase> range = argList.GetRange(0, num4);
			List<AIScriptTokenBase> range2 = argList.GetRange(num4, argList.Count - num4);
			return new AIScriptNumericToken(AIDestroySimulationUtility.EvalTargetingOtherDestroy(range, range2, tagOwner, playPtn, situation, select_count));
		}
		case AIScriptTokenFuncType.EVAL_TARGETING_MULTI_DESTROY:
		{
			int num29 = 0;
			if (argList[0] is AIScriptNumericToken aIScriptNumericToken2)
			{
				num29 = (int)aIScriptNumericToken2.Value;
			}
			if (num29 == 0)
			{
				return new AIScriptNumericToken(0f);
			}
			argList.RemoveAt(0);
			argList.Reverse();
			return new AIScriptNumericToken(AIDestroySimulationUtility.EvalTargetingMultiDestroy(argList, num29, tagOwner, playPtn, situation));
		}
		case AIScriptTokenFuncType.EVAL_ALL_BOUNCE:
			argList.Reverse();
			return new AIScriptNumericToken(field?.EvalAllBounce(argList, tagOwner, playPtn) ?? 0f);
		case AIScriptTokenFuncType.EVAL_TARGETING_BOUNCE:
			argList.Reverse();
			return new AIScriptNumericToken(field?.EvalTargetingBounce(argList, tagOwner, playPtn) ?? 0f);
		case AIScriptTokenFuncType.EVAL_TARGETING_BANISH:
			argList.Reverse();
			return new AIScriptNumericToken((field != null) ? AIBanishSimulationUtility.EvalTargetingBanish(argList, playPtn, field, tagOwner, situation) : 0f);
		case AIScriptTokenFuncType.EVAL_RANDOM_BANISH:
		{
			int count3 = (int)argList[0].Value;
			List<AIScriptTokenBase> list5 = new List<AIScriptTokenBase>();
			for (int num9 = argList.Count - 1; num9 > 0; num9--)
			{
				list5.Add(argList[num9]);
			}
			return new AIScriptNumericToken((field != null) ? AIBanishSimulationUtility.EvalRandomBanish(list5, tagOwner, field, playPtn, count3, situation) : 0f);
		}
		case AIScriptTokenFuncType.EVAL_ALL_BANISH:
			argList.Reverse();
			return new AIScriptNumericToken(AIBanishSimulationUtility.EvalAllBanish(argList, tagOwner, field, playPtn, situation));
		case AIScriptTokenFuncType.EVAL_RANDOM_DESTROY:
		{
			int count2 = (int)argList[0].Value;
			argList.RemoveAt(0);
			argList.Reverse();
			return new AIScriptNumericToken(AIDestroySimulationUtility.EvalRandomDestroy(argList, count2, tagOwner, playPtn, situation));
		}
		case AIScriptTokenFuncType.EVAL_RANDOM_BOUNCE:
		{
			int count = (int)argList[0].Value;
			argList.Reverse();
			return new AIScriptNumericToken(field?.EvalRandomBounce(argList.GetRange(0, argList.Count - 1), tagOwner, field, playPtn, count) ?? 0f);
		}
		case AIScriptTokenFuncType.EVAL_TARGETING_HEAL:
		{
			List<AIScriptTokenBase> list4 = new List<AIScriptTokenBase>();
			for (int num7 = argList.Count - 1; num7 > 0; num7--)
			{
				list4.Add(argList[num7]);
			}
			int heal2 = (int)argList[0].Value;
			return new AIScriptNumericToken(AIHealSimulationUtility.EvalTargetingHeal(tagOwner, list4, playPtn, heal2));
		}
		case AIScriptTokenFuncType.EVAL_ALL_HEAL:
		{
			List<AIScriptTokenBase> list3 = new List<AIScriptTokenBase>();
			for (int num6 = argList.Count - 1; num6 > 0; num6--)
			{
				list3.Add(argList[num6]);
			}
			int heal = (int)argList[0].Value;
			return new AIScriptNumericToken(AIHealSimulationUtility.EvalAllHeal(tagOwner, list3, playPtn, heal));
		}
		case AIScriptTokenFuncType.EVAL_TARGETING_METAMORPHOSE:
		{
			List<AIScriptTokenBase> list = new List<AIScriptTokenBase>();
			for (int num2 = argList.Count - 1; num2 > 0; num2--)
			{
				list.Add(argList[num2]);
			}
			float value = 0f;
			if (argList[0] is AIScriptIDToken)
			{
				int iD = ((AIScriptIDToken)argList[0]).ID;
				value = AIMetamorphoseSimulationUtility.EvalTargetingMetamorphose(tagOwner, field, list, iD, playPtn, situation);
			}
			return new AIScriptNumericToken(value);
		}
		case AIScriptTokenFuncType.EVAL_RANDOM_METAMORPHOSE:
		{
			if (argList.Count < 2)
			{
				return new AIScriptNumericToken(0f);
			}
			List<AIScriptTokenBase> list18 = new List<AIScriptTokenBase>();
			for (int num24 = argList.Count - 1; num24 > 1; num24--)
			{
				list18.Add(argList[num24]);
			}
			int count8 = (int)argList[0].Value;
			float value6 = 0f;
			if (argList[1] is AIScriptIDToken)
			{
				int iD4 = ((AIScriptIDToken)argList[1]).ID;
				value6 = AIMetamorphoseSimulationUtility.EvalRandomMetamorphose(tagOwner, field, list18, iD4, playPtn, count8, situation);
			}
			return new AIScriptNumericToken(value6);
		}
		case AIScriptTokenFuncType.EVAL_ALL_METAMORPHOSE:
		{
			List<AIScriptTokenBase> list17 = new List<AIScriptTokenBase>();
			for (int num23 = argList.Count - 1; num23 > 0; num23--)
			{
				list17.Add(argList[num23]);
			}
			float value5 = 0f;
			if (argList[0] is AIScriptIDToken)
			{
				int iD3 = ((AIScriptIDToken)argList[0]).ID;
				value5 = AIMetamorphoseSimulationUtility.EvalAllMetamorphose(tagOwner, field, list17, iD3, playPtn, situation);
			}
			return new AIScriptNumericToken(value5);
		}
		case AIScriptTokenFuncType.EVAL_ALL_BUFF:
			return new AIScriptNumericToken(AIBuffEvaluationUtility.EvalAllBuff(tagOwner, field, argList, playPtn, situation));
		case AIScriptTokenFuncType.EVAL_RANDOM_BUFF:
			return new AIScriptNumericToken(AIBuffEvaluationUtility.EvalRandomBuff(tagOwner, field, argList, playPtn, situation));
		case AIScriptTokenFuncType.EVAL_TARGETING_BUFF:
			return new AIScriptNumericToken(AIBuffEvaluationUtility.EvalTargetingBuff(tagOwner, field, argList, playPtn, situation));
		case AIScriptTokenFuncType.EVAL_ECHO_DAMAGE:
		{
			List<AIScriptTokenBase> list11 = new List<AIScriptTokenBase>();
			for (int num17 = argList.Count - 1; num17 > 0; num17--)
			{
				list11.Add(argList[num17]);
			}
			int damage2 = (int)argList[0].Value;
			return new AIScriptNumericToken(AIDamageSimulationUtility.EvalEchoDamage(tagOwner, field, list11, damage2, playPtn, situation));
		}
		case AIScriptTokenFuncType.EVAL_LEADER_DAMAGE:
		{
			int damageAmount = (int)argList[0].Value;
			AIScriptTokenArgType argumentType3 = ((AIScriptArgumentToken)argList[1]).ArgumentType;
			AIVirtualCard aIVirtualCard4 = null;
			switch (argumentType3)
			{
			case AIScriptTokenArgType.ENEMY_CLASS:
				aIVirtualCard4 = (tagOwner.IsAlly ? field.EnemyClass : field.AllyClass);
				break;
			case AIScriptTokenArgType.ALLY_CLASS:
				aIVirtualCard4 = (tagOwner.IsAlly ? field.AllyClass : field.EnemyClass);
				break;
			default:
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(AILeaderLifeEvaluationUtility.Evaluate(aIVirtualCard4.Life - aIVirtualCard4.SimulateDamageAmount(damageAmount, isSkillDamage: true, tagOwner.IsSpell), aIVirtualCard4.Life, aIVirtualCard4.IsAlly, tagOwner.IsAlly));
		}
		case AIScriptTokenFuncType.EVAL_LEADER_HEAL:
		{
			int originalHealValue = (int)argList[0].Value;
			AIScriptTokenArgType argumentType4 = ((AIScriptArgumentToken)argList[1]).ArgumentType;
			AIVirtualCard aIVirtualCard5 = null;
			switch (argumentType4)
			{
			case AIScriptTokenArgType.ENEMY_CLASS:
				aIVirtualCard5 = (tagOwner.IsAlly ? field.EnemyClass : field.AllyClass);
				break;
			case AIScriptTokenArgType.ALLY_CLASS:
				aIVirtualCard5 = (tagOwner.IsAlly ? field.AllyClass : field.EnemyClass);
				break;
			}
			if (aIVirtualCard5 == null || aIVirtualCard5.Life <= 0)
			{
				return new AIScriptNumericToken(0f);
			}
			originalHealValue = AIHealSimulationUtility.CalcHealModifier(aIVirtualCard5, playPtn, situation, originalHealValue);
			return new AIScriptNumericToken(AILeaderLifeEvaluationUtility.Evaluate(Mathf.Min(aIVirtualCard5.Life + originalHealValue, aIVirtualCard5.MaxLife), aIVirtualCard5.Life, aIVirtualCard5.IsAlly, tagOwner.IsAlly));
		}
		case AIScriptTokenFuncType.DECK_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIDeckSimulationUtility.GetFilteredDeck(argList, tagOwner, field, playPtn, situation)?.Count ?? 0);
		case AIScriptTokenFuncType.IS_CLASH_TARGET:
			if (aIVirtualCard == null)
			{
				return new AIScriptNumericToken(0f);
			}
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, null, argList, playPtn, tagOwner, situation) ? 1f : 0f);
		case AIScriptTokenFuncType.IS_ATTACK_TARGET:
		{
			if (aIVirtualCard2 == null)
			{
				return new AIScriptNumericToken(0f);
			}
			AIScriptArgumentToken obj = (AIScriptArgumentToken)argList[0];
			AIScriptTokenArgType argumentType2 = obj.ArgumentType;
			_ = obj.IsNot;
			return new AIScriptNumericToken(AISkillSimulationUtility.HasSkill(aIVirtualCard2, argumentType2, tagOwner.SelfField.AI, playPtn) ? 1f : 0f);
		}
		case AIScriptTokenFuncType.IS_SELECTED_TARGET:
		{
			AIScriptTokenArgType aIScriptTokenArgType2 = AIScriptTokenArgType.NONE;
			if (argList[0] is AIScriptArgumentToken aIScriptArgumentToken10)
			{
				aIScriptTokenArgType2 = aIScriptArgumentToken10.ArgumentType;
			}
			if (aIScriptTokenArgType2 != AIScriptTokenArgType.TARGET_SELECT && aIScriptTokenArgType2 != AIScriptTokenArgType.SECOND_TARGET_SELECT)
			{
				return new AIScriptNumericToken(0f);
			}
			AISelectedTargetInfo situationTarget = situation.GetSituationTarget(aIScriptTokenArgType2);
			if (situationTarget == null || !situationTarget.HasTarget)
			{
				return new AIScriptNumericToken(0f);
			}
			argList.RemoveAt(0);
			argList.Reverse();
			List<AIVirtualCard> targets = situationTarget.Targets;
			bool flag = false;
			for (int num26 = 0; num26 < targets.Count; num26++)
			{
				if (AIFilteringUtility.CheckMatchTargetFiltering(targets[num26], null, argList, playPtn, tagOwner, situation))
				{
					flag = true;
					break;
				}
			}
			return new AIScriptNumericToken(flag ? 1f : 0f);
		}
		case AIScriptTokenFuncType.IS_NEXT_PLAY:
			if (playPtn == null || playPtn.Count <= 0)
			{
				return new AIScriptNumericToken(0f);
			}
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.CheckMatchTargetFiltering(field.AllyHandCards[playPtn[0]], field.AllyHandCards, argList, playPtn, tagOwner, situation) ? 1 : 0);
		case AIScriptTokenFuncType.LIFE:
			argList.Reverse();
			return new AIScriptNumericToken(AIGetStatusUtility.GetLife(tagOwner, field, playPtn, situation, argList));
		case AIScriptTokenFuncType.ATTACK:
			argList.Reverse();
			return new AIScriptNumericToken(AIGetStatusUtility.GetAttack(tagOwner, field, playPtn, situation, argList));
		case AIScriptTokenFuncType.EVAL_RANDOM_MULTI_SELECT_DAMAGE:
		{
			List<AIScriptTokenBase> list16 = new List<AIScriptTokenBase>();
			for (int num20 = argList.Count - 1; num20 > 0; num20--)
			{
				list16.Add(argList[num20]);
			}
			int damage5 = (int)argList[0].Value;
			int selectCount = (int)argList[1].Value;
			return new AIScriptNumericToken(AIDamageSimulationUtility.EvalRandomMultiSelectDamage(tagOwner, field, list16, playPtn, situation, damage5, selectCount));
		}
		case AIScriptTokenFuncType.EVAL_DIVIDED_DAMAGE:
		{
			AIScriptTokenBase aIScriptTokenBase4 = argList[0];
			int damage4 = (int)aIScriptTokenBase4.Value;
			argList.Remove(aIScriptTokenBase4);
			argList.Reverse();
			List<AIVirtualCard> candidates = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, argList, tagOwner, playPtn, situation);
			return new AIScriptNumericToken(AIDamageSimulationUtility.EvalOldestDamage(tagOwner, candidates, field, damage4, playPtn, situation));
		}
		case AIScriptTokenFuncType.DECK_NAME_COUNT:
		{
			argList.Reverse();
			List<AIVirtualCard> filteredDeck2 = AIDeckSimulationUtility.GetFilteredDeck(argList, tagOwner, field, playPtn, situation);
			return new AIScriptNumericToken((filteredDeck2 != null) ? AIFilteringUtility.GetCardNameCountFromList(filteredDeck2, argList, tagOwner, playPtn, situation) : 0);
		}
		case AIScriptTokenFuncType.BROKEN_COUNT:
		{
			float num10 = 0f;
			AIScriptTokenBase aIScriptTokenBase2 = argList[0];
			int num11 = (int)aIScriptTokenBase2.Value;
			if (num11 == 142)
			{
				argList.Remove(aIScriptTokenBase2);
			}
			argList.Reverse();
			List<AIVirtualCard> list6 = AIFilteringUtility.MultipleFiltering(tagOwner.IsAlly ? field.CardListSet.AllyDestroyedCards : field.CardListSet.EnemyDestroyedCards, argList, tagOwner, playPtn, null, isBlockDeadCard: false);
			if (num11 == 142)
			{
				if (list6 != null)
				{
					foreach (AIVirtualCard item in list6)
					{
						if (item.DeadTurn)
						{
							num10 += 1f;
						}
					}
				}
				return new AIScriptNumericToken(num10);
			}
			num10 = ((list6 != null) ? ((float)list6.Count) : 0f);
			return new AIScriptNumericToken(num10);
		}
		case AIScriptTokenFuncType.BROKEN_NAME_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.GetCardNameCountFromList(tagOwner.IsAlly ? field.CardListSet.AllyDestroyedCards : field.CardListSet.EnemyDestroyedCards, argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.BROKEN_COST_SUM:
			argList.Reverse();
			return new AIScriptNumericToken(AIBrokenCostSumUtility.GetBrokenCostSum(field, tagOwner, argList, playPtn, situation));
		case AIScriptTokenFuncType.LEAVE_NAME_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.GetCardNameCountFromList(tagOwner.IsAlly ? field.CardListSet.AllyLeftCards : field.CardListSet.EnemyLeftCards, argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.LEAVE_COUNT:
		{
			AIScriptTokenBase aIScriptTokenBase5 = argList[0];
			int num30 = (int)aIScriptTokenBase5.Value;
			if (num30 == 142)
			{
				argList.Remove(aIScriptTokenBase5);
			}
			argList.Reverse();
			List<AIVirtualCard> targets2 = ((num30 != 142) ? (tagOwner.IsAlly ? field.CardListSet.AllyLeftCards : field.CardListSet.EnemyLeftCards) : (tagOwner.IsAlly ? field.CardListSet.AllyLeftCardsThisTurn : field.CardListSet.EnemyLeftCardsThisTurn));
			return new AIScriptNumericToken(AIFilteringUtility.MultipleFiltering(targets2, argList, tagOwner, playPtn, situation, isBlockDeadCard: false)?.Count ?? 0);
		}
		case AIScriptTokenFuncType.DESTROYED_COUNT:
		{
			AIScriptIDToken aIScriptIDToken2 = argList[0] as AIScriptIDToken;
			float num27 = 0f;
			if (aIScriptIDToken2 != null)
			{
				int iD7 = aIScriptIDToken2.ID;
				foreach (AIVirtualCard enemyDestroyedCard in field.CardListSet.EnemyDestroyedCards)
				{
					if (!enemyDestroyedCard.IsUnit)
					{
						continue;
					}
					List<BattleCardBase.DestroyedBySkillInfo> destroyedBySkillList = enemyDestroyedCard.BaseCard.DestroyedBySkillList;
					for (int num28 = 0; num28 < destroyedBySkillList.Count; num28++)
					{
						if (destroyedBySkillList.ElementAt(num28).BaseCardId == iD7)
						{
							num27 += 1f;
						}
					}
				}
			}
			return new AIScriptNumericToken(num27);
		}
		case AIScriptTokenFuncType.IS_TRIBE:
		{
			AIScriptTokenArgType argumentType6 = ((AIScriptArgumentToken)argList[0]).ArgumentType;
			return new AIScriptNumericToken(tagOwner.IsTribe(argumentType6) ? 1f : 0f);
		}
		case AIScriptTokenFuncType.PLAYED_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(field.PlayedCardContainer.GetPlayedCountInTurn(argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.PLAYED_COUNT_IN_GAME:
			argList.Reverse();
			return new AIScriptNumericToken(field.PlayedCardContainer.GetPlayedCountInGame(argList, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.PLAYED_COUNT_IN_PREVIOUS_TURN:
			argList.Reverse();
			if (argList[0] is AIScriptArgumentToken aIScriptArgumentToken9 && aIScriptArgumentToken9.IsSideArgType(includeBoth: false))
			{
				bool isAllyList = tagOwner.IsAlly == (aIScriptArgumentToken9.ArgumentType == AIScriptTokenArgType.ALLY);
				return new AIScriptNumericToken(field.PlayedCardContainer.GetPlayedCountInPreviousTurn(isAllyList, argList, tagOwner, playPtn, situation));
			}
			AIConsoleUtility.LogError("PLAYED_COUNT_IN_PREVIOUS_TURN error!! side parameter is missing");
			return new AIScriptNumericToken(0f);
		case AIScriptTokenFuncType.PLAYED_NAME_COUNT:
		{
			AIScriptTokenArgType turnOrGame = AIScriptTokenArgType.NONE;
			if (argList[0] is AIScriptArgumentToken aIScriptArgumentToken8)
			{
				turnOrGame = aIScriptArgumentToken8.ArgumentType;
			}
			argList.RemoveAt(0);
			argList.Reverse();
			return new AIScriptNumericToken(field.PlayedCardContainer.GetPlayedNameCount(argList, turnOrGame, tagOwner, playPtn, situation));
		}
		case AIScriptTokenFuncType.RANDOM:
		{
			if (argList[0].Value == 0f)
			{
				return new AIScriptNumericToken(0f);
			}
			int num21 = field.AI.ALLY.DeckCardList.Count;
			for (int num22 = 0; num22 < field.AllyHandCards.Count; num22++)
			{
				num21 += field.AllyHandCards[num22].BaseCost;
			}
			return new AIScriptNumericToken((float)num21 % argList[0].Value);
		}
		case AIScriptTokenFuncType.IS_REANIMATE:
		{
			int cost = (int)argList[0].Value;
			return new AIScriptNumericToken(AIReanimateSimulationUtility.IsReanimate(tagOwner, cost) ? 1f : 0f);
		}
		case AIScriptTokenFuncType.DAMAGE_COUNT:
		{
			argList.Reverse();
			AIScriptTokenArgType argumentType5 = ((AIScriptArgumentToken)argList[0]).ArgumentType;
			if (argumentType5 != AIScriptTokenArgType.TURN && argumentType5 != AIScriptTokenArgType.GAME)
			{
				return new AIScriptNumericToken(0f);
			}
			int num19 = 0;
			if (argList.Count == 1)
			{
				num19 = AIPlayerLifeSimulationUtility.GetAllSelfTurnDamageCount(tagOwner, field, playPtn, argumentType5);
			}
			else
			{
				switch (((AIScriptArgumentToken)argList[1]).ArgumentType)
				{
				case AIScriptTokenArgType.PLAYED:
					num19 = AIPlayerLifeSimulationUtility.GetSelfTurnDamageCountAtPlayed(tagOwner, field, playPtn, argumentType5);
					break;
				case AIScriptTokenArgType.PLAYPTN:
					num19 = AIPlayerLifeSimulationUtility.GetSelfTurnDamageCountAtPlayPtn(tagOwner, field, playPtn);
					break;
				case AIScriptTokenArgType.BEFORE_PLAYPTN:
					num19 = AIPlayerLifeSimulationUtility.GetSelfTurnDamageCountAtBeforePlayPtn(tagOwner, field, playPtn, argumentType5);
					break;
				}
			}
			return new AIScriptNumericToken(num19);
		}
		case AIScriptTokenFuncType.HEAL_COUNT:
			return new AIScriptNumericToken(AIHealCountUtility.GetHealCount(tagOwner, field, situation, playPtn, argList));
		case AIScriptTokenFuncType.FORCED_EXCHANGE:
		{
			int attackLimit = (int)argList[0].Value;
			return new AIScriptNumericToken(field.EvalForcedExchangeVirtual(attackLimit, playPtn));
		}
		case AIScriptTokenFuncType.TOTAL_DAMAGE:
			return new AIScriptNumericToken(field.GetTotalDamage(argList, tagOwner, situation));
		case AIScriptTokenFuncType.EVAL_REANIMATE:
			return new AIScriptNumericToken(AIEvalReanimateUtility.EvalReanimate(tagOwner, playPtn, situation, argList));
		case AIScriptTokenFuncType.FUSION_COUNT:
			return new AIScriptNumericToken(AIFusionCountUtility.GetFusionCount(tagOwner, argList, playPtn, situation));
		case AIScriptTokenFuncType.NOW_FUSION_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIFusionCountUtility.GetNowFusionCount(tagOwner, argList, playPtn));
		case AIScriptTokenFuncType.FUSION_COUNT_AT_ONCE:
			return new AIScriptNumericToken(AIFusionCountUtility.GetFusionCountAtOnce(tagOwner, argList, playPtn, situation));
		case AIScriptTokenFuncType.FUSION_NAME_COUNT:
			return new AIScriptNumericToken(AIFusionCountUtility.GetFusionNameCount(tagOwner, argList, playPtn, situation));
		case AIScriptTokenFuncType.IS_SKILL_OCCURRED:
			return new AIScriptNumericToken(tagOwner.IsSkillOccurred(field, playPtn, situation, argList) ? 1f : 0f);
		case AIScriptTokenFuncType.SKILL_ACTIVATE_COUNT:
			return new AIScriptNumericToken(tagOwner.GetSkillActivateCount(field, playPtn, situation, argList));
		case AIScriptTokenFuncType.HAND_MAX_ATTACK:
		{
			argList.Reverse();
			argList.Add(new AIScriptArgumentToken(AIScriptTokenArgType.MAX_ATTACK, isNot: false));
			List<AIVirtualCard> list9 = AIFilteringUtility.MultipleFiltering(field.AllyHandCards, argList, tagOwner, playPtn, situation);
			if (list9 == null || list9.Count <= 0)
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(list9[0].Attack);
		}
		case AIScriptTokenFuncType.DECK_MAX_COST:
		{
			argList.Reverse();
			List<AIVirtualCard> filteredDeck = AIDeckSimulationUtility.GetFilteredDeck(argList, tagOwner, field, playPtn, situation);
			if (filteredDeck == null || filteredDeck.Count <= 0)
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(filteredDeck.Max((AIVirtualCard c) => c.BaseCost));
		}
		case AIScriptTokenFuncType.INPLAY_LARGEST_LIFE:
		{
			argList.Reverse();
			List<AIVirtualCard> list7 = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothInplayCards, argList, tagOwner, playPtn, situation);
			if (list7 == null || list7.Count <= 0)
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(list7.Max((AIVirtualCard c) => c.Life));
		}
		case AIScriptTokenFuncType.LEADER_MAX_LIFE:
		{
			if (!(argList[0] is AIScriptArgumentToken aIScriptArgumentToken5))
			{
				return new AIScriptNumericToken(0f);
			}
			AIVirtualCard aIVirtualCard3 = null;
			if (aIScriptArgumentToken5.ArgumentType == AIScriptTokenArgType.OPPONENT)
			{
				aIVirtualCard3 = (tagOwner.IsAlly ? field.EnemyClass : field.AllyClass);
			}
			else if (aIScriptArgumentToken5.ArgumentType == AIScriptTokenArgType.ALLY)
			{
				aIVirtualCard3 = (tagOwner.IsAlly ? field.AllyClass : field.EnemyClass);
			}
			if (aIVirtualCard3 == null)
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(aIVirtualCard3.MaxLife);
		}
		case AIScriptTokenFuncType.IS_DISCARD_TARGET:
			argList.Reverse();
			return new AIScriptNumericToken(AIDiscardUtility.IsMatchedDiscardTarget(tagOwner, argList, playPtn, situation) ? 1f : 0f);
		case AIScriptTokenFuncType.DISCARD_COUNT:
			if (!(argList[0] is AIScriptArgumentToken aIScriptArgumentToken2))
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(AIDiscardUtility.GetDiscardCount(aIScriptArgumentToken2.ArgumentType, tagOwner, playPtn, situation));
		case AIScriptTokenFuncType.BOUNCE_COUNT:
			if (argList[0] is AIScriptArgumentToken { ArgumentType: var argumentType })
			{
				return new AIScriptNumericToken(AIBounceSimulationUtility.GetBounceCount(field, situation, argumentType));
			}
			return new AIScriptNumericToken(0f);
		case AIScriptTokenFuncType.EMOTE_PLAY_COUNT:
		{
			int category = (int)argList[0].Value;
			return new AIScriptNumericToken(field.AI.EmoteMng.GetEmotePlayCount(category));
		}
		case AIScriptTokenFuncType.DRAW_COUNT:
			return GetDrawCountResultToken(argList, tagOwner, field, playPtn);
		case AIScriptTokenFuncType.MEMBER_MAX_ATK:
			argList.Reverse();
			return new AIScriptNumericToken(field.GetMemberMaxAttack(tagOwner, argList, playPtn, situation));
		case AIScriptTokenFuncType.IS_BURIAL_RITE:
			argList.Reverse();
			return new AIScriptNumericToken(AIBurialUtility.IsBurialRite(situation, argList, field) ? 1f : 0f);
		case AIScriptTokenFuncType.BURIAL_COUNT:
			if (!(argList[0] is AIScriptArgumentToken aIScriptArgumentToken13))
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(AIBurialRiteSimulationUtility.GetBurialCount(aIScriptArgumentToken13.ArgumentType, field));
		case AIScriptTokenFuncType.RESONANCE_START_COUNT:
			if (!(argList[0] is AIScriptArgumentToken aIScriptArgumentToken11) || !(argList[1] is AIScriptArgumentToken aIScriptArgumentToken12))
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(GetResonanceStartCount(tagOwner, field, aIScriptArgumentToken12.ArgumentType, aIScriptArgumentToken11.ArgumentType));
		case AIScriptTokenFuncType.SUMMON_COUNT:
		{
			if (!AISummonCountUtility.ClassificateSummonCountArgument(argList, out var filters3, out var turnOrGame2, out var playedType))
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(AISummonCountUtility.GetSummonCount(tagOwner, filters3, turnOrGame2, playedType, playPtn, situation));
		}
		case AIScriptTokenFuncType.BUFF_COUNT:
			return new AIScriptNumericToken(AIBuffCountUtility.GetBuffCount(tagOwner, field, situation, playPtn, argList));
		case AIScriptTokenFuncType.IS_HOLDING_BATTLE_SKILL:
			if (!(argList[0] is AIScriptTextToken aIScriptTextToken2))
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(tagOwner.IsHoldingBattleSkill(aIScriptTextToken2.Text) ? 1 : 0);
		case AIScriptTokenFuncType.IS_LEADER_HOLDING_BATTLE_SKILL:
			if (!(argList[0] is AIScriptTextToken aIScriptTextToken))
			{
				return new AIScriptNumericToken(0f);
			}
			return new AIScriptNumericToken(field.AllyClass.IsHoldingBattleSkill(aIScriptTextToken.Text) ? 1 : 0);
		case AIScriptTokenFuncType.STACK_COUNT:
		{
			if (argList.Count <= 0 || !(argList[0] is AIScriptArgumentToken aIScriptArgumentToken7))
			{
				return new AIScriptNumericToken(0f);
			}
			if (aIScriptArgumentToken7.ArgumentType != AIScriptTokenArgType.ALLY && aIScriptArgumentToken7.ArgumentType != AIScriptTokenArgType.OPPONENT)
			{
				return new AIScriptNumericToken(0f);
			}
			bool isAlly = tagOwner.IsAlly ^ (aIScriptArgumentToken7.ArgumentType == AIScriptTokenArgType.OPPONENT);
			return new AIScriptNumericToken(AIStackCountUtility.GetStackCount(field, isAlly));
		}
		case AIScriptTokenFuncType.OWN_DESTROY_COUNT:
		{
			List<AIVirtualCard> ownDestroyedCards = situation.GetOwnDestroyedCards();
			if (ownDestroyedCards == null)
			{
				return new AIScriptNumericToken(0f);
			}
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.MultipleFiltering(ownDestroyedCards, argList, tagOwner, playPtn, situation, isBlockDeadCard: false)?.Count ?? 0);
		}
		case AIScriptTokenFuncType.IS_SELECTABLE:
		{
			float value4 = argList[0].Value;
			argList.RemoveAt(0);
			argList.Reverse();
			return new AIScriptNumericToken(AIIsSelectableUtility.IsSelectable(argList, value4, tagOwner, field, playPtn, situation) ? 1 : 0);
		}
		case AIScriptTokenFuncType.IS_ENEMY_AI_ID:
			return new AIScriptNumericToken((argList[0].Value == (float)EnemyAI.EnemyAIID) ? 1f : 0f);
		case AIScriptTokenFuncType.ADD_HAND_COUNT:
		{
			AIScriptTokenBase aIScriptTokenBase3 = argList[0];
			AIScriptTokenArgType aIScriptTokenArgType = (AIScriptTokenArgType)aIScriptTokenBase3.Value;
			if (aIScriptTokenArgType != AIScriptTokenArgType.TURN && aIScriptTokenArgType != AIScriptTokenArgType.GAME)
			{
				AIConsoleUtility.LogError("ADD_HAND_COUNT Arg Error : Last arg is only valid TURN or GAME");
				return new AIScriptNumericToken(0f);
			}
			argList.Remove(aIScriptTokenBase3);
			argList.Reverse();
			List<AIVirtualCard> list13 = new List<AIVirtualCard>();
			switch (aIScriptTokenArgType)
			{
			case AIScriptTokenArgType.TURN:
			{
				List<AIVirtualCard> list15 = (tagOwner.IsAlly ? field.AllyTurnHandAddedCards : field.EnemyTurnHandAddedCards);
				if (list15 != null && list15.Count > 0)
				{
					list13.AddRange(list15);
				}
				break;
			}
			case AIScriptTokenArgType.GAME:
			{
				List<AIVirtualCard> list14 = (tagOwner.IsAlly ? field.AllyGameHandAddedCards : field.EnemyGameHandAddedCards);
				if (list14 != null && list14.Count > 0)
				{
					list13.AddRange(list14);
				}
				break;
			}
			}
			return new AIScriptNumericToken(AIFilteringUtility.MultipleFiltering(list13, argList, tagOwner, playPtn, situation)?.Count ?? 0);
		}
		case AIScriptTokenFuncType.BASE_COST:
		{
			argList.Reverse();
			List<AIVirtualCard> list10 = AIFilteringUtility.MultipleFiltering(field.CardListSet.AllReferableCards, argList, tagOwner, playPtn, situation, isBlockDeadCard: false);
			if (list10 == null || list10.Count <= 0)
			{
				return new AIScriptNumericToken(0f);
			}
			if (list10.Count > 1)
			{
				AIConsoleUtility.LogError("BASE_COST Func : Error!!! Multiple cards have been selected");
			}
			return new AIScriptNumericToken(list10[0].BaseCost);
		}
		case AIScriptTokenFuncType.RECEIVED_DAMAGE_SUM:
		{
			argList.Reverse();
			List<AIVirtualCard> list8 = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, argList, tagOwner, playPtn, situation);
			if (list8 == null || list8.Count <= 0)
			{
				return new AIScriptNumericToken(0f);
			}
			float num14 = 0f;
			for (int num15 = 0; num15 < field.DamagedCardsByLastAction.Count; num15++)
			{
				Tuple<AIVirtualCard, int> tuple = field.DamagedCardsByLastAction[num15];
				for (int num16 = 0; num16 < list8.Count; num16++)
				{
					AIVirtualCard card = list8[num16];
					if (tuple.first.IsSameCard(card))
					{
						num14 += (float)tuple.second;
					}
				}
			}
			return new AIScriptNumericToken(num14);
		}
		case AIScriptTokenFuncType.IS_PLAYER_CHARA_ID:
			if (argList == null || argList.Count <= 0)
			{
				AIConsoleUtility.LogError("IS_PLAYER_CHARA_ID error!! argList is invalid");
				return new AIScriptNumericToken(0f);
			}
			if (argList[0] is AIScriptNumericToken aIScriptNumericToken)
			{
				int num8 = (int)aIScriptNumericToken.Value;
				return new AIScriptNumericToken((field.AI.PlayerCharaId == num8) ? 1f : 0f);
			}
			return new AIScriptNumericToken(0f);
		case AIScriptTokenFuncType.FUSION_COUNT_IN_GAME:
			argList.Reverse();
			return new AIScriptNumericToken(AIFusionCountUtility.GetFusedCardCountInGame(tagOwner, argList, playPtn, situation, isWithoutSameId: false));
		case AIScriptTokenFuncType.FUSION_NAME_COUNT_IN_GAME:
			argList.Reverse();
			return new AIScriptNumericToken(AIFusionCountUtility.GetFusedCardCountInGame(tagOwner, argList, playPtn, situation, isWithoutSameId: true));
		case AIScriptTokenFuncType.IS_PLAYER_ABILITY_ID:
			if (argList != null && argList.Count > 0 && argList[0] is AIScriptIDToken { ID: var iD2 })
			{
				return new AIScriptNumericToken(field.AI.HasPlayerAbilityId(iD2) ? 1f : 0f);
			}
			return new AIScriptNumericToken(0f);
		case AIScriptTokenFuncType.BANISH_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIBanishSimulationUtility.GetInplayBanishedCount(tagOwner, field, playPtn, situation, argList));
		case AIScriptTokenFuncType.HAND_BANISH_COUNT:
			argList.Reverse();
			return new AIScriptNumericToken(AIBanishSimulationUtility.GetHandBanishedCount(tagOwner, field, playPtn, situation, argList));
		case AIScriptTokenFuncType.IS_BOTH_CLASS:
		{
			if (argList == null || argList.Count <= 1)
			{
				AIConsoleUtility.LogError("IS_BOTH_CLASS error!! arg count is not enough");
				return new AIScriptNumericToken(0f);
			}
			argList.Reverse();
			CardBasePrm.ClanType clanType = CardBasePrm.ClanType.NONE;
			CardBasePrm.ClanType clanType2 = CardBasePrm.ClanType.NONE;
			if (argList[0] is AIScriptArgumentToken aIScriptArgumentToken3 && aIScriptArgumentToken3.IsSideArgType(includeBoth: false))
			{
				bool num = tagOwner.IsAlly == (aIScriptArgumentToken3.ArgumentType == AIScriptTokenArgType.ALLY);
				clanType = (num ? field.AllyClass.Clan : field.EnemyClass.Clan);
				clanType2 = (num ? field.AI.AISubClassType : field.AI.PlayerSubClassType);
				for (int i = 1; i < argList.Count; i++)
				{
					if (argList[i] is AIScriptArgumentToken aIScriptArgumentToken4)
					{
						CardBasePrm.ClanType clanType3 = AIPlayTagInitializingUtility.ConvertTokenArgTypeToClanType(aIScriptArgumentToken4.ArgumentType);
						if (clanType3 != CardBasePrm.ClanType.NONE && (clanType == clanType3 || clanType2 == clanType3))
						{
							return new AIScriptNumericToken(1f);
						}
					}
				}
				return new AIScriptNumericToken(0f);
			}
			AIConsoleUtility.LogError("IS_BOTH_CLASS error!! side parameter is invalid");
			return new AIScriptNumericToken(0f);
		}
		case AIScriptTokenFuncType.EVAL_ATTACK_REMOVE:
			return new AIScriptNumericToken(AIEvalAttackRemoveUtility.EvalAttackRemove(tagOwner, field, playPtn, situation, argList));
		case AIScriptTokenFuncType.ADDED_DECK_COUNT_IN_GAME:
			argList.Reverse();
			return new AIScriptNumericToken(AIFilteringUtility.MultipleFiltering(tagOwner.IsAlly ? field.AllyGameAddUpdateDeckCards : field.EnemyGameAddUpdateDeckCards, argList, tagOwner, playPtn, situation)?.Count ?? 0);
		default:
			return new AIScriptNumericToken(0f);
		}
	}

	private static int GetResonanceStartCount(AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType side, AIScriptTokenArgType gameOrTurn)
	{
		if (side != AIScriptTokenArgType.ALLY)
		{
			_ = 85;
		}
		if (gameOrTurn != AIScriptTokenArgType.GAME)
		{
			_ = 142;
		}
		if ((side == AIScriptTokenArgType.ALLY && tagOwner.IsAlly) || (side == AIScriptTokenArgType.OPPONENT && !tagOwner.IsAlly))
		{
			if (gameOrTurn == AIScriptTokenArgType.GAME)
			{
				return field.AllyGameResonanceStartCount;
			}
			return field.AllyTurnResonanceStartCount;
		}
		if (gameOrTurn == AIScriptTokenArgType.GAME)
		{
			return field.EnemyGameResonanceStartCount;
		}
		return field.EnemyTurnResonanceStartCount;
	}

	private static AIScriptNumericToken GetDrawCountResultToken(List<AIScriptTokenBase> argList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn)
	{
		if (argList.Count <= 0 || argList.Count > 2)
		{
			AIConsoleUtility.LogError("DRAW_COUNT error!! argList.Count == " + argList.Count);
			return new AIScriptNumericToken(0f);
		}
		AIScriptTokenArgType aIScriptTokenArgType = AIScriptTokenArgType.NONE;
		AIScriptTokenArgType range = AIScriptTokenArgType.NONE;
		if (argList[argList.Count - 1] is AIScriptArgumentToken aIScriptArgumentToken)
		{
			aIScriptTokenArgType = aIScriptArgumentToken.ArgumentType;
		}
		if (argList.Count > 1 && argList[argList.Count - 2] is AIScriptArgumentToken aIScriptArgumentToken2)
		{
			range = aIScriptArgumentToken2.ArgumentType;
		}
		if (aIScriptTokenArgType != AIScriptTokenArgType.TURN && aIScriptTokenArgType != AIScriptTokenArgType.GAME)
		{
			AIConsoleUtility.LogError("DRAW_COUNT error!!! period == " + aIScriptTokenArgType);
			return new AIScriptNumericToken(0f);
		}
		return new AIScriptNumericToken(AIDrawCountUtility.GetDrawCount(owner, field, playPtn, aIScriptTokenArgType, range));
	}
}

using System.Collections.Generic;

namespace Wizard;

public static class AIBarrierSimulationUtility
{

	public static AIScriptTokenArgType[] LegalDamageType = new AIScriptTokenArgType[4]
	{
		AIScriptTokenArgType.ALL_DAMAGE,
		AIScriptTokenArgType.SKILL_DAMAGE,
		AIScriptTokenArgType.ATTACK_DAMAGE,
		AIScriptTokenArgType.SPELL_DAMAGE
	};

	public static AIScriptTokenArgType[] LegalStopTimingList = new AIScriptTokenArgType[7]
	{
		AIScriptTokenArgType.WHEN_ALLY_TURNEND,
		AIScriptTokenArgType.WHEN_ALLY_TURNSTART,
		AIScriptTokenArgType.WHEN_OPPONENT_TURNEND,
		AIScriptTokenArgType.WHEN_OPPONENT_TURNSTART,
		AIScriptTokenArgType.WHEN_DAMAGED,
		AIScriptTokenArgType.WHEN_LEAVE,
		AIScriptTokenArgType.NONE
	};

	public static ulong BARRIER_AMOUNT_HASH_COEFFICIENT = 137uL;

	public static ulong DAMAGE_TYPE_HASH_COEFFICIENT = 199uL;

	public static ulong BARRIER_TYPE_HASH_COEFFICIENT = 223uL;

	public static ulong DAMAGE_CLIP_RANGE_COEFFICIENT = 1109uL;

	private static readonly ulong[] STOP_TIMING_HASH_COEFFICIENT_ARRAY = new ulong[5] { 907uL, 911uL, 919uL, 929uL, 937uL };

	public static void AddShieldToAll(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType damageTypeArgument, AIScriptTokenArgType stopTimingArgument)
	{
		AIDamageType damageTypeFromArgType = GetDamageTypeFromArgType(damageTypeArgument);
		AIBarrierStopTiming barrierStopTimingFromArgType = GetBarrierStopTimingFromArgType(stopTimingArgument, tagOwner);
		for (int i = 0; i < targets.Count; i++)
		{
			AddShieldToSingle(targets[i], tagOwner, field, damageTypeFromArgType, barrierStopTimingFromArgType);
		}
	}

	private static void AddShieldToSingle(AIVirtualCard card, AIVirtualCard tagOwner, AIVirtualField field, AIDamageType damageType, AIBarrierStopTiming stopTiming)
	{
		AIShieldInfo aIShieldInfo = new AIShieldInfo(damageType, stopTiming);
		card.BarrierInfoCollection.AddBarrierInfo(aIShieldInfo);
		RegisterBarrierStopPreprocess(card, tagOwner, field, stopTiming, aIShieldInfo.Hash);
	}

	public static void AddMultipleStopTimingShieldToAll(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType damageTypeArgument, List<AIScriptTokenArgType> stopTimingArgumentList)
	{
		AIDamageType damageTypeFromArgType = GetDamageTypeFromArgType(damageTypeArgument);
		List<AIBarrierStopTiming> barrierStopTimingListFromArgType = GetBarrierStopTimingListFromArgType(stopTimingArgumentList, tagOwner);
		for (int i = 0; i < targets.Count; i++)
		{
			AddMultipleStopTimingShieldToSingle(targets[i], tagOwner, field, damageTypeFromArgType, barrierStopTimingListFromArgType);
		}
	}

	public static void AddMultipleStopTimingShieldToTarget(AISituationInfo situation, AIScriptTokenArgType whichTarget, AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType damageTypeArgument, List<AIScriptTokenArgType> stopTimingArgumentList)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("AddMultipleStopTimingShieldToTarget error!! No target!!!!!");
		}
		else
		{
			AddMultipleStopTimingShieldToAll(situationTarget.Targets, tagOwner, field, damageTypeArgument, stopTimingArgumentList);
		}
	}

	private static void AddMultipleStopTimingShieldToSingle(AIVirtualCard card, AIVirtualCard tagOwner, AIVirtualField field, AIDamageType damageType, List<AIBarrierStopTiming> stopTimingList)
	{
		AIShieldInfo aIShieldInfo = new AIShieldInfo(damageType, stopTimingList);
		card.BarrierInfoCollection.AddBarrierInfo(aIShieldInfo);
		if (stopTimingList != null && stopTimingList.Count > 0)
		{
			for (int i = 0; i < stopTimingList.Count; i++)
			{
				RegisterBarrierStopPreprocess(card, tagOwner, field, stopTimingList[i], aIShieldInfo.Hash);
			}
		}
	}

	public static void PseudoAddMultipleStopTimingShieldToSingle(AIBarrierPseudoSimulationInfo info, AIDamageType damageType, List<AIBarrierStopTiming> stopTimingList)
	{
		AIShieldInfo barrier = new AIShieldInfo(damageType, stopTimingList);
		info.AddBarrierInfo(barrier);
	}

	public static void AddDamageCutToAll(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType damageTypeArgument, AIScriptTokenArgType stopTimingArgument, int cutAmount)
	{
		AIDamageType damageTypeFromArgType = GetDamageTypeFromArgType(damageTypeArgument);
		AIBarrierStopTiming barrierStopTimingFromArgType = GetBarrierStopTimingFromArgType(stopTimingArgument, tagOwner);
		for (int i = 0; i < targets.Count; i++)
		{
			AddDamageCutToSingle(targets[i], tagOwner, field, damageTypeFromArgType, barrierStopTimingFromArgType, cutAmount);
		}
	}

	private static void AddDamageCutToSingle(AIVirtualCard card, AIVirtualCard tagOwner, AIVirtualField field, AIDamageType damageType, AIBarrierStopTiming stopTiming, int cutAmount)
	{
		AIDamageCutInfo aIDamageCutInfo = new AIDamageCutInfo(cutAmount, damageType, stopTiming);
		card.BarrierInfoCollection.AddBarrierInfo(aIDamageCutInfo);
		RegisterBarrierStopPreprocess(card, tagOwner, field, stopTiming, aIDamageCutInfo.Hash);
	}

	public static void AddDamageClipToAll(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType damageTypeArgument, AIScriptTokenArgType stopTimingArgument, int clipAmount, int clipRange = 9999)
	{
		AIDamageType damageTypeFromArgType = GetDamageTypeFromArgType(damageTypeArgument);
		AIBarrierStopTiming barrierStopTimingFromArgType = GetBarrierStopTimingFromArgType(stopTimingArgument, tagOwner);
		for (int i = 0; i < targets.Count; i++)
		{
			AddDamageClipToSingle(targets[i], tagOwner, field, damageTypeFromArgType, barrierStopTimingFromArgType, clipAmount, clipRange);
		}
	}

	private static void AddDamageClipToSingle(AIVirtualCard card, AIVirtualCard tagOwner, AIVirtualField field, AIDamageType damageType, AIBarrierStopTiming stopTiming, int clipAmount, int clipRange)
	{
		AIDamageClippingInfo aIDamageClippingInfo = new AIDamageClippingInfo(clipAmount, clipRange, damageType, stopTiming);
		card.BarrierInfoCollection.AddBarrierInfo(aIDamageClippingInfo);
		RegisterBarrierStopPreprocess(card, tagOwner, field, stopTiming, aIDamageClippingInfo.Hash);
	}

	public static void PseudoAddDamageClipToSingle(AIBarrierPseudoSimulationInfo info, AIDamageType damageType, AIBarrierStopTiming stopTiming, int clipAmount, int clipRange = 9999)
	{
		AIDamageClippingInfo barrier = new AIDamageClippingInfo(clipAmount, clipRange, damageType, stopTiming);
		info.AddBarrierInfo(barrier);
	}

	public static void AddLifeLowerLimitToAll(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType damageTypeArgToken, AIScriptTokenArgType stopTimingArgToken)
	{
		AIDamageType damageTypeFromArgType = GetDamageTypeFromArgType(damageTypeArgToken);
		AIBarrierStopTiming barrierStopTimingFromArgType = GetBarrierStopTimingFromArgType(stopTimingArgToken, tagOwner);
		for (int i = 0; i < targets.Count; i++)
		{
			AddLifeLowerLimitToSingle(targets[i], tagOwner, field, damageTypeFromArgType, barrierStopTimingFromArgType);
		}
	}

	public static void AddLifeLowerLimitToSingle(AIVirtualCard target, AIVirtualCard tagOwner, AIVirtualField field, AIDamageType damageType, AIBarrierStopTiming stopTiming)
	{
		AILifeLowerLimitInfo aILifeLowerLimitInfo = new AILifeLowerLimitInfo(damageType, stopTiming);
		target.BarrierInfoCollection.AddBarrierInfo(aILifeLowerLimitInfo);
		RegisterBarrierStopPreprocess(target, tagOwner, field, stopTiming, aILifeLowerLimitInfo.Hash);
	}

	private static void RegisterBarrierStopPreprocess(AIVirtualCard target, AIVirtualCard tagOwner, AIVirtualField field, AIBarrierStopTiming stopTiming, ulong barrierHash)
	{
		AIBarrierStopPreprocessOption option = new AIBarrierStopPreprocessOption(target, barrierHash);
		switch (stopTiming)
		{
		case AIBarrierStopTiming.AllyTurnEnd:
			field.TagPreprocessContainer.AppendAllyTurnEndStopInfo(option);
			break;
		case AIBarrierStopTiming.OpponentTurnEnd:
			field.TagPreprocessContainer.AppendOpponentTurnEndStopInfo(option);
			break;
		case AIBarrierStopTiming.AllyTurnStart:
			field.TagPreprocessContainer.AppendAllyTurnStartStopInfo(option);
			break;
		case AIBarrierStopTiming.OpponentTurnStart:
			field.TagPreprocessContainer.AppendOpponentTurnStartStopInfo(option);
			break;
		case AIBarrierStopTiming.WhenLeaveStop:
			field.TagPreprocessContainer.AppendLeaveStopInfo(option, tagOwner);
			break;
		case AIBarrierStopTiming.AfterDamage:
			field.TagPreprocessContainer.AppendAfterDamageStopInfo(option);
			break;
		}
	}

	public static AIDamageType GetDamageTypeFromArgType(AIScriptTokenArgType damageTypeArgument)
	{
		switch (damageTypeArgument)
		{
		case AIScriptTokenArgType.SKILL_DAMAGE:
			return AIDamageType.Skill;
		case AIScriptTokenArgType.ATTACK_DAMAGE:
			return AIDamageType.Attack;
		case AIScriptTokenArgType.SPELL_DAMAGE:
			return AIDamageType.Spell;
		case AIScriptTokenArgType.ALL_DAMAGE:
			return AIDamageType.All;
		default:
			AIConsoleUtility.LogError("AIBarrierSimulationUtility.GetDamageTypeFromArgType() error!! argument " + damageTypeArgument.ToString() + " is illegal!!!!!");
			return AIDamageType.All;
		}
	}

	public static List<AIBarrierStopTiming> GetBarrierStopTimingListFromArgType(List<AIScriptTokenArgType> arguments, AIVirtualCard tagOwner)
	{
		List<AIBarrierStopTiming> list = null;
		for (int i = 0; i < arguments.Count; i++)
		{
			AIBarrierStopTiming barrierStopTimingFromArgType = GetBarrierStopTimingFromArgType(arguments[i], tagOwner);
			if (barrierStopTimingFromArgType != AIBarrierStopTiming.None)
			{
				list = AIParamQuery.AddElementToList(barrierStopTimingFromArgType, list);
			}
		}
		return list;
	}

	public static AIBarrierStopTiming GetBarrierStopTimingFromArgType(AIScriptTokenArgType argument, AIVirtualCard tagOwner)
	{
		switch (argument)
		{
		case AIScriptTokenArgType.NONE:
			return AIBarrierStopTiming.None;
		case AIScriptTokenArgType.WHEN_LEAVE:
			return AIBarrierStopTiming.WhenLeaveStop;
		case AIScriptTokenArgType.WHEN_DAMAGED:
			return AIBarrierStopTiming.AfterDamage;
		case AIScriptTokenArgType.WHEN_ALLY_TURNEND:
			if (!tagOwner.IsAlly)
			{
				return AIBarrierStopTiming.OpponentTurnEnd;
			}
			return AIBarrierStopTiming.AllyTurnEnd;
		case AIScriptTokenArgType.WHEN_OPPONENT_TURNEND:
			if (!tagOwner.IsAlly)
			{
				return AIBarrierStopTiming.AllyTurnEnd;
			}
			return AIBarrierStopTiming.OpponentTurnEnd;
		case AIScriptTokenArgType.WHEN_ALLY_TURNSTART:
			if (!tagOwner.IsAlly)
			{
				return AIBarrierStopTiming.OpponentTurnStart;
			}
			return AIBarrierStopTiming.AllyTurnStart;
		case AIScriptTokenArgType.WHEN_OPPONENT_TURNSTART:
			if (!tagOwner.IsAlly)
			{
				return AIBarrierStopTiming.AllyTurnStart;
			}
			return AIBarrierStopTiming.OpponentTurnStart;
		default:
			AIConsoleUtility.LogError("AIBarrierSimulationUtility.GetBarrierStopTimingFromArgType() error!! argument " + argument.ToString() + " is illegal!!!!!");
			return AIBarrierStopTiming.None;
		}
	}

	public static ulong CalculateBarrierInfoBaseHash(AIDamageType damageType, AIBarrierType barrierType, List<AIBarrierStopTiming> stopTimingList)
	{
		ulong num = 0uL;
		num += (ulong)((long)damageType * (long)DAMAGE_TYPE_HASH_COEFFICIENT);
		num += (ulong)((long)barrierType * (long)BARRIER_TYPE_HASH_COEFFICIENT);
		if (stopTimingList != null && stopTimingList.Count > 0)
		{
			for (int i = 0; i < stopTimingList.Count; i++)
			{
				int num2 = i % 5;
				num += (ulong)((long)stopTimingList[i] * (long)STOP_TIMING_HASH_COEFFICIENT_ARRAY[num2]);
			}
		}
		return num;
	}

	public static ulong CalculateBarrierInfoBaseHash(AIDamageType damageType, AIBarrierType barrierType, AIBarrierStopTiming stopTiming)
	{
		return (ulong)(0 + (long)damageType * (long)DAMAGE_TYPE_HASH_COEFFICIENT + (long)barrierType * (long)BARRIER_TYPE_HASH_COEFFICIENT + (long)stopTiming * (long)STOP_TIMING_HASH_COEFFICIENT_ARRAY[0]);
	}

	public static ulong CalculateDamageCutInfoHash(AIDamageType damageType, AIBarrierType barrierType, List<AIBarrierStopTiming> stopTimingList, int cutAmount)
	{
		return CalculateBarrierInfoBaseHash(damageType, barrierType, stopTimingList) + (ulong)((long)cutAmount * (long)BARRIER_AMOUNT_HASH_COEFFICIENT);
	}

	public static ulong CalculateDamageCutInfoHash(AIDamageType damageType, AIBarrierType barrierType, AIBarrierStopTiming stopTiming, int cutAmount)
	{
		return CalculateBarrierInfoBaseHash(damageType, barrierType, stopTiming) + (ulong)((long)cutAmount * (long)BARRIER_AMOUNT_HASH_COEFFICIENT);
	}

	public static ulong CalculateDamageClipInfoHash(AIDamageType damageType, AIBarrierType barrierType, List<AIBarrierStopTiming> stopTimingList, int clipAmount, int clipRange)
	{
		return (ulong)((long)CalculateBarrierInfoBaseHash(damageType, barrierType, stopTimingList) + (long)clipAmount * (long)BARRIER_AMOUNT_HASH_COEFFICIENT + (long)clipRange * (long)DAMAGE_CLIP_RANGE_COEFFICIENT);
	}

	public static ulong CalculateDamageClipInfoHash(AIDamageType damageType, AIBarrierType barrierType, AIBarrierStopTiming stopTiming, int clipAmount, int clipRange)
	{
		return (ulong)((long)CalculateBarrierInfoBaseHash(damageType, barrierType, stopTiming) + (long)clipAmount * (long)BARRIER_AMOUNT_HASH_COEFFICIENT + (long)clipRange * (long)DAMAGE_CLIP_RANGE_COEFFICIENT);
	}
}

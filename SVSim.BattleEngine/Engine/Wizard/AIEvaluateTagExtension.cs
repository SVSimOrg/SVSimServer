using System.Collections.Generic;

namespace Wizard;

public static class AIEvaluateTagExtension
{
	public static int GetChoiceTransformCost(this AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		return AIChoiceTransformUtility.GetChoiceTransformCost(card, field, playPtn);
	}

	public static bool IsEnhanced(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null)
		{
			return false;
		}
		return AIEnhanceUtility.IsEnhanced(tagOwner, field, playPtn, situation);
	}

	public static bool IsAccelerated(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (tagOwner == null)
		{
			return false;
		}
		return AIAccelerateUtility.IsAccelerate(tagOwner, field, playPtn, situation);
	}

	public static bool IsCrystalize(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (tagOwner == null)
		{
			return false;
		}
		return AICrystalizeUtility.IsCrystalize(tagOwner, field, playPtn, situation);
	}

	public static float EvaluateValueOnField(this AIVirtualCard card, List<int> playPtn, AISituationInfo situation, bool useStyle, bool doesUseLostLife = true, bool useOthersTag = true, bool useIgnoreInBattle = false)
	{
		if (card.IsUnit)
		{
			float num = card.EvaluateUnitBase(playPtn, useStyle, doesUseLostLife) + card.GetFieldBonus(playPtn);
			float num2 = card.EvaluateAllBattleBonusRate(playPtn, useOthersTag, useIgnoreInBattle, situation);
			if (useStyle)
			{
				num2 *= card.SelfField.StyleQuery.GetUnitRate(card.SelfField, card, playPtn);
			}
			return num * num2;
		}
		return card.GetFieldBonus(playPtn);
	}

	public static float EvaluateUnitBase(this AIVirtualCard card, List<int> playPtn, bool useStyle, bool doseUseLostLife = true)
	{
		float num = card.Attack * card.MaxAttackableCount;
		num += (doseUseLostLife ? ((float)card.Life) : ((float)card.DefLife - (float)(card.DefLife - card.Life) * 0.01f));
		if (useStyle)
		{
			num += card.SelfField.StyleQuery.GetUnitBonus(card.SelfField, card, playPtn);
		}
		return num;
	}

	public static float EvaluateBounceValue(this AIVirtualCard card, List<int> playPtn, int restPp, bool useStyle = true)
	{
		float num = 0f;
		num = card.EvaluateValueOnField(playPtn, null, useStyle, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true);
		if (card.IsAlly)
		{
			if (card.Cost > restPp)
			{
				return 0f - num + card.GetBounceBonus() + card.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true);
			}
			return num - (float)(card.Attack - card.BaseParameter.DefaultAttack) - (float)(card.Life - card.BaseParameter.DefaultLife) - num + card.GetBounceBonus() + card.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true);
		}
		return num + card.EvaluateBreakValue(playPtn, useIgnoreBreak: true) - card.GetBounceBonus() - card.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true);
	}

	public static float GetBounceBonus(this AIVirtualCard tagOwner)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BounceBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.BounceBonusTags.GetBounceBonus(tagOwner.SelfField, tagOwner);
	}

	public static float GetGetOffTokenValue(this AIVirtualCard tagOwner, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (!tagOwner.IsGetOn || useIgnoreInBattle)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		AIVirtualCard tokenFromId = selfField.AI.tokenManager.GetTokenFromId(tagOwner.GetOnCardId, tagOwner.IsAlly, selfField);
		if (tokenFromId != null)
		{
			num += tokenFromId.EvaluateValueOnField(playPtn, null, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle);
		}
		return num;
	}

	public static float EvaluateLeaveValue(this AIVirtualCard card, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation = null)
	{
		float num = card.GetLeaveBonus(playPtn, useIgnoreInBattle, situation);
		if (useIgnoreInBattle)
		{
			num += card.GetLeaveTokenBonus(playPtn, useIgnoreInBattle, situation);
		}
		return num;
	}

	public static float GetPriority(this AIVirtualCard card, List<int> playPtn)
	{
		if (card == null)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(card, card, AIOperationType.PLAY);
		if (card.AIData != null)
		{
			num += card.AIData.PriorityExpr.EvalArg(card, playPtn, card.SelfField, situation);
		}
		if (card.TagCollectionContainer.HasTag(AIPlayTagType.Priority))
		{
			num += card.TagCollectionContainer.PriorityTags.GetPriorityBonus(card, playPtn, situation);
		}
		return num;
	}

	public static float EvaluatePlayValue(this AIVirtualCard card, List<int> playPtn, AISituationInfo situation = null)
	{
		if (card == null)
		{
			return 0f;
		}
		AIVirtualField selfField = card.SelfField;
		return (card.AIData.PlayBonusExpr.EvalArg(card, playPtn, selfField, situation) + card.GetPlayBonus(playPtn, situation) + card.GetFanfareBonus(playPtn, situation) + AIEvaluateBonusFromOhterUtility.GetAllyPlayBonus(card, playPtn, situation) + AIEvaluateBonusFromOhterUtility.GetEnemyPlayBonus(card, playPtn, situation)) * card.GetPlayBonusRate(playPtn, situation);
	}

	public static float GetPlayBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.PlayBonusTags.GetPlayBonus(tagOwner, playPtn, situation);
	}

	public static float GetPlayBonusRate(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayBonusRate))
		{
			return 1f;
		}
		return tagOwner.TagCollectionContainer.PlayBonusRateTags.GetPlayBonusRate(tagOwner, playPtn, situation);
	}

	public static float GetFanfareBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.FanfareBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.FanfareBonusTags.GetFanfareBonus(tagOwner, playPtn, situation);
	}

	public static bool IsEnableIgnoreFanfareBonus(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.IgnoreFanfareBonus))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.IgnoreFanfareBonusTags.IsEnableIgnoreFanfareBonus(tagOwner, playPtn);
	}

	public static int GetHandPlusCount(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.HandPlus))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.HandPlusTags.GetHandPlus(tagOwner, playPtn);
	}

	public static float GetAllyPlayBonus(this AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation, ref float currentUseMinValue)
	{
		if (tagOwner.IsDead || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.AllyPlayBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.AllyPlayBonusTags.GetAllyPlayBonus(tagOwner, targetCard, playPtn, situation, ref currentUseMinValue);
	}

	public static float GetEnemyPlayBonus(this AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.EnemyPlayBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.EnemyPlayBonusTags.GetEnemyPlayBonus(tagOwner, targetCard, playPtn, situation);
	}

	public static int GetCostBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.CostBonus))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.CostBonusTags.GetCostBonus(tagOwner, playPtn, situation);
	}

	public static int GerWhenPlayRecoverPp(this AIVirtualCard tagOwner, AIVirtualCard playCard, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayRecoverPP))
		{
			PlayTagCollection playTags = tagOwner.TagCollectionContainer.PlayTags;
			num += playTags.GetWhenPlayRecoverPp(tagOwner, playPtn, situation);
		}
		if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.FanfareRecoverPp))
		{
			FanfareTagCollection fanfareTags = tagOwner.TagCollectionContainer.FanfareTags;
			num += fanfareTags.GetWhenPlayRecoverPp(tagOwner, playPtn, situation);
		}
		return num;
	}

	public static int GetOtherPlayRecoverPp(this AIVirtualCard tagOwner, AIVirtualCard playCard, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OtherPlayRecoverPp))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.OtherPlayTags.GetTotalOtherPlayRecoverPp(tagOwner, playCard, field, playPtn, situation);
	}

	public static int GetPlayDrawCount(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayDraw))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.PlayDrawTags.GetPlayDrawCount(tagOwner, playPtn, situation);
	}

	public static float GetPlayLimit(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayLimit))
		{
			return float.MinValue;
		}
		return tagOwner.TagCollectionContainer.PlayLimitTags.GetPlayLimit(tagOwner, playPtn);
	}

	public static float GetPlayPtnBonus(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayptnBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.PlayptnBonusTags.GetPlayPtnBonus(tagOwner, playPtn);
	}

	public static float GetFieldBonus(this AIVirtualCard card, List<int> playPtn)
	{
		return 0f + card.GetBattleBonus(playPtn) + AIEvaluateBonusFromOhterUtility.GetOtherBattleBonus(card, playPtn);
	}

	public static float GetAttackBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.AttackBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.AttackBonusTags.GetAttackBonus(tagOwner, playPtn, situation);
	}

	public static float GetBattleBonus(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		float num = 0f;
		if (tagOwner.AIData != null && !tagOwner.AIData.BattleBonusExpr.IsHoldingEVAL() && !tagOwner.IsSkillLost)
		{
			num = tagOwner.AIData.BattleBonusExpr.EvalArg(tagOwner, playPtn, tagOwner.SelfField);
		}
		if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BattleBonus))
		{
			num += tagOwner.TagCollectionContainer.BattleBonusTags.GetBattleBonus(tagOwner, playPtn);
		}
		return num;
	}

	public static float GetOtherBattleBonusFromOneCard(this AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn)
	{
		if (targetCard.IsSameCard(tagOwner))
		{
			return 0f;
		}
		float num = 0f;
		if (tagOwner.IsAlly == targetCard.IsAlly)
		{
			if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.MemberBattleBonus))
			{
				num += tagOwner.TagCollectionContainer.MemberBattleBonusTags.GetMemberBattleBonus(tagOwner, targetCard, playPtn);
			}
		}
		else if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.EnemyBattleBonus))
		{
			num += tagOwner.TagCollectionContainer.EnemyBattleBonusTags.GetEnemyBattleBonus(tagOwner, targetCard, playPtn);
		}
		return num;
	}

	public static float EvaluateAllBattleBonusRate(this AIVirtualCard targetCard, List<int> playPtn, bool useOthersTag, bool useIgnoreInBattle, AISituationInfo situation)
	{
		float num = 1f;
		num *= targetCard.GetBattleBonusRate(playPtn);
		if (useOthersTag)
		{
			num *= AIEvaluateBonusFromOhterUtility.GetOtherBattleBonusRate(targetCard, playPtn, useIgnoreInBattle, situation);
		}
		return num;
	}

	public static float GetBattleBonusRate(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BattleBonusRate))
		{
			return 1f;
		}
		return tagOwner.TagCollectionContainer.BattleBonusRateTags.GetBattleBonusRate(tagOwner, playPtn);
	}

	public static float GetOtherBattleBonusRateFromOneCard(this AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation)
	{
		if (tagOwner.IsSameCard(targetCard))
		{
			return 1f;
		}
		float result = 1f;
		if (tagOwner.IsAlly == targetCard.IsAlly)
		{
			if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.MemberBattleBonusRate))
			{
				result = tagOwner.TagCollectionContainer.MemberBattleBonusRateTags.GetMemberBattleBonusRate(tagOwner, targetCard, playPtn, useIgnoreInBattle, situation);
			}
		}
		else if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.EnemyBattleBonusRate))
		{
			result = tagOwner.TagCollectionContainer.EnemyBattleBonusRateTags.GetEnemyBattleBonusRate(tagOwner, targetCard, playPtn, useIgnoreInBattle, situation);
		}
		return result;
	}

	public static float EvaluateAllEvoBonus(this AIVirtualCard card, List<int> playPtn, AISituationInfo situation)
	{
		return 0f + card.GetEvoBonus(playPtn, situation) + AIEvaluateBonusFromOhterUtility.GetAllOtherEvoBonus(situation, playPtn);
	}

	public static float GetEvoBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.EvoBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.EvoBonusTags.GetEvoBonus(tagOwner, playPtn, situation);
	}

	public static float GetMemberEvoBonus(this AIVirtualCard tagOwner, AISituationInfo situation, List<int> playPtn)
	{
		if (tagOwner == null || tagOwner.IsDead || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.MemberEvoBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.MemberEvoBonusTags.GetMemberEvoBonus(tagOwner, situation, playPtn);
	}

	public static float GetEnemyEvoBonus(this AIVirtualCard tagOwner, AISituationInfo situation, List<int> playPtn)
	{
		if (tagOwner == null || tagOwner.IsDead || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.EnemyEvoBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.EnemyEvoBonusTags.GetEnemyEvoBonus(tagOwner, situation, playPtn);
	}

	public static float EvaluateBreakValue(this AIVirtualCard card, List<int> playPtn, bool useIgnoreBreak)
	{
		float num = card.GetAllBreakBonus(playPtn, useIgnoreBreak);
		if (useIgnoreBreak)
		{
			num += card.GetLastwordTokenBonus(playPtn, useIgnoreBreak);
		}
		return num;
	}

	public static bool HasBreakBonus(this AIVirtualCard card, AIVirtualField field)
	{
		if (card.TagCollectionContainer.HasTag(AIPlayTagType.Break) || field.CardListSet.HasOtherBreakBonusHolder)
		{
			return true;
		}
		return false;
	}

	public static float GetAllBreakBonus(this AIVirtualCard tagOwner, List<int> playPtn, bool useIgnoreInBattle)
	{
		return tagOwner.GetBreakBonus(playPtn, useIgnoreInBattle) + AIEvaluateBonusFromOhterUtility.GetOtherBreakBonus(tagOwner, tagOwner.SelfField, playPtn, useIgnoreInBattle);
	}

	public static float GetBreakBonus(this AIVirtualCard tagOwner, List<int> playPtn, bool useIgnoreBreak)
	{
		if (useIgnoreBreak && tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.IgnoreBreak) && tagOwner.TagCollectionContainer.IgnoreBreakTags.IsIgnoreBreak(tagOwner, playPtn))
		{
			return 0f;
		}
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.Break))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.BreakBonusTags.GetBreakBonus(tagOwner, playPtn);
	}

	public static float GetOtherBreakBonus(this AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, List<int> playPtn, bool useIgnoreBreak)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OtherBreakBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.OtherBreakBonusTags.GetOtherBreakBonus(tagOwner, target, field, playPtn, useIgnoreBreak);
	}

	public static float GetLastwordTokenBonus(this AIVirtualCard tagOwner, List<int> playPtn, bool useIgnoreInBattle)
	{
		float num = 0f;
		AITokenIdCollection lastwordTokenTargetSideAndIds = tagOwner.GetLastwordTokenTargetSideAndIds(playPtn);
		if (lastwordTokenTargetSideAndIds == null)
		{
			return num;
		}
		if (lastwordTokenTargetSideAndIds.HasAllyToken)
		{
			for (int i = 0; i < lastwordTokenTargetSideAndIds.AllyTokenIdList.Count; i++)
			{
				int tokenId = lastwordTokenTargetSideAndIds.AllyTokenIdList[i].TokenId;
				AIVirtualCard tokenFromId = tagOwner.SelfField.AI.tokenManager.GetTokenFromId(tokenId, isAlly: true, tagOwner.SelfField);
				if (tokenFromId != null)
				{
					num += tokenFromId.EvaluateValueOnField(playPtn, null, useStyle: true, doesUseLostLife: true, useOthersTag: false, useIgnoreInBattle) * (tagOwner.IsAlly ? 1f : (-1f));
				}
				else
				{
					AIConsoleUtility.LogError("GetLastwordTokenBonus: tokenCard is null!id = " + tokenId);
				}
			}
		}
		if (lastwordTokenTargetSideAndIds.HasOpponentToken)
		{
			for (int j = 0; j < lastwordTokenTargetSideAndIds.OpponentTokenIdList.Count; j++)
			{
				int tokenId2 = lastwordTokenTargetSideAndIds.OpponentTokenIdList[j].TokenId;
				AIVirtualCard tokenFromId2 = tagOwner.SelfField.AI.tokenManager.GetTokenFromId(tokenId2, isAlly: false, tagOwner.SelfField);
				if (tokenFromId2 != null)
				{
					num += tokenFromId2.EvaluateValueOnField(playPtn, null, useStyle: true, doesUseLostLife: true, useOthersTag: false, useIgnoreInBattle) * ((!tagOwner.IsAlly) ? 1f : (-1f));
				}
				else
				{
					AIConsoleUtility.LogError("GetLastwordTokenBonus: tokenCard is null!id = " + tokenId2);
				}
			}
		}
		return num;
	}

	public static float GetBanishBonus(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BanishBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.BanishBonusTags.GetBanishBonus(tagOwner, playPtn);
	}

	public static float GetOtherBanishBonus(this AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OtherBanishBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.OtherBanishBonusTags.GetOtherBanishBonus(tagOwner, target, field, playPtn, useIgnoreInBattle);
	}

	public static float GetAllBanishBonus(this AIVirtualCard card, List<int> playPtn, bool useIgnoreInBattle)
	{
		return 0f + card.GetBanishBonus(playPtn) + AIEvaluateBonusFromOhterUtility.GetOtherBanishBonus(card, card.SelfField, playPtn, useIgnoreInBattle);
	}

	public static float GetLeaveBonus(this AIVirtualCard tagOwner, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation = null)
	{
		if (tagOwner == null)
		{
			return 0f;
		}
		float num = 0f;
		if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.LeaveBonus))
		{
			num = tagOwner.TagCollectionContainer.LeaveBonusTags.GetLeaveBonus(tagOwner, playPtn, situation, useIgnoreInBattle);
		}
		return num + tagOwner.GetGetOffTokenValue(playPtn, useIgnoreInBattle);
	}

	public static float GetOtherLeaveBonus(this AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OtherLeaveBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.OtherLeaveBonusTags.GetOtherLeaveBonus(tagOwner, target, field, playPtn, useIgnoreInBattle);
	}

	public static float GetAllLeaveBonus(this AIVirtualCard tagOwner, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation = null)
	{
		return 0f + tagOwner.GetLeaveBonus(playPtn, useIgnoreInBattle, situation) + AIEvaluateBonusFromOhterUtility.GetOtherLeaveBonus(tagOwner, tagOwner.SelfField, playPtn, useIgnoreInBattle);
	}

	public static float GetLeaveTokenBonus(this AIVirtualCard card, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation)
	{
		float num = 0f;
		if (card == null)
		{
			return num;
		}
		AIVirtualField selfField = card.SelfField;
		_ = selfField.ParamQuery;
		List<AITokenInformation> leaveTokenIds = card.GetLeaveTokenIds(selfField, playPtn, situation);
		if (leaveTokenIds == null)
		{
			return num;
		}
		for (int i = 0; i < leaveTokenIds.Count; i++)
		{
			AIVirtualCard tokenFromId = selfField.AI.tokenManager.GetTokenFromId(leaveTokenIds[i].TokenId, card.IsAlly, selfField);
			if (tokenFromId != null)
			{
				num += tokenFromId.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle);
			}
		}
		return num;
	}

	public static float GetDiscardedBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isIgnroeInBattle)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.DiscardedBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.DiscardedBonusTags.GetDiscardedBonus(tagOwner, playPtn, situation, isIgnroeInBattle);
	}

	public static float GetAllyDiscardBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool useIgnoreInBattle)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.AllyDiscardBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.AfterDiscardTags.GetAllyDiscardBonus(tagOwner, playPtn, situation, useIgnoreInBattle);
	}

	public static AITokenIdCollection GetLastwordTokenTargetSideAndIds(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.LastwordToken))
		{
			return null;
		}
		return tagOwner.TagCollectionContainer.LastwordTags.GetLastwordTokenIds(tagOwner, tagOwner.SelfField, playPtn);
	}

	public static int GetAllyLastwordTokenCount(this AIVirtualCard owner, List<int> playPtn)
	{
		if (!owner.TagCollectionContainer.HasTag(AIPlayTagType.LastwordToken))
		{
			return 0;
		}
		int num = 0;
		AITokenIdCollection lastwordTokenIds = owner.TagCollectionContainer.LastwordTags.GetLastwordTokenIds(owner, owner.SelfField, playPtn);
		if (lastwordTokenIds != null)
		{
			if (owner.IsAlly && lastwordTokenIds.HasAllyToken)
			{
				num += lastwordTokenIds.AllyTokenIdList.Count;
			}
			if (!owner.IsAlly && lastwordTokenIds.HasOpponentToken)
			{
				num += lastwordTokenIds.OpponentTokenIdList.Count;
			}
		}
		return num;
	}

	public static List<AITokenInformation> GetLeaveTokenIds(this AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!card.TagCollectionContainer.HasTag(AIPlayTagType.LeaveToken))
		{
			return null;
		}
		return card.TagCollectionContainer.LeaveTags.GetLeaveTokenIds(card, field, playPtn, situation);
	}

	public static List<AITokenInformation> GetDiscardedTokenIds(this AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!card.TagCollectionContainer.HasTag(AIPlayTagType.DiscardedToken))
		{
			return null;
		}
		return card.TagCollectionContainer.DiscardedTags.GetDiscardedTokenIds(card, field, playPtn, situation);
	}

	public static float GetDiscardedTokenBonus(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool useIgnoreInBattle)
	{
		List<AITokenInformation> discardedTokenIds = tagOwner.GetDiscardedTokenIds(field, playPtn, situation);
		if (discardedTokenIds == null || discardedTokenIds.Count <= 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < discardedTokenIds.Count; i++)
		{
			AIVirtualCard tokenFromId = tagOwner.SelfField.AI.tokenManager.GetTokenFromId(discardedTokenIds[i].TokenId, tagOwner.IsAlly, field);
			if (tokenFromId != null)
			{
				num += tokenFromId.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: false, useIgnoreInBattle);
			}
		}
		return num;
	}

	public static int GetNecromanceCountOrDefault(this AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType timing)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.Necromance))
		{
			return -1;
		}
		return tagOwner.TagCollectionContainer.PreprocessTags.GetNecromanceOrDefault(tagOwner, field, situation, timing);
	}

	public static int GetEarthRiteCount(this AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType timing)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.EarthRite))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.PreprocessTags.GetEarthRiteCount(tagOwner, field, situation, timing);
	}

	public static int GetBurialRiteCount(this AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, List<int> playPtn, AIScriptTokenArgType timing)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BurialRite))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.PreprocessTags.GetBurialRiteCount(tagOwner, field, situation, playPtn, timing);
	}

	public static float GetReanimateBonus(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.ReanimateBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.ReanimateBonusTags.GetReanimateBonus(tagOwner, playPtn);
	}

	public static bool IsReanimateEvo(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.ReanimateEvo))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.ReanimateEvoTags.IsReanimateEvo(tagOwner, playPtn);
	}

	public static bool IsBreakFirst(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner.SelfField.AI.IsFullSimulation || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BreakFirst))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.BreakFirstTags.EnabledBreakFirst(tagOwner, playPtn);
	}

	public static bool IsBreakLast(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner.SelfField.AI.IsFullSimulation || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BreakLast))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.BreakLastTags.EnabledBreakLast(tagOwner, playPtn);
	}

	public static bool IsBreakBeforePlay(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BreakBeforePlay))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.BreakBeforePlayTags.EnabledBreakBeforePlay(tagOwner, playPtn);
	}

	public static bool IsFirstEvo(this AIVirtualCard tagOwner, AIVirtualCard evoTarget, List<int> playPtn)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.FirstEvo))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.FirstEvoTags.IsFirstEvo(tagOwner, evoTarget, playPtn);
	}

	public static int GetEvoTokenCount(this AIVirtualCard evolver, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int num = (evolver.TagCollectionContainer.HasTag(AIPlayTagType.EvoToken) ? evolver.TagCollectionContainer.EvoTags.GetEvoTokenSummonCount(evolver, playPtn, situation) : 0);
		List<AIVirtualCard> otherEvoTagHolders = field.CardListSet.OtherEvoTagHolders;
		if (otherEvoTagHolders == null)
		{
			return num;
		}
		for (int i = 0; i < otherEvoTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = otherEvoTagHolders[i];
			if (!aIVirtualCard.IsSameCard(evolver) && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenOtherEvo))
			{
				num += aIVirtualCard.TagCollectionContainer.OtherEvoTags.GetOtherEvoTokenSummonCount(aIVirtualCard, field, playPtn, situation);
			}
		}
		return num;
	}

	public static int GetPlayPlusCount(this AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayPlus))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.PlayPlusTags.GetPlayPlusCount(tagOwner, playPtn);
	}

	public static int GetPlayoutAttackBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayoutAttackBonus))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.PlayoutBonusTags.GetPlayoutAttackBonus(tagOwner, playPtn, situation);
	}

	public static int GetAllPlayoutDamageBonus(this AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return card.GetPlayoutDamageBonus(playPtn, situation) + AIEvaluateBonusFromOhterUtility.GetOtherPlayoutDamageBonus(card, field, playPtn);
	}

	public static int GetPlayoutDamageBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.PlayoutDamageBonus))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.PlayoutBonusTags.GetPlayoutDamageBonus(tagOwner, playPtn, situation);
	}

	public static int GetAllyPlayoutDamageBonus(this AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, List<int> playPtn)
	{
		if (tagOwner == null || !tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.AllyPlayoutDamageBonus))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.OtherPlayoutBonusTags.GetPlayoutDamageBonus(tagOwner, target, field, playPtn);
	}

	public static float GetHandBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isIgnoreInFusion)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.HandBonus))
		{
			return HandBonusTagCollection.DEFAULT_HAND_BONUS;
		}
		return tagOwner.TagCollectionContainer.HandBonusTags.GetHandBonus(tagOwner, playPtn, situation, isIgnoreInFusion);
	}

	public static float GetFusionBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.FusionBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.FusionBonusTags.GetFusionBonus(tagOwner, playPtn, situation);
	}

	public static int GetFusionDrawCount(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.FusionDraw))
		{
			return 0;
		}
		return tagOwner.TagCollectionContainer.FusionDrawTags.GetFusionDrawCount(tagOwner, tagOwner.SelfField, playPtn, situation);
	}

	public static List<BattleCardBase> GetCondChoiceTargets(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, IEnumerable<BattleCardBase> selectableCards)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.CondChoice))
		{
			return null;
		}
		return tagOwner.TagCollectionContainer.CondChoiceTags.GetCondChoiceTargets(tagOwner, field, playPtn, selectableCards);
	}

	public static List<AIVirtualCard> GetChoiceTargets(this AIVirtualCard actor, AIVirtualField field, List<AIVirtualCard> candidates, List<int> playPtn, int choiceCount, AISituationInfo situation)
	{
		if (!actor.TagCollectionContainer.HasTag(AIPlayTagType.CondChoice))
		{
			return null;
		}
		return actor.TagCollectionContainer.CondChoiceTags.GetCondChoiceTargets(actor, field, playPtn, candidates, choiceCount, situation);
	}

	public static List<int> GetAddedCardIdsWhenPlayout(this AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!card.TagCollectionContainer.HasTag(AIPlayTagType.AddCardToPlayoutPlayPtn))
		{
			return null;
		}
		return card.TagCollectionContainer.AddCardToPlayoutPlayPtnTags.GetIdList(card, field, playPtn, situation);
	}

	public static bool IsNoInstantAttack(this AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		if (!card.TagCollectionContainer.HasTag(AIPlayTagType.NoInstantAttack))
		{
			return false;
		}
		return card.TagCollectionContainer.NoInstantAttackTags.IsNoInstantAttackActivate(card, field, playPtn);
	}

	public static int GetEmoteOnTurnEndCategory(this AIVirtualCard owner, bool isAllyTurnEnd)
	{
		if (!owner.TagCollectionContainer.HasTag(AIPlayTagType.EmoteOnTurnEnd))
		{
			return EmoteTagCollection.INVALID_EMOTE_CATEGORY;
		}
		return owner.TagCollectionContainer.EmoteTags.GetEmoteOnTurnEndCategory(owner, owner.IsAlly == isAllyTurnEnd);
	}

	public static int GetEmoteCategory(this AIVirtualCard owner, AIPlayTagType emoteType)
	{
		if (!owner.TagCollectionContainer.HasTag(emoteType))
		{
			return EmoteTagCollection.INVALID_EMOTE_CATEGORY;
		}
		return owner.TagCollectionContainer.EmoteTags.GetEmoteCategory(owner, emoteType);
	}

	public static int GetEvoHandPlusCount(this AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!card.TagCollectionContainer.HasTag(AIPlayTagType.EvoHandPlus))
		{
			return 0;
		}
		return card.TagCollectionContainer.EvoHandPlusTags.GetEvoHandPlusCount(card, field, playPtn, situation);
	}

	public static bool IsNoNormalEvo(this AIVirtualCard card, AIVirtualField field)
	{
		if (!card.TagCollectionContainer.HasTag(AIPlayTagType.NoNormalEvo))
		{
			return false;
		}
		return card.TagCollectionContainer.NoNormalEvoTags.IsNoNormalEvo(card, field);
	}

	public static bool IsPlagueCity(this AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		if (!card.TagCollectionContainer.HasTag(AIPlayTagType.PlagueCity))
		{
			return false;
		}
		return card.TagCollectionContainer.PlagueCityTags.IsPlagueCity(card, field, playPtn);
	}

	public static bool IsRemoveByDestroy(this AIVirtualCard card, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> removeByDestroyHolders = card.SelfField.CardListSet.RemoveByDestroyHolders;
		if (removeByDestroyHolders == null || removeByDestroyHolders.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < removeByDestroyHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = removeByDestroyHolders[i];
			if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.RemoveByDestroy) && aIVirtualCard.TagCollectionContainer.RemoveByDestroyTags.IsRemoveByDestroy(card, aIVirtualCard, playPtn, situation))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasWhenPlayRemovalTag(this AIVirtualCard card)
	{
		if (!card.TagCollectionContainer.HasAnyTag(PlayTagCollection.TAG_FOR_REMOVAL_CHECK))
		{
			return card.TagCollectionContainer.HasAnyTag(FanfareTagCollection.TAG_FOR_REMOVAL_CHECK);
		}
		return true;
	}

	public static AIVirtualTargetSelectInfo GetChoiceSelectInfo(this AIVirtualCard card, AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		if (!card.TagCollectionContainer.HasTagCollection(TagCollectionType.Choice))
		{
			return null;
		}
		return card.TagCollectionContainer.ChoiceTags.GetSelectInfo(card, field, situation);
	}

	public static bool CheckAITribe(this AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, string tribe)
	{
		if (tribe == null)
		{
			return false;
		}
		if (owner.PermanentAITribeList != null && owner.PermanentAITribeList.Contains(tribe))
		{
			return true;
		}
		if (!owner.TagCollectionContainer.HasTag(AIPlayTagType.SetAITribe))
		{
			return false;
		}
		return owner.TagCollectionContainer.SetAITribeTags.CheckTribe(owner, field, tribe, playPtn, situation);
	}

	public static void ExecuteWhenPlayTagsForEvaluation(this AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (owner.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			owner.TagCollectionContainer.FanfareTags.ExecuteForPlayPtnEvaluation(owner, field, playPtn, situation);
		}
		if (owner.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			owner.TagCollectionContainer.PlayTags.ExecuteForPlayPtnEvaluation(owner, field, playPtn, situation);
		}
	}

	public static void PseudoExecuteWhenPlayTags(this AIVirtualCard owner, AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
		if (owner.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			owner.TagCollectionContainer.FanfareTags.PseudoExecute(field, playInfo, record, situation);
		}
		if (owner.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			owner.TagCollectionContainer.PlayTags.PseudoExecute(field, playInfo, record, situation);
		}
	}

	public static List<PlaySkipInformation> GetPlaySkipInformation(this AIVirtualCard owner, List<int> playPtn, List<PlaySkipInformation> lastInfo, AISituationInfo situation)
	{
		if (!owner.TagCollectionContainer.HasTagCollection(TagCollectionType.PlaySkip))
		{
			return lastInfo;
		}
		return owner.TagCollectionContainer.PlaySkipTags.RegisterPlaySkipInfo(owner, playPtn, lastInfo, situation);
	}

	public static void EnqueueGiveSkill(this AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (owner.TagCollectionContainer.HasTag(AIPlayTagType.GiveSkill))
		{
			owner.TagCollectionContainer.GiveSkillTags.EnqueueGiveSkill(owner, field, playPtn, situation);
		}
	}

	public static void ExecuteBounceSkills(this AIVirtualCard owner, AIVirtualCard bouncedCard, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (owner.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenBounce))
		{
			owner.TagCollectionContainer.BounceTags.Execute(owner, bouncedCard, playPtn, situation);
		}
	}

	public static bool IsDelayHeal(this AIVirtualCard owner, AIVirtualField field, AISituationInfo situation)
	{
		if (owner.TagCollectionContainer.HasTag(AIPlayTagType.PlayHeal) && owner.TagCollectionContainer.PlayTags.IsDelayHeal(owner, field, situation))
		{
			return true;
		}
		if (owner.TagCollectionContainer.HasTag(AIPlayTagType.FanfareHeal) && owner.TagCollectionContainer.FanfareTags.IsDelayHeal(owner, field, situation))
		{
			return true;
		}
		return false;
	}

	public static int GetWhenPlayHealCount(this AIVirtualCard owner, List<AIVirtualCard> wishHealTargetList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		if (owner.TagCollectionContainer.HasTag(AIPlayTagType.PlayHeal))
		{
			num += owner.TagCollectionContainer.PlayTags.GetHealCount(owner, wishHealTargetList, field, playPtn, situation);
		}
		if (owner.TagCollectionContainer.HasTag(AIPlayTagType.FanfareHeal))
		{
			num += owner.TagCollectionContainer.FanfareTags.GetHealCount(owner, wishHealTargetList, field, playPtn, situation);
		}
		return num;
	}

	public static AIVirtualCard FindRealActor(this AIVirtualCard card, AISinglePlayptnRecord playptnRecord)
	{
		if (playptnRecord == null)
		{
			return card;
		}
		return playptnRecord.FindRealActor(card);
	}

	public static void ExecuteWhenChangeInplayTag(this AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (owner.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenChangeInplay))
		{
			owner.TagCollectionContainer.ChangeInplayTags.Execute(owner, field, playPtn, situation);
		}
	}

	public static bool HasDestroyPlayPtnTag(this AIVirtualCard actor, AIOperationType operationType)
	{
		if (operationType == AIOperationType.PLAY)
		{
			return actor.TagCollectionContainer.HasWhenPlayDestroyPlayPtnTags();
		}
		return false;
	}

	public static int GetAttackDamageToCertainTarget(this AIVirtualCard tagOwner, AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, AIBarrierPseudoSimulationInfo targetBarrierInfo)
	{
		if (tagOwner.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			return tagOwner.TagCollectionContainer.AttackTags.GetAttackDamageToCertainTarget(tagOwner, situation, field, playPtn, targetBarrierInfo);
		}
		return 0;
	}

	public static float GetClashBonus(this AIVirtualCard tagOwner)
	{
		if (tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.ClashBonus))
		{
			return tagOwner.EvaluateClashBonus() * (float)(tagOwner.IsAlly ? 1 : (-1));
		}
		return 0f;
	}

	public static void PreparateOtherToEvolve(this AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.TagCollectionContainer.HasTagCollection(TagCollectionType.EvolveToOther))
		{
			tagOwner.TagCollectionContainer.EvolveToOtherTags.PreparateBeforeEvolve(tagOwner, field, playPtn, situation);
		}
	}

	public static float GetBuffBonus(this AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.BuffBonus))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.BuffBonusTags.GetBuffBonus(tagOwner, playPtn, situation);
	}

	public static bool IsForceImmediateAttack(this AIVirtualCard tagOwner, AIVirtualField field)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.ForceImmediateAttack))
		{
			return false;
		}
		return tagOwner.TagCollectionContainer.ForceImmediateAttackTags.IsForceImmediateAttack(tagOwner, field);
	}
}

using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIBuffEvaluationUtility
{
	public static readonly AIScriptTokenArgType[] LEGAL_TEMP_OR_PERM_ARGUMENTS = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.TEMP,
		AIScriptTokenArgType.PERM
	};

	public static float EvalAllBuff(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> argList, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || field == null || argList == null || argList.Count <= 3)
		{
			return 0f;
		}
		CreateBuffInfoAndFilters(argList, out var filters, out var buffInfo);
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null);
		if (list == null || list.Count == 0)
		{
			return 0f;
		}
		list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		return CalculateAllBuffValue(list, field, buffInfo, playPtn, situation);
	}

	public static float EvalRandomBuff(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> argList, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || field == null || argList == null || argList.Count <= 3)
		{
			return 0f;
		}
		CreateBuffInfoAndFilters(argList, out var filters, out var buffInfo);
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		list.AddRange(field.CardListSet.BothClassAndInplayCards);
		List<AIVirtualCard> list2 = AIFilteringUtility.MultipleFiltering(list, filters, tagOwner, playPtn, null);
		if (list2 == null || list2.Count == 0)
		{
			return 0f;
		}
		list2.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		float num = 0f;
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			AIVirtualCard target = list2[num2];
			num += CalculateBuffValue(target, field, buffInfo, playPtn, situation);
		}
		return num / (float)list2.Count;
	}

	public static float EvalTargetingBuff(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> argList, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || field == null || argList == null || argList.Count <= 3)
		{
			return 0f;
		}
		CreateBuffInfoAndFilters(argList, out var filters, out var buffInfo);
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		list.AddRange(field.CardListSet.BothClassAndInplayCards);
		List<AIVirtualCard> list2 = AIFilteringUtility.MultipleFiltering(list, filters, tagOwner, playPtn, null);
		if (list2 == null || list2.Count == 0)
		{
			return 0f;
		}
		list2.RemoveAll((AIVirtualCard c) => c.IsAmulet);
		bool flag = true;
		float num = float.MinValue;
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			AIVirtualCard aIVirtualCard = list2[num2];
			if (!aIVirtualCard.IsIndependent && (aIVirtualCard.IsAlly == tagOwner.IsAlly || !aIVirtualCard.CantBeFocusedSkill))
			{
				flag = false;
				float num3 = CalculateBuffValue(aIVirtualCard, field, buffInfo, playPtn, situation);
				if (num3 > num)
				{
					num = num3;
				}
			}
		}
		if (flag)
		{
			num = 0f;
		}
		return num;
	}

	private static void CreateBuffInfoAndFilters(List<AIScriptTokenBase> argList, out List<AIScriptTokenBase> filters, out AISimulationBuffInfo buffInfo)
	{
		int index = 0;
		int index2 = 1;
		int num = 2;
		AIScriptArgumentToken aIScriptArgumentToken = argList[index] as AIScriptArgumentToken;
		bool flag = false;
		if (aIScriptArgumentToken != null)
		{
			flag = aIScriptArgumentToken.ArgumentType == AIScriptTokenArgType.TEMP;
		}
		int num2 = (int)argList[index2].Value;
		int num3 = (int)argList[num].Value;
		if (flag)
		{
			buffInfo = new AISimulationBuffInfo(num3, num2, num3, num2);
		}
		else
		{
			buffInfo = new AISimulationBuffInfo(0, 0, num3, num2);
		}
		filters = new List<AIScriptTokenBase>();
		for (int num4 = argList.Count - 1; num4 > num; num4--)
		{
			filters.Add(argList[num4]);
		}
	}

	private static float CalculateAllBuffValue(List<AIVirtualCard> targets, AIVirtualField field, AISimulationBuffInfo buffInfo, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCardStatusInfo> list = new List<AIVirtualCardStatusInfo>();
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (aIVirtualCard.IsUnit)
			{
				list.Add(new AIVirtualCardStatusInfo(aIVirtualCard, aIVirtualCard.Attack, aIVirtualCard.Life));
			}
		}
		List<AIVirtualCardStatusInfo> list2 = new List<AIVirtualCardStatusInfo>();
		for (int j = 0; j < field.EnemyInplayCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = field.EnemyInplayCards[j];
			if (aIVirtualCard2.IsUnit)
			{
				list2.Add(new AIVirtualCardStatusInfo(aIVirtualCard2, aIVirtualCard2.Attack, aIVirtualCard2.Life));
			}
		}
		float num = 0f;
		for (int k = 0; k < targets.Count; k++)
		{
			AIVirtualCard target = targets[k];
			if (!target.IsUnit)
			{
				continue;
			}
			int attack = target.Attack + buffInfo.TotalAttackBuff;
			int num2 = target.Life + buffInfo.TotalLifeBuff;
			AIVirtualCardStatusInfo aIVirtualCardStatusInfo = null;
			List<AIVirtualCardStatusInfo> list3 = (target.IsAlly ? list : list2);
			aIVirtualCardStatusInfo = list3.FirstOrDefault((AIVirtualCardStatusInfo info) => info.BaseCard.IsSameCard(target));
			if (num2 <= 0)
			{
				float num3 = target.EvaluateValueOnField(playPtn, situation, useStyle: true) - target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
				num += num3 * (target.IsAlly ? (-1f) : 1f);
				if (aIVirtualCardStatusInfo != null)
				{
					list3.Remove(aIVirtualCardStatusInfo);
				}
			}
			else
			{
				num += (float)(buffInfo.TotalAttackBuff + buffInfo.TotalLifeBuff) * (target.IsAlly ? 1f : (-1f));
				aIVirtualCardStatusInfo?.ModifyStatus(attack, num2);
			}
		}
		if (list2 != null && list2.Count > 0 && list.Count > 0)
		{
			num += AISimulationUtility.EvaluateAttackValueAfterAllSkill(field, situation, list, list2, playPtn);
		}
		if (buffInfo.TempAttackBuff > 0)
		{
			for (int num4 = 0; num4 < targets.Count; num4++)
			{
				AIVirtualCard target2 = targets[num4];
				AIVirtualCardStatusInfo aIVirtualCardStatusInfo2 = null;
				aIVirtualCardStatusInfo2 = ((!target2.IsAlly) ? list2.FirstOrDefault((AIVirtualCardStatusInfo info) => info.BaseCard.IsSameCard(target2)) : list.FirstOrDefault((AIVirtualCardStatusInfo info) => info.BaseCard.IsSameCard(target2)));
				if (aIVirtualCardStatusInfo2 != null && aIVirtualCardStatusInfo2.Life > 0)
				{
					num += (float)buffInfo.TempAttackBuff * (target2.IsAlly ? (-1f) : 1f);
				}
			}
		}
		return num;
	}

	private static float CalculateBuffValue(AIVirtualCard target, AIVirtualField field, AISimulationBuffInfo buffInfo, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCardStatusInfo> list = new List<AIVirtualCardStatusInfo>();
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards[i];
			if (aIVirtualCard.IsUnit)
			{
				list.Add(new AIVirtualCardStatusInfo(aIVirtualCard, aIVirtualCard.Attack, aIVirtualCard.Life));
			}
		}
		List<AIVirtualCardStatusInfo> list2 = new List<AIVirtualCardStatusInfo>();
		for (int j = 0; j < field.EnemyInplayCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = field.EnemyInplayCards[j];
			if (aIVirtualCard2.IsUnit)
			{
				list2.Add(new AIVirtualCardStatusInfo(aIVirtualCard2, aIVirtualCard2.Attack, aIVirtualCard2.Life));
			}
		}
		float num = 0f;
		int attack = target.Attack + buffInfo.TotalAttackBuff;
		int num2 = target.Life + buffInfo.TotalLifeBuff;
		List<AIVirtualCardStatusInfo> list3 = (target.IsAlly ? list : list2);
		AIVirtualCardStatusInfo aIVirtualCardStatusInfo = list3.FirstOrDefault((AIVirtualCardStatusInfo info) => info.BaseCard.IsSameCard(target));
		if (num2 <= 0)
		{
			float num3 = target.EvaluateValueOnField(playPtn, situation, useStyle: true) - target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
			num += num3 * (target.IsAlly ? (-1f) : 1f);
			if (aIVirtualCardStatusInfo != null)
			{
				list3.Remove(aIVirtualCardStatusInfo);
			}
		}
		else
		{
			num += (float)(buffInfo.TotalAttackBuff + buffInfo.TotalLifeBuff) * (target.IsAlly ? 1f : (-1f));
			aIVirtualCardStatusInfo?.ModifyStatus(attack, num2);
		}
		if (list2 != null && list2.Count > 0 && list.Count > 0)
		{
			num += AISimulationUtility.EvaluateAttackValueAfterAllSkill(field, situation, list, list2, playPtn);
		}
		if (buffInfo.TempAttackBuff > 0)
		{
			AIVirtualCardStatusInfo aIVirtualCardStatusInfo2 = null;
			aIVirtualCardStatusInfo2 = ((!target.IsAlly) ? list2.FirstOrDefault((AIVirtualCardStatusInfo info) => info.BaseCard.IsSameCard(target)) : list.FirstOrDefault((AIVirtualCardStatusInfo info) => info.BaseCard.IsSameCard(target)));
			if (aIVirtualCardStatusInfo2 != null && aIVirtualCardStatusInfo2.Life > 0)
			{
				num += (float)buffInfo.TempAttackBuff * (target.IsAlly ? (-1f) : 1f);
			}
		}
		return num;
	}
}

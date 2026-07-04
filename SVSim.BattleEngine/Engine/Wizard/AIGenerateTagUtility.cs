using System.Collections.Generic;

namespace Wizard;

public static class AIGenerateTagUtility
{
	private class GenerateTagExecutingInfo
	{
		public AIVirtualCard Owner;

		public AIVirtualCard Target;

		public AIAttachedTagInformation AttachedTagInfo;
	}

	public static void ExecuteGenerateTag(AISituationInfo situation, AIVirtualField field, AIGenerateTagOwnerTable generateTagOwnerTable, AttachedSkillInfoReceiveDataCollection attachedInfoReceiveCollection)
	{
		List<GenerateTagExecutingInfo> list = CreateGeneratedTagExecutingInfoList(generateTagOwnerTable, attachedInfoReceiveCollection, field, situation);
		if (list != null)
		{
			AttachGenerateTagOnField(list, field, situation);
		}
		attachedInfoReceiveCollection.Clear();
	}

	private static List<GenerateTagExecutingInfo> CreateGeneratedTagExecutingInfoList(AIGenerateTagOwnerTable generateTagOwnerTable, AttachedSkillInfoReceiveDataCollection attachedInfoReceiveCollection, AIVirtualField field, AISituationInfo situation)
	{
		List<GenerateTagExecutingInfo> list = null;
		for (int i = 0; i < attachedInfoReceiveCollection.InfoList.Count; i++)
		{
			List<GenerateTagExecutingInfo> list2 = CreateGenerateTagExecutingInfoFromReceiveData(generateTagOwnerTable, attachedInfoReceiveCollection.InfoList[i], field, situation);
			if (list2 != null)
			{
				list = AIParamQuery.AddRangeToList(list2, list);
			}
		}
		return list;
	}

	private static List<GenerateTagExecutingInfo> CreateGenerateTagExecutingInfoFromReceiveData(AIGenerateTagOwnerTable generateTagOwnerTable, AttachedSkillInfoReceiveData attachedInfoReceiveData, AIVirtualField field, AISituationInfo situation)
	{
		List<GenerateTagExecutingInfo> list = null;
		AIGenerateTagOwnerTable.GenerateTagOwnerInfo generateTagOwnerInfo = generateTagOwnerTable.GetGenerateTagOwnerInfo(attachedInfoReceiveData.OwnerBaseCardId, attachedInfoReceiveData.OwnerIndex, attachedInfoReceiveData.OwnerIsPlayer);
		if (generateTagOwnerInfo == null || generateTagOwnerInfo.GenerateTagList == null)
		{
			return null;
		}
		List<AIPlayTag> generateTagList = generateTagOwnerInfo.GenerateTagList;
		bool flag = field.AllyBattlePlayer.IsPlayer == generateTagOwnerInfo.OwnerIsPlayer;
		AIVirtualCard owner = FindVirtualCard(generateTagOwnerInfo.OwnerCardIndex, flag, field);
		for (int i = 0; i < attachedInfoReceiveData.TargetInfoList.Count; i++)
		{
			AttachedSkillInfoReceiveData.TargetInfo targetInfo = attachedInfoReceiveData.TargetInfoList[i];
			bool flag2 = field.AllyBattlePlayer.IsPlayer == targetInfo.TargetIsPlayer;
			AIVirtualCard aIVirtualCard = FindVirtualCard(targetInfo.TargetIndex, flag2, field);
			if (aIVirtualCard == null)
			{
				continue;
			}
			for (int j = 0; j < generateTagList.Count; j++)
			{
				if (generateTagList[j].ArgumentExpressions is AIGenerateTag aIGenerateTag && aIGenerateTag.CheckMatchedHashList(targetInfo.SkillHashList))
				{
					list = AIParamQuery.AddElementToList(new GenerateTagExecutingInfo
					{
						Owner = owner,
						Target = aIVirtualCard,
						AttachedTagInfo = new AIAttachedTagInformation(aIGenerateTag.Tag, aIGenerateTag.RemoveTiming, generateTagOwnerInfo.OwnerBaseCardId, generateTagOwnerInfo.OwnerCardIndex, flag, targetInfo.TargetIndex, flag2)
					}, list);
				}
			}
		}
		return list;
	}

	private static void AttachGenerateTagOnField(List<GenerateTagExecutingInfo> executingInfoList, AIVirtualField field, AISituationInfo situation)
	{
		if (executingInfoList != null)
		{
			for (int i = 0; i < executingInfoList.Count; i++)
			{
				GenerateTagExecutingInfo generateTagExecutingInfo = executingInfoList[i];
				AIVirtualCard target = generateTagExecutingInfo.Target;
				target.TagCollectionContainer.AttachTag(generateTagExecutingInfo.AttachedTagInfo, target, situation);
			}
		}
	}

	private static AIVirtualCard FindVirtualCard(int cardIndex, bool isAlly, AIVirtualField field)
	{
		AIVirtualCard aIVirtualCard = null;
		aIVirtualCard = FindFromList<AIVirtualCard>(field.CardListSet.AllReferableCards);
		if (aIVirtualCard != null)
		{
			return aIVirtualCard;
		}
		return FindFromList<AIVirtualCard>(field.GetEnemyHandCardList());
		AIVirtualCard FindFromList<T>(List<T> _cardList) where T : AIVirtualCard
		{
			for (int i = 0; i < _cardList.Count; i++)
			{
				T val = _cardList[i];
				if (val.CardIndex == cardIndex && val.IsAlly == isAlly)
				{
					return val;
				}
			}
			return null;
		}
	}
}

using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIGenerateTagOwnerTable
{
	public class GenerateTagOwnerInfo
	{
		public int OwnerBaseCardId { get; private set; }

		public int OwnerCardIndex { get; private set; }

		public bool OwnerIsPlayer { get; private set; }

		public List<AIPlayTag> GenerateTagList { get; private set; }

		public GenerateTagOwnerInfo(int baseCardId, int cardIndex, bool isPlayer)
		{
			OwnerBaseCardId = baseCardId;
			OwnerCardIndex = cardIndex;
			OwnerIsPlayer = isPlayer;
			GenerateTagList = new List<AIPlayTag>();
		}

		public bool IsOwner(int baseCardId, int cardIndex, bool isPlayer)
		{
			if (OwnerBaseCardId == baseCardId && OwnerCardIndex == cardIndex)
			{
				return OwnerIsPlayer == isPlayer;
			}
			return false;
		}

		public void RegisterNewGenerateTag(List<AIPlayTag> tagList)
		{
			List<AIPlayTag> list = null;
			for (int i = 0; i < tagList.Count; i++)
			{
				AIPlayTag aIPlayTag = tagList[i];
				if (aIPlayTag.Type == AIPlayTagType.GenerateTag && !GenerateTagList.Contains(aIPlayTag))
				{
					list = AIParamQuery.AddElementToList(aIPlayTag, list);
				}
			}
			if (list != null)
			{
				GenerateTagList.AddRange(list);
			}
		}

		public GenerateTagOwnerInfo Clone()
		{
			return new GenerateTagOwnerInfo(OwnerBaseCardId, OwnerCardIndex, OwnerIsPlayer)
			{
				GenerateTagList = new List<AIPlayTag>(GenerateTagList)
			};
		}
	}

	private List<GenerateTagOwnerInfo> _ownerInfoList;

	public AIGenerateTagOwnerTable()
	{
		_ownerInfoList = new List<GenerateTagOwnerInfo>();
	}

	public AIGenerateTagOwnerTable Clone()
	{
		AIGenerateTagOwnerTable aIGenerateTagOwnerTable = new AIGenerateTagOwnerTable();
		for (int i = 0; i < _ownerInfoList.Count; i++)
		{
			aIGenerateTagOwnerTable._ownerInfoList.Add(_ownerInfoList[i].Clone());
		}
		return aIGenerateTagOwnerTable;
	}

	public void RegisterAllGenerateTagOwner(AIVirtualField field)
	{
		BattlePlayerPair playerPair = new BattlePlayerPair(field.AllyBattlePlayer, field.EnemyBattlePlayer);
		List<AIVirtualCard> allReferableCards = field.CardListSet.AllReferableCards;
		for (int i = 0; i < allReferableCards.Count; i++)
		{
			if (allReferableCards[i].TagCollectionContainer.HasTag(AIPlayTagType.GenerateTag))
			{
				RegisterGenerateTagOwnerOneCard(playerPair, allReferableCards[i]);
			}
		}
		List<AIVirtualCard> enemyHandCardList = field.GetEnemyHandCardList();
		for (int j = 0; j < enemyHandCardList.Count; j++)
		{
			if (enemyHandCardList[j].TagCollectionContainer.HasTag(AIPlayTagType.GenerateTag))
			{
				RegisterGenerateTagOwnerOneCard(playerPair, enemyHandCardList[j]);
			}
		}
	}

	private void RegisterGenerateTagOwnerOneCard(BattlePlayerPair playerPair, AIVirtualCard card)
	{
		BattleCardBase battleCard = AIRealBattleCardSearcher.SearchBattleCardFromVirtualCard(playerPair, card);
		if (battleCard != null)
		{
			GenerateTagOwnerInfo generateTagOwnerInfo = _ownerInfoList.FirstOrDefault((GenerateTagOwnerInfo o) => o.IsOwner(battleCard.BaseParameter.BaseCardId, battleCard.Index, battleCard.IsPlayer));
			if (generateTagOwnerInfo == null)
			{
				generateTagOwnerInfo = new GenerateTagOwnerInfo(battleCard.BaseParameter.BaseCardId, battleCard.Index, battleCard.IsPlayer);
				_ownerInfoList.Add(generateTagOwnerInfo);
			}
			if (card.TagCollectionContainer.HasTag(AIPlayTagType.GenerateTag))
			{
				generateTagOwnerInfo.RegisterNewGenerateTag(card.TagCollectionContainer.GenerateTags.TagList);
			}
		}
	}

	public GenerateTagOwnerInfo GetGenerateTagOwnerInfo(int baseCardId, int ownerIndex, bool ownerIsPlayer)
	{
		return _ownerInfoList.FirstOrDefault((GenerateTagOwnerInfo o) => o.IsOwner(baseCardId, ownerIndex, ownerIsPlayer));
	}
}

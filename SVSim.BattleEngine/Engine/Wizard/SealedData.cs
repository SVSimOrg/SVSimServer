using System.Collections.Generic;
using System.Linq;
using LitJson;
using Wizard.Scripts.Network.Data.TaskData.Arena;

namespace Wizard;

public class SealedData
{

	private List<SealedCardInfo> _registeredSealedCardInfoList = new List<SealedCardInfo>();

	private List<SealedCardInfo> _sortedSealedCardInfoList;

	public int? EntryId { get; private set; }

	public int? SeasonId { get; private set; }

	public List<SealedClassInfo> ClassInfoList { get; private set; }

	public int? SelectedClassId { get; private set; }

	public List<CardPack> GachaCardList { get; private set; }

	public List<Wizard.Scripts.Network.Data.TaskData.Arena.Reward> GachaSupplyList { get; private set; }

	public List<int> SortedOwnSealedCardList => _sortedSealedCardInfoList.SelectMany((SealedCardInfo x) => Enumerable.Repeat(x.SealedCardId, x.OwnNum)).ToList();

	private List<int> SortedDeckSealedCardList => _sortedSealedCardInfoList.SelectMany((SealedCardInfo x) => Enumerable.Repeat(x.SealedCardId, x.DeckUsingNum)).ToList();

	public int[] DeckOriginalExcludedPhantomCardList => _sortedSealedCardInfoList.Where((SealedCardInfo x) => !x.IsPhantom).SelectMany((SealedCardInfo x) => Enumerable.Repeat(x.OriginalCardId, x.DeckUsingNum)).ToArray();

	public int[] DeckOriginalPhantomCardList => _sortedSealedCardInfoList.Where((SealedCardInfo x) => x.IsPhantom).SelectMany((SealedCardInfo x) => Enumerable.Repeat(x.OriginalCardId, x.DeckUsingNum)).ToArray();

	public DeckData DeckData { get; private set; }

	public int? DeckCardNumMax { get; private set; }

	public bool[] BattleResultList { get; private set; } = new bool[0];

	public int BattleWinNum { get; private set; }

	public List<int> RewardCardCandidates { get; private set; } = new List<int>();

	public List<ReceivedReward> RewardList { get; private set; }

	public bool? IsRetired { get; private set; }

	public bool IsSelectedClass => SelectedClassId.HasValue;

	public bool? IsCompletedDeck { get; private set; }

	public bool IsSpecialEffect { get; private set; }

	public void SetEntryInfo(JsonData rootData)
	{
		if (!rootData.Keys.Contains("entry_info"))
		{
			return;
		}
		JsonData jsonData = rootData["entry_info"];
		if (jsonData != null)
		{
			EntryId = jsonData["id"].ToInt();
			SeasonId = jsonData["reward_schedule_id"].ToInt();
			IsRetired = jsonData["is_retire"].ToInt() == 1;
			JsonData jsonData2 = jsonData["selected_class"];
			if (jsonData2 != null)
			{
				SelectedClassId = jsonData2.ToInt();
				// Pre-Phase-5b: dropped class-prm chara-id write; headless has no class prm map.
			}
		}
	}

	public void SetClassInfo(JsonData rootData)
	{
		if (rootData.Keys.Contains("candidate_class"))
		{
			JsonData jsonData = rootData["candidate_class"];
			ClassInfoList = new List<SealedClassInfo>();
			List<SealedCardInfo> list = new List<SealedCardInfo>();
			for (int i = 0; i < jsonData.Count; i++)
			{
				JsonData jsonData2 = jsonData[i];
				List<SealedCardInfo> list2 = new List<SealedCardInfo>
				{
					new SealedCardInfo(jsonData2["card_id_1"].ToInt(), isPhantom: true, 0, 0),
					new SealedCardInfo(jsonData2["card_id_2"].ToInt(), isPhantom: true, 0, 0)
				};
				list.AddRange(list2);
				ClassInfoList.Add(new SealedClassInfo(jsonData2["class"].ToInt(), list2));
			}
			(from x in list
				group x by x.OriginalCardId into x
				select x.First()).ToList().ForEach(RegisterSealedCard);
		}
	}

	public void SetGachaCardInfo(JsonData rootData)
	{
		if (rootData.Keys.Contains("cards"))
		{
			JsonData jsonData = rootData["cards"];
			int count = jsonData.Count;
			GachaCardList = new List<CardPack>(count);
			for (int i = 0; i < count; i++)
			{
				JsonData jsonData2 = jsonData[i];
				GachaCardList.Add(new CardPack
				{
					card_id = ConvertToSealedCardId(jsonData2["card_id"].ToInt(), jsonData2["is_phantom"].ToInt() == 1),
					rarity = jsonData2["rarity"].ToInt()
				});
			}
		}
	}

	public void ClearGachaCardInfo()
	{
		GachaCardList = null;
	}

	public void SetGachaSupplyInfo(JsonData rootData)
	{
		if (!rootData.Keys.Contains("rewards"))
		{
			return;
		}
		JsonData jsonData = rootData["rewards"];
		int count = jsonData.Count;
		if (count > 0)
		{
			GachaSupplyList = new List<Wizard.Scripts.Network.Data.TaskData.Arena.Reward>(count);
			for (int i = 0; i < count; i++)
			{
				Wizard.Scripts.Network.Data.TaskData.Arena.Reward item = new Wizard.Scripts.Network.Data.TaskData.Arena.Reward(jsonData[i]);
				GachaSupplyList.Add(item);
			}
		}
	}

	public void SetIsSpecialEffect(JsonData jsonData)
	{
		IsSpecialEffect = jsonData.GetValueOrDefault("is_special_effect", defaultValue: false);
	}

	public void ClearGachaSupplyInfo()
	{
		GachaSupplyList = null;
	}

	public void SetSealedCardInfo(JsonData rootData, bool isRegisterSealedCard)
	{
		if (!rootData.Keys.Contains("card_list"))
		{
			return;
		}
		JsonData jsonData = rootData["card_list"];
		int count = jsonData.Count;
		List<SealedCardInfo> list = new List<SealedCardInfo>(count);
		for (int i = 0; i < count; i++)
		{
			list.Add(new SealedCardInfo(jsonData[i]));
		}
		DeckCardNumMax = list.Sum((SealedCardInfo x) => x.OwnNum);
		if (isRegisterSealedCard)
		{
			list.ForEach(RegisterSealedCard);
			list.ForEach(delegate(SealedCardInfo info)
			{
				// Pre-Phase-5b: SetIsNewCard write dropped; headless has no user card state.
			});
		}
		list.Sort(delegate(SealedCardInfo a, SealedCardInfo b)
		{
			UIBase_CardManager.ComparableCard comparableCard = new UIBase_CardManager.ComparableCard(a.SealedCardId, CardMaster.CardMasterId.Default);
			UIBase_CardManager.ComparableCard other = new UIBase_CardManager.ComparableCard(b.SealedCardId, CardMaster.CardMasterId.Default);
			return comparableCard.CompareTo(other);
		});
		_sortedSealedCardInfoList = list;
		DeckData = new DeckData(Format.Sealed);
		DeckData.SetDeckName(string.Empty);
		DeckData.SetDeckClassID(SelectedClassId.Value);
		DeckData.SetCardIdList(SortedDeckSealedCardList);
	}

	public void SetDeckCompleted(JsonData rootData)
	{
		if (rootData.Keys.Contains("deck_info"))
		{
			JsonData jsonData = rootData["deck_info"];
			IsCompletedDeck = jsonData["is_completed"].ToBoolean();
			DeckData.SetDeckIsComplete(IsCompletedDeck.Value);
		}
	}

	public void SetBattleResultInfo(JsonData rootData)
	{
		if (rootData.Keys.Contains("battle_results"))
		{
			JsonData jsonData = rootData["battle_results"];
			JsonData jsonData2 = jsonData["result_list"];
			int count = jsonData2.Count;
			BattleResultList = new bool[count];
			for (int i = 0; i < count; i++)
			{
				BattleResultList[i] = jsonData2[i].ToInt() == 1;
			}
			BattleWinNum = jsonData["win_count"].ToInt();
		}
	}

	public void SetRewardCardCandidates(JsonData rootData)
	{
		if (rootData.Keys.Contains("acquire_phantom_cards"))
		{
			JsonData jsonData = rootData["acquire_phantom_cards"];
			RewardCardCandidates.Clear();
			for (int i = 0; i < jsonData.Count; i++)
			{
				RewardCardCandidates.Add(jsonData[i].ToInt());
			}
			RewardCardCandidates = RewardCardCandidates.Distinct().ToList();
		}
	}

	public void SetRewardInfo(JsonData rootData)
	{
		if (rootData.Keys.Contains("rewards"))
		{
			JsonData jsonData = rootData["rewards"];
			int count = jsonData.Count;
			RewardList = new List<ReceivedReward>(count);
			for (int i = 0; i < count; i++)
			{
				RewardList.Add(new ReceivedReward(jsonData[i]));
			}
		}
	}

	public void UpdateHaveUserGoodsNum(JsonData rootData)
	{
		if (rootData.Keys.Contains("reward_list"))
		{
			PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(rootData["reward_list"]);
		}
	}

	public void SetSelectedClassId(int classId)
	{
		SelectedClassId = classId;
	}

	public void SetRetired(bool isRetired)
	{
		IsRetired = isRetired;
	}

	public SealedCardInfo GetSealedCardInfo(int sealedCardId)
	{
		return _registeredSealedCardInfoList.Find((SealedCardInfo x) => x.SealedCardId == sealedCardId);
	}

	private void RegisterSealedCard(SealedCardInfo cardInfo)
	{
		CardMaster instance = CardMaster.GetInstance(CardMaster.CardMasterId.Default);
		int sealedCardId = cardInfo.SealedCardId;
		int originalCardId = cardInfo.OriginalCardId;
		_registeredSealedCardInfoList.Add(cardInfo);
		CardParameter cardParam = instance.GetCardParameterFromId(originalCardId).Clone(sealedCardId);
		// Pre-Phase-5b: DataMgr.SetIsNewCard + RegisterUserOwnCardData dropped; headless has
		// no user card state to update.
		instance.RegisterCardParameter(sealedCardId, cardParam);
		RegisterSealedCardInAllCardIdList(sealedCardId);
	}

	private static void RegisterSealedCardInAllCardIdList(int sealedCardId)
	{
		List<int> allCardIds = CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetAllCardIds();
		int count = allCardIds.Count;
		int num = sealedCardId / 10;
		int num2 = sealedCardId % 10;
		int i;
		for (i = allCardIds.IndexOf(num * 10) + 1; i < count; i++)
		{
			int num3 = allCardIds[i];
			if (num != num3 / 10 || num2 < num3 % 10)
			{
				break;
			}
		}
		allCardIds.Insert(i, sealedCardId);
	}

	public void UnregisterAllSealedCard()
	{
		for (int num = _registeredSealedCardInfoList.Count - 1; num >= 0; num--)
		{
			UnregisterSealedCard(_registeredSealedCardInfoList[num].SealedCardId);
		}
	}

	private void UnregisterSealedCard(int sealedCardId)
	{
		_registeredSealedCardInfoList.RemoveAll((SealedCardInfo x) => x.SealedCardId == sealedCardId);
		CardMaster.GetInstance(CardMaster.CardMasterId.Default).UnregisterCardParameter(sealedCardId);
		// Pre-Phase-5b: DataMgr.UnregisterUserOwnCardData dropped; headless has no user card state.
		CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetAllCardIds().Remove(sealedCardId);
	}

	public static int ConvertToSealedCardId(int originalCardId, bool isPhantomCard)
	{
		bool isFoil = CardMaster.GetInstance(CardMaster.CardMasterId.Default).GetCardParameterFromId(originalCardId).IsFoil;
		int num = 0;
		num = (isPhantomCard ? (isFoil ? 9 : 8) : (isFoil ? 7 : 6));
		return originalCardId / 10 * 10 + num;
	}

	public static bool IsPhantomCard(int cardId)
	{
		return Data.ArenaData.SealedData.GetSealedCardInfo(cardId)?.IsPhantom ?? false;
	}

	public static void GroupByPhantomCard(List<int> inputCardList, out List<int> excludedPhantomCardList, out List<int> phantomCardList, bool isConvertToOriginalCardId)
	{
		excludedPhantomCardList = new List<int>();
		phantomCardList = new List<int>();
		foreach (int inputCard in inputCardList)
		{
			SealedCardInfo sealedCardInfo = Data.ArenaData.SealedData.GetSealedCardInfo(inputCard);
			if (sealedCardInfo != null)
			{
				((!sealedCardInfo.IsPhantom) ? excludedPhantomCardList : phantomCardList).Add(isConvertToOriginalCardId ? sealedCardInfo.OriginalCardId : inputCard);
			}
		}
	}
}

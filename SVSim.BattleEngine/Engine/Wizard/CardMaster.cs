using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using LitJson;
using Wizard.Battle.Recovery;
// TODO(engine-cleanup-pass2): 19 of 25 methods unrun in baseline
//   Type: Wizard.CardMaster
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class CardMaster
{
	public class UpdateInfo
	{
		public bool NeedUpdateLocalCardMaster { get; private set; }

		public string NewCardMasterHash { get; private set; }

		public UpdateInfo(JsonData data)
		{
			if (data.Keys.Contains("card_master_hash"))
			{
				NeedUpdateLocalCardMaster = true;
				NewCardMasterHash = data["card_master_hash"].ToString();
			}
			else
			{
				NeedUpdateLocalCardMaster = false;
				NewCardMasterHash = string.Empty;
			}
		}
	}

	public enum CardMasterId
	{
		Default = 1	}

	private static Dictionary<CardMasterId, CardMaster> _dictCardMaster = null;

	private readonly IDictionary<int, CardParameter> m_cardParameters;

	private readonly IDictionary<string, int> _hashIdCardId;

	private CardParameter _classCardParam;

	public static CardMasterId BatttleCardMasterId { get; private set; } = CardMasterId.Default;

	public static CardMaster GetInstance(CardMasterId cardMasterId)
	{
		if (_dictCardMaster == null)
		{
			return null;
		}
		CardMaster value = null;
		_dictCardMaster.TryGetValue(cardMasterId, out value);
		return value;
	}

	public static CardMaster GetInstanceForBattle()
	{
		return GetInstance(BatttleCardMasterId);
	}

	public static void DeleteAllInstance()
	{
		_dictCardMaster = null;
		BatttleCardMasterId = CardMasterId.Default;
	}

	private CardMaster(List<CardCSVData> cardList)
	{
		m_cardParameters = cardList.ToDictionary((CardCSVData entry) => int.Parse(entry.card_id), (CardCSVData entry) => new CardParameter(entry));
		if (!IsUseLocalCardMaster())
		{
			_hashIdCardId = cardList.ToDictionary((CardCSVData entry) => entry.CardHashId, (CardCSVData entry) => int.Parse(entry.card_id));
		}
		_classCardParam = new CardParameter();
	}

	public static bool IsClass(int cardId)
	{
		return cardId < 100;
	}

	public static bool IsMutationCardCheck(int baseCardID)
	{
		if (baseCardID > 800000000)
		{
			return baseCardID % 800000000 < 10000000;
		}
		return false;
	}

	public static bool IsChoiceBraveCardCheck(int baseCardId)
	{
		return baseCardId / 1000000 == 930;
	}

	public IEnumerable<CardParameter> GetAllParameters()
	{
		return m_cardParameters.Values;
	}

	public List<int> GetAllCardIds()
	{
		return m_cardParameters.Keys.ToList();
	}

	public CardParameter GetCardParameterFromId(int cardId)
	{
		if (IsClass(cardId))
		{
			return _classCardParam;
		}
		m_cardParameters.TryGetValue(cardId, out var value);
		return value;
	}

	public bool CardExists(int cardId)
	{
		return m_cardParameters.Keys.Contains(cardId);
	}

	public List<int> GetSameCardListByBaseCardId(int baseCardId)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, CardParameter> cardParameter in m_cardParameters)
		{
			CardParameter value = cardParameter.Value;
			if (value.BaseCardId == baseCardId)
			{
				list.Add(value.CardId);
			}
		}
		return list;
	}

	public void RegisterCardParameter(int cardId, CardParameter cardParam)
	{
		m_cardParameters.Add(cardId, cardParam);
	}

	public void UnregisterCardParameter(int cardId)
	{
		m_cardParameters.Remove(cardId);
	}

	public static void SetBattleCardMasterId(CardMasterId cardMasterId)
	{
		BatttleCardMasterId = cardMasterId;
	}

	public static void SetBattleCardMasterId(int cardMasterId)
	{
		if (Enum.IsDefined(typeof(CardMasterId), cardMasterId))
		{
			SetBattleCardMasterId((CardMasterId)cardMasterId);
		}
	}

	public static bool IsUseLocalCardMaster()
	{
		return false;
	}
}

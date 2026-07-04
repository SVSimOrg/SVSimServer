using System.Collections.Generic;
using System.Linq;
using Wizard;

public class SendKeyActionDataManager
{
	public enum KeyActionParameter
	{
		type,
		cardId,
		cardIdx,
		selectCard,
		open
	}

	public enum KeyActionType
	{
		None,
		Choice,
		Accelerated,
		Crystallize,
		Fusion,
		HaveBeforeSkillChoice,
		BurialRate,
		ChoiceEvolution,
		ChoiceBrave
	}

	public class KeyActionData
	{
		public KeyActionType keyActionType;

		public int keyActionBeforeCardId = -1;

		public List<int> selectCardIndexs = new List<int>();

		public List<int> selectCardIds = new List<int>();

		public bool selectCardIsOpen;
	}

	private List<KeyActionData> _sendKeyActionDataList = new List<KeyActionData>();

	public SendKeyActionDataManager()
	{
		_sendKeyActionDataList.Clear();
	}

	public Dictionary<string, object> MakeSendData(Dictionary<string, object> data, int playCardIndex)
	{
		if (_sendKeyActionDataList.Count == 0)
		{
			return data;
		}
		List<object> list = new List<object>();
		foreach (KeyActionData sendKeyActionData in _sendKeyActionDataList)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add(KeyActionParameter.type.ToString(), sendKeyActionData.keyActionType);
			if (sendKeyActionData.keyActionBeforeCardId != -1)
			{
				dictionary.Add(KeyActionParameter.cardId.ToString(), sendKeyActionData.keyActionBeforeCardId);
			}
			if (sendKeyActionData.keyActionType == KeyActionType.BurialRate)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2.Add(KeyActionParameter.cardIdx.ToString(), sendKeyActionData.selectCardIndexs);
				dictionary2.Add(KeyActionParameter.open.ToString(), sendKeyActionData.selectCardIsOpen ? 1 : 0);
				dictionary.Add(KeyActionParameter.selectCard.ToString(), dictionary2);
			}
			else if (sendKeyActionData.selectCardIndexs.Count >= 1)
			{
				Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
				dictionary3.Add(KeyActionParameter.cardId.ToString(), sendKeyActionData.selectCardIndexs);
				dictionary3.Add(KeyActionParameter.open.ToString(), sendKeyActionData.selectCardIsOpen ? 1 : 0);
				dictionary.Add(KeyActionParameter.selectCard.ToString(), dictionary3);
			}
			else if (sendKeyActionData.selectCardIds.Count >= 1)
			{
				Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
				dictionary4.Add(KeyActionParameter.cardId.ToString(), sendKeyActionData.selectCardIds);
				dictionary4.Add(KeyActionParameter.open.ToString(), sendKeyActionData.selectCardIsOpen ? 1 : 0);
				dictionary.Add(KeyActionParameter.selectCard.ToString(), dictionary4);
			}
			list.Add(dictionary);
		}
		data.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.keyAction], list);
		return data;
	}

	public void SettingKeyActionData(BattleCardBase originalCard, BattleCardBase playCard, List<int> chosenIndexs, bool isEvol = false)
	{
		KeyActionData keyActionData = new KeyActionData();
		keyActionData.keyActionBeforeCardId = originalCard.CardId;
		if (SettingChoiceKeyActionData(originalCard, chosenIndexs, isEvol))
		{
			return;
		}
		if (isEvol && originalCard.IsChoiceEvolutionCard && chosenIndexs != null && chosenIndexs.Count() >= 1)
		{
			keyActionData.keyActionBeforeCardId = originalCard.CardId;
			keyActionData.keyActionType = KeyActionType.ChoiceEvolution;
			keyActionData.selectCardIsOpen = true;
			foreach (int chosenIndex in chosenIndexs)
			{
				keyActionData.selectCardIndexs.Add(chosenIndex);
			}
			_sendKeyActionDataList.Add(keyActionData);
		}
		else if (playCard.IsChoiceBraveSkillCard)
		{
			keyActionData.keyActionType = KeyActionType.ChoiceBrave;
			keyActionData.selectCardIds = new List<int> { playCard.CardId };
			keyActionData.selectCardIsOpen = true;
			_sendKeyActionDataList.Add(keyActionData);
		}
		else if (NetworkBattleGenericTool.IsAcceleratedCard(originalCard) && !isEvol)
		{
			if ((NetworkBattleGenericTool.GetMutationPpFixedUseSkill(originalCard) as Skill_pp_fixeduse).IsMutationFixedUseCost)
			{
				keyActionData.keyActionType = KeyActionType.Accelerated;
			}
			_sendKeyActionDataList.Add(keyActionData);
			SettingChoiceKeyActionData(playCard, chosenIndexs, isEvol);
		}
		else if (NetworkBattleGenericTool.IsCrystallizeCard(originalCard) && !isEvol)
		{
			if ((NetworkBattleGenericTool.GetMutationPpFixedUseSkill(originalCard) as Skill_pp_fixeduse).IsMutationFixedUseCost)
			{
				keyActionData.keyActionType = KeyActionType.Crystallize;
			}
			_sendKeyActionDataList.Add(keyActionData);
		}
	}

	private bool SettingChoiceKeyActionData(BattleCardBase card, List<int> chosenIndexs, bool isEvol)
	{
		if (NetworkBattleGenericTool.IsChoiceCard(card, isEvol) && chosenIndexs != null && chosenIndexs.Count() >= 1)
		{
			KeyActionData keyActionData = new KeyActionData();
			keyActionData.keyActionBeforeCardId = card.CardId;
			keyActionData.keyActionType = ((!card.Skills.HaveBeforeChoiceSkill()) ? KeyActionType.Choice : KeyActionType.HaveBeforeSkillChoice);
			keyActionData.selectCardIsOpen = NetworkBattleGenericTool.IsOpenChoiceSkillCard(card, isEvol);
			foreach (int chosenIndex in chosenIndexs)
			{
				keyActionData.selectCardIndexs.Add(chosenIndex);
			}
			_sendKeyActionDataList.Add(keyActionData);
			return true;
		}
		return false;
	}

	public void SettingFusionKeyActionData(BattleCardBase originalCard, IEnumerable<BattleCardBase> fusionCards)
	{
		if (fusionCards != null && fusionCards.Count() >= 1)
		{
			KeyActionData keyActionData = new KeyActionData();
			keyActionData.keyActionBeforeCardId = originalCard.CardId;
			keyActionData.keyActionType = KeyActionType.Fusion;
			keyActionData.selectCardIsOpen = true;
			foreach (BattleCardBase fusionCard in fusionCards)
			{
				keyActionData.selectCardIds.Add(fusionCard.CardId);
			}
			_sendKeyActionDataList.Add(keyActionData);
		}
		else
		{
			LocalLog.AccumulateLastTraceLog("Trying to fusion without the selected fusion materials");
		}
	}

	public void SettingBurialRiteKeyActionData(BattleCardBase playCard, IEnumerable<BattleCardBase> selectCards, bool isEvolve)
	{
		IEnumerable<SkillBase> source = from s in playCard.GetSelectTypeSkill(isEvolve)
			where s.IsBurialRite
			select s;
		if (source.Count() >= 1 && selectCards.Count() >= 1)
		{
			KeyActionData keyActionData = new KeyActionData();
			List<BattleCardBase> list = selectCards.ToList();
			for (int num = 0; num < source.Count(); num++)
			{
				keyActionData.selectCardIndexs.Add(list[num].Index);
			}
			keyActionData.keyActionType = KeyActionType.BurialRate;
			keyActionData.selectCardIsOpen = true;
			_sendKeyActionDataList.Add(keyActionData);
		}
		else
		{
			if (source.Count() == 0)
			{
				LocalLog.AccumulateLastTraceLog("Trying to burial rite without burial rite skills");
			}
			if (selectCards == null || selectCards.Count() == 0)
			{
				LocalLog.AccumulateLastTraceLog("Trying to burial rite without burial rite cards");
			}
		}
	}

	public void Clear()
	{
		_sendKeyActionDataList.Clear();
	}
}

using System.Collections;
using System.Collections.Generic;
using LitJson;
using Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;
using Wizard.Scripts.Network.Data.TaskData.Arena.TwoPick;

namespace Wizard.Scripts.Network.Data.TaskData.Arena;

public class TwoPickInfo
{
	public enum PREPARE_STATE
	{
	}

	public EntryInfo entryInfo;

	public CandidateClass classInfo;

	public CandidateChaos chaosInfo;

	public CandidateCardInfo candidateCardInfo;

	public Deck deckInfo;

	public BattleResult battleResult = new BattleResult();

	public CampaignBattleWin TreasureCP = new CampaignBattleWin();

	public TwoPickInfo()
	{
	}

	public TwoPickInfo(JsonData data, JsonData headerData)
	{
		SetEntryInfo(data["entry_info"]);
		if (data.Keys.Contains("class_info"))
		{
			SetClassInfo(data["class_info"]);
		}
		if (data.Keys.Contains("chaos_info") && data["chaos_info"] != null)
		{
			if (data.Keys.Contains("candidate_chaos_ids"))
			{
				SetChaosInfoFormEntry(data["chaos_info"], data["candidate_chaos_ids"]);
			}
			else
			{
				SetChaosInfo(data["chaos_info"], data["class_info"]);
			}
		}
		if (data.Keys.Contains("deck_info"))
		{
			SetDeckInfo(data["deck_info"]);
		}
		if (data.Keys.Contains("candidate_card_list"))
		{
			SetCandidateCardList(data["candidate_card_list"]);
		}
		if (data.Keys.Contains("battle_results"))
		{
			SetBattleResult(data["battle_results"]);
		}
		if (data.Keys.Contains("leader_skin_id"))
		{
			/* Pre-Phase-5b: ClassPrm.SetCurrentCharaId not reachable headless (no chara master) */
		}
		if (data.TryGetValue("treasure_info", out var value))
		{
			TreasureCP.Parse(value);
		}
		if (data.TryGetValue("upgrade_treasure_box_info", out var value2))
		{
			Wizard.Data.TreasureBoxCp.Parse(value2, headerData);
		}
	}

	public void SetEntryInfo(JsonData entryInfoData)
	{
		entryInfo = new EntryInfo(entryInfoData);
	}

	public void SetClassInfo(JsonData classInfoData)
	{
		classInfo = new CandidateClass(classInfoData);
	}

	public void SetChaosInfo(JsonData chaosInfoData, JsonData classInfoData)
	{
		chaosInfo = new CandidateChaos(chaosInfoData, classInfoData);
	}

	public void SetChaosInfoFormEntry(JsonData chaosInfoData, JsonData classIds)
	{
		List<string> list = new List<string>();
		foreach (object item in (IEnumerable)classIds)
		{
			list.Add(item.ToString());
		}
		chaosInfo = new CandidateChaos(chaosInfoData, list);
	}

	public void SetDeckInfo(JsonData deckInfoData)
	{
		deckInfo = new Deck(deckInfoData);
	}

	public void SetCandidateCardList(JsonData candidateCardListData)
	{
		candidateCardInfo = new CandidateCardInfo(candidateCardListData);
	}

	public void SetBattleResult(JsonData battleResultInfoData)
	{
		battleResult = new BattleResult(battleResultInfoData);
	}
}

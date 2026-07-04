using System.Collections.Generic;
using LitJson;
using MiniJSON;
using Wizard;

public class ReplayDetailInfo
{
	public bool is_win;

	public Format deck_format;

	public TwoPickFormat _twoPickFormat;

	public long battle_id;

	public int seed;

	public int field_id;

	public int first_turn;

	public Dictionary<string, object> play_list;

	public List<int> TurnStartIndexList = new List<int>();

	public int viewer_id;

	public string name;

	public int chara_id;

	public int class_id;

	public long emblem_id;

	public int degree_id;

	public string country_code;

	public long sleeve_id;

	public int battle_point;

	public int master_point;

	public int rank;

	public List<object> deck;

	public int ChaosId;

	public int SubClassId = 10;

	public string MyRotationId = "";

	public int opponent_viewer_id;

	public string opponent_name;

	public int opponent_chara_id;

	public int opponent_class_id;

	public long opponent_emblem_id;

	public int opponent_degree_id;

	public string opponent_country_code;

	public long opponent_sleeve_id;

	public int opponent_battle_point;

	public int opponent_master_point;

	public int opponent_rank;

	public List<object> opponent_deck;

	public int IdxChangeSeed;

	public int OppoIdxChangeSeed;

	public int OpponentChaosId;

	public int OpponentSubClassId = 10;

	public string OpponentMyRotationId = "";

	public bool IsOfficialUser { get; set; }

	public bool IsOpponentOfficialUser { get; set; }

	public bool IsChaosSkinOverride { get; private set; }

	public ReplayDetailInfo(JsonData data)
	{
		IdxChangeSeed = -1;
		OppoIdxChangeSeed = -1;
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		battle_id = data["battleId"].ToLong();
		seed = data["seed"].ToInt();
		field_id = data["fieldId"].ToInt();
		first_turn = data["firstTurn"].ToInt();
		if (data.Keys.Contains("deck_format"))
		{
			deck_format = Data.ParseApiFormat(data["deck_format"].ToInt());
		}
		if (data.Keys.Contains("twoPickType"))
		{
			_twoPickFormat = (TwoPickFormat)data["twoPickType"].ToInt();
		}
		if (data.Keys.Contains("idxChangeSeed"))
		{
			IdxChangeSeed = data["idxChangeSeed"].ToInt();
		}
		if (data.Keys.Contains("oppoIdxChangeSeed"))
		{
			OppoIdxChangeSeed = data["oppoIdxChangeSeed"].ToInt();
		}
		if (data.Keys.Contains("playlist"))
		{
			JsonData jsonData = data["playlist"];
			play_list = new Dictionary<string, object>();
			play_list.Add("uri", "Watch");
			List<object> list = new List<object>();
			for (int i = 0; i < jsonData.Count; i++)
			{
				CheckMulliganFlags(jsonData[i]);
				Dictionary<string, object> item = Json.Deserialize(jsonData[i].ToJson()) as Dictionary<string, object>;
				list.Add(item);
			}
			play_list.Add("playlist", list);
		}
		if (data.Keys.Contains("isChaosSkinOverride"))
		{
			IsChaosSkinOverride = data["isChaosSkinOverride"].ToInt() == 1;
		}
		if (data.Keys.Contains("mission_parameter"))
		{
			dataMgr.SetMissionNecessaryInformation(data["mission_parameter"]);
		}
		CardMaster.SetBattleCardMasterId(data["card_master_id"].ToInt());
		viewer_id = data["vid1"].ToInt();
		name = data["name1"].ToString();
		chara_id = data["charaId1"].ToInt();
		class_id = data["classId1"].ToInt();
		emblem_id = data["emblemId1"].ToLong();
		degree_id = data["degreeId1"].ToInt();
		country_code = data["countryCode1"].ToString();
		sleeve_id = data["sleeveId1"].ToLong();
		battle_point = data["battlePoint1"].ToInt();
		master_point = data["masterPoint1"].ToInt();
		rank = data["rank1"].ToInt();
		IsOfficialUser = data["isOfficial1"].ToBoolean();
		SetMyDeck(data);
		ChaosId = (data.Keys.Contains("chaosId1") ? data["chaosId1"].ToInt() : (-1));
		SubClassId = (data.Keys.Contains("subclassId1") ? data["subclassId1"].ToInt() : 10);
		MyRotationId = (data.Keys.Contains("rotationId1") ? data["rotationId1"].ToString() : "");
		string text = "deck1";
		if (data.Keys.Contains(text))
		{
			/* Pre-Phase-5b: SetDeckMaxCount dropped */
		}
		opponent_viewer_id = data["vid2"].ToInt();
		opponent_name = data["name2"].ToString();
		opponent_chara_id = data["charaId2"].ToInt();
		opponent_class_id = data["classId2"].ToInt();
		opponent_emblem_id = data["emblemId2"].ToLong();
		opponent_degree_id = data["degreeId2"].ToInt();
		opponent_country_code = data["countryCode2"].ToString();
		opponent_sleeve_id = data["sleeveId2"].ToLong();
		opponent_battle_point = data["battlePoint2"].ToInt();
		opponent_master_point = data["masterPoint2"].ToInt();
		opponent_rank = data["rank2"].ToInt();
		IsOpponentOfficialUser = data["isOfficial2"].ToBoolean();
		SetOpponentDeck(data);
		OpponentChaosId = (data.Keys.Contains("chaosId2") ? data["chaosId2"].ToInt() : (-1));
		OpponentSubClassId = (data.Keys.Contains("subclassId2") ? data["subclassId2"].ToInt() : 10);
		OpponentMyRotationId = (data.Keys.Contains("rotationId2") ? data["rotationId2"].ToString() : "");
		if (data.Keys.Contains("oppoDeckCount"))
		{
			/* Pre-Phase-5b: SetDeckMaxCount dropped */
		}
		if (data.Keys.Contains("turn_start_index_list"))
		{
			TurnStartIndexList = new List<int>();
			for (int j = 0; j < data["turn_start_index_list"].Count; j++)
			{
				TurnStartIndexList.Add(data["turn_start_index_list"][j].ToInt());
			}
		}
	}

	protected virtual void CheckMulliganFlags(JsonData data)
	{
	}

	protected void SetMyDeck(JsonData data)
	{
		JsonData jsonData = data["deck1"];
		deck = new List<object>();
		for (int i = 0; i < jsonData.Count; i++)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("idx", jsonData[i]["idx"].ToString());
			dictionary.Add("cardId", jsonData[i]["cardId"].ToString());
			deck.Add(dictionary);
		}
	}

	protected virtual void SetOpponentDeck(JsonData data)
	{
		JsonData jsonData = data["deck2"];
		opponent_deck = new List<object>();
		for (int i = 0; i < jsonData.Count; i++)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("idx", jsonData[i]["idx"].ToString());
			dictionary.Add("cardId", jsonData[i]["cardId"].ToString());
			opponent_deck.Add(dictionary);
		}
	}
}

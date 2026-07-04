using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using LitJson;
using UnityEngine;
// TODO(engine-cleanup-pass2): 310 of 317 methods unrun in baseline
//   Type: Wizard.Master
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class Master
{
	public interface ReadFromCsv
	{
		void ReadCsvColumns(string[] columns);
	}

	private List<BattleInformation> _roomChaosBattleInfo;

	private List<List<BattleInformation>> _chaosBattleInfoList;

	private Dictionary<int, List<int>> _classInfomationOrder;

	private Dictionary<int, List<int>> _roomClassInfomationOrder;

	private Dictionary<int, List<int>> _colosseumClassInfomationOrder;

	private List<Dictionary<int, List<int>>> _classInfomationOrderList;

	public List<ClassCharacterMasterData> ClassCharacterList { get; private set; }

	public Dictionary<int, SleeveCategory> SleeveCategoryIdDic { get; set; }

	public Dictionary<int, LeaderSkinSeries> LeaderSkinSeriesIdDic { get; set; }

	public SleeveMgr SleeveMgr { get; private set; }

	public EmblemMgr EmblemMgr { get; private set; }

	public DegreeMgr DegreeMgr { get; private set; }

	public List<Item> ItemList { get; set; }

	public List<GiftTransition> GiftTransitionList { get; set; }

	public Dictionary<int, List<int>> ClassInfomationOrder
	{
		get
		{
			switch (DataMgr.BattleType.FreeBattle) // Pre-Phase-5b: headless has no BattleType
			{
			case DataMgr.BattleType.RoomTwoPick:
			case DataMgr.BattleType.TwoPickBackdraft:
				return _roomClassInfomationOrder;
			case DataMgr.BattleType.ColosseumTwoPick:
				return _colosseumClassInfomationOrder;
			default:
				return _classInfomationOrder;
			}
		}
	}

	private Dictionary<string, string> _crossOverClassInfomationOrder { get; set; }

	public Dictionary<int, List<int>> AvatarClassInformationOrder { get; set; }

	public List<string> WhenPlayEffectKeywordMaster { get; set; }

	public List<PuzzleQuestData> PuzzleQuestDataList { get; private set; }

	public AIDeckFileNameList AIDeckFileNameList { get; set; }

	public AIEmoteFileNameList AIEmoteFileNameList { get; set; }

	public AIStyleFileNameList AIStyleFileNameList { get; set; }

	public IDictionary<string, string> BattleKeyWordDic { get; private set; }

	public IDictionary<string, string> CardFilterKeywordReplaceDic { get; private set; }

	private IDictionary<string, string> EmoteWordDic { get; set; }

	private IDictionary<string, string> CardNameDic { get; set; }

	private IDictionary<string, string> TribeNameDic { get; set; }

	private IDictionary<string, string> SkillDescDic { get; set; }

	private IDictionary<string, string> ItemTextDic { get; set; }

	public IDictionary<string, string> SleeveTextDic { get; private set; }

	private IDictionary<string, string> SleeveCategoryTextDic { get; set; }

	private IDictionary<string, string> LeaderSkinProductTextDic { get; set; }

	private IDictionary<string, string> LeaderSkinSeriesTextDic { get; set; }

	public IDictionary<string, string> ClassCharaTextDic { get; private set; }

	public IDictionary<string, string> EmblemTextDic { get; private set; }

	public IDictionary<string, string> DegreeTextDic { get; private set; }

	private IDictionary<string, string> DegreeAchievementTextDic { get; set; }

	private Dictionary<string, string> MyPageBGTextDic { get; set; }

	public CardSetNameMgr CardSetNameMgr { get; private set; }

	private IDictionary<string, string> CardVoiceTextDic { get; set; }

	private IDictionary<string, string> PracticeTextDic { get; set; }

	public Dictionary<int, List<int>> GleamingGemListMaster { get; set; }

	public Dictionary<string, MyPageCustomBGMasterData> MyPageCustomBGMaster { get; private set; }

	public List<MyPageCustomBGMasterData> MyPageCustomBGMasterList { get; private set; }

	public Dictionary<int, List<int>> RadiantCrystalListMaster { get; set; }

	public Dictionary<int, List<int>> GleamingGemListV2Master { get; set; }

	public Dictionary<int, List<int>> RadiantCrystalListV2Master { get; set; }

	public List<int> GetGleamingGemList(int classId)
	{
		if (GleamingGemListMaster.ContainsKey(classId))
		{
			return GleamingGemListMaster[classId];
		}
		return null;
	}

	public List<int> GetRadiantCrystalList(int classId)
	{
		if (RadiantCrystalListMaster.ContainsKey(classId))
		{
			return RadiantCrystalListMaster[classId];
		}
		return null;
	}

	public List<int> GetGleamingGemListV2Master(int classId)
	{
		if (GleamingGemListV2Master.ContainsKey(classId))
		{
			return GleamingGemListV2Master[classId];
		}
		return null;
	}

	public List<int> GetRadiantCrystalListV2Master(int classId)
	{
		if (RadiantCrystalListV2Master.ContainsKey(classId))
		{
			return RadiantCrystalListV2Master[classId];
		}
		return null;
	}

	public void LoadRoomChaosBattleInfo(int num)
	{
		if (num > 0 && _chaosBattleInfoList != null)
		{
			_roomChaosBattleInfo = _chaosBattleInfoList[num - 1];
		}
	}

	public void SetRoomClassInfomationOrder(int num)
	{
		if (num > 0 && _classInfomationOrderList != null)
		{
			_roomClassInfomationOrder = _classInfomationOrderList[num - 1];
		}
	}

	public List<int> GetCrossOverClassInfoListOrNull(int mainClass, int subClass)
	{
		return _crossOverClassInfomationOrder.GetValueOrDefault(mainClass + "|" + subClass, null)?.Split('|').Select(int.Parse).ToList();
	}

	public string GetText<Type>(Type key, IDictionary<Type, string> dic)
	{
		if (!dic.TryGetValue(key, out var value))
		{
			return key.ToString();
		}
		return value;
	}

	public string GetEmoteWordText(string key)
	{
		return GetText(key, EmoteWordDic);
	}

	public string GetCardNameText(string key)
	{
		return GetText(key, CardNameDic);
	}

	public string GetTribeNameText(string key)
	{
		return GetText(key, TribeNameDic);
	}

	public string GetSkillDescText(string key)
	{
		return GetText(key, SkillDescDic);
	}

	public string GetItemText(string key)
	{
		return GetText(key, ItemTextDic);
	}

	public string GetSleeveText(string key)
	{
		return GetText(key, SleeveTextDic);
	}

	public string GetSleeveCategoryText(string key)
	{
		return GetText(key, SleeveCategoryTextDic);
	}

	public string GetLeaderSkinProductText(string key)
	{
		return GetText(key, LeaderSkinProductTextDic);
	}

	public string GetLeaderSkinSeriesText(string key)
	{
		return GetText(key, LeaderSkinSeriesTextDic);
	}

	public string GetClassCharaText(string key)
	{
		return GetText(key, ClassCharaTextDic);
	}

	public string GetEmblemText(string key)
	{
		return GetText(key, EmblemTextDic);
	}

	public string GetDegreeText(string key)
	{
		return GetText(key, DegreeTextDic);
	}

	public string GetDegreeAchievementText(string key)
	{
		return GetText(key, DegreeAchievementTextDic);
	}

	public string GetCardVoiceText(string key)
	{
		return GetText(key, CardVoiceTextDic);
	}

	public string GetPracticeText(string key)
	{
		return GetText(key, PracticeTextDic);
	}

	public string GetMyPageBGText(string key)
	{
		return GetText(key, MyPageBGTextDic);
	}

	public List<string> GetAllTribeNameList()
	{
		List<string> list = new List<string>(TribeNameDic.Count);
		foreach (KeyValuePair<string, string> item in TribeNameDic)
		{
			list.Add(item.Value);
		}
		return list;
	}
}

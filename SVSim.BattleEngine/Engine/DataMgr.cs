using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using LitJson;
using Wizard;
using Wizard.AutoTest;
using Wizard.Battle.Recovery;

public class DataMgr
{
	public enum SpecialBattleResultType
	{
		None	}

	public class SpecialBattleSetting
	{
		public string Id { get; private set; }

		public bool? IsPlayerFirstTurn { get; private set; }

		public string PlayerAttachSkillText { get; private set; }

		public string[] PlayerAttachSkills { get; private set; }

		public string EnemyAttachSkillText { get; private set; }

		public string[] EnemyAttachSkills { get; private set; }

		public int PlayerStartPp { get; private set; }

		public int EnemyStartPp { get; private set; }

		public int PlayerStartLife { get; set; }

		public int PlayerStartMaxLife { get; private set; }

		public int EnemyStartMaxLife { get; private set; }

		public string IdOverrideInBattleLogText { get; private set; }

		public Dictionary<int, int> IdOverridePairDict { get; private set; }

		public string BanishEffectOverRideText { get; private set; }

		public string[] BanishEffectOverRideIds { get; private set; }

		public string TokenDrawEffectOverrideText { get; private set; }

		public Dictionary<int, string> TokenDrawOverrideEffectPair { get; private set; }

		public string SpecialTokenDrawEffectOverrideText { get; private set; }

		public Dictionary<int, string> SpecialTokenDrawOverrideEffectPair { get; private set; }

		public bool IsVsEffectOverride { get; private set; }

		public int ClassDestroyEffectOverride { get; private set; }

		public SpecialBattleSetting(string id, bool? isPlayerFirst, string playerSkill, string enemySkill, int playerPp, int enemyPp, int playerLife, int playerMaxLife, int enemyMaxLife, string idOverrideBattleLogText, string banishEffectOverride, string tokenDrawEffectOverride, string specialTokenDrawEffectOverride, bool isVsEffectOverride, int classDestroyEffectOverride)
		{
			Id = id;
			IsPlayerFirstTurn = isPlayerFirst;
			PlayerAttachSkillText = playerSkill;
			PlayerAttachSkills = SplitSpecialBattleDataText(PlayerAttachSkillText);
			EnemyAttachSkillText = enemySkill;
			EnemyAttachSkills = SplitSpecialBattleDataText(EnemyAttachSkillText);
			PlayerStartPp = playerPp;
			EnemyStartPp = enemyPp;
			PlayerStartLife = playerLife;
			PlayerStartMaxLife = playerMaxLife;
			EnemyStartMaxLife = enemyMaxLife;
			IdOverrideInBattleLogText = idOverrideBattleLogText;
			IdOverridePairDict = ParseIdOverrideText(IdOverrideInBattleLogText);
			BanishEffectOverRideText = banishEffectOverride;
			BanishEffectOverRideIds = SplitSpecialBattleDataText(BanishEffectOverRideText);
			TokenDrawEffectOverrideText = tokenDrawEffectOverride;
			TokenDrawOverrideEffectPair = SplitSpecialBattleDataPair(TokenDrawEffectOverrideText);
			SpecialTokenDrawEffectOverrideText = specialTokenDrawEffectOverride;
			SpecialTokenDrawOverrideEffectPair = SplitSpecialBattleDataPair(SpecialTokenDrawEffectOverrideText);
			IsVsEffectOverride = isVsEffectOverride;
			ClassDestroyEffectOverride = classDestroyEffectOverride;
		}
	}

	public enum BattleType
	{
		FreeBattle = 0,
		RankBattle = 1,
		TwoPick = 2,
		RoomBattle = 3,
		Story = 4,
		Practice = 5,
		RoomTwoPick = 6,
		TwoPickBackdraft = 7,
		ColosseumNormal = 8,
		ColosseumTwoPick = 9,
		ColosseumHof = 31,
		Sealed = 32,
		ColosseumWindFall = 33,
		Gathering = 34,
		Quest = 37,
		OfflineEvent = 40,
		CompetitionNormal = 42,
		BossRushQuest = 45,
		SecretBossQuest = 46,
		CompetitionTwoPick = 47,
		None = 100
	}

	private int _selectDeckId;

	private Format _selectDeckFormat;

	private IList<int> _currentDeckCardIdList;

	private int _deckMaxCardCount;

	private int _enemyDeckMaxCardCount;

	private IList<int> _currentEnemyDeckData;

	private int _playerCharaId;

	private int _playerSubClassId;

	private long _playerSleeveId;

	private string _playerEmotionId = "";

	private int _enemyCharaId;

	private int _enemySubClassId;

	private long _enemySleeveId;

	private string _enemyEmotionId = "";

	public AIDataLibrary m_AIDataLibrary;

	private int _soroPlay3DFieldId = 1;

	private string _storyBgmId = "NONE";

	public int m_EnemyAIDifficulty;

	public int m_EnemyAILogicLevel;

	public int m_EnemyAIDeckId;

	public int m_EnemyAIStyleId;

	public int m_EnemyAIEmoteId;

	// Default 20 mirrors the historic AITestGlobal.AI_MAX_LIFE static default. Callers that don't
	// invoke SetEnemyAI (e.g. non-AI test fixtures) still get a sensible enemy leader HP.
	public int m_EnemyAIMaxLife = 20;

	public bool m_EnemyAIUseInnerEmote = true;

	private IDictionary<int, int> _possessionCardDict;

	private IDictionary<int, int> _possessionCardDictIncludingSpotCard;

	private IDictionary<int, bool> _isNewCardDict = new Dictionary<int, bool>();

	private int[] _maintenanceCardIds;

	private IDictionary<int, ClassCharaPrm> _classPrmDict;

	private bool _isDirtyPossessionCardDict;

	public BattleType m_BattleType = BattleType.None;

	public string BattleId = "";

	private static string[] ClanNameTextIdList = new string[9] { "Common_0104", "Common_0105", "Common_0106", "Common_0107", "Common_0108", "Common_0109", "Common_0110", "Common_0111", "Common_0112" };

	public DeckAttributeType LastSelectDeckAttributeType { private get; set; }

	public int PracticeDifficultyDegreeId { get; set; }

	public List<int> FavoriteCardList { get; private set; }

	public SpecialBattleSetting SpecialBattleSettingInfo { get; set; }

	public BattleManagerBase.MissionNecessaryInformation MissionNecessaryInformation { get; set; } = new BattleManagerBase.MissionNecessaryInformation(new Dictionary<string, string>());

	private MyRotationInfo _playerMyRotationInfo { get; set; }

	private MyRotationInfo _enemyMyRotationInfo { get; set; }

	private AvatarBattleInfo _playerAvatarBattleInfo { get; set; }

	private AvatarBattleInfo _enemyAvatarBattleInfo { get; set; }

	public QuestBattleData QuestBattleData { get; private set; }

	public BossRushBattleData BossRushBattleData { get; private set; }

	public int PuzzleQuestId { get; set; }

	public int PuzzleDifficulty { get; set; }

	public int PuzzleEnemyClass { get; set; }

	public int StoryEnemyClassId { get; set; }

	public JsonData RecoveryData { get; private set; }

	public SpecialBattleResultType SkipStorySpecialBattleResult { get; private set; }

	public SpotCardData SpotCardData { get; set; }

	public string TitleId { get; set; } = "0";

	public bool IsLastSelectDeckAttributeType(DeckAttributeType deckAttributeType)
	{
		return deckAttributeType == LastSelectDeckAttributeType;
	}

	public void SetMissionNecessaryInformation(JsonData data)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (data.Count > 0)
		{
			foreach (string key in data.Keys)
			{
				dictionary.Add(key, data.ToStringOrDefault(key, string.Empty));
			}
		}
		MissionNecessaryInformation = new BattleManagerBase.MissionNecessaryInformation(dictionary);
	}

	public void SetQuestBattleData(QuestBattleData battleData)
	{
		QuestBattleData = battleData;
	}

	public void SetBossRushBattleData(BossRushBattleData battleData)
	{
		BossRushBattleData = battleData;
	}

	public void SetRecoveryData(JsonData recoveryData)
	{
		RecoveryData = recoveryData;
	}

	public bool IsQuestBattleType()
	{
		return IsQuestBattleType(m_BattleType);
	}

	public static bool IsQuestBattleType(BattleType battleType)
	{
		if (battleType == BattleType.Quest || (uint)(battleType - 45) <= 1u)
		{
			return true;
		}
		return false;
	}

	public DataMgr()
	{
		_possessionCardDict = new Dictionary<int, int>();
		_possessionCardDictIncludingSpotCard = new Dictionary<int, int>();
		_maintenanceCardIds = null;
		m_AIDataLibrary = new AIDataLibrary();
		FavoriteCardList = new List<int>();
		PracticeDifficultyDegreeId = 400001;
	}

	public void Load()
	{
		LoadClassData();
	}

	public void LoadEnemy()
	{
		LoadEnemyClassData();
	}

	public void LoadClassData()
	{
		if (_playerCharaId == 0)
		{
			SetPlayerCharaId(1);
		}
		SetPlayerSleeveId(_playerSleeveId);
	}

	public void LoadEnemyClassData()
	{
		if (_enemyCharaId == 0)
		{
			SetEnemyCharaId(1);
		}
		SetEnemySleeveId(_enemySleeveId);
	}

	public void SetPlayerCharaId(int charaId)
	{
		_playerCharaId = charaId;
		SetPlayerSubClassID(10);
		SetPlayerMyRotationInfo("");
	}

	public void SetPlayerSubClassID(int subClassId)
	{
		_playerSubClassId = subClassId;
	}

	public void SetPlayerMyRotationInfo(string myRotationId)
	{
		_playerMyRotationInfo = Data.MyRotationAllInfo.Get(myRotationId);
	}

	public void SetPlayerAvatarBattleInfo(string id)
	{
		_playerAvatarBattleInfo = Data.AvatarBattleAllInfo.Get(id);
	}

	public void SetPlayerCharaIdBySkinId(int skinId)
	{
		SetPlayerCharaId(GetCharaPrmBySkinId(skinId).chara_id);
	}

	public void SetPlayerSleeveId(long sleeveId)
	{
		_playerSleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(GetAbleSleeveId(sleeveId));
	}

	public void SetEnemyCharaId(int charaId)
	{
		_enemyCharaId = charaId;
		SetEnemySubClassID(10);
		SetEnemyMyRotationInfo("");
	}

	public void SetEnemySubClassID(int classId)
	{
		_enemySubClassId = classId;
	}

	public void SetEnemyMyRotationInfo(string myRotationId)
	{
		_enemyMyRotationInfo = Data.MyRotationAllInfo.Get(myRotationId);
	}

	public void ClearEnemyAvatarBattleInfo()
	{
		_enemyAvatarBattleInfo = null;
	}

	public void SetEnemySleeveId(long sleeveId)
	{
		_enemySleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(GetAbleSleeveId(sleeveId));
	}

	public bool GetSelectDefDeck()
	{
		if (_selectDeckId == 0)
		{
			return PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.LAST_BATTLE_IS_DEFDECK);
		}
		return IsLastSelectDeckAttributeType(DeckAttributeType.DefaultDeck);
	}

	public void SetSelectDeckId(int id)
	{
		_selectDeckId = id;
	}

	public void SetSelectDeckFormat(Format format)
	{
		_selectDeckFormat = format;
	}

	public Format GetSelectDeckFormat()
	{
		return _selectDeckFormat;
	}

	public void SetCurrentDeckData(IList<int> deckdata)
	{
		_currentDeckCardIdList = deckdata;
	}

	public void SetCurrentEnemyDeckData(IList<int> deckdata)
	{
		_currentEnemyDeckData = deckdata;
	}

	public void SetCurrentEnemyDeckDataFromAIDeck(int classID, int difficulty, int logicLevel, int maxLife, int deckId, int styleId, int emoteId, bool useInnerEmote, int enemyAiID = -1, List<int> specialAbilityIdList = null)
	{
		if (classID == 0)
		{
			classID = 8;
		}
		m_EnemyAIDifficulty = difficulty;
		m_EnemyAIDeckId = deckId;
		m_EnemyAIStyleId = styleId;
		m_EnemyAIEmoteId = emoteId;
		m_EnemyAILogicLevel = logicLevel;
		m_EnemyAIMaxLife = maxLife;
		m_EnemyAIUseInnerEmote = useInnerEmote;
		AI_LOGIC_LV logicLv = AI_LOGIC_LV.STRONG;
		switch (logicLevel)
		{
		case 0:
			logicLv = AI_LOGIC_LV.WEAK;
			break;
		case 1:
			logicLv = AI_LOGIC_LV.MIDDLE;
			break;
		case 2:
			logicLv = AI_LOGIC_LV.STRONG;
			break;
		}
		string text = "ai/" + Data.Master.AIDeckFileNameList.GetFileName(deckId);
		string styleName = "ai/" + Data.Master.AIStyleFileNameList.GetFileName(styleId);
		string emoteName = "ai/" + Data.Master.AIEmoteFileNameList.GetFileName(emoteId);
		m_AIDataLibrary.SaveBattleSetUpInfo(classID, logicLv, text, styleName, emoteName, useEmote: true, useInnerEmote, enemyAiID, specialAbilityIdList);
		SetCurrentEnemyDeckDataFromAIDeck(text);
	}

	public void SetEnemyAIDeckFromCustomDeck(int classId, IList<int> deck, int difficulty = 2, int logicLevel = 2, int maxLife = 20, int styleId = 0, int emoteId = 0, bool useInnerEmote = true, int enemyAiID = -1)
	{
		m_EnemyAIDifficulty = difficulty;
		m_EnemyAIDeckId = int.MinValue;
		m_EnemyAIStyleId = styleId;
		m_EnemyAIEmoteId = emoteId;
		m_EnemyAILogicLevel = logicLevel;
		m_EnemyAIMaxLife = maxLife;
		m_EnemyAIUseInnerEmote = useInnerEmote;
		AI_LOGIC_LV logicLv = AI_LOGIC_LV.STRONG;
		switch (logicLevel)
		{
		case 0:
			logicLv = AI_LOGIC_LV.WEAK;
			break;
		case 1:
			logicLv = AI_LOGIC_LV.MIDDLE;
			break;
		case 2:
			logicLv = AI_LOGIC_LV.STRONG;
			break;
		}
		string styleName = "ai/" + Data.Master.AIStyleFileNameList.GetFileName(styleId);
		string emoteName = "ai/" + Data.Master.AIEmoteFileNameList.GetFileName(emoteId);
		m_AIDataLibrary.SaveBattleSetUpInfo(classId, logicLv, "", styleName, emoteName, useEmote: true, m_EnemyAIUseInnerEmote, enemyAiID, null);
		SetCurrentEnemyDeckData(deck);
	}

	public void SetCurrentEnemyDeckDataFromAIDeck(string deckName)
	{
		AIDeckData aIDeckData = m_AIDataLibrary.SearchDeckData(deckName);
		if (aIDeckData == null)
		{
			return;
		}
		_currentEnemyDeckData = new List<int>();
		foreach (KeyValuePair<int, AICardData> item in aIDeckData.CardDic)
		{
			for (int i = 0; i < item.Value.CardNum; i++)
			{
				_currentEnemyDeckData.Add(item.Key);
			}
		}
	}

	public void SetSoroPlay3DFieldID(int fieldID)
	{
		_soroPlay3DFieldId = fieldID;
	}

	public void SetStoryBgmID(string bgmID)
	{
		_storyBgmId = bgmID;
	}

	public void SetSpecialBattleSetting(bool? isPlayerFirst, string playerSkill, string enemySkill, int playerPp, int enemyPp, int playerLife, int playerMaxLife, int enemyMaxLife, string idOverrideBattleLogText, string id = "", string banishEffectOverride = "", string tokenDrawEffectOverride = "", string specialTokenDrawEffectOverride = "", int skipResult = 0, bool isVsEffectOverride = false, int classDestroyEffectOverride = 0)
	{
		SpecialBattleSettingInfo = new SpecialBattleSetting(id, isPlayerFirst, playerSkill, enemySkill, playerPp, enemyPp, playerLife, playerMaxLife, enemyMaxLife, idOverrideBattleLogText, banishEffectOverride, tokenDrawEffectOverride, specialTokenDrawEffectOverride, isVsEffectOverride, classDestroyEffectOverride);
		SkipStorySpecialBattleResult = (SpecialBattleResultType)skipResult;
	}

	public void ClearSpecialBattleSettingInfo()
	{
		SpecialBattleSettingInfo = null;
	}

	private static Dictionary<int, int> ParseIdOverrideText(string idOverrideText)
	{
		if (string.IsNullOrEmpty(idOverrideText))
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		string[] array = SplitSpecialBattleDataText(idOverrideText);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			dictionary.Add(int.Parse(array2[0]), int.Parse(array2[1]));
			dictionary.Add(int.Parse(array2[0]) + 1, int.Parse(array2[1]) + 1);
		}
		return dictionary;
	}

	private static string[] SplitSpecialBattleDataText(string text)
	{
		return text.Split(',');
	}

	private static Dictionary<int, string> SplitSpecialBattleDataPair(string text)
	{
		string[] array = text.Split(',');
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		for (int i = 0; i < array.Count(); i++)
		{
			string[] array2 = array[i].Split('=');
			if (array2.Length == 2)
			{
				int key = int.Parse(array2[0]);
				dictionary[key] = array2[1];
			}
		}
		return dictionary;
	}

	public string GetClanNameByKey(int intclantype)
	{
		if (intclantype < ClanNameTextIdList.Length)
		{
			return Data.SystemText.Get(ClanNameTextIdList[intclantype]);
		}
		return "";
	}

	public ClassCharaPrm GetClassPrm(int classId)
	{
		return _classPrmDict[classId];
	}

	public ClassCharacterMasterData GetCharaPrmByClassId(int classId, bool isCurrentChara = true)
	{
		ClassCharaPrm classPrm = GetClassPrm(classId);
		if (!isCurrentChara)
		{
			return classPrm.DefaultCharaData;
		}
		return classPrm.CurrentCharaData;
	}

	public ClassCharacterMasterData GetCharaPrmByCharaId(int charaId)
	{
		return Data.Master.ClassCharacterList.Find((ClassCharacterMasterData x) => x.chara_id == charaId);
	}

	public ClassCharacterMasterData GetCharaPrmBySkinId(int skinId)
	{
		return Data.Master.ClassCharacterList.Find((ClassCharacterMasterData x) => x.skin_id == skinId && x.is_usable);
	}

	public int GetPlayerCharaId()
	{
		if (_playerCharaId == 0)
		{
			return PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_BATTLE_LEADER_ID);
		}
		return _playerCharaId;
	}

	public int GetPlayerSubClassId()
	{
		return _playerSubClassId;
	}

	public bool TryGetPlayerMyRotationInfo(out MyRotationInfo myRotationInfo)
	{
		myRotationInfo = _playerMyRotationInfo;
		return myRotationInfo != null;
	}

	public bool TryGetPlayerAvatarBattleInfo(out AvatarBattleInfo avatarBattleInfo)
	{
		avatarBattleInfo = _playerAvatarBattleInfo;
		return avatarBattleInfo != null;
	}

	public ClassCharacterMasterData GetPlayerCharaData()
	{
		return GetCharaPrmByCharaId(GetPlayerCharaId());
	}

	public int GetPlayerClassId()
	{
		return GetPlayerCharaData().class_id;
	}

	public int GetPlayerSkinId()
	{
		return GetPlayerCharaData().skin_id;
	}

	public long GetPlayerSleeveId()
	{
		return GetAbleSleeveId(_playerSleeveId);
	}

	public void SetPlayerEmotionId(string id)
	{
		_playerEmotionId = id;
	}

	public void SetEnemyEmotionId(string id)
	{
		_enemyEmotionId = id;
	}

	public int GetEnemyCharaId()
	{
		if (_enemyCharaId == 0)
		{
			return 1;
		}
		return _enemyCharaId;
	}

	public int GetEnemySubClassId()
	{
		return _enemySubClassId;
	}

	public ClassCharacterMasterData GetEnemyCharaData()
	{
		return GetCharaPrmByCharaId(GetEnemyCharaId());
	}

	public int GetEnemyClassId()
	{
		if (false /* Pre-Phase-5b: IsPuzzleQuest const-false */)
		{
			return PuzzleEnemyClass;
		}
		if (m_BattleType == BattleType.Story)
		{
			return StoryEnemyClassId;
		}
		return GetEnemyCharaData().class_id;
	}

	public int GetEnemySkinId()
	{
		return GetEnemyCharaData().skin_id;
	}

	public bool TryGetEnemyMyRotationInfo(out MyRotationInfo myRotationInfo)
	{
		myRotationInfo = _enemyMyRotationInfo;
		return myRotationInfo != null;
	}

	public bool TryGetEnemyAvatarBattleInfo(out AvatarBattleInfo avatarBattleInfo)
	{
		avatarBattleInfo = _enemyAvatarBattleInfo;
		return avatarBattleInfo != null;
	}

	public long GetEnemySleeveId()
	{
		return GetAbleSleeveId(_enemySleeveId);
	}

	public bool IsHighRankSkinPlayer()
	{
		return GetPlayerCharaData().IsHighRank;
	}

	public bool Is3DSkin(bool isPlayer)
	{
		if (isPlayer)
		{
			return GetPlayerCharaData().Is3d;
		}
		return GetEnemyCharaData().Is3d;
	}

	public IList<int> GetCurrentDeckData()
	{
		return _currentDeckCardIdList;
	}

	public IList<int> GetCurrentEnemyDeckData()
	{
		return _currentEnemyDeckData;
	}

	public int GetDeckMaxCount(bool isSelf)
	{
		if (!isSelf)
		{
			return _enemyDeckMaxCardCount;
		}
		return _deckMaxCardCount;
	}

	public IDictionary<int, int> GetUserOwnCardData(bool isIncludingSpotCard)
	{
		if (isIncludingSpotCard)
		{
			if (_isDirtyPossessionCardDict)
			{
				UpdatePossessionCardDictIncludingSpotCard();
				_isDirtyPossessionCardDict = false;
			}
			return _possessionCardDictIncludingSpotCard;
		}
		return _possessionCardDict;
	}

	public int GetPossessionCardNum(int cardId, bool isIncludingSpotCard)
	{
		int value = 0;
		GetUserOwnCardData(isIncludingSpotCard).TryGetValue(cardId, out value);
		return value;
	}

	public static int GetPossessionBaseCardNum(int baseCardId, IDictionary<int, int> cardPool, CardMaster.CardMasterId cardMasterId)
	{
		CardMaster instance = CardMaster.GetInstance(cardMasterId);
		int num = 0;
		foreach (KeyValuePair<int, int> item in cardPool)
		{
			if (instance.GetCardParameterFromId(item.Key).BaseCardId == baseCardId)
			{
				try
				{
					num = checked(num + item.Value);
				}
				catch (OverflowException)
				{
					return int.MaxValue;
				}
			}
		}
		return num;
	}

	private void UpdatePossessionCardDictIncludingSpotCard()
	{
		_possessionCardDictIncludingSpotCard = SpotCardData.CreateDictionaryIncludingSpotCard(_possessionCardDict);
	}

	public void SetIsNewCard(int cardId, bool isNew)
	{
		_isNewCardDict[cardId] = isNew;
	}

	public int GetSoroPlay3DFieldID()
	{
		return _soroPlay3DFieldId;
	}

	public string GetStoryBgmID()
	{
		return _storyBgmId;
	}

	public bool IsMaintenanceCard(int id)
	{
		if (_maintenanceCardIds != null)
		{
			return _maintenanceCardIds.Contains(id);
		}
		return false;
	}

	public static long GetAbleSleeveId(long sleeveId)
	{
		return Data.Master.SleeveMgr.Get(sleeveId).sleeve_id;
	}
}

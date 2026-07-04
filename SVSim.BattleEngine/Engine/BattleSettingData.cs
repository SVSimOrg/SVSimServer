using LitJson;

public class BattleSettingData
{
	public int DeckClassId { get; }

	public int DeckSkinId { get; }

	public int PlayerCharaId { get; }

	public string PlayerEmotionId { get; }

	public int EnemyCharaId { get; }

	public string EnemyEmotionId { get; }

	public int EnemyClassId { get; }

	public int EnemyAiId { get; }

	public int FieldId { get; }

	public string BgmId { get; }

	public BattleSettingData(JsonData jsonData, BattleSettingBaseData baseData)
	{
		DeckClassId = jsonData["deck_class_id"].ToInt();
		DeckSkinId = GetDeckSkinId(jsonData, DeckClassId);
		PlayerCharaId = GetPlayerCharaId(jsonData, baseData.PlayerCharaId, DeckClassId);
		PlayerEmotionId = GetPlayerEmotionId(jsonData, PlayerCharaId);
		EnemyCharaId = baseData.EnemyCharaId;
		EnemyEmotionId = GetEnemyEmotionId(jsonData, EnemyCharaId);
		EnemyClassId = baseData.EnemyClassId;
		EnemyAiId = baseData.EnemyAiId;
		FieldId = GetFieldId(jsonData, baseData.FieldId);
		BgmId = GetBgmId(jsonData, baseData.BgmId);
	}

	private static int GetDeckSkinId(JsonData jsonData, int deckClassId)
	{
		int num = jsonData["deck_skin_id_override"].ToInt();
		if (num == 0)
		{
			return 0; // Pre-Phase-5b: no chara master headless
		}
		return num;
	}

	private static int GetPlayerCharaId(JsonData jsonData, int baseCharaId, int classId)
	{
		int? overridePlayerCharaId = GetOverridePlayerCharaId(jsonData);
		if (overridePlayerCharaId.HasValue)
		{
			return overridePlayerCharaId.Value;
		}
		if (baseCharaId == 0)
		{
			return 0; // Pre-Phase-5b: no chara master headless
		}
		return baseCharaId;
	}

	private static int? GetOverridePlayerCharaId(JsonData jsonData)
	{
		int num = jsonData["skin_id_override"].ToInt();
		if (num == 0)
		{
			return null;
		}
		return num;
	}

	private static string GetPlayerEmotionId(JsonData jsonData, int charaId)
	{
		int? overridePlayerEmotionId = GetOverridePlayerEmotionId(jsonData);
		return GetEmotionId(charaId, overridePlayerEmotionId);
	}

	private static int? GetOverridePlayerEmotionId(JsonData jsonData)
	{
		int num = jsonData["player_emotion_override"].ToInt();
		if (num == 0)
		{
			return null;
		}
		return num;
	}

	private static string GetEnemyEmotionId(JsonData jsonData, int charaId)
	{
		int? overrideEnemyEmotionId = GetOverrideEnemyEmotionId(jsonData);
		return GetEmotionId(charaId, overrideEnemyEmotionId);
	}

	private static int? GetOverrideEnemyEmotionId(JsonData jsonData)
	{
		int num = jsonData["enemy_emotion_override"].ToInt();
		if (num == 0)
		{
			return null;
		}
		return num;
	}

	private static string GetEmotionId(int charaId, int? variationId)
	{
		int skin_id = 0; // Pre-Phase-5b: no chara master headless
		if (!variationId.HasValue)
		{
			return $"{skin_id}";
		}
		return $"{skin_id}_{variationId.Value}";
	}

	private static int GetFieldId(JsonData jsonData, int baseFieldId)
	{
		int? overrideFieldId = GetOverrideFieldId(jsonData);
		if (!overrideFieldId.HasValue)
		{
			return baseFieldId;
		}
		return overrideFieldId.Value;
	}

	private static int? GetOverrideFieldId(JsonData jsonData)
	{
		int num = jsonData["battle3dfield_id_override"].ToInt();
		if (num == 0)
		{
			return null;
		}
		return num;
	}

	private static string GetBgmId(JsonData jsonData, string baseBgmId)
	{
		string overrideBgmId = GetOverrideBgmId(jsonData);
		if (overrideBgmId == null)
		{
			return baseBgmId;
		}
		return overrideBgmId;
	}

	private static string GetOverrideBgmId(JsonData jsonData)
	{
		string text = jsonData["bgm_id_override"].ToString();
		if (!(text != "0"))
		{
			return null;
		}
		return text;
	}
}

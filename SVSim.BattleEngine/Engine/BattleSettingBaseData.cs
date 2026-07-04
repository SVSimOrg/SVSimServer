using LitJson;

public class BattleSettingBaseData
{
	public int PlayerCharaId { get; }

	public int EnemyCharaId { get; }

	public int EnemyClassId { get; }

	public int EnemyAiId { get; }

	public int FieldId { get; }

	public string BgmId { get; }

	public BattleSettingBaseData(JsonData jsonData)
	{
		PlayerCharaId = jsonData["chara_id"].ToInt();
		EnemyCharaId = jsonData["enemy_chara_id"].ToInt();
		EnemyClassId = jsonData["enemy_class"].ToInt();
		EnemyAiId = jsonData["enemy_ai_id"].ToInt();
		FieldId = jsonData["battle3dfield_id"].ToInt();
		BgmId = GetBgmId(jsonData);
	}

	private static string GetBgmId(JsonData jsonData)
	{
		string text = jsonData["bgm_id"].ToString();
		if (!(text == "0"))
		{
			return text;
		}
		return "NONE";
	}
}

using LitJson;

namespace Wizard;

public class SecretBossInfo
{
	public bool IsEnable { get; }

	public int CharaId { get; }

	public bool IsCleared { get; }

	public SecretBossInfo(JsonData data)
	{
		if (!data.TryGetValue("bossrush_info", out var value))
		{
			IsEnable = false;
			return;
		}
		if (value.TryGetValue("hidden_boss_character_info", out var value2))
		{
			CharaId = value2["texture_id"].ToInt();
		}
		IsEnable = value.GetValueOrDefault("is_hidden_boss_playable", defaultValue: false);
		IsCleared = value.GetValueOrDefault("is_win_hidden_boss", 0) != 0;
	}
}

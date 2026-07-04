using LitJson;
using Wizard.AutoTest;

namespace Wizard.Battle.Recovery;

public class BattleConditionEnemyInfo : BattleConditionPlayerInfo
{
	public int? AIDifficulty { get; private set; }

	public int? AILevel { get; private set; }

	public int? AIDeckId { get; private set; }

	public int? AIMaxLife { get; private set; }

	public int? AIStyleId { get; private set; }

	public int? AIEmoteId { get; private set; }

	public bool? AIUseInnerEmote { get; private set; }

	public BattleConditionEnemyInfo(JsonData jsonData, bool useDefaultInPlayCardValue)
		: base(jsonData, useDefaultInPlayCardValue)
	{
		AIDifficulty = jsonData.ToIntOrNull("ai_difficulty");
		AILevel = jsonData.ToIntOrNull("ai_logic_level");
		AIDeckId = jsonData.ToIntOrNull("ai_deck_id");
		AIMaxLife = jsonData.ToIntOrNull("ai_max_life");
		AIStyleId = jsonData.ToIntOrNull("ai_style_id");
		AIEmoteId = jsonData.ToIntOrNull("ai_emote_id");
		AIUseInnerEmote = jsonData.ToBooleanOrNull("ai_use_inner_emote");
	}
}

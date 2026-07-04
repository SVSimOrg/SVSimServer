using LitJson;

namespace Wizard;

public class AvatarBattleInfo
{
	public class AvatarBattleBonus
	{
		public string LeaderSkinId { get; private set; }

		public int BattleStartFirstPlayerTurnBp { get; private set; }

		public int BattleStartSecondPlayerTurnBp { get; private set; }

		public int BattleStartMaxLife { get; private set; }

		public string[] AbilityCosts { get; private set; }

		public string[] Abilities { get; private set; }

		public string[] PassiveAbilities { get; private set; }

		public string[] AbilityDesc { get; private set; }

		public string PassiveAbilityDesc { get; private set; }

		public AvatarBattleBonus(JsonData json)
		{
			LeaderSkinId = json["leader_skin_id"].ToString();
			BattleStartFirstPlayerTurnBp = json["battle_start_firstplayerturn_bp"].ToInt();
			BattleStartSecondPlayerTurnBp = json["battle_start_secondplayerturn_bp"].ToInt();
			BattleStartMaxLife = json["battle_start_max_life"].ToInt();
			AbilityCosts = json["ability_cost"].ToString().Split('|');
			Abilities = json["ability"].ToString().Split(',');
			PassiveAbilities = json["passive_ability"].ToString().Split(',');
			AbilityDesc = json["ability_desc"].ToString().Split('|');
			PassiveAbilityDesc = json["passive_ability_desc"].ToString();
		}
	}

	public string LeaderSkinId { get; private set; }

	public AvatarBattleBonus Bonus { get; private set; }

	public AvatarBattleInfo(JsonData json)
	{
		LeaderSkinId = json["leader_skin_id"].ToString();
	}
}

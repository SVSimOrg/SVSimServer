using System.Collections.Generic;

public class CardDataModel
{
	public int Index { get; set; }

	public int CardId { get; set; }

	public int RedrawCardPosition { get; set; }

	public bool isOpponent { get; set; }

	public NetworkBattleDefine.NetworkCardPlaceState fromState { get; set; }

	public List<NetworkBattleDefine.NetworkCardPlaceState> ToStateList { get; set; }

	public int skillCardIndex { get; set; }

	public int publishedActiveSkillCount { get; set; }

	public int skillMovementNum { get; set; }

	public List<int> skillKeyCardIdxList { get; set; }

	public List<int> SkillKeyCardIdList { get; set; }

	public int playCardCost { get; set; }

	public int? AddLife { get; set; }

	public int? SetLife { get; set; }

	public int? AddAtk { get; set; }

	public int? SetAtk { get; set; }

	public int Clan { get; set; }

	public string Tribe { get; set; }

	public bool IsOpen { get; set; }

	public int Spellboost { get; set; } = -1;

	public int? AddChantCount { get; set; }

	public int? SetChantCount { get; set; }

	public int UnionBurstCount { get; set; }

	public int SkyboundArtCount { get; set; }

	public int SkillIndex { get; set; }

	public string AttachTarget { get; private set; }

	public int RandomTargetIndex { get; set; } = -1;

	public List<int> FusionIngredientList { get; set; }

	public bool IsInvoked { get; set; }

	public bool IsGotUnapproved { get; set; }

	public int SkillCallCount { get; set; }

	public int SkillValueCount { get; set; }

	public int? SkillValueParameter { get; set; }

	public int activate { get; set; }

	public bool IsHighlander { get; set; }

	public CardDataModel()
	{
		Clan = -1;
		Tribe = "NONE";
		playCardCost = -1;
		publishedActiveSkillCount = -1;
		SkillCallCount = -1;
		SkillValueCount = -1;
		SkillValueParameter = null;
		activate = -1;
		SkillIndex = -1;
		IsHighlander = false;
		ToStateList = new List<NetworkBattleDefine.NetworkCardPlaceState>();
		skillKeyCardIdxList = new List<int>();
		SkillKeyCardIdList = new List<int>();
		UnionBurstCount = -1;
		SkyboundArtCount = -1;
	}

	public void SetAttachTarget(string attach)
	{
		AttachTarget = attach;
	}
}
